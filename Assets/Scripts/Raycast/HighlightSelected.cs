using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.Surfaces;

public class HighlightSelected : MonoBehaviour
{
    [SerializeField]
    private Material _highlightMaterial;

    [SerializeField]
    private Color _highlightColor = Color.yellow;

    [SerializeField]
    private float _glowIntensity = 1f;

    private Material _originalMaterial;
    private MeshRenderer _meshRenderer;
    private RayInteractable _rayInteractable;
    private ShapeController _shapeController;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _rayInteractable = GetComponent<RayInteractable>();
        _shapeController = GetComponent<ShapeController>();
        
        if (_meshRenderer != null)
        {
            _originalMaterial = _meshRenderer.material;
        }
    }

    public void ApplyHighlight(bool isSelected)
    {
        if (_meshRenderer == null) return;

        if (isSelected)
        {
            if (_highlightMaterial != null)
            {
                _meshRenderer.material = _highlightMaterial;
                _meshRenderer.material.SetColor("_EmissionColor", _highlightColor * _glowIntensity);
            }
            else
            {
                _meshRenderer.material.EnableKeyword("_EMISSION");
                _meshRenderer.material.SetColor("_EmissionColor", _highlightColor * _glowIntensity);
            }
            
            if (_shapeController != null)
            {
                _shapeController.SetTargetObject(Object.This);
            }
        }
        else
        {
            _meshRenderer.material = _originalMaterial;
            if (_shapeController != null)
            {
                _shapeController.ResetTargetObject();
            }
        }
    }
} 