using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    public int currentLevelIndex = 0;
    public List<LevelData> allLevels;
    [HideInInspector] public LevelData activeLevel;

    // Oluþturulan mekanikleri takip eden liste
    private List<GameObject> activeMechanics = new List<GameObject>();

    void Awake()
    {
        Instance = this;
        if (PlayerPrefs.HasKey("SavedLevel"))
        {
           //currentLevelIndex = PlayerPrefs.GetInt("SavedLevel");
        }
        else
        {
            currentLevelIndex = 0;
        }
    }
    void Start()
    {
        ApplyLevel();
    }
    public void NextLevel()
    {
        currentLevelIndex++;

        PlayerPrefs.SetInt("SavedLevel", currentLevelIndex);
        PlayerPrefs.Save();

        if (currentLevelIndex >= allLevels.Count)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            return;
        }

        ApplyLevel();
        FindFirstObjectByType<PlayerController>().ResetPosition();
    }

    public void ApplyLevel()
    {
        if (currentLevelIndex >= allLevels.Count) return;

        activeLevel = allLevels[currentLevelIndex];

        // 2. Eski mekanikleri sil (takip listesinden)
        foreach (GameObject obj in activeMechanics)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        activeMechanics.Clear();

        // 3. Yeni mekanikleri oluþtur ve listeye ekle
        if (activeLevel.specialMechanics != null)
        {
            foreach (GameObject prefab in activeLevel.specialMechanics)
            {
                if (prefab != null)
                {
                    GameObject spawned = Instantiate(prefab);
                    activeMechanics.Add(spawned);
                }
            }
        }

        // 4. Yerçekimini sýfýrla (her zaman aþaðý)
        Physics2D.gravity = new Vector2(0, -9.81f);

        // 5. Ýpucu güncelle
        if (HintManager.Instance != null)
        {
            HintManager.Instance.UpdateLevelHint();
        }

        Debug.Log("Aktif Seviye: " + activeLevel.levelName);
    }

    // Ölünce mekanikleri sýfýrla (silmeden)
    public void ResetAllMechanics()
    {
        foreach (GameObject obj in activeMechanics)
        {
            if (obj == null) continue;

            // Her objedeki resetlenebilir bileþenleri bul ve sýfýrla
            // Kapýlarý, anahtarlarý ve butonlarý bulup sýfýrla
            var gates = Object.FindObjectsByType<GateController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var g in gates) g.ResetGate();

            GateButton button = obj.GetComponentInChildren<GateButton>();
            if (button != null) button.ResetButton();

            Key key = obj.GetComponentInChildren<Key>();
            if (key != null) key.ResetKey();

            BoxButton boxbutton = obj.GetComponentInChildren<BoxButton>();
            if (boxbutton != null) boxbutton.ResetButton();
        }
    }
}