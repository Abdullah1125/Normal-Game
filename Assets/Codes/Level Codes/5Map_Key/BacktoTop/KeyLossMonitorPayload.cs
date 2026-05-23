using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Anahtarý izleyen ve kural ihlallerinde imha eden ajan.
/// </summary>
public class KeyLossMonitorPayload : MonoBehaviour
{
    private string _keyID;
    private int _destIdx;
    private bool _isGoalReached = false;

    /// <summary>
    /// Takip edilecek hedef verileri ayarlar.
    /// </summary>
    public void Setup(string id)
    {
        _keyID = id;
        _destIdx = PlayerPrefs.GetInt(Constants.PREF_TROLL_CURRENT_IDX, -1);
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// Bölüm geçiþlerini takip ederek cezayý keser veya anahtarý sýfýrlar.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == Constants.SCENE_LEVELS || scene.name == Constants.SCENE_MAIN_MENU) return;

        int currentID = PlayerPrefs.GetInt(Constants.PREF_LAST_LEVEL_ID, -1);

        // Bölüm bitti: Oyuncu bir sonraki seviyeye geçti
        if (currentID > _destIdx)
        {
            _isGoalReached = true;
            ApplyPenalty();
            return;
        }

        // Yanlýþ rota kontrolü
        int targetIdx = PlayerPrefs.GetInt(Constants.PREF_TROLL_TARGET_IDX, -1);
        if (currentID != _destIdx && currentID != targetIdx) ApplyPenalty();
    }

    /// <summary>
    /// Oyundan çýkýldýðýnda anahtarý siler.
    /// </summary>
    private void OnApplicationQuit() { if (!_isGoalReached) ApplyPenalty(); }

    private void ApplyPenalty()
    {
        PlayerPrefs.SetInt(_keyID, 0);
        PlayerPrefs.Save();
        Destroy(gameObject);
    }

    private void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;
}