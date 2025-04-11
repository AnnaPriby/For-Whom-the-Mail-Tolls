using UnityEngine;
using TMPro;

public class TooltipController : MonoBehaviour
{
    public static TooltipController Instance;

    public GameObject tooltipObject;
    public TextMeshProUGUI tooltipText;
    public Vector3 offset = new Vector3(10f, -10f, 0f);

    void Awake()
    {
        Instance = this;
        HideTooltip();
    }

    void Update()
    {
        if (tooltipObject.activeSelf)
        {
            tooltipObject.transform.position = Input.mousePosition + offset;
        }
    }

    public void ShowTooltip(string text)
    {
        tooltipText.text = text;
        tooltipObject.SetActive(true);
    }

    public void HideTooltip()
    {
        tooltipObject.SetActive(false);
    }
}
