using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public Animator _playeranimator;
    [SerializeField] private Focus_System _playerFocusSystem;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    public float rotationSpeed = 10f;

    [Header("Dash")]
    public float dashForce = 12f;
    public float dashDuration = 0.15f;
    bool isDashing = false;
    float dashTimer = 0f;

    [Header("Physics")]
    public float mass = 1f;
    public float gravity = -9.81f;

    [Header("Acceleration")]
    public float acceleration = 20f;
    public float deceleration = 25f;

    PlayerInput playerInput;
    InputAction moveAction;
    InputAction jumpAction;
    InputAction sprintAction;
    CharacterController controller;

    Vector3 velocity;
    bool isGrounded;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        sprintAction = playerInput.actions["Sprint"];

        moveAction?.Enable();
        jumpAction?.Enable();
        sprintAction?.Enable();
    }

    private void OnDisable()
    {
        moveAction?.Disable();
        jumpAction?.Disable();
        sprintAction?.Disable();
    }

    private void Update()
    {
        HandleGroundCheck();
        HandleMovement();
        HandleDash();
        if (!isDashing)
            ApplyGravity();
        ApplyFinalMovement();
        UpdateAnimator();
    }

    void HandleGroundCheck()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;
    }

    void HandleMovement()
    {
        Vector2 input2D = moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
        Vector3 move = new Vector3(input2D.x, 0f, input2D.y);

        bool isSprinting = sprintAction != null && sprintAction.ReadValue<float>() > 0.5f;
        float targetSpeed = isSprinting ? sprintSpeed : moveSpeed;

        Transform cam = Camera.main?.transform;
        if (cam != null)
        {
            Vector3 camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
            Vector3 camRight = cam.right;
            move = camForward * move.z + camRight * move.x;
        }

        Vector3 horizontalVel = new Vector3(velocity.x, 0, velocity.z);
        Vector3 targetVel = move.normalized * targetSpeed;

        float accel = move.magnitude > 0.01f ? acceleration : deceleration;

        horizontalVel = Vector3.MoveTowards(
            horizontalVel,
            targetVel,
            (accel / mass) * Time.deltaTime
        );

        velocity.x = horizontalVel.x;
        velocity.z = horizontalVel.z;

        if (move.magnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move.normalized, Vector3.up);
            if (!_playerFocusSystem.IsFocusing())
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
        }
    }

    void HandleDash()
    {
        if (jumpAction != null && jumpAction.triggered && isGrounded && !isDashing)
        {
            isDashing = true;
            dashTimer = dashDuration;
            _playeranimator.SetTrigger("Dodge");

            Vector2 input2D = moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
            Vector3 dashDir = new Vector3(input2D.x, 0f, input2D.y);

            if (dashDir.magnitude < 0.1f)
                dashDir = transform.forward;

            Transform cam = Camera.main?.transform;
            if (cam != null)
            {
                Vector3 camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
                Vector3 camRight = cam.right;
                dashDir = camForward * dashDir.z + camRight * dashDir.x;
            }

            dashDir.Normalize();
            velocity = dashDir * dashForce;
            velocity.y = 0f;
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
                isDashing = false;
        }
    }

    void ApplyGravity()
    {
        velocity.y += gravity * mass * Time.deltaTime;
    }

    void ApplyFinalMovement()
    {
        controller.Move(velocity * Time.deltaTime);
    }

    void UpdateAnimator()
    {
        Vector3 horizontal = new Vector3(velocity.x, 0, velocity.z);
        bool isMoving = horizontal.magnitude > 0.1f;
        bool isSprinting = sprintAction != null && sprintAction.ReadValue<float>() > 0.5f;

        _playeranimator.SetBool("IsMoving", isMoving);
        _playeranimator.SetBool("IsRunning", isSprinting && isMoving);

        Vector3 localDir = transform.InverseTransformDirection(horizontal.normalized);
        float forward = localDir.z;
        float right = localDir.x;

        _playeranimator.SetFloat("Forward", forward);
        _playeranimator.SetFloat("Right", right);
    }
}