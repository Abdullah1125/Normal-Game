using UnityEngine;
using System.Collections; // Coroutine için eklendi
using System.Collections.Generic;

public class SpecificLevelSoulTracker : MonoBehaviour, IResettable // IResettable eklendi (Sen öldüğünde haberi olması için)
{
    [Header("Senin Kendi Ayarladığın Tracker Prefabı")]
    public GameObject trackerPrefab;

    private float searchTimer = 0f;
    private Queue<GameObject> availableTrackers = new Queue<GameObject>();
    private List<SoulEffect> deadSouls = new List<SoulEffect>();

    // Mobil Optimizasyon: OverlapCircleNonAlloc için önceden ayrılmış sabit boyutlu dizi.
    // Her frame yeni dizi oluşturmak yerine bu diziyi tekrar tekrar kullanıyoruz → GC allocation = 0
    private readonly Collider2D[] _overlapBuffer = new Collider2D[8];

    private class TrackerData
    {
        public GameObject trackerObj;
        public Collider2D soulCollider;
        public bool hasPlayedSound;
    }
    private Dictionary<SoulEffect, TrackerData> activeTrackers = new Dictionary<SoulEffect, TrackerData>();

    void Start()
    {
        // Oyunun Reset sistemine (senin ölümüne) abone ol
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }

        Box[] allBoxes = FindObjectsByType<Box>(FindObjectsSortMode.None);
        for (int i = 0; i < allBoxes.Length; i++)
        {
            allBoxes[i].gameObject.tag = "Untagged";
        }
    }

    void Update()
    {
        searchTimer += Time.deltaTime;
        if (searchTimer >= 0.5f)
        {
            SoulEffect[] activeSouls = FindObjectsByType<SoulEffect>(FindObjectsSortMode.None);
            for (int i = 0; i < activeSouls.Length; i++)
            {
                SoulEffect soul = activeSouls[i];
                if (soul.gameObject.activeInHierarchy && !activeTrackers.ContainsKey(soul))
                {
                    // Mobil Optimizasyon: TryGetComponent, GetComponent'ten daha verimli.
                    // IL2CPP derlemesinde internal null-check overhead'i azaltır.
                    soul.TryGetComponent(out Collider2D soulCol);
                    TrackerData newData = new TrackerData
                    {
                        trackerObj = GetTrackerFromPool(),
                        soulCollider = soulCol,
                        hasPlayedSound = false
                    };
                    activeTrackers.Add(soul, newData);
                }
            }
            searchTimer = 0f;
        }

        deadSouls.Clear();

        foreach (var kvp in activeTrackers)
        {
            SoulEffect soul = kvp.Key;
            TrackerData data = kvp.Value;

            if (soul != null && soul.gameObject.activeInHierarchy)
            {
                Vector3 targetPos = (data.soulCollider != null) ? data.soulCollider.bounds.center : soul.transform.position;
                targetPos.y -= 0.5f;

                data.trackerObj.transform.position = targetPos;

                if (!data.hasPlayedSound)
                {
                    // Mobil Optimizasyon: OverlapCircleNonAlloc → her çağrıda yeni dizi OLUŞTURMAZ.
                    // OverlapCircleAll her çağrıda Collider2D[] dizisi yaratır → Garbage Collector tetiklenir.
                    // NonAlloc versiyonu önceden oluşturulmuş _overlapBuffer dizisini kullanır → 0 GC allocation.
                    int hitCount = Physics2D.OverlapCircleNonAlloc(targetPos, 1f, _overlapBuffer);
                    for (int i = 0; i < hitCount; i++)
                    {
                        // Mobil Optimizasyon: TryGetComponent, GetComponent'ten daha verimli.
                        // GetComponent null dönerse bile internal olarak exception-based kontrol yapar.
                        // TryGetComponent doğrudan bool döner, daha temiz ve hızlı.
                        if (_overlapBuffer[i].TryGetComponent(out BoxButton _) || _overlapBuffer[i].TryGetComponent(out FallingBoxButton _))
                        {
                            if (SoundManager.Instance != null)
                            {
                                SoundManager.PlayThemeSFX(SFXType.SlidingDoor);
                            }
                            data.hasPlayedSound = true;
                            break;
                        }
                    }
                }
            }
            else
            {
                data.trackerObj.transform.position = new Vector3(99999f, 99999f, 0f);
                deadSouls.Add(soul);
                availableTrackers.Enqueue(data.trackerObj);
            }
        }

        for (int i = 0; i < deadSouls.Count; i++)
        {
            activeTrackers.Remove(deadSouls[i]);
        }
    }

    private GameObject GetTrackerFromPool()
    {
        if (availableTrackers.Count > 0) return availableTrackers.Dequeue();

        GameObject newTracker = Instantiate(trackerPrefab);
        newTracker.transform.position = new Vector3(99999f, 99999f, 0f);
        newTracker.transform.SetParent(this.transform);

        return newTracker;
    }

    // OYUNCU ÖLDÜĞÜNDE (LEVEL RESETLENDİĞİNDE) TETİKLENİR
    public void ResetMechanic()
    {
        StartCoroutine(ReTriggerButtons());
    }

    private IEnumerator ReTriggerButtons()
    {
        // Diğer her şeyin (BoxButton vs.) sıfırlanması için 1 frame bekliyoruz
        yield return new WaitForEndOfFrame();

        foreach (var kvp in activeTrackers)
        {
            TrackerData data = kvp.Value;
            if (data.trackerObj != null)
            {
                // Tracker objesindeki Collider'ı kapatıp açarak Unity fizik motoruna
                // "Butona yeni bir kutu düştü" hissini zorla veriyoruz.
                // Bu sayede eski ölüler butonu ezmeye kaldıkları yerden devam ediyor!
                // Mobil Optimizasyon: TryGetComponent ile hem null-check hem component alma tek adımda.
                if (data.trackerObj.TryGetComponent(out Collider2D col))
                {
                    col.enabled = false;
                    col.enabled = true;
                }

                // Kapı sesinin tekrar çalması için ses kilidini aç
                data.hasPlayedSound = false;
            }
        }
    }

    // Oyun kapandığında veya bölüm değiştiğinde abonelikten çıkıyoruz (Hata vermemesi için önemli)
    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.UnregisterResettable(this);
        }
    }
}
