using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HoverText : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject descriptionText; // Text2 để hiển thị mô tả

    // Khi con trỏ chuột hover vào Text1
    public void OnPointerEnter(PointerEventData eventData)
    {
        descriptionText.SetActive(true); // Hiển thị Text2
    }

    // Khi con trỏ chuột rời khỏi Text1
    public void OnPointerExit(PointerEventData eventData)
    {
        descriptionText.SetActive(false); // Ẩn Text2
    }
}