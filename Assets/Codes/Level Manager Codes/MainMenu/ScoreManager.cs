using UnityEngine;
using TMPro;

public class ScoreManager : SingletonPersistent<ScoreManager>
{

    [Header("Timer(Zamanlayıcı)")]
    public bool isTimerRunning = false; // Oyun başladığında süre aksın mı?
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
        // Sayaç açıksa toplam süreyi sürekli artır
        if (isTimerRunning)
        {
            totalTime += Time.deltaTime;
        }
    }

    public void StartTimer() => isTimerRunning = true;
    public void StopTimer() => isTimerRunning = false;

    // PlayerController içindeki Die() fonksiyonunda çağırılır
    public void AddDeath()
    {
        totalDeaths++;
        SaveOfflineData(); // Her öldüğünde diske kaydet ki silinmesin!
    }

    // --- DİSK KAYIT SİSTEMİ ---
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

    // Süreyi 01:25 gibi şık bir formata (Dakika:Saniye) çeviren araç
    public string GetFormattedTime(float timeToFormat)
    {
        int minutes = Mathf.FloorToInt(timeToFormat / 60F);
        int seconds = Mathf.FloorToInt(timeToFormat - minutes * 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}

