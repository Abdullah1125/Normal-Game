using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level_", menuName = "NormalGame/LevelData")]
public class LevelData : ScriptableObject
{
    public int levelID; // Seviyenin benzersiz kimlik numarası
    public string levelName; // Seviyenin adı

    [Header("Restrictions (Yasaklar)")]
    public bool isLeftForbidden; // Sola gitmek yasak 
    public bool isRightForbidden; // Sağa gitmek yasak 
    public bool isJumpForbidden; // Zıplamak yasak    

    [Header("Interactive Element Settings (Etkileşim Ayarları)")]
    public bool isButtonActive = true; // Buton etkileşime açık 

    [Header("Special Mechanical Prefabs(Özel Mekanik Prefabları)")]
    public List<GameObject> specialMechanics; // Buraya istediğin kadar mekanik sürükle!

    [Header("Hint System(İpucu Sistemi)")]
    [TextArea(3, 10)]
    public string levelHint;
}