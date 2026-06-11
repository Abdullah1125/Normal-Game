using System.Collections;
using UnityEngine;

/// <summary>
/// Detects game restart to open the gate. Highly optimized using Coroutines instead of Update.
/// (Oyunun yeniden baþlatýldýðýný algýlar. Update yerine Coroutine kullanarak yüksek optimizasyon saðlar.)
/// </summary>
public class ParadoxMechanic : MonoBehaviour, IResettable
{
    private static string _appSessionID = "";
    private string _puzzleKey;
    private bool _isMemoryCleared = false;

    /// <summary>
    /// Initializes session IDs, registers the mechanic, and starts the background watchers.
    /// (Oturum kimliklerini hazýrlar, mekaniði kaydeder ve arka plan izleyicilerini baþlatýr.)
    /// </summary>
    private void Start()
    {
        // 1. Yeni açýlýþ kimliði oluþtur (Oyun tamamen kapatýlana kadar sabit kalýr)
        if (string.IsNullOrEmpty(_appSessionID))
        {
            _appSessionID = System.Guid.NewGuid().ToString();
        }

        // 2. Bu bölüme özel kayýt anahtarýný oluþtur
        int levelIndex = LevelManager.Instance != null ? LevelManager.Instance.currentLevelIndex : 0;
        _puzzleKey = Constants.PREF_LEVEL_COMPLETE_PREFIX + "ParadoxStrict_" + levelIndex;

        // 3. Ölünce haberdar olmak için sisteme kayýt ol
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }

        // 4. Kapý açýlma kontrolünü baþlat (Race Condition korumalý)
        StartCoroutine(CheckStrictParadoxRoutine());

        // 5. Bölüm bitiþini izleyen sistemi Update yerine sýfýr maliyetle baþlat
        StartCoroutine(WaitForLevelFinishRoutine());
    }

    /// <summary>
    /// Checks the session ID against memory. Opens the gate if the game was completely restarted.
    /// (Oturum kimliðini hafýza ile karþýlaþtýrýr. Oyun tamamen yeniden baþlatýldýysa kapýyý açar.)
    /// </summary>
    private IEnumerator CheckStrictParadoxRoutine()
    {
        string savedSession = PlayerPrefs.GetString(_puzzleKey, "");

        if (string.IsNullOrEmpty(savedSession))
        {
            // OYUNCU ÝLK DEFA GELDÝ: Oturumu kaydet, kapý kapalý kalsýn
            PlayerPrefs.SetString(_puzzleKey, _appSessionID);
            PlayerPrefs.Save();
        }
        else if (savedSession == _appSessionID)
        {
            // OYUNCU SADECE MENÜYE DÖNDÜ: Kapý kapalý kalsýn
        }
        else
        {
            // OYUNCU OYUNU KAPATIP AÇTI (BULMACA ÇÖZÜLDÜ): Kapýyý aç
            // Race Condition Korumasý: Kapý sahneye tam yüklenene kadar bekle
            yield return new WaitUntil(() => GateController.Instance != null);

            GateController.Instance.OpenGate();
        }
    }

    /// <summary>
    /// Waits silently in the background until the player touches the finish point, then clears memory.
    /// (Oyuncu bitiþ noktasýna deðene kadar arka planda sessizce bekler, ardýndan hafýzayý temizler.)
    /// </summary>
    private IEnumerator WaitForLevelFinishRoutine()
    {
        // Update fonksiyonu kullanmak yerine, IsLevelFinishing true olana kadar iþlemciyi yormadan bekle
        yield return new WaitUntil(() => FinishPoint.IsLevelFinishing);

        if (!_isMemoryCleared)
        {
            ClearPuzzleMemory();
        }
    }

    /// <summary>
    /// Called by LevelManager when the player dies. Re-evaluates the puzzle state.
    /// (Oyuncu öldüðünde LevelManager tarafýndan çaðrýlýr. Bulmaca durumunu yeniden deðerlendirir.)
    /// </summary>
    public void ResetMechanic()
    {
        // Oyuncu ölürse ve bulmacayý zaten çözmüþse kapýyý hemen tekrar aç
        StartCoroutine(CheckStrictParadoxRoutine());
    }

    /// <summary>
    /// Permanently deletes the puzzle data from PlayerPrefs.
    /// (Bulmaca verisini PlayerPrefs'ten kalýcý olarak siler.)
    /// </summary>
    private void ClearPuzzleMemory()
    {
        PlayerPrefs.DeleteKey(_puzzleKey);
        PlayerPrefs.Save();
        _isMemoryCleared = true;
    }

    /// <summary>
    /// Cleans up subscriptions to prevent memory leaks when the object is destroyed.
    /// (Obje yok edilirken bellek sýzýntýlarýný önlemek için abonelikleri temizler.)
    /// </summary>
    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.UnregisterResettable(this);
        }

        // Yedek Güvenlik Korumasý: Olur da Coroutine kaçýrýrsa (neredeyse imkansýz) son bir kez silmeyi dener
        if (FinishPoint.IsLevelFinishing && !_isMemoryCleared)
        {
            ClearPuzzleMemory();
        }
    }
}
