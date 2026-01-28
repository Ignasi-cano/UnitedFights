using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private string header;
    private string content;

    public void SetContent(string content, string header = "")
    {
        this.content = content;
        this.header = header;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"[TooltipTrigger] Mouse entered {gameObject.name}. Content: {content}");
        if (!string.IsNullOrEmpty(content))
        {
            string finalMsg = string.IsNullOrEmpty(header) ? content : $"<b>{header}</b>\n{content}";
            TooltipUI.Instance.Show(finalMsg);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipUI.Instance.Hide();
    }

    private void OnDisable()
    {
        if (TooltipUI.HasInstance) TooltipUI.Instance.Hide();
    }
}
