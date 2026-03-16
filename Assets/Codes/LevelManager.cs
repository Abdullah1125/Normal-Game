using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    public int mevcutLevelIndex = 0;
    public List<LevelData> tumLeveller; 

    [HideInInspector] public LevelData aktifLevel;

    void Awake() { Instance = this; }
    void Start() { SeviyeyiUygula(); }

    public void SeviyeAtla()
    {
        mevcutLevelIndex++;

        
        if (mevcutLevelIndex >= 6)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            return;
        }

        SeviyeyiUygula();
        
        FindFirstObjectByType<PlayerController>().ResetPosition();
    }

    void SeviyeyiUygula()
    {
        if (mevcutLevelIndex < tumLeveller.Count)
        {
            aktifLevel = tumLeveller[mevcutLevelIndex];

            // Yerçekimi ayarý
            Physics2D.gravity = new Vector2(0, aktifLevel.yercekimiTers ? 9.81f : -9.81f);
           

            // Gizli Duvar
            GameObject gizliDuvar = GameObject.Find("Tilemap_Secret");
            if (gizliDuvar != null)
            {
                gizliDuvar.GetComponent<Collider2D>().enabled = !aktifLevel.gizliGecitOdasiVar;
            }

          
         

            Debug.Log("Þu anki Seviye: " + aktifLevel.levelAdi);
        }
    }
}