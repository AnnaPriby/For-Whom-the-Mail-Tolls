using System;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
public class Credits : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI credits;
    public Vector3 startPosition;
    public Vector3 endPosition;

    private void Start()
    {
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        credits.transform.DOLocalMove(endPosition, 0.7f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        credits.transform.DOLocalMove(startPosition, 0.7f);
    }
}
