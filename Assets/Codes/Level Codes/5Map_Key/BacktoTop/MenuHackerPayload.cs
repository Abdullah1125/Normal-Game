using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

/// <summary>
/// Menü kilitlerini bypass eden, oyuncu kaçmaya çalýþýrsa kendini imha eden ajan.
/// </summary>
public class MenuHackerPayload : MonoBehaviour
{
    private void Awake() => DontDestroyOnLoad(gameObject);

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        LevelMenuButton.OnCheckTrollBypass += BypassLockForTarget;

        // LevelUIManager iþini bitirince ajana haber veriyor
        LevelUIManager.OnMenuReady += HackTheMenuButtons;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        LevelMenuButton.OnCheckTrollBypass -= BypassLockForTarget;

        // Aboneliði iptal et (Hafýza sýzýntýsýný önlemek için)
        LevelUIManager.OnMenuReady -= HackTheMenuButtons;
    }

    private bool BypassLockForTarget(int clickedIndex)
    {
        bool isHackActive = PlayerPrefs.GetInt(Constants.PREF_TROLL_HACK_ACTIVE, 0) == 1;
        int targetIdx = PlayerPrefs.GetInt(Constants.PREF_TROLL_TARGET_IDX, -1);
        return (isHackActive && clickedIndex == targetIdx);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Anahtar alýndýysa veya sistem iptal edildiyse ajaný sil
        if (PlayerPrefs.GetInt(Constants.PREF_TROLL_HACK_ACTIVE, 0) == 0)
        {
            Destroy(gameObject);
            return;
        }

        // 1. ÝSTÝSNA: Ana Menü
        if (scene.name == Constants.SCENE_MAIN_MENU) return;

        // 2. ÝSTÝSNA: Bölüm Seçme Ekraný
        if (scene.name == Constants.SCENE_LEVELS)
        {
            // Artýk burada 0.2 saniye beklemiyoruz. 
            // LevelUIManager, butonlarý dizdiðinde HackTheMenuButtons() otomatik çalýþacak.
            return;
        }

        // OYUN ÝÇÝ (LEVEL) KONTROLÜ
        int loadedLevelID = PlayerPrefs.GetInt(Constants.PREF_LAST_LEVEL_ID, -1);
        int target = PlayerPrefs.GetInt(Constants.PREF_TROLL_TARGET_IDX, -1);
        int current = PlayerPrefs.GetInt(Constants.PREF_TROLL_CURRENT_IDX, -1);

        // Hedef veya Mevcut bölüm dýþýnda BAÞKA BÝR BÖLÜME kaçýlýrsa cezayý kes!
        if (loadedLevelID != target && loadedLevelID != current && loadedLevelID != -1)
        {
            CancelHackAndDestroy();
        }
    }

    /// <summary>
    /// LevelUIManager butonlarý dizmeyi bitirdiðinde tetiklenir ve hedef butonu "hackler".
    /// </summary>
    private void HackTheMenuButtons()
    {
        LevelMenuButton[] allButtons = UnityEngine.Object.FindObjectsByType<LevelMenuButton>(FindObjectsSortMode.None);
        int targetIdx = PlayerPrefs.GetInt(Constants.PREF_TROLL_TARGET_IDX, -1);

        foreach (var mb in allButtons)
        {
            if (mb.levelText != null && mb.levelText.text == (targetIdx + 1).ToString())
            {
                // BUTONU "TAMAMLANDI" GÝBÝ GÖSTER
                if (mb.buttonImage != null)
                {
                    mb.buttonImage.color = Color.green;
                }

                // Butonu týklanabilir yap
                Button btn = mb.GetComponent<Button>();
                if (btn != null) btn.interactable = true;

                // Ekstra: Eðer isim/yazý gizleniyorsa onu da görünür yap
                if (mb.levelNameText != null) mb.levelNameText.gameObject.SetActive(true);
            }
        }
    }

    private void OnApplicationQuit()
    {
        // Oyundan çýkýþ yapýlýrsa affetme, sistemi kilitle!
        CancelHackAndDestroy();
    }

    private void CancelHackAndDestroy()
    {
        PlayerPrefs.SetInt(Constants.PREF_TROLL_HACK_ACTIVE, 0);
        PlayerPrefs.Save();
        Destroy(gameObject);
    }
}