using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles professional mobile movement logic, including finger sliding and dual-press cancellation.
/// (Kaydýrma ve çift basýmý iptal etme dahil profesyonel mobil hareket mantýðýný yönetir.)
/// </summary>
public class MobileDirectionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Button Settings (Buton Ayarý)")]
    [Tooltip("Bu sol buton mu? (Sað için tiki kaldýrýn)")]
    public bool isLeftButton;

    // Statik þalterler: Tüm butonlar birbirinin durumunu bilir
    private static bool leftPressed;
    private static bool rightPressed;

    // Parmaðý butona ilk bastýðýnda ÇALIÞIR
    public void OnPointerDown(PointerEventData eventData) => Press();

    // Parmaðý basýlý tutup, kaydýrarak butonun üstüne geldiðinde ÇALIÞIRk
    public void OnPointerEnter(PointerEventData eventData) => Press();

    // Parmaðý ekrandan çektiðinde ÇALIÞIR
    public void OnPointerUp(PointerEventData eventData) => Release();

    // Parmaðý basýlý tutup butonun dýþýna kaydýrdýðýnda ÇALIÞIR
    public void OnPointerExit(PointerEventData eventData) => Release();

    private void Press()
    {
        if (isLeftButton) leftPressed = true;
        else rightPressed = true;

        UpdateMovement();
    }

    private void Release()
    {
        if (isLeftButton) leftPressed = false;
        else rightPressed = false;

        UpdateMovement();
    }

    /// <summary>
    /// Þalterlere bakarak nihai hareket kararýný PlayerController'a iletir.
    /// (Statik yaptýk ki PlayerController dirildiðinde bunu uzaktan tetikleyebilsin!)
    /// </summary>
    public static void UpdateMovement()
    {
        if (PlayerController.Instance == null) return;

        if (leftPressed && rightPressed) 
        {
            PlayerController.Instance.Move(0);
        }
        else if (leftPressed) 
        {
            PlayerController.Instance.Move(-1);
        }
        else if (rightPressed) 
        {
            PlayerController.Instance.Move(1);
        }
        else 
        {
            PlayerController.Instance.Move(0);
        }
    }
}