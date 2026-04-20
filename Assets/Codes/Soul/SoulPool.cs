using System.Collections.Generic;
using UnityEngine;

public class SoulPool : MonoBehaviour
{
    public static SoulPool Instance;
    public GameObject soulPrefab;
    public int poolSize = 10;

    private List<GameObject> pooledSouls = new List<GameObject>();

    void Awake()
    {
        Instance = this;

        // Oyun baþýnda binalarý inþa et ve gizle
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(soulPrefab);
            obj.SetActive(false);
            pooledSouls.Add(obj);
        }
    }

    public GameObject GetSoul()
    {
        // Depoda boþta (aktif olmayan) ruh var mý bak
        foreach (GameObject soul in pooledSouls)
        {
            if (!soul.activeInHierarchy)
            {
                return soul;
            }
        }

        // Eðer hepsi doluysa (ki 10 tane yetecektir) bir tane daha yaratýp listeye ekle
        GameObject newSoul = Instantiate(soulPrefab);
        newSoul.SetActive(false);
        pooledSouls.Add(newSoul);
        return newSoul;
    }
}