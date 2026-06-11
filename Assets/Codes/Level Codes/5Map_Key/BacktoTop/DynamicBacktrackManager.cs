using UnityEngine;

/// <summary>
/// Geri dönüþ mekanizmasýný ve anahtar kontrolünü yöneten merkezi sýnýf.
/// </summary>
public class DynamicBacktrackManager : MonoBehaviour, IResettable
{
    [Header("Geri Dönüþ Ayarlarý")]
    public string requiredKeyID = "SecretKey_A";

    [Tooltip("Geri dönülecek bölümün LevelData dosyasýný buraya sürükle aga!")]
    public LevelData targetLevelData;

    public int currentLevelIndex = 9;

    // Oturum boyunca kapýnýn açýk kalýp kalmayacaðýný belirleyen geçici deðiþken
    private bool gateOpenedInThisSession = false;

    private void Start()
    {
        if (LevelManager.Instance != null) LevelManager.Instance.RegisterResettable(this);

        if (PlayerPrefs.GetInt(requiredKeyID, 0) == 1)
        {
            gateOpenedInThisSession = true;

            // Baþka giriþlerde kapalý olmasý için anahtarý kalýcý hafýzadan hemen tüket
            PlayerPrefs.SetInt(requiredKeyID, 0);
            PlayerPrefs.Save();

            KeyLossMonitorPayload monitor = UnityEngine.Object.FindFirstObjectByType<KeyLossMonitorPayload>();
            if (monitor != null) Destroy(monitor.gameObject);
        }

        ProcessLogic();
    }

    public void ProcessLogic()
    {
        if (gateOpenedInThisSession)
        {
            if (GateController.Instance != null)
            {
                int needed = GateController.Instance.totalKeysNeeded;
                for (int i = 0; i < needed; i++) GateController.Instance.RegisterKeyCollected();
            }
        }
        else
        {
            if (targetLevelData != null)
            {
                PlayerPrefs.SetInt(Constants.PREF_TROLL_TARGET_IDX, targetLevelData.levelID);
                PlayerPrefs.SetInt(Constants.PREF_TROLL_CURRENT_IDX, currentLevelIndex);
                PlayerPrefs.SetInt(Constants.PREF_TROLL_HACK_ACTIVE, 1);
                PlayerPrefs.Save();

                if (UnityEngine.Object.FindFirstObjectByType<MenuHackerPayload>() == null)
                {
                    new GameObject("MenuHackerPayload_Agent").AddComponent<MenuHackerPayload>();
                }
            }
            else
            {
            }
        }
    }

    public void ResetMechanic()
    {
        // Ölüm anýnda þalteri indirmek yerine kapý durumunu koruyarak mantýðý yeniler
        Invoke(nameof(ProcessLogic), 0.2f);
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance != null) LevelManager.Instance.UnregisterResettable(this);
    }
}
