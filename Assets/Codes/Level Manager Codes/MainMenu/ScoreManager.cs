using UnityEngine;
using TMPro;

public class ScoreManager : SingletonPersistent<ScoreManager>
{

    [Header("Timer(ZamanlayÄ±cÄ±)")]
    public bool isTimerRunning = false; // Oyun baÅŸladÄ±ÄŸÄ±nda sÃ¼re aksÄ±n mÄ±?
    public string playerName = "Misafir";

    [Header("Persistent Data(KalÄ±cÄ± Veriler)")]
    public int totalDeaths = 0;
    public float totalTime = 0f;

    protected override void Awake()
    {
        base.Awake();
        LoadOfflineData();
    }
    private void Start()
    {
        Application.targetFrameRate = 60;
    }

    private void Update()
    {
        // SayaÃ§ aÃ§Ä±ksa toplam sÃ¼reyi sÃ¼rekli artÄ±r
        if (isTimerRunning)
        {
            totalTime += Time.deltaTime;
        }
    }

    public void StartTimer() => isTimerRunning = true;
    public void StopTimer() => isTimerRunning = false;

    // PlayerController iÃ§indeki Die() fonksiyonunda Ã§aÄŸÄ±rÄ±lÄ±r
    public void AddDeath()
    {
        totalDeaths++;
        SaveOfflineData(); // Her Ã¶ldÃ¼ÄŸÃ¼nde diske kaydet ki silinmesin!
    }

    // --- DÄ°SK KAYIT SÄ°STEMÄ° ---
    public void SaveOfflineData()
    {
        PlayerPrefs.SetInt(Constants.PREF_TOTAL_DEATHS, totalDeaths);
        PlayerPrefs.SetFloat(Constants.PREF_TOTAL_TIME, totalTime);
        PlayerPrefs.Save();
    }

    public void LoadOfflineData()
    {
        totalDeaths = PlayerPrefs.GetInt(Constants.PREF_TOTAL_DEATHS, 0);
        totalTime = PlayerPrefs.GetFloat(Constants.PREF_TOTAL_TIME, 0f);
    }

    // SÃ¼reyi 01:25 gibi ÅŸÄ±k bir formata (Dakika:Saniye) Ã§eviren araÃ§
    public string GetFormattedTime(float timeToFormat)
    {
        int minutes = Mathf.FloorToInt(timeToFormat / 60F);
        int seconds = Mathf.FloorToInt(timeToFormat - minutes * 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}

