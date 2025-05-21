using UnityEngine;
using TMPro;

public class TooltipController : MonoBehaviour
{
    public static TooltipController Instance; // Singleton

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

    // ✅ NEW ShowTooltip: Accepts stamina and sanity separately
    public void ShowTooltip(int stamina, int sanity, int damage)
    {
        tooltipText.text = $"<b>Stamina:</b> {stamina}\n<b>Sanity:</b> {sanity}\n<b>Damage:</b> {damage}";
        gameObject.SetActive(true);
    }

    // Hide
    public void HideTooltip()
    {
        tooltipObject.SetActive(false);
    }
}
