using UnityEngine;

/// <summary>
/// Centralized manager for handling UI interaction states and input blocking.
/// (UI etkileşim durumlarını ve giriş engellemeyi yöneten merkezi yönetici.)
/// </summary>
public class UIManager : Singleton<UIManager>
{    [Header("UI Interaction Groups (UI Etkileşim Grupları)")]
    public CanvasGroup mainMenuButtons;   // Ana menü buton grubu
    public CanvasGroup hudButtons;        // Oyun içi HUD (Pause/İpucu) grubu
    public CanvasGroup pauseMenuButtons;  // Duraklatma menüsü grubu
    public CanvasGroup hintPanelButtons;  // İpucu paneli grubu
    public CanvasGroup levelMenuButtons;  // Level seçim ekranı buton grubu



    /// <summary>
    /// Sets the block state of a specific CanvasGroup.
    /// (Belirli bir CanvasGroup'un kilit durumunu ayarlar.)
    /// </summary>
    private void SetGroupBlock(CanvasGroup group, bool block)
    {
        if (group == null) return;

        // block = true (kilitli) ise, tıklamalar (raycast) kapalı (false) olmalı.
        group.blocksRaycasts = !block;
    }

    /// <summary>
    /// Checks if a specific CanvasGroup is currently blocked.
    /// (Belirli bir CanvasGroup'un şu an kilitli olup olmadığını kontrol eder.)
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

    // Level Menu Controls
    public void SetLevelMenuBlock(bool state) => SetGroupBlock(levelMenuButtons, state);
    public bool IsLevelMenuBlocked() => IsGroupBlocked(levelMenuButtons);
}
