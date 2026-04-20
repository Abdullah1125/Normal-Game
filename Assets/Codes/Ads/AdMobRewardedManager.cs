using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AdMobRewardedManager : MonoBehaviour
{
    public static AdMobRewardedManager Instance;

    [Header("Ad Unit IDs(Reklam Birimi Kimlikleri)")]
    // Test ID'leri yüklüdür; yayýna çýkarken kendi ID'lerinle deðiþtir.
    private string _adUnitId = "ca-app-pub-3940256099942544/5224354917";

    private RewardedAd _rewardedAd;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Aga bu kodun yolda ölmesini engeller!
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Yeni SDK sürümüne uygun Test Cihazý yapýlandýrmasý
        RequestConfiguration requestConfiguration = new RequestConfiguration
        {
            TestDeviceIds = new List<string> { "TEST_DEVICE_ID_BURAYA" }
        };

        MobileAds.SetRequestConfiguration(requestConfiguration);
        MobileAds.Initialize((InitializationStatus status) => { LoadRewardedAd(); });
    }

    public void LoadRewardedAd()
    {
        if (_rewardedAd != null) { _rewardedAd.Destroy(); _rewardedAd = null; }

        var adRequest = new AdRequest();
        RewardedAd.Load(_adUnitId, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null) return;
            _rewardedAd = ad;

            // Arka planda patlamamasý için yeni güncel Thread sistemi eklendi
            _rewardedAd.OnAdFullScreenContentClosed += () =>
            {
                MobileAdsEventExecutor.ExecuteInUpdate(() => { LoadRewardedAd(); });
            };

            _rewardedAd.OnAdFullScreenContentFailed += (AdError err) =>
            {
                MobileAdsEventExecutor.ExecuteInUpdate(() => { LoadRewardedAd(); });
            };
        });
    }

    public bool ShowRewardedAd(Action onReward)
    {
        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            _rewardedAd.Show((Reward reward) =>
            {
                // Ödül verme (UI açma) iþlemini zorla ana iþlemciye yolluyoruz
                MobileAdsEventExecutor.ExecuteInUpdate(() =>
                {
                    onReward?.Invoke();
                });
            });
            return true;
        }
        else
        {
            Debug.Log("Ödüllü reklam henüz hazýr deðil.");
            LoadRewardedAd();
            return false;
        }
    }
}