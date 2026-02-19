using System.Collections.Generic;
using UnityEngine;

public class Focusable : MonoBehaviour
{
    [Header("Paramètres")]
    [SerializeField] private Transform focusPoint;
    [SerializeField] private bool canBeFocused = true;

    [Header("Indicateur Visuel")]
    [SerializeField] private GameObject focusIndicator;

    [Header("Material de Focus")]
    [SerializeField] private Material focusMaterial; // <-- SERIALIZE

    private Renderer[] renderers;
    private List<Material[]> originalMaterials = new List<Material[]>();
    private bool isFocused = false;

    void Start()
    {
        // Création automatique du point de focus si absent
        if (focusPoint == null)
        {
            GameObject fp = new GameObject("FocusPoint");
            fp.transform.SetParent(transform);
            fp.transform.localPosition = Vector3.up * 1.5f;
            focusPoint = fp.transform;
        }

        // Cache l’indicateur visuel
        if (focusIndicator != null)
            focusIndicator.SetActive(false);

        // Récupère tous les renderers et stocke leurs materials d’origine
        renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer r in renderers)
        {
            originalMaterials.Add(r.materials);
        }
    }

    // ---------------------------------------------------------
    // FOCUS
    // ---------------------------------------------------------

    public void OnFocused()
    {
        if (isFocused)
            return;

        isFocused = true;

        if (focusIndicator != null)
            focusIndicator.SetActive(true);

        ApplyFocusMaterial();
    }

    public void OnUnfocused()
    {
        if (!isFocused)
            return;

        isFocused = false;

        if (focusIndicator != null)
            focusIndicator.SetActive(false);

        RestoreOriginalMaterials();
    }

    // ---------------------------------------------------------
    // MATERIAL HANDLING
    // ---------------------------------------------------------

    void ApplyFocusMaterial()
    {
        if (focusMaterial == null)
            return;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];

            List<Material> mats = new List<Material>(r.materials);

            if (!mats.Contains(focusMaterial))
            {
                mats.Add(focusMaterial);
                r.materials = mats.ToArray();
            }
        }
    }

    void RestoreOriginalMaterials()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].materials = originalMaterials[i];
        }
    }

    // ---------------------------------------------------------
    // GETTERS / SETTERS
    // ---------------------------------------------------------

    public bool CanBeFocused()
    {
        return canBeFocused && gameObject.activeSelf;
    }

    public Transform GetFocusPoint()
    {
        return focusPoint;
    }

    public bool IsFocused()
    {
        return isFocused;
    }

    public void SetCanBeFocused(bool value)
    {
        canBeFocused = value;

        if (!value && isFocused)
            OnUnfocused();
    }

    void OnDisable()
    {
        if (isFocused)
            OnUnfocused();
    }
}