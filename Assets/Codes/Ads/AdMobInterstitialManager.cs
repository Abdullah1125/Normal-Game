using UnityEngine;
using GoogleMobileAds.Api;
using System;

/// <summary>
/// Manages interstitial ads, persists between scenes, and safely dispatches callbacks to the Main Thread.
/// (Geçiþ reklamlarýný yönetir, sahneler arasý kalýcýlýk saðlar ve geri aramalarý Ana Ýþ Ýpliðine güvenle aktarýr.)
/// </summary>
public class AdMobInterstitialManager : MonoBehaviour
{
    public static AdMobInterstitialManager Instance;

    private string _adUnitId = "ca-app-pub-3940256099942544/1033173712"; // Test ID
    private InterstitialAd _interstitialAd;

    private Action _onAdClosedCallback;

    // MAIN THREAD DISPATCHER (Ana iþ ipliðine aktarýlacak görev)
    private Action _executeOnMainThread;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        MobileAds.Initialize((InitializationStatus status) => { LoadInterstitialAd(); });
    }

    /// <summary>
    /// Executes pending callbacks on the Main Thread.
    /// (Bekleyen geri aramalarý Ana Ýþ Ýpliðinde çalýþtýrýr.)
    /// </summary>
    private void Update()
    {
        if (_executeOnMainThread != null)
        {
            _executeOnMainThread.Invoke();
            _executeOnMainThread = null; // Çalýþtýrdýktan sonra temizle
        }
    }

    /// <summary>
    /// Loads a new interstitial ad from Google.
    /// (Google'dan yeni bir geçiþ reklamý yükler.)
    /// </summary>
    public void LoadInterstitialAd()
    {
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        var adRequest = new AdRequest();
        InterstitialAd.Load(_adUnitId, adRequest, (InterstitialAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null) return;

            _interstitialAd = ad;

            // REKLAM NORMAL KAPANDIÐINDA
            _interstitialAd.OnAdFullScreenContentClosed += () =>
            {
                // Sahnede iþlem yapabilmesi için görevi Update'e devrediyoruz!
                _executeOnMainThread = _onAdClosedCallback;
                _onAdClosedCallback = null;
                LoadInterstitialAd();
            };

            // REKLAM HATA VERÝP ÇÖKERSE (ÝSÝM DÜZELTÝLDÝ!)
            _interstitialAd.OnAdFullScreenContentFailed += (AdError adError) =>
            {
                Debug.LogError("Reklam gösterilemedi: " + adError.GetMessage());
                // Reklam çökse bile oyuncu ekranda takýlý kalmasýn, levele devam etsin
                _executeOnMainThread = _onAdClosedCallback;
                _onAdClosedCallback = null;
                LoadInterstitialAd();
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
            return true;
        }
        else
        {
            LoadInterstitialAd();
            return false;
        }
    }
}