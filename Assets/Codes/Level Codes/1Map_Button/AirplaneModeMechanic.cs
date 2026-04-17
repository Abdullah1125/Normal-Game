using UnityEngine;

public class AirplaneModeMechanic : MonoBehaviour
{
    public float checkInterval = 1.0f;
    private float timer;
    private bool isGateOpen = false;

#if UNITY_EDITOR
    [Header("Editor Test")]
    public bool testAirplaneMode = false;
#endif

    void Update()
    {
       
        timer += Time.deltaTime;
        if (timer >= checkInterval)
        {
            timer = 0f;
            bool isAirplaneOn = IsAirplaneModeEnabled();

            
            if (isAirplaneOn && !isGateOpen)
            {
                OpenTheGate();
            }
           
            else if (!isAirplaneOn && isGateOpen)
            {
                CloseTheGate();
            }
        }
    }

    private void OpenTheGate()
    {
        isGateOpen = true;
        Debug.Log(" Uçak modu algılandı! Kapı açılıyor.");

        if (GateController.Instance != null)
        {
            GateController.Instance.OpenGate();
        }
    }

    private void CloseTheGate()
    {
        isGateOpen = false;
        Debug.Log(" Bağlantı geri geldi! Kapı kapanıyor.");

        if (GateController.Instance != null)
        {
           
            GateController.Instance.CloseGate();
        }
    }

    private bool IsAirplaneModeEnabled()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            // Android Java sınıflarına güvenli erişim
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject resolver = currentActivity.Call<AndroidJavaObject>("getContentResolver"))
            using (AndroidJavaClass settingsGlobal = new AndroidJavaClass("android.provider.Settings$Global"))
            {
                // API 17+ (Android 4.2+) için standart yöntem
                int state = settingsGlobal.CallStatic<int>("getInt", resolver, "airplane_mode_on", 0);
                return state == 1;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Uçak modu okuma hatası: " + e.Message);
            return false;
        }
#else
        return testAirplaneMode;
#endif
    }
}