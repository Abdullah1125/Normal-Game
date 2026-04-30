using UnityEngine;
using GoogleMobileAds.Api;
using System;

/// <summary>
/// Manages interstitial ads, persists between scenes, and safely dispatches callbacks to the Main Thread.
/// (Geçiş reklamlarını yönetir, sahneler arası kalıcılık sağlar ve geri aramaları Ana İş İpliğine güvenle aktarır.)
/// </summary>
public class AdMobInterstitialManager : SingletonPersistent<AdMobInterstitialManager>
{

    private string _adUnitId = "ca-app-pub-3940256099942544/1033173712"; // Test ID
    private InterstitialAd _interstitialAd;

    private Action _onAdClosedCallback;

    // MAIN THREAD DISPATCHER (Ana iş ipliğine aktarılacak görev)
    private Action _executeOnMainThread;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        MobileAds.Initialize((InitializationStatus status) => { LoadInterstitialAd(); });
    }

    /// <summary>
    /// Executes pending callbacks on the Main Thread.
    /// (Bekleyen geri aramaları Ana İş İpliğinde çalıştırır.)
    /// </summary>
    private void Update()
    {
        if (_executeOnMainThread != null)
        {
            _executeOnMainThread.Invoke();
            _executeOnMainThread = null; // Çalıştırdıktan sonra temizle
        }
    }

    /// <summary>
    /// Loads a new interstitial ad from Google.
    /// (Google'dan yeni bir geçiş reklamı yükler.)
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

            // REKLAM NORMAL KAPANDIĞINDA
            _interstitialAd.OnAdFullScreenContentClosed += () =>
            {
                // Sahnede işlem yapabilmesi için görevi Update'e devrediyoruz!
                _executeOnMainThread = _onAdClosedCallback;
                _onAdClosedCallback = null;
                LoadInterstitialAd();
            };

            // REKLAM HATA VERİP ÇÖKERSE (İSİM DÜZELTİLDİ!)
            _interstitialAd.OnAdFullScreenContentFailed += (AdError adError) =>
            {
                Debug.LogError("Reklam gösterilemedi: " + adError.GetMessage());
                // Reklam çökse bile oyuncu ekranda takılı kalmasın, levele devam etsin
                _executeOnMainThread = _onAdClosedCallback;
                _onAdClosedCallback = null;
                LoadInterstitialAd();
            };
        });
    }

    public bool IsAdReady() => _interstitialAd != null && _interstitialAd.CanShowAd();

    /// <summary>
    /// Shows the interstitial ad if ready and returns the status.
    /// (Reklam hazırsa gösterir ve durum bilgisini bool olarak döner.)
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
