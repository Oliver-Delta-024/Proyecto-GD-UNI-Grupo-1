using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChecklistItemHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image previewImage;
    [SerializeField] private Sprite itemSprite;

    public void OnPointerEnter(PointerEventData eventData)
    {
        previewImage.sprite = itemSprite;
        previewImage.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        previewImage.gameObject.SetActive(false);
    }
}
