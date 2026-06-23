using UnityEngine;
using UnityEngine.EventSystems;

public class UITooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public string tooltipText;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.ShowTooltip(tooltipText, transform.position);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.HideTooltip();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.ShowTooltip(tooltipText, transform.position);
        }
    }
}
