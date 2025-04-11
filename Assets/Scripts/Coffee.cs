using UnityEngine;
using UnityEngine.EventSystems;

public class Coffee : MonoBehaviour, IPointerClickHandler
{
    [Header("Stamina Refill Settings")]
    public int refillAmount = 20;

    [Header("Reference to Stat Manager")]
    public StatManager statManager;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (statManager != null)
        {
            statManager.ApplyStaminaDelta(refillAmount);
            Debug.Log($"☕ Coffee used! +{refillAmount} stamina.");
        }
    }
}
