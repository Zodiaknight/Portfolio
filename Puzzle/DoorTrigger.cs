using System.Collections.Generic;
using UnityEngine;

public class doorTrigger : PuzzleInteractable
{
    List<Collider> colliders = new List<Collider>();

    void FixedUpdate()
    {
        for (int i = 0; i < colliders.Count; i++)
        {
            Collider collider = colliders[i];

            if (!collider || !collider.gameObject.activeInHierarchy)
            {
                colliders.RemoveAt(i--);

                if (colliders.Count == 0)
                {
                    IsSolved = false;
                    enabled = false;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        ReliableOnTriggerExit.NotifyTriggerEnter(other, gameObject, OnTriggerExit);

        if (colliders.Count == 0)
        {
            IsSolved = true;
            enabled = true;

            NotifyDoor();
        }

        colliders.Add(other);
    }
    private void OnTriggerExit(Collider other)
    {
        ReliableOnTriggerExit.NotifyTriggerExit(other, gameObject);

        if (colliders.Remove(other) && colliders.Count == 0)
        {
            IsSolved = false;
            enabled = false;

            NotifyDoor();
        }
    }

    void OnDisable()
    {
#if UNITY_EDITOR
        if (enabled && gameObject.activeInHierarchy)
        {
            return;
        }
#endif
        if (colliders.Count > 0)
        {
            colliders.Clear();
            IsSolved = false;
        }
    }
}
