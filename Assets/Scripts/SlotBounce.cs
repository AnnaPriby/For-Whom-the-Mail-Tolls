using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using DG.Tweening;
public class SlotBounce : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler
{
    public Vector3 startScale = new Vector3(1f, 1f, 1f);
    public Vector3 endScale = new Vector3(1.1f, 1.1f, 1.1f);
    

    private bool bounceEnabled = false; // 🔹 Internal guard
    
    public List<SlotBounce> allSlots;

    public void EnableBounce()
    {
        bounceEnabled = true;
    }

    public void DisableBounce()
    {
        bounceEnabled = false;
        StopBounce();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!bounceEnabled) return; // 🔒 Prevent animation when disabled
        transform.DOScale(endScale, 0.3f).SetEase(Ease.InOutSine);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!bounceEnabled) return;
        transform.DOScale(startScale, 0.3f).SetEase(Ease.InOutSine);
    }

    public void OnDrop(PointerEventData eventData)
    {
        transform.DOScale(startScale, 0.3f).SetEase(Ease.InOutSine);
        foreach (var slot in allSlots)
        {
            slot.DisableBounce();
        }
    }

    public void StopBounce()
    {
        transform.DOKill();
        transform.DOScale(startScale, 0.2f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            bounceEnabled = false;
        });
    }
}
