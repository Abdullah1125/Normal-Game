using UnityEngine;
using GoogleMobileAds.Api;
using System;

/// <summary>
/// Manages interstitial ads and persists between scenes.
/// (Geçiþ reklamlarýný yönetir ve sahneler arasý kalýcýlýk saðlar.)
/// </summary>
public class AdMobInterstitialManager : MonoBehaviour
{
    public static AdMobInterstitialManager Instance;

    private string _adUnitId = "ca-app-pub-3940256099942544/1033173712"; // Test ID
    private InterstitialAd _interstitialAd;
    private Action _onAdClosedCallback;

    private void Awake()
    {
        // SINGLETON VE SAHNE GEÇÝÞÝ KORUMASI
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Obje sahneler arasý silinmez
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // REKLAMI ANA MENÜDEYKEN ÝNDÝRMEYE BAÞLA
        MobileAds.Initialize((InitializationStatus status) => { LoadInterstitialAd(); });
    }

    /// <summary>
    /// Loads a new interstitial ad from Google.
    /// (Google'dan yeni bir geçiþ reklamý yükler.)
    /// </summary>
    public void LoadInterstitialAd()
    {
        if (_interstitialAd != null) _interstitialAd.Destroy();

        var adRequest = new AdRequest();
        InterstitialAd.Load(_adUnitId, adRequest, (InterstitialAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null) return;
            _interstitialAd = ad;

            _interstitialAd.OnAdFullScreenContentClosed += () =>
            {
                _onAdClosedCallback?.Invoke();
                _onAdClosedCallback = null;
                LoadInterstitialAd(); // Kapanýnca hemen yenisini indir
            };
        });
    }

    public bool IsAdReady() => _interstitialAd != null && _interstitialAd.CanShowAd();

    /// <summary>
    /// Shows the interstitial ad if ready and returns the status.
    /// (Reklam hazýrsa gösterir ve durum bilgisini bool olarak döner.)
    /// </summary>
    public bool ShowInterstitialAd(Action onClosed)
    {
        if (IsAdReady())
        {
            _onAdClosedCallback = onClosed;
            _interstitialAd.Show();
            return true; // Reklam baþarýyla gösteriliyor!
        }
        else
        {
            // Reklam hazýr deðilse yenisini yükle ve false dön ki fake loading baþlasýn
            LoadInterstitialAd();
            return false; // Reklam yok!
        }
    }
}