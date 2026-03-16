using UnityEngine;

[CreateAssetMenu(fileName = "Level_", menuName = "NormalGame/LevelData")]
public class LevelData : ScriptableObject
{
    public int levelID; // Seviyenin benzersiz kimlik numarasý
    public string levelName; // Seviyenin adý

    [Header("Restrictions (Yasaklar)")]
    public bool isLeftForbidden; // Sola gitmek yasak mý?
    public bool isRightForbidden; // Saða gitmek yasak mý?
    public bool isJumpForbidden; // Zýplamak yasak mý?

    [Header("World Settings (Dünya Ayarlarý)")]
    public bool isGravityInverted; // Yerçekimi ters mi?

    [Header("Secret Passage Settings (Gizli Geçit Ayarlarý)")]
    public bool hasSecretPassage; // Gizli geçit odasý var mý?
    public Vector3 secretRoomPos; // Gizli odanýn kamera pozisyonu

    [Header("Interactive Element Settings (Etkileþim Ayarlarý)")]
    public bool isButtonActive = true; // Buton etkileþime açýk mý?
}