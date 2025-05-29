using System;
using UnityEngine;
using UnityEngine.EventSystems;

// This script allows a Coffee object to be clicked to restore stamina and reduce sanity
public class Coffee : MonoBehaviour, IPointerClickHandler
{
    [Header("Stamina Refill Settings")]
    public int refillAmount = 12; // Amount of stamina to restore

    [Header("Sanity Adjustment Settings")]
    public int sanityDivisionFactor = 3; // Factor to divide current sanity by

    [Header("Reference to Stat Manager")]
    public StatManager statManager; // Reference to StatManager

    // Called automatically when this object is clicked
    public void OnPointerClick(PointerEventData eventData)
    {
        if (statManager != null)
        {
            // Apply stamina boost
            statManager.ApplyStaminaDelta(refillAmount);

            // Sanity division logic
            int currentSanity = statManager.CurrentSanity;
            int newSanity = currentSanity / Mathf.Max(sanityDivisionFactor, 1); // prevent divide by 0
            int deltaSanity = newSanity - currentSanity;
            statManager.ApplySanityDelta(deltaSanity);

            // Removed GameLoop.Instance.Coffee();
            // GameLoop.Instance.Coffee();

            Debug.Log($"☕ Coffee used! +{refillAmount} stamina, sanity reduced to {newSanity} (÷{sanityDivisionFactor}).");
        }
    }
}
