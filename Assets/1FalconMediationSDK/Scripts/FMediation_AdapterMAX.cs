using System;
using UnityEngine;

namespace FMediation
{
#if EXISTED_MAX
    public class FMediation_AdapterMAX : FMediation_Adapter
    {
        private string MaxSdkKey = "ENTER_MAX_SDK_KEY_HERE";
        private string InterstitialAdUnitId = "ENTER_INTERSTITIAL_AD_UNIT_ID_HERE";
        private string RewardedAdUnitId = "ENTER_REWARD_AD_UNIT_ID_HERE";
        private string RewardedInterstitialAdUnitId = "ENTER_REWARD_INTER_AD_UNIT_ID_HERE";
        private string BannerAdUnitId = "ENTER_BANNER_AD_UNIT_ID_HERE";
        private string MRecAdUnitId = "ENTER_MREC_AD_UNIT_ID_HERE";
        private int interstitialRetryAttempt;
        private int rewardedRetryAttempt;
        private int rewardedInterstitialRetryAttempt;

        private FMediation_AdapterMAX()
        {
        }

        public FMediation_AdapterMAX Setup(string maxSdkKey, string interstitialAdUnitId, string rewardedAdUnitId,
            string rewardedInterstitialAdUnitId, string bannerAdUnitId, string mRecAdUnitId)
        {
            this.MaxSdkKey = maxSdkKey;
            this.InterstitialAdUnitId = interstitialAdUnitId;
            this.RewardedAdUnitId = rewardedAdUnitId;
            this.RewardedInterstitialAdUnitId = rewardedInterstitialAdUnitId;
            this.BannerAdUnitId = bannerAdUnitId;
            this.MRecAdUnitId = mRecAdUnitId;
            return this;
        }

        public override void Init(bool isShowInterBackupForVideoNotAvailable, BannerPosition bannerPosition)
        {
            this.isShowInterBackupForVideoNotAvailable = isShowInterBackupForVideoNotAvailable;
            this.BannerPosition = bannerPosition;
            MaxSdkCallbacks.OnSdkInitializedEvent += sdkConfiguration =>
            {
                // AppLovin SDK is initialized, configure and start loading ads.
                Debug.Log("MAX SDK Initialized");

                InitializeInterstitialAds();
                InitializeRewardedAds();
                InitializeBannerAds();
            };

            MaxSdk.SetSdkKey(MaxSdkKey);
            MaxSdk.InitializeSdk();
            Debug.Log("[MaxAdapter] InitComplete");
        }

        public override bool isShowInterBackupForVideoNotAvailable { get; set; }
        public override bool bannerReady { get; set; }

        // Banner
        private void InitializeBannerAds()
        {
            if (string.IsNullOrEmpty(BannerAdUnitId))
            {
                Debug.LogError("BannerAdUnitId can be not null");
                return;
            }
            // Banners are automatically sized to 320x50 on phones and 728x90 on tablets.
            // You may use the utility method `MaxSdkUtils.isTablet()` to help with view sizing adjustments.
            if(this.BannerPosition == BannerPosition.TOP)
                MaxSdk.CreateBanner(BannerAdUnitId, MaxSdkBase.BannerPosition.TopCenter);
            else if(this.BannerPosition == BannerPosition.BOTTOM)
                MaxSdk.CreateBanner(BannerAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);

            // Set background or background color for banners to be fully functional.
            MaxSdk.SetBannerBackgroundColor(BannerAdUnitId, bannerBackgroundColor);
            MaxSdkCallbacks.Banner.OnAdLoadedEvent += (s, info) => OnBannerLoaded();
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += (s, info) => OnBannerLoadFailed();
        }

        public void OnBannerLoaded()
        {
            bannerReady = true;
        }
        public void OnBannerLoadFailed()
        {
            bannerReady = false;
        }

        [Obsolete("No need, Only Ironsource need this function")]
        public override void LoadBanner()
        {
            Debug.Log("No need, it auto load");
        }

        public override void ShowBanner()
        {
            MaxSdk.ShowBanner(BannerAdUnitId);
        }

        public override void HideBanner()
        {
            MaxSdk.HideBanner(BannerAdUnitId);
        }

// Interstitials
        private void InitializeInterstitialAds()
        {
            if (string.IsNullOrEmpty(InterstitialAdUnitId))
            {
                Debug.LogError("InterstitialAdUnitId can be not null");
                return;
            }
            // Attach callbacks
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += InterstitialFailedToDisplayEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialDismissedEvent;

            // Load the first interstitial
            LoadInterstitial();
        }

        public override bool interstitialReady
        {
            get => MaxSdk.IsInterstitialReady(InterstitialAdUnitId);
            set { }
        }

        public override void LoadInterstitial()
        {
            Debug.LogError("[MAX] Load Interstitial video");
            MaxSdk.LoadInterstitial(InterstitialAdUnitId);
        }

        public override void ShowInterstitial(string where, int maxLevel)
        {
            if (interstitialReady)
            {
                Debug.Log("Show Interstitial");
#if EXISTED_FALCON_SDK
                if(FMediation_AdaptSwitcher.enableLogData4Game)
                     LogAds(AdsType.Interstitial, where, maxLevel);
#endif
                MaxSdk.ShowInterstitial(InterstitialAdUnitId);
            }

            //else LoadInterstitial();
        }

        protected override void onInterstitialReady()
        {
            // Not implement
        }

        protected override void onInterstitialClose()
        {
            // Not implement
        }

