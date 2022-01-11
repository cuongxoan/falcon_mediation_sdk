using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FMediation
{
    public class FMediation_AdaptSwitcher
    {
        public static bool enableLogData4Game;
        public static void Setup(bool isShowInterBackupForVideoNotAvailable, BannerPosition bannerPosition)
        {
#if EXISTED_IRON_SOURCE
            FMediation_AdaptSwitcher.SetupIronsource(isShowInterBackupForVideoNotAvailable, bannerPosition);
#elif EXISTED_MAX
            FMediation_AdaptSwitcher.SetupMAX(isShowInterBackupForVideoNotAvailable, bannerPosition);
#endif
        }
        
#if EXISTED_IRON_SOURCE
        public static string ironsource_key_android;
        public static string ironsource_key_ios;

        public static void SetupIronsource(bool isShowInterBackupForVideoNotAvailable, BannerPosition bannerPosition)
        {
            var config = JsonUtility.FromJson<FMediation_Config.config>(Resources.Load<TextAsset>("falcon_mediation_config").text);
            enableLogData4Game = config.enableLogData4Game;
            ironsource_key_android = config.ironsource.ironsource_key_android;
            ironsource_key_ios = config.ironsource.ironsource_key_ios;
            Debug.Log($"[Ironsource setup] androidKey {ironsource_key_android} " +
                      $"iosKey {ironsource_key_ios}");
            
            var maxGo = new GameObject("[FMediation] Ironsource", typeof(FMediation_AdapterIronsource));
            GameObject.DontDestroyOnLoad(maxGo);
            adapter = maxGo.GetComponent<FMediation_AdapterIronsource>().Setup(ironsource_key_android, ironsource_key_ios);
            adapter.Init(isShowInterBackupForVideoNotAvailable, bannerPosition);
        }

#elif EXISTED_MAX
        public static string MaxSdkKey;
        public static string InterstitialAdUnitId;
        public static string RewardedAdUnitId;
        public static string RewardedInterstitialAdUnitId;
        public static string BannerAdUnitId;
        public static string MRecAdUnitId;

        public static void SetupMAX(bool isShowInterBackupForVideoNotAvailable, BannerPosition bannerPosition)
        {
            var config = JsonUtility.FromJson<FMediation_Config.config>(Resources.Load<TextAsset>("falcon_mediation_config").text);
            enableLogData4Game = config.enableLogData4Game;
            MaxSdkKey = config.max.MaxSdkKey;
            InterstitialAdUnitId = config.max.InterstitialAdUnitId;
            RewardedAdUnitId = config.max.RewardedAdUnitId;
            RewardedInterstitialAdUnitId = config.max.RewardedInterstitialAdUnitId;
            BannerAdUnitId = config.max.BannerAdUnitId;
            MRecAdUnitId = config.max.MRecAdUnitId;
            
            Debug.Log($"[MAX setup] MaxSdkKey {MaxSdkKey} " +
                      $"InterstitialAdUnitId {InterstitialAdUnitId} " +
                      $"RewardedAdUnitId {RewardedAdUnitId} " +
                      $"RewardedInterstitialAdUnitId {RewardedInterstitialAdUnitId} " +
                      $"BannerAdUnitId {BannerAdUnitId} " +
                      $"MRecAdUnitId {MRecAdUnitId}");

            var maxGo = new GameObject("[FMediation] MAX", typeof(FMediation_AdapterMAX));
            GameObject.DontDestroyOnLoad(maxGo);
            adapter = maxGo.GetComponent<FMediation_AdapterMAX>()
                .Setup(MaxSdkKey, InterstitialAdUnitId, RewardedAdUnitId, RewardedInterstitialAdUnitId, BannerAdUnitId, MRecAdUnitId);
            adapter.Init(isShowInterBackupForVideoNotAvailable, bannerPosition);
        }

#endif
        private static FMediation_Adapter adapter;

        public static bool IsBannerReady => adapter.bannerReady;

        public static void LoadBanner()
        {
            adapter.LoadBanner();
        }
        public static void ShowBanner()
        {
            adapter.ShowBanner();
        }

        public static void HideBanner()
        {
            adapter.HideBanner();
        }

        public static bool IsInterstitialReady => adapter.interstitialReady;
        public static void ShowInterstitial(string where, int maxLevel)
        {
            adapter.ShowInterstitial(where, maxLevel);
        }

        public static void LoadInterstitial()
        {
            adapter.LoadInterstitial();
        }

        public static bool IsRewardedVideoReady => adapter.rewardedVideoReady;

        // #if UNITY_EDITOR
        // private static bool testVideoAvailable = false;
        // #endif
        public static void ShowRewardedVideo(Action onComplete, Action onFail, string where, int maxLevel)
        {
            adapter.onRewardedVideoSuccess = onComplete;
            adapter.onRewardedVideoFail = onFail;
            if (adapter.rewardedVideoReady 
            // #if UNITY_EDITOR
            // && testVideoAvailable
            // #endif
            )
            {
                adapter.isApplyInterBackupForVideo = false;
                adapter.ShowRewardedVideo(where, maxLevel);
            }
            else if (adapter.isShowInterBackupForVideoNotAvailable)
            {
                Debug.Log("Show Interstitial backup for rewarded video not available");
                if (adapter.interstitialReady)
                {
                    adapter.isApplyInterBackupForVideo = true;
                    adapter.ShowInterstitial(where, maxLevel);
                }
                else
                {
                    adapter.isApplyInterBackupForVideo = false;
                    adapter.onRewardedVideoFail?.Invoke();
                }
            }
        }

        /// <summary>
        /// Only MAX mediation
        /// </summary>
        public static void LoadRewardedVideo()
        {
            adapter.LoadRewardedVideo();
        }
    }
}