using UnityEngine;

public class AirplaneModeMechanic : MonoBehaviour
{
    public float checkInterval = 1.0f;
    private float timer;
    private bool isGateOpen = false;
    private bool currentAirplaneState = false;

#if UNITY_EDITOR
    [Header("Editor Test(Editör Testi)")]
    public bool testAirplaneMode = false;
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaObject resolver;
    private AndroidJavaClass settingsGlobal;
#endif

    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                resolver = currentActivity.Call<AndroidJavaObject>("getContentResolver");
                settingsGlobal = new AndroidJavaClass("android.provider.Settings$Global");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Java Objeleri Başlatılamadı: " + e.Message);
        }
#endif
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= checkInterval)
        {
            timer = 0f;
            currentAirplaneState = IsAirplaneModeEnabled();

            if (currentAirplaneState && !isGateOpen)
            {
                OpenTheGate();
            }
            else if (!currentAirplaneState && isGateOpen)
            {
                CloseTheGate();
                if (AirplaneUI.Instance != null) AirplaneUI.Instance.HideImmediately();
            }

            FinishPoint.isFinishBlocked = currentAirplaneState;
        }

        if (AirplaneUI.Instance != null)
        {
            if (currentAirplaneState && FinishPoint.isPlayerInZone)
            {
                AirplaneUI.Instance.ShowPanel();
            }
            else
            {
                AirplaneUI.Instance.HidePanel();
            }
        }
    }

    private void OnEnable()
    {
        LevelManager.OnLevelStarted += ResetMechanic;
    }

    private void OnDisable()
    {
        LevelManager.OnLevelStarted -= ResetMechanic;
        FinishPoint.isFinishBlocked = false;
    }

    private void OpenTheGate()
    {
        isGateOpen = true;
        if (GateController.Instance != null) GateController.Instance.OpenGate();
    }

    private void CloseTheGate()
    {
        isGateOpen = false;
        if (GateController.Instance != null) GateController.Instance.CloseGate();
    }

    private bool IsAirplaneModeEnabled()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (settingsGlobal != null && resolver != null)
        {
            try
            {
                int state = settingsGlobal.CallStatic<int>("getInt", resolver, "airplane_mode_on", 0);
                return state == 1;
            }
            catch (System.Exception e)
            {
                return false;
            }
        }
        return false;
#else
        return testAirplaneMode;
#endif
    }

    private void ResetMechanic()
    {
        if (this.gameObject.activeInHierarchy)
        {
            isGateOpen = false;
            timer = checkInterval;
            currentAirplaneState = false;
            FinishPoint.isFinishBlocked = false;
            if (AirplaneUI.Instance != null) AirplaneUI.Instance.HideImmediately();
        }
    }
}