        private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo info)
        {
            // Interstitial ad is ready to be shown. MaxSdk.IsInterstitialReady(interstitialAdUnitId) will now return 'true'
            // Reset retry attempt
            Debug.Log("Interstitial loaded");
            interstitialRetryAttempt = 0;
        }

        private void OnInterstitialFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            // Interstitial ad failed to load. We recommend retrying with exponentially higher delays up to a maximum delay (in this case 64 seconds).
            interstitialRetryAttempt++;
            int retryDelay = (int) (Math.Pow(2, Math.Min(6, interstitialRetryAttempt)));
            Debug.Log("Interstitial failed to load with error code: " + errorInfo.Code);
            Invoke("LoadInterstitial", retryDelay);
        }

        private void InterstitialFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo info)
        {
            // Interstitial ad failed to display. We recommend loading the next ad
            Debug.Log("Interstitial failed to display with error code: " + errorInfo.Code);
            LoadInterstitial();
        }

        private void OnInterstitialDismissedEvent(string adUnitId, MaxSdkBase.AdInfo info)
        {
            // Interstitial ad is hidden. Pre-load the next ad
            Debug.Log("Interstitial dismissed");
            LoadInterstitial();
            if (isApplyInterBackupForVideo)
            {
                isApplyInterBackupForVideo = false;
                onRewardedVideoSuccess?.Invoke();
                Debug.Log("onRewardedVideoSuccess Invoke");
            }
        }

// Rewarded video
        private void InitializeRewardedAds()
        {
            if (string.IsNullOrEmpty(RewardedAdUnitId))
            {
                Debug.LogError("RewardedAdUnitId can be not null");
                return;
            }
            // Attach callbacks
            MaxSdkCallbacks.OnRewardedAdLoadedEvent += OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.OnRewardedAdLoadFailedEvent += OnRewardedVideoFailed;
            MaxSdkCallbacks.OnRewardedAdFailedToDisplayEvent += OnRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.OnRewardedAdHiddenEvent += OnRewardedAdDismissedEvent;
            MaxSdkCallbacks.OnRewardedAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;

            // Load the first RewardedAd
            LoadRewardedVideo();
        }

        public override bool rewardedVideoReady
        {
            get => MaxSdk.IsRewardedAdReady(RewardedAdUnitId);
            set { }
        }

        private bool rewarded;

        public override void LoadRewardedVideo()
        {
            Debug.LogError("[MAX] Load Reward video");
            MaxSdk.LoadRewardedAd(RewardedAdUnitId);
        }

        public override void ShowRewardedVideo(string where, int maxLevel)
        {
            if (rewardedVideoReady)
            {
                rewarded = false;
                MaxSdk.ShowRewardedAd(RewardedAdUnitId);
#if EXISTED_FALCON_SDK
                if(FMediation_AdaptSwitcher.enableLogData4Game)
                    LogAds(AdsType.RewardedVideo, where, maxLevel);
#endif
            }
            else
            {
                //LoadRewardedVideo();
                onRewardedVideoFail?.Invoke();
            }
        }

        private void OnRewardedAdLoadedEvent(string adUnitId)
        {
            // Rewarded ad is ready to be shown. MaxSdk.IsRewardedAdReady(rewardedAdUnitId) will now return 'true'
            Debug.Log("Rewarded ad loaded");

            // Reset retry attempt
            rewardedRetryAttempt = 0;
            rewardedVideoReady = true;
        }

        protected void OnRewardedAdDismissedEvent(string adUnitId)
        {
            // Rewarded ad is hidden. Pre-load the next ad
            Debug.Log("Rewarded ad dismissed");
            if (!rewarded)
            {
                onRewardedVideoFail?.Invoke();

#if EXISTED_FALCON_SDK
                if(FMediation_AdaptSwitcher.enableLogData4Game)
                    LogManager.UpdateLogAds(AdsStatus.Fail);
#endif
            }

            LoadRewardedVideo();
        }

        protected void OnRewardedVideoFailed(string adUnitId, int errorCode)
        {
            rewardedRetryAttempt++;
            int retryDelay = (int) (Math.Pow(2, Math.Min(6, rewardedRetryAttempt)));

            Debug.Log("Rewarded ad failed to load with error code: " + errorCode);
            Invoke("LoadRewardedVideo", retryDelay);
#if EXISTED_FALCON_SDK
            if(FMediation_AdaptSwitcher.enableLogData4Game)
                LogManager.UpdateLogAds(AdsStatus.Fail);
#endif
            rewardedVideoReady = false;
        }

        private void OnRewardedAdFailedToDisplayEvent(string adUnitId, int errorCode)
        {
            // Rewarded ad failed to display. We recommend loading the next ad
            Debug.Log("Rewarded ad failed to display with error code: " + errorCode);
            LoadRewardedVideo();
        }

        protected override void OnRewardedVideoCheck()
        {
            if (rewarded)
            {
                Debug.Log("Invoke onRewardedSuccess");
                onRewardedVideoSuccess?.Invoke();
#if EXISTED_FALCON_SDK
                if(FMediation_AdaptSwitcher.enableLogData4Game)
                    LogManager.UpdateLogAds(AdsStatus.Success);
#endif
            }
        }

        private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward)
        {
            rewarded = true;
            // Rewarded ad was displayed and user should receive the reward
            Debug.Log("Rewarded ad received reward");
            OnRewardedVideoCheck();
        }
    }
#endif
}