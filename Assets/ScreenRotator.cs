using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class NightmareRotator : MonoBehaviour
{
    [Header("Referanslar")]
    public Camera mainCamera;

    [Header("UI Ayarları (İsimle Bulma)")]
    public List<UIPlacementData> uiElementsToMove;

    [System.Serializable]
    public class UIPlacementData
    {
        public string objectName;
        public Vector2 newPosition;
        public float rotationZ = 180f;
       
    }

    private void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        // Sahne değişimini dinle
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene current)
    {
        ResetEverything();
    }

    private void Start()
    {
        // Siyah ekran kalktığı için direkt işleme geçiyoruz
        StartCoroutine(InstantRotationSequence());
    }

    IEnumerator InstantRotationSequence()
    {
        // 1. Kare: Sahne objelerinin oturması için minik bir nefes
        yield return null;

        // 2. Kare: Dünyayı ve ekranı anında tersyüz et
        ApplyNightmareChanges();
    }

    void ApplyNightmareChanges()
    {
        // --- DONANIMSAL YÖN AYARI ---
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;

        // Oyun içinde LandscapeLeft (Ters) yapıyoruz
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        // --- MAP & KAMERA ---
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.transform.eulerAngles = new Vector3(0, 0, 180);
        }

        // --- UI MANİPÜLASYONU ---
        RectTransform[] allRects = Resources.FindObjectsOfTypeAll<RectTransform>();

        foreach (var data in uiElementsToMove)
        {
            foreach (RectTransform r in allRects)
            {
                if (r.name == data.objectName && r.gameObject.scene.name != null)
                {
                    r.anchoredPosition = data.newPosition;
                    r.localRotation = Quaternion.Euler(0, 0, data.rotationZ);
                   
                }
            }
        }

        Canvas.ForceUpdateCanvases();
        RefreshRaycasts();
        Debug.Log("💥 Nightmare Activated: Siyah ekransız anlık değişim!");
    }

    public void ResetEverything()
    {
        // MENÜYE DÖNERKEN LANDSCAPE RIGHT ZORUNLU
        Screen.orientation = ScreenOrientation.LandscapeRight;
        Screen.autorotateToLandscapeRight = true;

        if (mainCamera != null)
            mainCamera.transform.rotation = Quaternion.identity;
    }

    private void OnDestroy()
    {
        ResetEverything();
    }

    void UpdateText(GameObject obj, string text)
    {
        var tmp = obj.GetComponentInChildren<TextMeshProUGUI>(true);
        if (tmp != null) { tmp.text = text; return; }
        var legacy = obj.GetComponentInChildren<Text>(true);
        if (legacy != null) legacy.text = text;
    }

    void RefreshRaycasts()
    {
        var raycasters = FindObjectsByType<GraphicRaycaster>(FindObjectsSortMode.None);
        foreach (var gr in raycasters) { gr.enabled = false; gr.enabled = true; }
    }
}