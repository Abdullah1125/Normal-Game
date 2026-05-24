using UnityEngine;

/// <summary>
/// Alýndýðýnda bekçi ajanýný devreye sokan gizli anahtar.
/// (Secret key that activates the monitor agent when collected.)
/// </summary>
public class DynamicCrossKey : MonoBehaviour, IResettable
{
    [Header("Settings (Ayarlar)")]
    public string keyID = "SecretKey_A";

    /// <summary>
    /// Baþlangýçta anahtarýn durumunu kontrol eder ve sisteme kaydolur.
    /// </summary>
    private void Start()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.RegisterResettable(this);

        // Eðer hack aktif deðilse anahtarý baþtan gizle
        if (PlayerPrefs.GetInt(Constants.PREF_TROLL_HACK_ACTIVE, 0) == 0)
            gameObject.SetActive(false);
    }

    /// <summary>
    /// Oyuncu çarptýðýnda bekçiyi yaratýr ve kendini gizler.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(Constants.TAG_PLAYER))
        {
            PlayerPrefs.SetInt(keyID, 1);

            // Hack durumunu kapatýyoruz
            PlayerPrefs.SetInt(Constants.PREF_TROLL_HACK_ACTIVE, 0);

            // Bekçi ajaný sahneye çaðýrýyoruz
            if (Object.FindFirstObjectByType<KeyLossMonitorPayload>() == null)
            {
                new GameObject("KeyMonitor_Agent").AddComponent<KeyLossMonitorPayload>().Setup(keyID);
            }

            if (SoundManager.Instance != null)
                SoundManager.PlayThemeSFX(SFXType.Key);

            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Oyuncu öldüðünde (resetlendiðinde) anahtarý doðru þarta göre geri getirir.
    /// </summary>
    public void ResetMechanic()
    {
        // HATANIN ÇÖZÜLDÜÐÜ YER: Varsayýlan deðeri 1 yerine 0 yaptýk!
        // Artýk sadece sistem gerçekten "Hack" modundaysa (deðer 1 ise) anahtar dirilecek.
        if (PlayerPrefs.GetInt(Constants.PREF_TROLL_HACK_ACTIVE, 0) == 1 && PlayerPrefs.GetInt(keyID, 0) == 0)
        {
            gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Obje silindiðinde listelerden kendini temizler.
    /// </summary>
    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.UnregisterResettable(this);
    }
}