using UnityEngine;
using System.Collections.Generic;

public class ColliderHandler : MonoBehaviour
{
    public List<ColliderBehavior> assignedBehaviors = new List<ColliderBehavior>();

    private void OnTriggerEnter(Collider other)
    {
        ExecuteBehaviors(other.gameObject);
    }

    private void ExecuteBehaviors(GameObject target)
    {
        foreach (var behavior in assignedBehaviors)
        {
            behavior.ExecuteBehavior(target);
        }
    }

    // This method allows you to add behaviors through code
    public void AddBehavior(ColliderBehavior behavior)
    {
        assignedBehaviors.Add(behavior);
    }

    // Optional: Remove a behavior
    public void RemoveBehavior(ColliderBehavior behavior)
    {
        assignedBehaviors.Remove(behavior);
    }
}
