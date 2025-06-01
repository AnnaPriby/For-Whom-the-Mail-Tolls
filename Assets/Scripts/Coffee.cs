using System;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

// This script allows a Coffee object to be clicked to restore stamina and reduce sanity
public class Coffee : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Stamina Refill Settings")]
    public int refillAmount = 12; // Amount of stamina to restore

    [Header("Sanity Adjustment Settings")]
    public int sanityDivisionFactor = 3; // Factor to divide current sanity by

    [Header("Reference to Stat Manager")]
    public StatManager statManager; // Reference to StatManager
    
    [Header("UI")]
    public Vector3 startScale = new Vector3(1f, 1f, 1f);
    public Vector3 endScale = new Vector3(1.1f, 1.1f, 1.1f);

    // Called automatically when this object is clicked
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Make the item slightly larger when hovered over
        //transform.localScale = new Vector3(1.2f, 1.2f, 1.2f); 
        transform.DOScale(endScale, 0.3f).SetEase(Ease.InOutSine); // Increase size on hover
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Reset the size when hover ends
        //transform.localScale = startScale; // Reset to original size
        transform.DOScale(startScale, 0.3f).SetEase(Ease.InOutSine);
    }
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
