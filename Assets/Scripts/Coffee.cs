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

    public float sanityMultiplier = 2f / 3f;

    [Header("Reference to Stat Manager")]
    public StatManager statManager; // Reference to StatManager

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip drinkSound;

    [Header("UI")]
    public Vector3 startScale = new Vector3(1f, 1f, 1f);
    public Vector3 endScale = new Vector3(1.1f, 1.1f, 1.1f);

    [Header("Animations")]
    public Animator handsAnimator;

    private bool hasBeenUsedToday = false;

    // Called automatically when this object is clicked
    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(endScale, 0.3f).SetEase(Ease.InOutSine); // Increase size on hover
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(startScale, 0.3f).SetEase(Ease.InOutSine);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (hasBeenUsedToday) return;

        if (statManager != null)
        {
            statManager.ApplyStaminaDelta(refillAmount);

            int currentSanity = statManager.CurrentSanity;
            int newSanity = (int)(currentSanity * sanityMultiplier);
            int deltaSanity = newSanity - currentSanity;
            statManager.ApplySanityDelta(deltaSanity);

            Debug.Log($"☕ Coffee used! +{refillAmount} stamina, sanity reduced to {newSanity} (÷{sanityDivisionFactor}).");
        }

        if (audioSource != null && drinkSound != null)
        {
            audioSource.PlayOneShot(drinkSound);
        }

        handsAnimator.SetTrigger("CoffeeDrink");
        hasBeenUsedToday = true;
    }


    public void ResetDailyUse()
    {
        hasBeenUsedToday = false;
    }
}
