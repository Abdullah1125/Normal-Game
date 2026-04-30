using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class FadeInHintButton : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    public float fadeDelay = 20.0f;
    public float fadeSpeed = 0.8f;

    private float timeSpentInLevel = 0f;
    private bool isFadedIn = false;
    private int currentLevelID = -1;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        LevelManager.OnLevelStarted += CheckLevelChange;
        CheckLevelChange();
    }

    private void OnDisable()
    {
        LevelManager.OnLevelStarted -= CheckLevelChange;
    }

    // Level de�i�imi veya yeniden ba�lama durumunda sayac� ve g�r�n�rl��� y�netir.
    private void CheckLevelChange()
    {
        int newLevelID = -1;
        if (LevelManager.Instance != null && LevelManager.Instance.activeLevel != null)
        {
            newLevelID = LevelManager.Instance.activeLevel.levelID;
        }

        if (newLevelID != currentLevelID)
        {
            currentLevelID = newLevelID;
            isFadedIn = false;
            timeSpentInLevel = 0f;
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        else if (isFadedIn)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    void Update()
    {
        if (isFadedIn) return;

        timeSpentInLevel += Time.deltaTime;

        if (timeSpentInLevel >= fadeDelay)
        {
            if (canvasGroup.alpha < 1f)
            {
                canvasGroup.alpha += Time.deltaTime * fadeSpeed;
            }
            else
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                isFadedIn = true;
            }
        }
    }
}