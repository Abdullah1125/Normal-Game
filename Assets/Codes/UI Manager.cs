using UnityEngine;

/// <summary>
/// Centralized manager for handling UI interaction states and input blocking.
/// (UI etkileþim durumlarýný ve giriþ engellemeyi yöneten merkezi yönetici.)
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Interaction Groups (UI Etkileþim Gruplarý)")]
    public CanvasGroup mainMenuButtons;   // Ana menü buton grubu
    public CanvasGroup hudButtons;        // Oyun içi HUD (Pause/Ýpucu) grubu
    public CanvasGroup pauseMenuButtons;  // Duraklatma menüsü grubu
    public CanvasGroup hintPanelButtons;  // Ýpucu paneli grubu

    /// <summary>
    /// Singleton initialization and reference cleanup.
    /// </summary>
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Sets the block state of a specific CanvasGroup.
    /// (Belirli bir CanvasGroup'un kilit durumunu ayarlar.)
    /// </summary>
    private void SetGroupBlock(CanvasGroup group, bool block)
    {
        if (group == null) return;

        // block = true (kilitli) ise, týklamalar (raycast) kapalý (false) olmalý.
        group.blocksRaycasts = !block;
    }

    /// <summary>
    /// Checks if a specific CanvasGroup is currently blocked.
    /// (Belirli bir CanvasGroup'un þu an kilitli olup olmadýðýný kontrol eder.)
    /// </summary>
    private bool IsGroupBlocked(CanvasGroup group)
    {
        if (group == null) return false;
        return !group.blocksRaycasts;
    }

    // --- INTERFACE METHODS (ARAYÜZ METOTLARI) ---

    // Main Menu Controls
    public void SetMainMenuBlock(bool state) => SetGroupBlock(mainMenuButtons, state);
    public bool IsMainMenuBlocked() => IsGroupBlocked(mainMenuButtons);

    // Gameplay HUD Controls
    public void SetHUDBlock(bool state) => SetGroupBlock(hudButtons, state);
    public bool IsHUDBlocked() => IsGroupBlocked(hudButtons);

    // Pause Menu Controls
    public void SetPauseBlock(bool state) => SetGroupBlock(pauseMenuButtons, state);
    public bool IsPauseBlocked() => IsGroupBlocked(pauseMenuButtons);

    // Hint Panel Controls
    public void SetHintBlock(bool state) => SetGroupBlock(hintPanelButtons, state);
    public bool IsHintBlocked() => IsGroupBlocked(hintPanelButtons);
}