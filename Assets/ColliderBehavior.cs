using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Collider Behavior", menuName = "Custom/Collider Behavior")]
// [ExtensionOfNativeClass]
public class ColliderBehavior : ScriptableObject
{
    public enum BehaviorType
    {
        ChangeColor,
        ChangeScale,
        CustomFunction
    }

    public string behaviorName;
    public BehaviorType behaviorType;

    // For ChangeColor
    public Color targetColor = Color.white;

    // For ChangeScale
    public Vector3 targetScale = Vector3.one;

    // For CustomFunction
    public UnityEvent customEvent;

    public void ExecuteBehavior(GameObject target)
    {
        switch (behaviorType)
        {
            case BehaviorType.ChangeColor:
                ChangeColor(target);
                break;
            case BehaviorType.ChangeScale:
                ChangeScale(target);
                break;
            case BehaviorType.CustomFunction:
                customEvent.Invoke();
                break;
        }
    }

    private void ChangeColor(GameObject target)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = targetColor;
        }
    }

    private void ChangeScale(GameObject target)
    {
        target.transform.localScale = targetScale;
    }
}
