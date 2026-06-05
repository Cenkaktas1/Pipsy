using UnityEngine;
using GoogleMobileAds.Api;
using System;
using System.Collections.Generic;

public class AdManager : MonoBehaviour
{
    public static AdManager instance;

    [Header("Ödüllü Reklam (Rewarded Ad) ID'leri")]
#if UNITY_ANDROID
    private string rewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IOS
    private string rewardedAdUnitId = "ca-app-pub-3940256099942544/1712485313";
#else
    private string rewardedAdUnitId = "unexpected_platform";
#endif
    private RewardedAd rewardedAd;

    [Header("Geçiþ Reklamý (Interstitial Ad) ID'leri")]
#if UNITY_ANDROID
    private string interstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_IOS
    private string interstitialAdUnitId = "ca-app-pub-3940256099942544/4411468910";
#else
    private string interstitialAdUnitId = "unexpected_platform";
#endif
    private InterstitialAd interstitialAd;

    // YENÝ EKLENEN: Unity'nin çökmemesi için iþlemleri sýraya sokan güvenli kuyruk sistemi
    private Queue<Action> mainThreadActions = new Queue<Action>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        MobileAds.Initialize(initStatus => {
            LoadRewardedAd();
            LoadInterstitialAd();
        });
    }

    // YENÝ EKLENEN: Kuyruktaki iþlemleri Unity'nin Ana Kanalýnda (Main Thread) güvenle çalýþtýrýr
    private void Update()
    {
        while (mainThreadActions.Count > 0)
        {
            Action action = null;
            lock (mainThreadActions)
            {
                action = mainThreadActions.Dequeue();
            }
            action?.Invoke();
        }
    }

    // ==========================================
    // ÖDÜLLÜ REKLAM
    // ==========================================

    public void LoadRewardedAd()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        var adRequest = new AdRequest();

        RewardedAd.Load(rewardedAdUnitId, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null) return;
            rewardedAd = ad;
        });
    }

    public void ShowRewardedAd()
    {
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardedAd.Show((Reward reward) =>
            {
                // YENÝ: Oyuncu ödülü kazandýðýnda iþlemi güvenli kuyruða ekle (Çökmeyi engeller)
                lock (mainThreadActions)
                {
                    mainThreadActions.Enqueue(() =>
                    {
                        if (GameManager.instance != null) GameManager.instance.RevivePlayer();
                        LoadRewardedAd(); // Yenisini yükle
                    });
                }
            });
        }
    }

    // ==========================================
    // GEÇÝÞ REKLAMI
    // ==========================================

    public void LoadInterstitialAd()
    {
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }

        var adRequest = new AdRequest();

        InterstitialAd.Load(interstitialAdUnitId, adRequest, (InterstitialAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null) return;
            interstitialAd = ad;
        });
    }

    public void ShowInterstitialAd()
    {
        if (interstitialAd != null && interstitialAd.CanShowAd())
        {
            interstitialAd.OnAdFullScreenContentClosed += () =>
            {
                // YENÝ: Reklam kapandýðýnda LevelComplete panelini güvenli kuyrukla aç
                lock (mainThreadActions)
                {
                    mainThreadActions.Enqueue(() =>
                    {
                        LoadInterstitialAd();
                        if (GameManager.instance != null) GameManager.instance.ProceedToNextLevelAfterAd();
                    });
                }
            };

            interstitialAd.Show();
        }
        else
        {
            if (GameManager.instance != null) GameManager.instance.ProceedToNextLevelAfterAd();
        }
    }
}