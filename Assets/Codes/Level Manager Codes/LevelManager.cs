using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : Singleton<LevelManager>
{

    public int currentLevelIndex = 0;
    public List<LevelData> allLevels;
    [HideInInspector] public LevelData activeLevel;
    public static System.Action OnLevelStarted;

    private List<GameObject> activeMechanics = new List<GameObject>();
    private List<IResettable> masterResetList = new List<IResettable>();
    protected override void Awake()
    {
        base.Awake();
        currentLevelIndex = PlayerPrefs.GetInt(Constants.PREF_SELECTED_INTERNAL_INDEX, 0);

        foreach (var level in allLevels)
        {
            level.isUnlocked = PlayerPrefs.GetInt(Constants.PREF_LEVEL_UNLOCKED_PREFIX + level.levelID, level.levelID == 0 ? 1 : 0) == 1;
            level.isCompleted = PlayerPrefs.GetInt(Constants.PREF_LEVEL_COMPLETE_PREFIX + level.levelID, 0) == 1;
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
            Debug.Log("Bu level zaten bitmiÅŸti, MenÃ¼ye dÃ¶nÃ¼lÃ¼yor...");
            GoToLevelSelect();
            return;
        }

        allLevels[currentLevelIndex].isCompleted = true;
        PlayerPrefs.SetInt(Constants.PREF_LEVEL_COMPLETE_PREFIX + allLevels[currentLevelIndex].levelID, 1);

        int nextGlobalID = allLevels[currentLevelIndex].levelID + 1;
        PlayerPrefs.SetInt(Constants.PREF_LEVEL_UNLOCKED_PREFIX + nextGlobalID, 1);
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
    /// (BÃ¶lÃ¼mdeki tÃ¼m etkileÅŸimli Ã¶ÄŸeleri -kutular, anahtarlar, butonlar, kapÄ±lar- sÄ±fÄ±rlar.)
    /// </summary>
    public void ResetAllMechanics()
    {
        masterResetList.RemoveAll(mechanic => (mechanic as Object) == null);

        // Listeyi dÃ¶nerken hem null kontrolÃ¼ yapÄ±yoruz hem de Unity objesi mi diye bakÄ±yoruz
        foreach (IResettable mechanic in masterResetList)
        {
            // SÄ°HÄ°R: (mechanic as Object) kÄ±smÄ±, objenin Unity tarafÄ±nda silinip silinmediÄŸini kontrol eder
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
            LevelTransition.Instance.FadeOut(() => { SceneManager.LoadScene(Constants.SCENE_LEVELS); });
        }
        else SceneManager.LoadScene(Constants.SCENE_LEVELS);
    }

   
}

