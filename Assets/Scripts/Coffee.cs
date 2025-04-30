using System;
using UnityEngine;
using UnityEngine.EventSystems;

// This script allows a Coffee object to be clicked to restore stamina and reduce sanity
public class Coffee : MonoBehaviour, IPointerClickHandler
{
    [Header("Stamina Refill Settings")]
    public int refillAmount = 20; // Amount of stamina to restore when clicked

    [Header("Reference to Stat Manager")]
    public StatManager statManager; // Reference to StatManager

    // Called automatically when this object is clicked
    public void OnPointerClick(PointerEventData eventData)
    {
        if (statManager != null)
        {
            // Apply stamina boost
            statManager.ApplyStaminaDelta(refillAmount);

            // ❗Apply sanity penalty
            statManager.ApplySanityDelta(-1);

            // Trigger coffee event logic
            GameLoop.Instance.Coffee();

            Debug.Log($"☕ Coffee used! +{refillAmount} stamina, -1 sanity.");
        }
    }
}
