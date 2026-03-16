using UnityEngine;

[CreateAssetMenu(fileName = "Level_", menuName = "NormalGame/LevelData")]
public class LevelData : ScriptableObject
{
    public int levelID;
    public string levelAdi;

    [Header("Yasaklar")]
    public bool solYasak;
    public bool sagYasak;
    public bool ziplamaYasak;

    [Header("Dünya Ayarlarý")]
    public bool yercekimiTers;

    [Header("Ters Kapý / Gizli Geçit Ayarlarý")]
    public bool gizliGecitOdasiVar; 
    public Vector3 gizliOdaPozisyonu;

    [Header("Etkileþimli Öge Ayarlarý")]
    public bool butonAktif = true;
}
