using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    public int currentLevelIndex = 0;
    public List<LevelData> allLevels;
    [HideInInspector] public LevelData activeLevel;
    public static System.Action OnLevelStarted;

    private List<GameObject> activeMechanics = new List<GameObject>();

    void Awake()
    {
        Instance = this;
        currentLevelIndex = PlayerPrefs.GetInt("SelectedInternalIndex", 0);

        foreach (var level in allLevels)
        {
            level.isUnlocked = PlayerPrefs.GetInt("LevelUnlocked_" + level.levelID, level.levelID == 0 ? 1 : 0) == 1;
            level.isCompleted = PlayerPrefs.GetInt("LevelComplete_" + level.levelID, 0) == 1;
        }
        Screen.fullScreen = true;
        Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
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

        allLevels[currentLevelIndex].isCompleted = true;
        PlayerPrefs.SetInt("LevelComplete_" + allLevels[currentLevelIndex].levelID, 1);

        int nextGlobalID = allLevels[currentLevelIndex].levelID + 1;
        PlayerPrefs.SetInt("LevelUnlocked_" + nextGlobalID, 1);
        PlayerPrefs.Save();

  

        if (currentLevelIndex + 1 >= allLevels.Count)
        {
            GoToLevelSelect();
            return;
        }

        if (LevelTransition.Instance != null)
        {
            LevelTransition.Instance.DoTransition(() =>
            {
                currentLevelIndex++;
                ApplyLevel();
                PlayerController.Instance.ResetPosition();
                Camera.main.transform.position = new Vector3(0f, 0f, -10f);
            });
        }
        else
        {
            currentLevelIndex++;
            ApplyLevel();
            PlayerController.Instance.ResetPosition();
            Camera.main.transform.position = new Vector3(0f, 0f, -10f);
        }
    }

    public void ApplyLevel()
    {
        if (currentLevelIndex >= allLevels.Count) return;

        activeLevel = allLevels[currentLevelIndex];

        SoundManager.UpdateThemeByLevelID(activeLevel.levelID);

        foreach (GameObject obj in activeMechanics)
        {
            if (obj != null) Destroy(obj);
        }
        activeMechanics.Clear();

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

        Physics2D.gravity = new Vector2(0, -9.81f);

        if (HintManager.Instance != null) HintManager.Instance.UpdateLevelHint();

        PlayerController.Instance.ResetSpeed();
        Physics2D.gravity = new Vector2(0, -9.81f);

        if (PlayerController.Instance != null) PlayerController.Instance.UpdateGravityDirection();

        OnLevelStarted?.Invoke();
    }

    public void ResetAllMechanics()
    {
        // 1. OPTÝMÝZASYON: Bütün sahneyi YALNIZCA 1 KERE TARA! Döngünün dýþýnda!
        var gates = Object.FindObjectsByType<GateController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var g in gates) g.ResetGate();

        // 2. Sadece aktif mekaniklerin bileþenlerini kontrol et
        foreach (GameObject obj in activeMechanics)
        {
            if (obj == null) continue;

            GateButton button = obj.GetComponentInChildren<GateButton>();
            if (button != null) button.ResetButton();

            Key key = obj.GetComponentInChildren<Key>();
            if (key != null) key.ResetKey();

            EscapeKey ekey = obj.GetComponentInChildren<EscapeKey>();
            if (ekey != null) ekey.ResetKey();

            BoxButton boxbutton = obj.GetComponentInChildren<BoxButton>();
            if (boxbutton != null) boxbutton.ResetButton();
        }

        Physics2D.gravity = new Vector2(0, -9.81f);

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.UpdateGravityDirection();
        }

        OnLevelStarted?.Invoke();
    }

    void GoToLevelSelect()
    {
        if (LevelTransition.Instance != null)
        {
            LevelTransition.Instance.FadeOut(() => { SceneManager.LoadScene("Levels"); });
        }
        else SceneManager.LoadScene("Levels");
    }
}