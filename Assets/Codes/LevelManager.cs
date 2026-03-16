using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    // Singleton yapýsý: Diðer scriptlerden LevelManager.Instance ile eriþilir
    public static LevelManager Instance;

    public int currentLevelIndex = 0;   // Mevcut seviyenin liste sýrasý
    public List<LevelData> allLevels;   // Tüm seviye verilerini tutan liste

    [HideInInspector] public LevelData activeLevel; // O an oynanan seviyenin verisi

    void Awake()
    {
        Instance = this;
        if (PlayerPrefs.HasKey("SavedLevel"))
        {

            currentLevelIndex = PlayerPrefs.GetInt("SavedLevel");
        }
        else
        {
            currentLevelIndex = 0;
        }
        
    }

    void Start()
    {
        ApplyLevel(); // Ýlk seviye ayarlarýný uygula
    }

    public void NextLevel()
    {
        currentLevelIndex++;

        PlayerPrefs.SetInt("SavedLevel", currentLevelIndex);
        PlayerPrefs.Save();

        // Eðer 6. seviyeye ulaþýldýysa bir sonraki sahneye (Scene) geç
        if (currentLevelIndex >= allLevels.Count)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            return;
        }

        ApplyLevel(); // Yeni seviyenin verilerini yükle

        // Oyuncuyu bul ve baþlangýç noktasýna geri gönder
        FindFirstObjectByType<PlayerController>().ResetPosition();
    }

    void ApplyLevel()
    {
        // Liste sýnýrlarý içerisinde olduðumuzdan emin olalým
        if (currentLevelIndex < allLevels.Count)
        {
            activeLevel = allLevels[currentLevelIndex];

            // Yerçekimi ayarý: LevelData içindeki bool deðerine göre yön deðiþtirir
            Physics2D.gravity = new Vector2(0, activeLevel.isGravityInverted ? 9.81f : -9.81f);

            // Gizli Duvar Kontrolü: "Tilemap_Secret" isimli objeyi sahnede ara
            GameObject secretWall = GameObject.Find("Tilemap_Secret");
            if (secretWall != null)
            {
                // Eðer seviyede gizli geçit varsa duvarýn collider'ýný kapat (geçilebilir yap)
                secretWall.GetComponent<Collider2D>().enabled = !activeLevel.hasSecretPassage;
            }

            Debug.Log("Aktif Seviye: " + activeLevel.levelName);
        }
    }
}