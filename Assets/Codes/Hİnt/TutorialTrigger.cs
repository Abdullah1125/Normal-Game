using UnityEngine;
using System; // Event sistemi için gerekli

[RequireComponent(typeof(BoxCollider2D))]
public class TutorialTrigger : MonoBehaviour
{
    [Header("Doðurulacak Hayalet (Prefab)")]
    public GameObject ghostPrefab;
    private GameObject spawnedGhost;

    private bool isPlayerInside = false; // Adam alanýn içinde mi?

    // YENÝ: Pause sistemini dinleyecek dev anons sistemi!
    public static Action<bool> OnPauseToggled;

    private void OnEnable()
    {
        OnPauseToggled += HandlePause;
    }

    private void OnDisable()
    {
        OnPauseToggled -= HandlePause;
    }

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

    // PAUSE AÇILIP KAPANDIÐINDA BURASI ÇALIÞIR
    private void HandlePause(bool isPaused)
    {
        if (spawnedGhost != null)
        {
            // Eðer pause açýldýysa gizle. 
            // Eðer pause kapandýysa VE adam hala alanýn içindeyse geri göster!
            spawnedGhost.SetActive(!isPaused && isPlayerInside);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;

            if (spawnedGhost == null)
            {
                // SADECE ÝLK SEFERDE YARAT 
                spawnedGhost = Instantiate(ghostPrefab);
            }
            else
            {
                // DAHA ÖNCE YARATILDIYSA SADECE GÖRÜNÜR YAP
                spawnedGhost.SetActive(true);
            }
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