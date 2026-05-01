using UnityEngine;

/// <summary>
/// Fizik kullanmadan görsel objeyi sabit bir hýzla aþaðý kaydýrýr.
/// </summary>
public class VisualFallingTile : MonoBehaviour
{
    [Header("Movement Settings (Hareket Ayarlarý)")]
    public float fallSpeed = 20f; // Düþüþ hýzý (Taþlarýn aþaðý akma hýzý)

    void Update()
    {
        // FÝZÝK YOK! Sadece pozisyonu aþaðý doðru kaydýrýyoruz.
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);
    }
}