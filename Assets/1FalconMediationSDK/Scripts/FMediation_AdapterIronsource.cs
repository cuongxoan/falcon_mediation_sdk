using UnityEngine;

namespace FMediation
{
    #if EXISTED_IRON_SOURCE
    public class FMediation_AdapterIronsource : FMediation_Adapter
    {
        private string ironsource_key_android;
        private string ironsource_key_ios;
        
        private FMediation_AdapterIronsource(){}
        public FMediation_AdapterIronsource Setup(string ironsourceKeyAndroid, string ironsourceKeyIOS)
        {
            this.ironsource_key_android = ironsourceKeyAndroid;
            this.ironsource_key_ios = ironsourceKeyIOS;
            return this;
        }
        public override void Init(bool isShowInterBackupForVideoNotAvailable, BannerPosition bannerPosition)
        {
            this.isShowInterBackupForVideoNotAvailable = isShowInterBackupForVideoNotAvailable;
            this.BannerPosition = bannerPosition;
#if UNITY_ANDROID
            IronSource.Agent.init(ironsource_key_android, IronSourceAdUnits.REWARDED_VIDEO, IronSourceAdUnits.INTERSTITIAL, IronSourceAdUnits.BANNER);
#elif UNITY_IPHONE
            IronSource.Agent.init(ironsource_key_ios, IronSourceAdUnits.REWARDED_VIDEO, IronSourceAdUnits.INTERSTITIAL, IronSourceAdUnits.BANNER);
#endif

            IronSource.Agent.validateIntegration();
            IronSource.Agent.loadInterstitial();
            IronSourceEvents.onBannerAdLoadedEvent += OnBannerLoaded;
            IronSourceEvents.onBannerAdLoadFailedEvent += (IronSourceError error) => OnBannerLoadFailed();
            IronSourceEvents.onInterstitialAdReadyEvent += onInterstitialReady;
            IronSourceEvents.onInterstitialAdClosedEvent += onInterstitialClose;

            IronSourceEvents.onRewardedVideoAdClosedEvent += OnRewardedVideoClosed;
            IronSourceEvents.onRewardedVideoAdRewardedEvent += OnRewardedVideoRewarded;

            Debug.Log("[IronsourceAdapter] InitComplete");
        }

        public override bool isShowInterBackupForVideoNotAvailable { get; set; }

        // Banner
        public override bool bannerReady { get; set; }

        
        public void OnBannerLoaded()
        {
            bannerReady = true;
        }
        public void OnBannerLoadFailed()
        {
            bannerReady = false;
        }

        public override void LoadBanner()
        {
            Debug.Log("[IronsourceAdapter] Load banner");
            if(this.BannerPosition == BannerPosition.TOP)
                IronSource.Agent.loadBanner(IronSourceBannerSize.SMART, IronSourceBannerPosition.TOP);
            else if(this.BannerPosition == BannerPosition.BOTTOM)
                IronSource.Agent.loadBanner(IronSourceBannerSize.SMART, IronSourceBannerPosition.BOTTOM);
        }

        public override void ShowBanner()
        {
            Debug.Log("[IronsourceAdapter] Show banner");
            LoadBanner();
            IronSource.Agent.displayBanner();
        }

        public override void HideBanner()
        {
            Debug.Log("[IronsourceAdapter] Hide banner");
            IronSource.Agent.hideBanner();
        }
    
// Interstitials
        public override bool interstitialReady
        {
            get => IronSource.Agent.isInterstitialReady();
            set { }
        }

        public override void LoadInterstitial()
        {
            Debug.Log("[IronsourceAdapter] Load Interstitial");
            IronSource.Agent.loadInterstitial();
        }

        public override void ShowInterstitial(string where, int maxLevel)
        {
            if (interstitialReady)
            {
                Debug.Log("[IronsourceAdapter] Show Interstitial");
                #if EXISTED_FALCON_SDK
                if(FMediation_AdaptSwitcher.enableLogData4Game)
                    LogAds(AdsType.Interstitial, where, maxLevel);
#endif
                IronSource.Agent.showInterstitial();
            }
            else LoadInterstitial();
        }

        protected override void onInterstitialReady()
        {
            interstitialReady = true;
        }

        protected override void onInterstitialClose()
        {
            if (isApplyInterBackupForVideo)
            {
                isApplyInterBackupForVideo = false;
                onRewardedVideoSuccess?.Invoke();
                Debug.Log("onRewardedVideoSuccess Invoke");
            }
        }

        // Rewarded video
        public override bool rewardedVideoReady { get => IronSource.Agent.isRewardedVideoAvailable();
            set { }
        }
        private bool rewarded;

        public override void LoadRewardedVideo()
        {                
            Debug.Log("[IronsourceAdapter] Ironsource automatic load reward video");
        }

        public override void ShowRewardedVideo(string where, int maxLevel)
        {
            if (rewardedVideoReady)
            {
                Debug.Log("[IronsourceAdapter] Show Rewarded video");
                #if EXISTED_FALCON_SDK
                if(FMediation_AdaptSwitcher.enableLogData4Game)
                    LogAds(AdsType.RewardedVideo, where, maxLevel);
#endif
                IronSource.Agent.showRewardedVideo();
            }
        }

        void OnRewardedVideoRewarded(IronSourcePlacement placement)
        {
            rewarded = true;
        }

        protected void OnRewardedVideoClosed()
        {
            OnRewardedVideoCheck();
        }

        protected override void OnRewardedVideoCheck(){
            if (rewarded)
            {
                rewarded = false;
                onRewardedVideoSuccess?.Invoke();
                #if EXISTED_FALCON_SDK
                if(FMediation_AdaptSwitcher.enableLogData4Game)
                    LogManager.UpdateLogAds(AdsStatus.Success);
#endif
            }
            else
            {
                onRewardedVideoFail?.Invoke();
                #if EXISTED_FALCON_SDK
                if(FMediation_AdaptSwitcher.enableLogData4Game)
                    LogManager.UpdateLogAds(AdsStatus.Fail);
#endif
            }
            onRewardedVideoSuccess = null;
            onRewardedVideoFail = null;
        }

        public bool interstitialRewardedVideoReady { get => false;
            set { }
        }
    }
#endif
}