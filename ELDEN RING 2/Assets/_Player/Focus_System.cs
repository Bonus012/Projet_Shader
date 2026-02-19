using Unity.Cinemachine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class Focus_System : MonoBehaviour
{
    [Header("Paramètres de Focus")]
    [SerializeField] private float focusRange = 20f;
    [SerializeField] private float maxFocusAngle = 60f;
    [SerializeField] private LayerMask focusableLayer;

    [Header("Cinemachine")]
    [SerializeField] private CinemachineCamera focusCamera;
    [SerializeField] private CinemachineCamera normalCamera;
    [SerializeField] private float focusCameraDistance = 5f;
    [SerializeField] private float focusCameraHeight = 1.5f;
    [SerializeField] private float targetSwitchSpeed = 5f;

    private Focusable currentTarget;
    private List<Focusable> availableTargets = new();
    private bool isFocusing;
    private CinemachineFollow focusFollow;
    private Transform smoothLookAtTarget;

    private PlayerInput playerInput;
    private InputAction focusAction;
    private InputAction switchTargetAction;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        if (playerInput != null)
        {
            focusAction = playerInput.actions["Focus"];
            switchTargetAction = playerInput.actions["SwitchTarget"];
        }
    }

    void OnEnable()
    {
        if (focusAction != null)
            focusAction.performed += OnFocusPerformed;

        if (switchTargetAction != null)
            switchTargetAction.performed += OnSwitchTargetPerformed;
    }

    void OnDisable()
    {
        if (focusAction != null)
            focusAction.performed -= OnFocusPerformed;

        if (switchTargetAction != null)
            switchTargetAction.performed -= OnSwitchTargetPerformed;
    }

    void Start()
    {
        // Crée un transform vide pour le smooth look at
        GameObject smoothTarget = new GameObject("SmoothLookAtTarget");
        smoothLookAtTarget = smoothTarget.transform;
        smoothLookAtTarget.SetParent(null);

        if (focusCamera != null)
        {
            focusFollow = focusCamera.GetComponent<CinemachineFollow>();
            focusCamera.Priority = 0;
        }

        if (normalCamera != null)
        {
            normalCamera.Priority = 10;
        }
    }

    void Update()
    {
        if (focusCamera != null)
        {
            if (!isFocusing && focusCamera.Priority == 0) return;
        }
        if (currentTarget != null)
        {
            smoothLookAtTarget.position = Vector3.Lerp(
                smoothLookAtTarget.position,
                currentTarget.GetFocusPoint().position,
                Time.deltaTime * targetSwitchSpeed
            );

            RotateTowardsTarget();
            UpdateFocusCamera();
        }
    }


    void OnFocusPerformed(InputAction.CallbackContext context)
    {
        if (!isFocusing)
            FindAndFocusNearestTarget();
        else
            ReleaseFocus();
    }

    void OnSwitchTargetPerformed(InputAction.CallbackContext context)
    {
        if (!isFocusing) return;

        float value = context.ReadValue<float>();

        if (value > 0)
            SwitchToNextTarget();
        else if (value < 0)
            SwitchToPreviousTarget();
    }

    void FindAndFocusNearestTarget()
    {
        availableTargets.Clear();

        Collider[] colliders = Physics.OverlapSphere(transform.position, focusRange, focusableLayer);

        foreach (Collider col in colliders)
        {
            if (!col.TryGetComponent(out Focusable focusable)) continue;
            if (!focusable.CanBeFocused()) continue;

            Vector3 dir = (focusable.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dir);

            if (angle <= maxFocusAngle)
                availableTargets.Add(focusable);
        }

        if (availableTargets.Count > 0)
        {
            availableTargets = availableTargets
                .OrderBy(t => Vector3.Distance(transform.position, t.transform.position))
                .ToList();

            SetFocus(availableTargets[0]);
        }
    }

    void SetFocus(Focusable target)
    {
        currentTarget = target;
        isFocusing = true;

        // Si c'est le premier focus, positionne directement le smooth target
        if (smoothLookAtTarget.position == transform.position)
        {
            smoothLookAtTarget.position = currentTarget.GetFocusPoint().position;
        }

        focusCamera.Priority = 20;
        focusCamera.LookAt = smoothLookAtTarget;

        normalCamera.Priority = 0;

        currentTarget.OnFocused();
    }

    void ReleaseFocus()
    {
        if (currentTarget != null)
            currentTarget.OnUnfocused();

        currentTarget = null;
        isFocusing = false;
        availableTargets.Clear();

        focusCamera.Priority = 0;
        normalCamera.Priority = 10;
    }

    void SwitchToNextTarget()
    {
        if (availableTargets.Count <= 1) return;

        int index = (availableTargets.IndexOf(currentTarget) + 1) % availableTargets.Count;
        currentTarget.OnUnfocused();
        SetFocus(availableTargets[index]);
    }

    void SwitchToPreviousTarget()
    {
        if (availableTargets.Count <= 1) return;

        int index = (availableTargets.IndexOf(currentTarget) - 1 + availableTargets.Count) % availableTargets.Count;
        currentTarget.OnUnfocused();
        SetFocus(availableTargets[index]);
    }

    void RotateTowardsTarget()
    {
        Vector3 dir = currentTarget.transform.position - transform.position;
        dir.y = 0;

        if (dir.sqrMagnitude < 0.01f) return;

        Quaternion rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 10f);
    }

    void UpdateFocusCamera()
    {
        if (focusFollow == null || currentTarget == null) return;

        Vector3 dir = (currentTarget.transform.position - transform.position).normalized;

        focusFollow.FollowOffset = new Vector3(
            -dir.x * focusCameraDistance,
            focusCameraHeight,
            -dir.z * focusCameraDistance
        );
    }

    public bool IsFocusing() => isFocusing;
    public Focusable GetCurrentTarget() => currentTarget;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, focusRange);
    }
}