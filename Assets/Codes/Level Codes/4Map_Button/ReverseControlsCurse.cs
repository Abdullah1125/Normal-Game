using UnityEngine;
using UnityEngine.EventSystems;
using System.Reflection;
using System.Collections.Generic;

/// <summary>
/// Inverts player movement controls. Works for both Keyboard and Mobile buttons.
/// (Oyuncu hareket kontrollerini tersine çevirir. Hem klavye hem mobil butonlar için çalýþýr.)
/// </summary>
public class ReverseControlsCurse : MonoBehaviour
{
    private FieldInfo _moveInputUpdate;
    private List<MobileReverseAgent> _injectedAgents = new List<MobileReverseAgent>();

    // Mobil butonlarýn durumunu takip etmek için statik deðiþkenler
    public static bool IsLeftPressed;
    public static bool IsRightPressed;

    void Start()
    {
        // 1. PlayerController içindeki private "moveInput" alanýna eriþim saðla (Reflection)
        _moveInputUpdate = typeof(PlayerController).GetField("moveInput", BindingFlags.NonPublic | BindingFlags.Instance);

        if (_moveInputUpdate == null)
        {
            Debug.LogError("JÝLET HATA: PlayerController içinde 'moveInput' alaný bulunamadý!");
            return;
        }

        // 2. Sahnedeki mobil butonlarý bul ve "Tersine Çevirici Ajan"ý enjekte et
        MobileDirectionButton[] mobileButtons = Object.FindObjectsByType<MobileDirectionButton>(FindObjectsSortMode.None);
        foreach (MobileDirectionButton btn in mobileButtons)
        {
            MobileReverseAgent agent = btn.gameObject.AddComponent<MobileReverseAgent>();
            agent.Setup(btn.isLeftButton);
            _injectedAgents.Add(agent);
        }

        Debug.Log("JÝLET TROLL: Kontroller tersine çevrildi! Sað -> Sol / Sol -> Sað");
    }

    /// <summary>
    /// PlayerController.Update() çalýþtýktan hemen sonra devreye girer ve girdiyi terse çevirir.
    /// </summary>
    void LateUpdate()
    {
        if (PlayerController.Instance == null || !PlayerController.Instance.canMove) return;

        // 1. Mevcut girdiyi hesapla (Klavye + Mobil)
        float rawInput = 0;

        // Klavye Kontrolü
        rawInput += Input.GetAxisRaw("Horizontal");

        // Mobil Kontrolü
        if (IsLeftPressed) rawInput -= 1;
        if (IsRightPressed) rawInput += 1;

        rawInput = Mathf.Clamp(rawInput, -1, 1);

        // 2. Girdiyi TERSÝNE ÇEVÝR (1 ise -1, -1 ise 1 yap)
        float reversedInput = -rawInput;

        // 3. PlayerController içindeki özel moveInput alanýný güncelle
        // Bu sayede karakter hem ters yöne gider hem de ters yöne bakar (Visuals fix)
        _moveInputUpdate.SetValue(PlayerController.Instance, reversedInput);
    }

    void OnDestroy()
    {
        // Temizlik: Ajanlarý kaldýr ve statik deðerleri sýfýrla
        foreach (var agent in _injectedAgents)
        {
            if (agent != null) Destroy(agent);
        }
        IsLeftPressed = false;
        IsRightPressed = false;
    }
}

// =========================================================================
// MOBÝL BUTONLARA SIZAN TERSÝNE ÇEVÝRÝCÝ AJAN (Mobile Reverse Agent)
// =========================================================================
public class MobileReverseAgent : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    private bool _isLeftButton;

    public void Setup(bool isLeft) => _isLeftButton = isLeft;

    public void OnPointerDown(PointerEventData eventData) => SetPressed(true);
    public void OnPointerEnter(PointerEventData eventData) => SetPressed(true);
    public void OnPointerUp(PointerEventData eventData) => SetPressed(false);
    public void OnPointerExit(PointerEventData eventData) => SetPressed(false);

    private void SetPressed(bool state)
    {
        if (_isLeftButton) ReverseControlsCurse.IsLeftPressed = state;
        else ReverseControlsCurse.IsRightPressed = state;
    }
}