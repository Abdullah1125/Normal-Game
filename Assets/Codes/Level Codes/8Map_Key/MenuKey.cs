
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MenuKeyMonitorPayload : MonoBehaviour, IResettable
{
    public int level8Index = 7;

    private static MenuKeyMonitorPayload _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        StartCoroutine(WaitAndGiveKeys());
    }

    private void Update()
    {
        // SENİN İSTEDİĞİN ÖZELLİK: Eğer FinishPoint'e değersek (bölüm biterse) anında kendini imha et!
        if (FinishPoint.IsLevelFinishing)
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == Constants.SCENE_MAIN_MENU)
        {
            FindAndHackMenuKey();
            return;
        }

        if (scene.name == Constants.SCENE_LEVELS)
        {
            // Level seçme ekranında ajan şimdilik uykuya geçer, kendini silmez
            return;
        }

        if (PlayerPrefs.GetInt(Constants.PREF_LAST_LEVEL_ID, -1) == level8Index)
        {
            StopAllCoroutines();
            StartCoroutine(WaitAndGiveKeys());
        }
        else
        {
            Destroy(gameObject); // 8. Bölüm harici bir yere girilirse kendini imha et
        }
    }

    private IEnumerator WaitAndGiveKeys()
    {
        yield return new WaitForSecondsRealtime(0.2f);

        if (LevelManager.Instance != null)
            LevelManager.Instance.RegisterResettable(this);

        CheckIfBroughtKey();
    }

    private void FindAndHackMenuKey()
    {
        GameObject keyObj = GameObject.FindWithTag("menukey");
        if (keyObj == null) return;

        if (PlayerPrefs.GetInt("HasMenuSecretKey", 0) == 1)
        {
            keyObj.SetActive(false);
        }
        else
        {
            keyObj.SetActive(true);

            Button btn = keyObj.GetComponent<Button>();
            if (btn == null) btn = keyObj.AddComponent<Button>();

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                PlayerPrefs.SetInt("HasMenuSecretKey", 1);
                PlayerPrefs.Save();
                keyObj.SetActive(false);

                if (SoundManager.Instance != null)
                    SoundManager.PlayThemeSFX(SFXType.Key);
            });
        }
    }

    private void CheckIfBroughtKey()
    {
        if (PlayerPrefs.GetInt("HasMenuSecretKey", 0) == 1)
        {
            if (GateController.Instance != null)
            {
                GateController.Instance.RegisterKeyCollected();
                GateController.Instance.RegisterKeyCollected();
            }
        }
    }

    public void ResetMechanic()
    {
        StopAllCoroutines();
        StartCoroutine(WaitAndGiveKeys());
    }

    private void OnDestroy()
    {
        if (_instance != this) return;

        if (LevelManager.Instance != null)
            LevelManager.Instance.UnregisterResettable(this);

        SceneManager.sceneLoaded -= OnSceneLoaded;

        // Ajan yok olduğunda anahtarı sıfırlar
        PlayerPrefs.SetInt("HasMenuSecretKey", 0);
        PlayerPrefs.Save();
    }
}
