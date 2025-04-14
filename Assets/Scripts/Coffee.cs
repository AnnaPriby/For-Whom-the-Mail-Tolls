using UnityEngine;
using UnityEngine.EventSystems;

// This script allows a Coffee object to be clicked to restore stamina
public class Coffee : MonoBehaviour, IPointerClickHandler
{
    [Header("Stamina Refill Settings")]
    public int refillAmount = 20; // Amount of stamina to restore when clicked (editable in Inspector)

    [Header("Reference to Stat Manager")]
    public StatManager statManager; // Reference to the central StatManager that controls stamina

    // Called automatically when this object is clicked
    public void OnPointerClick(PointerEventData eventData)
    {
        if (statManager != null)
        {
            // Apply a positive stamina change to the stat manager
            statManager.ApplyStaminaDelta(refillAmount);

            // Log a confirmation message
            Debug.Log($"☕ Coffee used! +{refillAmount} stamina.");
        }
    }
}
