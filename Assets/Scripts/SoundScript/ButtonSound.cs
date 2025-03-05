using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonSound : MonoBehaviour, IPointerEnterHandler
{
    public AudioSource audioSource; // Tham chiếu đến AudioSource
    public AudioClip hoverSound;    // Âm thanh khi di chuột qua
    public AudioClip clickSound;    // Âm thanh khi click chuột

    void Start()
    {
        // Lấy component Button và thêm sự kiện onClick
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(PlayClickSound);
        }
    }

    // Phương thức này được gọi khi con trỏ chuột di chuyển vào button
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (audioSource != null && hoverSound != null)
        {
            audioSource.PlayOneShot(hoverSound); // Phát âm thanh khi hover
        }
    }

    // Phương thức phát âm thanh khi click
    void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound); // Phát âm thanh khi click
        }
    }
}