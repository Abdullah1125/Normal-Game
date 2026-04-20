using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class AdMobInterstitialManager : MonoBehaviour
{
    public static AdMobInterstitialManager Instance;

    [Header("Ad Unit IDs(Reklam Birimi Kimlikleri)")]
    // Geçiþ reklamý test ID'si (Gerçek yayýnda kendi ID'ni koyacaksýn)
    private string _adUnitId = "ca-app-pub-3940256099942544/1033173712";

    private InterstitialAd _interstitialAd;
    private Action _onAdClosedCallback;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        MobileAds.Initialize((InitializationStatus status) => { LoadInterstitialAd(); });
    }

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

            // Adam reklamý kapattýðýnda (çarpýya bastýðýnda) tetiklenir
            _interstitialAd.OnAdFullScreenContentClosed += () =>
            {
                _onAdClosedCallback?.Invoke(); // Oyuncuyu bölüme gönder
                _onAdClosedCallback = null;    // Tetikleyiciyi temizle
                LoadInterstitialAd();          // Arka planda yeni reklam yükle
            };

            // Reklam bir hatadan dolayý çökerse
            _interstitialAd.OnAdFullScreenContentFailed += (AdError err) =>
            {
                _onAdClosedCallback?.Invoke(); // Adamý bekletme, bölüme sal
                _onAdClosedCallback = null;
                LoadInterstitialAd();
            };
        });
    }

    // LevelMenuButton bu fonksiyonu çaðýracak
    public bool ShowInterstitialAd(Action onClosed)
    {
        if (_interstitialAd != null && _interstitialAd.CanShowAd())
        {
            _onAdClosedCallback = onClosed;
            _interstitialAd.Show();
            return true; // Baþarýlý! Reklam patladý.
        }
        else
        {
            Debug.Log("Geçiþ reklamý henüz hazýr deðil (Yavaþ Ýnternet).");
            LoadInterstitialAd(); // Arka planda yenisini istemeyi unutma
            return false; // Baþarýsýz! Reklam gösterilemedi.
        }
    }
}