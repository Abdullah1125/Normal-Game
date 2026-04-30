using System.Collections.Generic;
using UnityEngine;

public class SoulPool : Singleton<SoulPool>
{
    public GameObject soulPrefab;
    public int poolSize = 10;

    private List<GameObject> pooledSouls = new List<GameObject>();

    protected override void Awake()
    {
        base.Awake();

        // Oyun başında binaları inşa et ve gizle
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(soulPrefab);
            obj.SetActive(false);
            pooledSouls.Add(obj);
        }
    }

    public GameObject GetSoul()
    {
        // Depoda boşta (aktif olmayan) ruh var mı bak
        foreach (GameObject soul in pooledSouls)
        {
            if (!soul.activeInHierarchy)
            {
                return soul;
            }
        }

        // Eğer hepsi doluysa (ki 10 tane yetecektir) bir tane daha yaratıp listeye ekle
        GameObject newSoul = Instantiate(soulPrefab);
        newSoul.SetActive(false);
        pooledSouls.Add(newSoul);
        return newSoul;
    }
}
