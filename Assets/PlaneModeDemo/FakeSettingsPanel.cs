using UnityEngine;

public class FakeSettingsPanel : MonoBehaviour
{
    public RectTransform panel;
    public float closedY = -600f;
    public float openY = 0f;
    public float speed = 10f;

    private bool isOpen = false;

    void Start()
    {
        panel.anchoredPosition = new Vector2(0, closedY);
    }

    void Update()
    {
        float targetY = isOpen ? openY : closedY;
        panel.anchoredPosition = Vector2.Lerp(
            panel.anchoredPosition,
            new Vector2(0, targetY),
            speed * Time.deltaTime
        );

        // Space tuþu ile test
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isOpen = !isOpen;
            Debug.Log(isOpen ? "Panel AÇIK" : "Panel KAPALI");
        }
    }
}