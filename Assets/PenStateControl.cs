using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenStateControl : MonoBehaviour
{
    private float lastToggleTime = 0f;
    private const float toggleDeadTime = 0.2f;

    [SerializeField] private GameObject syncTarget;
    [SerializeField] private bool isSynchronized = false;

    public void ToggleActiveState()
    {
        if (Time.time - lastToggleTime >= toggleDeadTime)
        {
            gameObject.SetActive(!gameObject.activeSelf);
            lastToggleTime = Time.time;
        }
    }

    public void SynchronizeState()
    {
        if (isSynchronized)
        {
            return;
        }

        if (syncTarget == null)
        {
            Debug.LogError("Sync target is not set");
            return;
        }

        gameObject.SetActive(syncTarget.gameObject.activeSelf);
    }
}
