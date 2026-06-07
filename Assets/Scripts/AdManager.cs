using UnityEngine;
using GoogleMobileAds.Api;
using System;
using System.Collections.Generic;

public class AdManager : MonoBehaviour
{
    public static AdManager instance;

    [Header("Ödüllü Reklam (Rewarded Ad) ID'leri")]
#if UNITY_ANDROID
    private string rewardedAdUnitId = "ca-app-pub-2253052581488553/5587774728";
#elif UNITY_IOS
    private string rewardedAdUnitId = "ca-app-pub-2253052581488553/4495174586";
#else
    private string rewardedAdUnitId = "unexpected_platform";
#endif
    private RewardedAd rewardedAd;

    [Header("Geçiþ Reklamý (Interstitial Ad) ID'leri")]
#if UNITY_ANDROID
    private string interstitialAdUnitId = "ca-app-pub-2253052581488553/9906979441";
#elif UNITY_IOS
    private string interstitialAdUnitId = "ca-app-pub-2253052581488553/3182092911";
#else
    private string interstitialAdUnitId = "unexpected_platform";
#endif
    private InterstitialAd interstitialAd;

    // YENÝ EKLENEN: Unity'nin çökmemesi için iþlemleri sýraya sokan güvenli kuyruk sistemi
    private Queue<Action> mainThreadActions = new Queue<Action>();

    private void Awake()
    {
        MobileAds.RaiseAdEventsOnUnityMainThread = true;

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
            // YENÝ EKLENEN: Reklam kapandýðýnda (ödül alýnsa da alýnmasa da) her halükarda yenisini yükle
            rewardedAd.OnAdFullScreenContentClosed += () =>
            {
                lock (mainThreadActions)
                {
                    mainThreadActions.Enqueue(() =>
                    {
                        LoadRewardedAd();
                    });
                }
            };

            rewardedAd.Show((Reward reward) =>
            {
                // Oyuncu ödülü kazandýðýnda sadece canlandýrma iþlemini yap
                lock (mainThreadActions)
                {
                    mainThreadActions.Enqueue(() =>
                    {
                        if (GameManager.instance != null) GameManager.instance.RevivePlayer();
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