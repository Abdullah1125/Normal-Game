using UnityEngine;

/// <summary>
/// Alýndýðýnda bekçi ajanýný devreye sokan gizli anahtar.
/// </summary>
public class DynamicCrossKey : MonoBehaviour, IResettable
{
    public string keyID = "SecretKey_A";

    private void Start()
    {
        if (LevelManager.Instance != null) LevelManager.Instance.RegisterResettable(this);
        if (PlayerPrefs.GetInt(Constants.PREF_TROLL_HACK_ACTIVE, 0) == 0) gameObject.SetActive(false);
    }

    /// <summary>
    /// Oyuncu çarptýðýnda bekçiyi yaratýr ve kendini gizler.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(Constants.TAG_PLAYER))
        {
            PlayerPrefs.SetInt(keyID, 1);
            PlayerPrefs.SetInt(Constants.PREF_TROLL_HACK_ACTIVE, 0);
            PlayerPrefs.Save();

            if (Object.FindFirstObjectByType<KeyLossMonitorPayload>() == null)
            {
                new GameObject("KeyMonitor_Agent").AddComponent<KeyLossMonitorPayload>().Setup(keyID);
            }

            if (SoundManager.Instance != null) SoundManager.PlayThemeSFX(SFXType.Key);
            gameObject.SetActive(false);
        }
    }

    public void ResetMechanic() { if (PlayerPrefs.GetInt(Constants.PREF_TROLL_HACK_ACTIVE, 1) == 1 && PlayerPrefs.GetInt(keyID, 0) == 0) gameObject.SetActive(true); }
    private void OnDestroy() { if (LevelManager.Instance != null) LevelManager.Instance.UnregisterResettable(this); }
}