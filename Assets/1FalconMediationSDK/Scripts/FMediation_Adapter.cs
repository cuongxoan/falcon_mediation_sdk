using System;
using System.Collections.Generic;
#if EXISTED_FALCON_SDK
using FalconSDK;
#endif
using UnityEngine;

namespace FMediation
{
    public enum BannerPosition
    {
        TOP, BOTTOM
    }
    public abstract class FMediation_Adapter : MonoBehaviour
    {
        protected BannerPosition BannerPosition;
        public abstract void Init(bool isShowInterBackupForVideoNotAvailable, BannerPosition bannerPosition);

        public bool isApplyInterBackupForVideo;
        public abstract bool isShowInterBackupForVideoNotAvailable { get; set; }

// Banner
        public abstract bool bannerReady { get; set; }
        public static Color bannerBackgroundColor;
        public abstract void LoadBanner();

        public abstract void ShowBanner();

        public abstract void HideBanner();

// Interstitials
        public abstract bool interstitialReady { get; set; }

        public abstract void LoadInterstitial();

        public abstract void ShowInterstitial(string where, int maxLevel);

        protected abstract void onInterstitialReady(); // Loaded
        protected abstract void onInterstitialClose();

// Rewarded video
        public abstract bool rewardedVideoReady { get; set; }

        public Action onRewardedVideoSuccess { get; set; }
        public Action onRewardedVideoFail { get; set; }

        public abstract void LoadRewardedVideo();

        public abstract void ShowRewardedVideo(string where, int maxLevel);

        protected abstract void OnRewardedVideoCheck();

#if EXISTED_FALCON_SDK
        protected string sessionId;

        protected void LogAds(AdsType type, string where, int maxLevel)
        {
            Debug.Log("Log ads " + type.ToString());
            sessionId = Guid.NewGuid().ToString();
            AdsInformation adsInfo = null;
            try
            {
                adsInfo = MediationInfo.GetAvailableAdsInformation();
            }
            catch (Exception e)
            {
                Debug.LogError("Exception get infor mediation : " + e.StackTrace);
            }

            var infoNotNull = adsInfo != null;
            var id = infoNotNull && adsInfo.Id != null ? adsInfo.Id : "";
            var mediation = infoNotNull && adsInfo.Detail != null ? adsInfo.Detail : "";
            var name = infoNotNull && adsInfo.Name != null ? adsInfo.Name : "";
            try
            {
                LogManager.LogAds(type, id, mediation, name, where, maxLevel);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Exception logads : " + e.StackTrace);
            }
        }

#endif
    }
}