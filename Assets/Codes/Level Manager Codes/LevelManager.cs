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
        // Sahneye girerken hangi level'dan baþlayacaðýmýzý seçme ekranýndan alýyoruz
        currentLevelIndex = PlayerPrefs.GetInt("SelectedInternalIndex", 0);

        // Verileri diskten tazele (Kilitler doðru gelsin)
        foreach (var level in allLevels)
        {
            level.isUnlocked = PlayerPrefs.GetInt("LevelUnlocked_" + level.levelID, level.levelID == 0 ? 1 : 0) == 1;
            level.isCompleted = PlayerPrefs.GetInt("LevelComplete_" + level.levelID, 0) == 1;
        }
    }
    void Start()
    {
        ApplyLevel();
    }
    public void NextLevel()
    {
        if (allLevels[currentLevelIndex].isCompleted)
        {
            Debug.Log("Bu level zaten bitmiþti, Menüye dönülüyor...");
            GoToLevelSelect();
            return; 
        }
        //Mevcut level'ý bitir ve kaydet
        allLevels[currentLevelIndex].isCompleted = true;
        PlayerPrefs.SetInt("LevelComplete_" + allLevels[currentLevelIndex].levelID, 1);

        //Sonraki level kilidini aç
        int nextGlobalID = allLevels[currentLevelIndex].levelID + 1;
        PlayerPrefs.SetInt("LevelUnlocked_" + nextGlobalID, 1);
        PlayerPrefs.Save();

        if (LevelTransition.Instance != null)
        {
            string mesaj = allLevels[currentLevelIndex].levelID+1 + ".Level Tamamlandý!";
            LevelTransition.Instance.DoTransition(mesaj , () =>
            {
                currentLevelIndex++;
                ApplyLevel();
                FindFirstObjectByType<PlayerController>().ResetPosition();
            });
        }
        else
        {
            currentLevelIndex++;
            ApplyLevel();
            FindFirstObjectByType<PlayerController>().ResetPosition();
        }
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
    void GoToLevelSelect()
    {
        if (LevelTransition.Instance != null)
        {
            LevelTransition.Instance.FadeOut(() =>
            {
                SceneManager.LoadScene("Levels");
            });
        }
        else
        {
            SceneManager.LoadScene("Levels");
        }
    }
}