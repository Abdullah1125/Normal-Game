using UnityEngine;
using UnityEngine.EventSystems;

public class TopPanelHandler : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public RectTransform panel;
    public float openY = -600f; // Panelin açık hali (eksi değer olmalı)
    public float closeY = 0f;   // Panelin kapalı hali
    public float speed = 15f;   // Mıknatıs hızı
    public float threshold = 0.05f; // Yüzde kaç çekilince açılsın? (0.5 = yarısı)

    private bool isOpen = false;
    private bool isDragging = false;

    public void OnDrag(PointerEventData eventData)
    {
        isDragging = true;
        // Mevcut pozisyonun üzerine sürükleme miktarını ekle
        float newY = panel.anchoredPosition.y + eventData.delta.y;

        // Panelin dışarı taşmasını engelle (Sınırlandır)
        newY = Mathf.Clamp(newY, openY, closeY);

        panel.anchoredPosition = new Vector2(0, newY);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        // Panel açık mı kalsın kapansın mı? (Eşik kontrolü)
        // Eğer panel openY'nin yarısından daha aşağıdaysa açık kalsın
        if (panel.anchoredPosition.y < (openY * threshold))
        {
            isOpen = true;
        }
        else
        {
            isOpen = false;
        }
    }

    void Update()
    {
        // Eğer kullanıcı sürüklemiyorsa, hedef konuma yumuşakça git
        if (!isDragging)
        {
            float targetY = isOpen ? openY : closeY;
            Vector2 targetPos = new Vector2(0, targetY);

            panel.anchoredPosition = Vector2.Lerp(panel.anchoredPosition, targetPos, Time.deltaTime * speed);
        }
    }
}