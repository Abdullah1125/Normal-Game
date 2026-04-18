using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class TutorialTrigger : MonoBehaviour
{
    [Header("Doðurulacak Hayalet (Prefab)")]
    public GameObject ghostPrefab;

    // Sahnedeki hayaleti aklýmýzda tutmak için
    private GameObject spawnedGhost;

    private void Start()
    {
        // Ne olur ne olmaz Trigger modunu kesin açalým
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Oyuncu alana girdiðinde ve hayalet yoksa
        if (other.CompareTag("Player") && spawnedGhost == null)
        {
            // DÝKKAT: Konum vermedik! Böylece Prefab kendi kaydedildiði orijinal konumda doðar.
            spawnedGhost = Instantiate(ghostPrefab);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Oyuncu alandan çýktýðýnda
        if (other.CompareTag("Player") && spawnedGhost != null)
        {
            // Hayaleti yok et
            Destroy(spawnedGhost);
        }
    }
}