using UnityEngine;
using System;

[RequireComponent(typeof(BoxCollider2D))]
public class TutorialTrigger : MonoBehaviour
{
    [Header("Ghost Prefab (Doğurulacak Hayalet)")]
    public GameObject ghostPrefab;
    private GameObject spawnedGhost;

    private bool isPlayerInside = false; // Adam alanın içinde mi?

    // Pause ve İpucu sistemlerini dinleyecek dev anons sistemi!
    public static Action<bool> OnPauseToggled;
    public static Action<bool> OnHintToggled; // 🚀 YENİ: İpucu/Reklam dinleyicisi

    private bool isGamePaused = false;
    private bool isHintPanelOpen = false;

    /// <summary>
    /// Subscribes to UI events.
    /// (Arayüz olaylarına abone olur.)
    /// </summary>
    private void OnEnable()
    {
        OnPauseToggled += HandlePause;
        OnHintToggled += HandleHint;
    }

    /// <summary>
    /// Unsubscribes from UI events.
    /// (Arayüz olaylarından aboneliği kaldırır.)
    /// </summary>
    private void OnDisable()
    {
        OnPauseToggled -= HandlePause;
        OnHintToggled -= HandleHint;
    }

    /// <summary>
    /// Initializes trigger logic and checks level completion status.
    /// (Tetikleyici mantığını başlatır ve bölüm tamamlanma durumunu kontrol eder.)
    /// </summary>
    private void Start()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;

        if (LevelManager.Instance != null && LevelManager.Instance.activeLevel != null)
        {
            if (LevelManager.Instance.activeLevel.isCompleted)
            {
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Updates visibility when the pause state changes.
    /// (Duraklatma durumu değiştiğinde görünürlüğü günceller.)
    /// </summary>
    private void HandlePause(bool isPaused)
    {
        isGamePaused = isPaused;
        UpdateGhostVisibility();
    }

    /// <summary>
    /// Updates visibility when the hint or ad panel state changes.
    /// (İpucu veya reklam paneli durumu değiştiğinde görünürlüğü günceller.)
    /// </summary>
    private void HandleHint(bool isHintActive)
    {
        isHintPanelOpen = isHintActive;
        UpdateGhostVisibility();
    }

    /// <summary>
    /// Determines whether the ghost should be visible based on UI states and player position.
    /// (Arayüz durumlarına ve oyuncu pozisyonuna göre hayaletin görünür olup olmayacağını belirler.)
    /// </summary>
    private void UpdateGhostVisibility()
    {
        if (spawnedGhost != null)
        {
            // Eğer Pause menüsü açıksa VEYA İpucu/Reklam ekranı açıksa hayaleti gizle!
            bool isAnyUIMenuOpen = isGamePaused || isHintPanelOpen;

            // Sadece menüler kapalıysa ve oyuncu alanın içindeyse göster
            spawnedGhost.SetActive(!isAnyUIMenuOpen && isPlayerInside);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;

            if (spawnedGhost == null)
            {
                spawnedGhost = Instantiate(ghostPrefab);
            }

            UpdateGhostVisibility();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;

            if (spawnedGhost != null)
            {
                spawnedGhost.SetActive(false);
            }
        }
    }
}