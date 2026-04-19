using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("Timer(Zamanlayýcý)")]
    public bool isTimerRunning = false; // Oyun baþladýðýnda süre aksýn mý?
    public string playerName = "Misafir";

    [Header("Persistent Data(Kalýcý Veriler)")]
    public int totalDeaths = 0;
    public float totalTime = 0f;

    private void Awake()
    {
        // Sahneler arasý silinmeyen tekil (Singleton) yapý
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Oyun açýldýðýnda eski kayýtlarý diskten çek!
            LoadOfflineData();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        Application.targetFrameRate = 60;
    }

    private void Update()
    {
        // Sayaç açýksa toplam süreyi sürekli artýr
        if (isTimerRunning)
        {
            totalTime += Time.deltaTime;
        }
    }

    public void StartTimer() => isTimerRunning = true;
    public void StopTimer() => isTimerRunning = false;

    // PlayerController içindeki Die() fonksiyonunda çaðýrýlýr
    public void AddDeath()
    {
        totalDeaths++;
        SaveOfflineData(); // Her öldüðünde diske kaydet ki silinmesin!
    }

    // --- DÝSK KAYIT SÝSTEMÝ ---
    public void SaveOfflineData()
    {
        PlayerPrefs.SetInt("TotalDeaths", totalDeaths);
        PlayerPrefs.SetFloat("TotalTime", totalTime);
        PlayerPrefs.Save();
    }

    public void LoadOfflineData()
    {
        totalDeaths = PlayerPrefs.GetInt("TotalDeaths", 0);
        totalTime = PlayerPrefs.GetFloat("TotalTime", 0f);
    }

    // Süreyi 01:25 gibi þýk bir formata (Dakika:Saniye) çeviren araç
    public string GetFormattedTime(float timeToFormat)
    {
        int minutes = Mathf.FloorToInt(timeToFormat / 60F);
        int seconds = Mathf.FloorToInt(timeToFormat - minutes * 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}