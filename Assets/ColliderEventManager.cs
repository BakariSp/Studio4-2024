using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ColliderEventManager : MonoBehaviour
{
    public enum CollisionType
    {
        Enter,
        Exit,
        Stay
    }

    [System.Serializable]
    public class ColliderPair
    {
        public string eventName; // New field to store the event name
        public Collider collider1;
        public Collider collider2;
        public CollisionType collisionType;
        public UnityEvent onCollisionEvent;
        public bool wasColliding; // To track state for Exit and Stay
    }

    public List<ColliderPair> colliderPairs = new List<ColliderPair>();

    private void Update()
    {
        CheckCollisions();
    }

    private void CheckCollisions()
    {
        foreach (var pair in colliderPairs)
        {
            if (pair.collider1 != null && pair.collider2 != null)
            {
                bool isColliding = Physics.ComputePenetration(
                    pair.collider1, pair.collider1.transform.position, pair.collider1.transform.rotation,
                    pair.collider2, pair.collider2.transform.position, pair.collider2.transform.rotation,
                    out Vector3 direction, out float distance
                );

                switch (pair.collisionType)
                {
                    case CollisionType.Enter:
                        if (isColliding && !pair.wasColliding)
                        {
                            pair.onCollisionEvent.Invoke();
                        }
                        break;
                    case CollisionType.Exit:
                        if (!isColliding && pair.wasColliding)
                        {
                            pair.onCollisionEvent.Invoke();
                        }
                        break;
                    case CollisionType.Stay:
                        if (isColliding)
                        {
                            pair.onCollisionEvent.Invoke();
                        }
                        break;
                }

                pair.wasColliding = isColliding;
            }
        }
    }

    // Updated method to add a new collider pair event with a name
    public void AddColliderEvent(string eventName, Collider collider1, Collider collider2, CollisionType collisionType, UnityAction action)
    {
        var existingPair = colliderPairs.Find(p => 
            p.eventName == eventName &&
            (p.collider1 == collider1 && p.collider2 == collider2 || 
             p.collider1 == collider2 && p.collider2 == collider1) && 
            p.collisionType == collisionType);

        if (existingPair == null)
        {
            existingPair = new ColliderPair
            {
                eventName = eventName,
                collider1 = collider1,
                collider2 = collider2,
                collisionType = collisionType,
                onCollisionEvent = new UnityEvent(),
                wasColliding = false
            };
            colliderPairs.Add(existingPair);
        }
        existingPair.onCollisionEvent.AddListener(action);
    }

    // Updated method to remove a collider pair event by name
    public void RemoveColliderEvent(string eventName)
    {
        colliderPairs.RemoveAll(p => p.eventName == eventName);
    }

    // New method to get a specific collider pair event by name
    public ColliderPair GetColliderEvent(string eventName)
    {
        return colliderPairs.Find(p => p.eventName == eventName);
    }

    // Updated method to add a new collider pair event with collision type
    public void AddColliderEvent(Collider collider1, Collider collider2, CollisionType collisionType, UnityAction action)
    {
        var existingPair = colliderPairs.Find(p => 
            (p.collider1 == collider1 && p.collider2 == collider2 || 
             p.collider1 == collider2 && p.collider2 == collider1) && 
            p.collisionType == collisionType);

        if (existingPair == null)
        {
            existingPair = new ColliderPair
            {
                collider1 = collider1,
                collider2 = collider2,
                collisionType = collisionType,
                onCollisionEvent = new UnityEvent(),
                wasColliding = false
            };
            colliderPairs.Add(existingPair);
        }
        existingPair.onCollisionEvent.AddListener(action);
    }

    // Updated method to remove a collider pair event with collision type
    public void RemoveColliderEvent(Collider collider1, Collider collider2, CollisionType collisionType)
    {
        colliderPairs.RemoveAll(p => 
            (p.collider1 == collider1 && p.collider2 == collider2 || 
             p.collider1 == collider2 && p.collider2 == collider1) && 
            p.collisionType == collisionType);
    }

    // Method to clear all collider pair events
    public void ClearColliderEvents()
    {
        colliderPairs.Clear();
    }
}
