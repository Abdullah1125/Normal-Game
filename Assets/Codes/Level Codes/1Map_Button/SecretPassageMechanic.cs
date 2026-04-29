using UnityEngine;

/// <summary>
/// Gizli geçit duvarýnýn çarpýþmasýný kapatýr. Kamera hareket tetikleyicileri kaldýrýlmýþtýr.
/// </summary>
public class SecretPassageMechanic : MonoBehaviour
{
    [Header("Wall Settings (Duvar Ayarlarý)")]
    public string targetObjectName = "Tilemap_Secret";

    private GameObject secretWall;
    private Collider2D wallCollider;

    /// <summary>
    /// Baþlangýçta gizli duvarý bulur ve oyuncunun içinden geçebilmesi için collider'ýný kapatýr.
    /// </summary>
    void Start()
    {
        secretWall = GameObject.Find(targetObjectName);
        if (secretWall != null)
        {
            wallCollider = secretWall.GetComponent<Collider2D>();
            if (wallCollider != null) wallCollider.enabled = false;
        }
    }

    /// <summary>
    /// Obje devre dýþý kaldýðýnda duvar collider'ýný tekrar aktif eder.
    /// </summary>
    private void OnDisable()
    {
        if (wallCollider != null) wallCollider.enabled = true;
    }
}