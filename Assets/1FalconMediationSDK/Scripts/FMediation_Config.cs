using System;
using System.IO;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FMediation
{
    public class FMediation_Config
    {
        public const string version = "v2.7";
        [System.Serializable]
        public class config
        {
            public string fmediationVersion = version;
            public bool enableLogData4Game = false;
            
            public ironsourceConfig ironsource = new ironsourceConfig();
            public maxConfig max = new maxConfig();
        }

        [System.Serializable]
        public class ironsourceConfig
        {
            public string ironsource_key_android = "";
            public string ironsource_key_ios = "";

            public ironsourceConfig clone()
            {
                var clone = new ironsourceConfig();
                clone.ironsource_key_android = ironsource_key_android;
                clone.ironsource_key_ios = ironsource_key_ios;
                return clone;
            }
        }

        [System.Serializable]
        public class maxConfig
        {
            public string MaxSdkKey = "";
            public string InterstitialAdUnitId = "";
            public string RewardedAdUnitId = "";
            public string RewardedInterstitialAdUnitId = "";
            public string BannerAdUnitId = "";
            public string MRecAdUnitId = "";

            public maxConfig clone()
            {
                var clone = new maxConfig();
                clone.MaxSdkKey = MaxSdkKey;
                clone.InterstitialAdUnitId = InterstitialAdUnitId;
                clone.RewardedAdUnitId = RewardedAdUnitId;
                clone.RewardedInterstitialAdUnitId = RewardedInterstitialAdUnitId;
                clone.BannerAdUnitId = BannerAdUnitId;
                clone.MRecAdUnitId = MRecAdUnitId;
                return clone;
            }
        }

        private const string configPath = "Assets/1FalconMediationSDK/Resources/falcon_mediation_config.txt";

        private static config _config;

        static config ReadConfig()
        {
            bool isCreateNewConfig = false;
            if (File.Exists(configPath))
            {
                using (StreamReader file = new StreamReader(configPath))
                {
                    string txt = file.ReadToEnd();
                    if (!string.IsNullOrEmpty(txt))
                    {
                        _config = JsonUtility.FromJson<config>(txt);
                    }
                    else
                    {
                        _config = new config();
                        isCreateNewConfig = true;
                    }
                }

                if (isCreateNewConfig || _config.fmediationVersion != version)
                    WriteConfig();
            }
            else
            {
                _config = new config();
                WriteConfig();
            }

            return _config;
        }

        static void WriteConfig()
        {
            if (_config == null)
            {
                Debug.LogError("Config null");
                return;
            }

            _config.fmediationVersion = version;
            using (StreamWriter file = new StreamWriter(configPath))
            {
                file.WriteLine(JsonUtility.ToJson(_config));
            }

#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }

        public static config ValidateConfig()
        {
            return ReadConfig();
        }

        public static string GetIronsourceVersion()
        {
            
#if EXISTED_IRON_SOURCE
            var type = typeof(IronSource);
            var version = type.GetField("UNITY_PLUGIN_VERSION", BindingFlags.Static | BindingFlags.NonPublic);
            return version.GetValue(null).ToString();
#else
            return "none";
#endif
        }

        public static ironsourceConfig GetIronsourceConfig()
        {
            if (_config == null) ReadConfig();
            return _config.ironsource.clone();
        }

        public static maxConfig GetMaxConfig()
        {
            if (_config == null) ReadConfig();
            return _config.max.clone();
        }

        public static void SetLogData4Game(bool enable)
        {
            _config.enableLogData4Game = enable;
            WriteConfig();
        }
        public static void SetIronsourceConfig(ironsourceConfig ironsource)
        {
            if (_config == null) ReadConfig();
            _config.ironsource = ironsource;
            WriteConfig();
        }

        public static void SetMaxConfig(maxConfig max)
        {
            if (_config == null) ReadConfig();
            _config.max = max;
            WriteConfig();
        }

        public static void SetSdkVersion(string version)
        {
            if (_config == null) ReadConfig();
            _config.fmediationVersion = version;
            WriteConfig();
        }

        public static void ConfigIronsource(string ironsource_key_android, string ironsource_key_ios)
        {
            if (_config == null) ReadConfig();
            _config.ironsource.ironsource_key_android = ironsource_key_android;
            _config.ironsource.ironsource_key_ios = ironsource_key_ios;
            WriteConfig();
        }

        public static void ConfigMAX(string MaxSdkKey, string InterstitialAdUnitId, string RewardedAdUnitId, string RewardedInterstitialAdUnitId, string BannerAdUnitId, string MRecAdUnitId)
        {
            if (_config == null) ReadConfig();
            _config.max.MaxSdkKey = MaxSdkKey;
            _config.max.InterstitialAdUnitId = InterstitialAdUnitId;
            _config.max.RewardedAdUnitId = RewardedAdUnitId;
            _config.max.RewardedInterstitialAdUnitId = RewardedInterstitialAdUnitId;
            _config.max.BannerAdUnitId = BannerAdUnitId;
            _config.max.MRecAdUnitId = MRecAdUnitId;
            WriteConfig();
        }
    }
}