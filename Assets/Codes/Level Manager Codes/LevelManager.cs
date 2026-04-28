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
    private List<IResettable> masterResetList = new List<IResettable>();
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
    public void RegisterResettable(IResettable mechanic)
    {
        if (!masterResetList.Contains(mechanic))
        {
            masterResetList.Add(mechanic);
        }
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

        masterResetList.RemoveAll(mechanic => mechanic as UnityEngine.Object == null);

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

    /// <summary>
    /// Resets all interactive elements (boxes, keys, buttons, gates) in the level.
    /// (Bölümdeki tüm etkileþimli öðeleri -kutular, anahtarlar, butonlar, kapýlar- sýfýrlar.)
    /// </summary>
    public void ResetAllMechanics()
    {
        masterResetList.RemoveAll(mechanic => (mechanic as Object) == null);

        // Listeyi dönerken hem null kontrolü yapýyoruz hem de Unity objesi mi diye bakýyoruz
        foreach (IResettable mechanic in masterResetList)
        {
            // SÝHÝR: (mechanic as Object) kýsmý, objenin Unity tarafýnda silinip silinmediðini kontrol eder
            if (mechanic != null && (mechanic as Object) != null)
            {
                mechanic.ResetMechanic();
            }
        }

        Physics2D.gravity = new Vector2(0, -9.81f);
        if (PlayerController.Instance != null) PlayerController.Instance.UpdateGravityDirection();
        OnLevelStarted?.Invoke();
    }
    public void UnregisterResettable(IResettable mechanic)
    {
        if (masterResetList.Contains(mechanic))
        {
            masterResetList.Remove(mechanic);
        }
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