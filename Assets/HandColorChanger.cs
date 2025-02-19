using UnityEngine;

public class HandColorChanger : MonoBehaviour
{
    public Renderer handRenderer; // Assign RightHand SkinnedMeshRenderer in Inspector
    private MaterialPropertyBlock propertyBlock;
    private Color originalColor; // Store the original color
    private Color newColor;

    void Start()
    {
        propertyBlock = new MaterialPropertyBlock();

        // Get and store the original color at the start
        handRenderer.GetPropertyBlock(propertyBlock);
        originalColor = propertyBlock.GetColor("_Color"); // Ensure the correct property name
    }

    public void ChangeColor()
    {
        newColor = new Color(0f, 0f, 1f);
        handRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor("_Color", newColor);
        handRenderer.SetPropertyBlock(propertyBlock);
    }

    public void RevertColor()
    {
        handRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor("_Color", originalColor);
        handRenderer.SetPropertyBlock(propertyBlock);
    }
}
