using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
#if UNITY_EDITOR
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor;

#endif

namespace FMediation
{
    public enum mediation
    {
        NONE,
        IRON_SOURCE,
        MAX,
        MOPUB
    }
#if UNITY_EDITOR
    [InitializeOnLoad]
    public class FMediation_EditorMenuItem
    {
        private static Assembly _asEditor;
        static FMediation_EditorMenuItem()
        {
            AssetDatabase.importPackageStarted += OnImportPackageStarted;
            AssetDatabase.importPackageCompleted += OnImportPackageCompleted;
            AssetDatabase.importPackageFailed += OnImportPackageFailed;
            AssetDatabase.importPackageCancelled += OnImportPackageCancelled;
            //EditorApplication.update += CheckPlugins;
        }

        public static void CheckPlugins()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name == "Assembly-CSharp-Editor")
                {
                    _asEditor = assembly;
                }
            }
            if (_asEditor != null)
            {
                bool existedIronsource = _asEditor.GetType("IronSourceMenu") != null;
                bool existedMax = _asEditor.GetType("AppLovinIntegrationManager") != null;
                bool existedFalconSDK = _asEditor.GetType("FSDK_UpdateLoader") != null;
                if (existedIronsource)
                    FMediation_EditorMenuItem.AddMediationDefine(mediation.IRON_SOURCE);
                else
                    FMediation_EditorMenuItem.RemoveMediationDefine(mediation.IRON_SOURCE);
                if (existedMax)
                    FMediation_EditorMenuItem.AddMediationDefine(mediation.MAX);
                else
                    FMediation_EditorMenuItem.RemoveMediationDefine(mediation.MAX);
                if (existedFalconSDK)
                    FMediation_Utils.AddDefine("EXISTED_FALCON_SDK");
                else FMediation_Utils.RemoveDefine("EXISTED_FALCON_SDK");
            }
            EditorApplication.update -= CheckPlugins;
        }

        // [MenuItem("Falcon Mediation/Integration/Remove MAX Mediation")]
        public static void RemoveMAXMediation()
        {
            RemoveMediation(mediation.MAX);
        }

        // [MenuItem("Falcon Mediation/Integration/Remove Ironsource Mediation")]
        public static void RemoveIronsourceMediation()
        {
            RemoveMediation(mediation.IRON_SOURCE);
        }

        // [MenuItem("Falcon Mediation/Integration/Remove MoPub Mediation")]
        public static void RemoveMoPubMediation()
        {
            RemoveMediation(mediation.MOPUB);
        }

        // [MenuItem("Falcon Mediation/Integration/Add MAX Mediation")]
        public static void AddMAXMediation()
        {
            Debug.Log("Click download MAX");
            if (CheckContainMediation(mediation.IRON_SOURCE))
            {
                Debug.Log("Already exist Ironsource mediation in this project");
                return;
            }
            else if (CheckContainMediation(mediation.MAX))
            {
                Debug.Log("Already exist MAX mediation in this project");
                return;
            }
            else if (CheckContainMediation(mediation.MOPUB))
            {
                Debug.Log("Already exist MoPub mediation in this project");
                return;
            }

            FMediation_EditorCorountine.StartEditorCoroutine(DownloadFile(mediation.MAX, "http://35.240.144.46/sdk/unity-packages/fmediation/MAX_IntegrationManager_{0}.unitypackage", "v1",
                () => AddMediationDefine(mediation.MAX))); //http://35.240.144.46/sdk/unity-packages/fsdk/
        }

        // [MenuItem("Falcon Mediation/Integration/Add Ironsource Mediation")]
        public static void AddIronsourceMediation(string version)
        {
            Debug.Log("Click download Ironsource");
            if (CheckContainMediation(mediation.IRON_SOURCE))
            {
                Debug.Log("Already exist Ironsource mediation in this project");
                return;
            }
            else if (CheckContainMediation(mediation.MAX))
            {
                Debug.Log("Already exist MAX mediation in this project");
                return;
            }
            else if (CheckContainMediation(mediation.MOPUB))
            {
                Debug.Log("Already exist Mopub mediation in this project");
                return;
            }

            string newIronsrouceVerion = PlayerPrefs.GetString("editor_ironsource_newversion");
            
            FMediation_EditorCorountine.StartEditorCoroutine(DownloadFile(mediation.IRON_SOURCE, "https://github.com/ironsource-mobile/Unity-sdk/raw/master/{0}/IronSource_IntegrationManager_v{0}.unitypackage", version,
                () => AddMediationDefine(mediation.IRON_SOURCE)));
        }

        // [MenuItem("Falcon Mediation/Integration/Add Mopub Mediation")]
        public static void AddMopubMediation()
        {
            Debug.Log("Click download Mopub");
            if (CheckContainMediation(mediation.IRON_SOURCE))
            {
                Debug.Log("Already exist Ironsource mediation in this project");
                return;
            }
            else if (CheckContainMediation(mediation.MAX))
            {
                Debug.Log("Already exist MAX mediation in this project");
                return;
            }
            else if (CheckContainMediation(mediation.MOPUB))
            {
                Debug.Log("Already exist Mopub mediation in this project");
                return;
            }

            FMediation_EditorCorountine.StartEditorCoroutine(DownloadFile(mediation.IRON_SOURCE, "https://github.com/mopub/mopub-unity-sdk/releases/download/{0}/MoPubUnity.unitypackage", "v5.16.4",
                () => AddMediationDefine(mediation.MOPUB)));
        }


        public static void AddMediationDefine(mediation mediation)
        {
            string define = $"EXISTED_{mediation}";
            // if (mediation == mediation.IRON_SOURCE)
            // {
            //     FMediation_Utils.RemoveDefine($"EXISTED_{mediation.MAX.ToString()}");
            //     FMediation_Utils.RemoveDefine($"EXISTED_{mediation.MOPUB.ToString()}");
            // }
            // else if (mediation == mediation.MAX)
            // {
            //     FMediation_Utils.RemoveDefine($"EXISTED_{mediation.IRON_SOURCE.ToString()}");
            //     FMediation_Utils.RemoveDefine($"EXISTED_{mediation.MOPUB.ToString()}");
            // }
            // else if (mediation == mediation.MOPUB)
            // {
            //     FMediation_Utils.RemoveDefine($"EXISTED_{mediation.MAX.ToString()}");
            //     FMediation_Utils.RemoveDefine($"EXISTED_{mediation.IRON_SOURCE.ToString()}");
            // }

            FMediation_Utils.AddDefine(define);
            // PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "F_" + mediation.ToString().ToUpper() + "_ENABLE");
        }

        public static void RemoveMediationDefine(mediation mediation)
        {
            if (mediation == mediation.IRON_SOURCE)
            {
                FMediation_Utils.RemoveDefine($"EXISTED_{mediation.IRON_SOURCE.ToString()}");
            }
            else if (mediation == mediation.MAX)
            {
                FMediation_Utils.RemoveDefine($"EXISTED_{mediation.MAX.ToString()}");
            }
            else if (mediation == mediation.MOPUB)
            {
                FMediation_Utils.RemoveDefine($"EXISTED_{mediation.MOPUB.ToString()}");
            }

            // PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "F_" + mediation.ToString().ToUpper() + "_ENABLE");
        }


        static void RemoveMediation(mediation mediation)
        {
            if (mediation == mediation.IRON_SOURCE)
            {
                FileUtil.DeleteFileOrDirectory(@"Assets/Ironsource");
                FileUtil.DeleteFileOrDirectory(@"Assets/Ironsource.meta");
                FMediation_Utils.RemoveDefine($"EXISTED_{mediation.IRON_SOURCE}");
                AssetDatabase.Refresh();
            }

            if (mediation == mediation.MAX)
            {
                FileUtil.DeleteFileOrDirectory(@"Assets/MaxSdk");
                FileUtil.DeleteFileOrDirectory(@"Assets/MaxSdk.meta");
                FMediation_Utils.RemoveDefine($"EXISTED_{mediation.MAX}");
                AssetDatabase.Refresh();
            }

            if (mediation == mediation.MOPUB)
            {
                FileUtil.DeleteFileOrDirectory(@"Assets/MoPub");
                FileUtil.DeleteFileOrDirectory(@"Assets/MoPub.meta");
                FMediation_Utils.RemoveDefine($"EXISTED_{mediation.MOPUB}");
                AssetDatabase.Refresh();
            }
        }

        static IEnumerator DownloadFile(mediation mediation, string url, string version, Action oncomplete)
        {
            Debug.Log("Start download");
            string fullPath = string.Format(url, version);
            string fileName = mediation.ToString() + "_" + version + ".unitypackage";
            string packagePath = Application.dataPath + "/1FalconMediationSDK/Package/";

            UnityWebRequest webRequest = UnityWebRequest.Get(fullPath);

            webRequest.SendWebRequest();

            Debug.Log("Download " + fullPath);

            if (!webRequest.isHttpError && !webRequest.isNetworkError)
            {
                while (!webRequest.isDone)
                {
                    yield return new WaitForSeconds(0.1f);
                    if (EditorUtility.DisplayCancelableProgressBar("Download Manager", "Download " + mediation.ToString(), webRequest.downloadProgress))
                    {
                        if (webRequest.error != null)
                        {
                            Debug.LogError(webRequest.error);
                            yield break;
                        }
                    }
                }
            }

            while (!webRequest.isDone)
            {
                EditorUtility.DisplayProgressBar("Downloading", "Downloading " + fileName, webRequest.downloadProgress);
            }

            EditorUtility.ClearProgressBar();

            if (webRequest.isHttpError)
            {
                Debug.LogError("URL " + fullPath + " Error, Please Report For Us");
                yield break;
            }

            if (webRequest.isNetworkError)
            {
                Debug.LogError("Please Check Your Internet And Try Again");
                yield break;
            }

            byte[] package = webRequest.downloadHandler.data;

            Debug.Log(AssetDatabase.IsValidFolder($"Assets/1FalconMediationSDK/Package"));
            if (!AssetDatabase.IsValidFolder($"Assets/1FalconMediationSDK/Package"))
            {
                AssetDatabase.CreateFolder("Assets/1FalconMediationSDK", "Package");
                AssetDatabase.Refresh();
            }

            File.WriteAllBytes(packagePath + fileName, package);

            yield return null;
            AssetDatabase.ImportPackage(packagePath + fileName, true);

            oncomplete?.Invoke();
            Debug.Log($"Import {fileName} completed");
        }

        private static void OnImportPackageCancelled(string packageName)
        {
            CheckPlugins();
            Debug.Log($"Cancelled the import of package: {packageName}");
        }

        private static void OnImportPackageCompleted(string packagename)
        {
            CheckPlugins();
            Debug.Log($"Imported package: {packagename}");
        }

        private static void OnImportPackageFailed(string packagename, string errormessage)
        {
            CheckPlugins();
            Debug.Log($"Failed importing package: {packagename} with error: {errormessage}");
        }

        private static void OnImportPackageStarted(string packagename)
        {
            Debug.Log($"Started importing package: {packagename}");
        }

        static bool CheckContainMediation(mediation medation)
        {
#if EXISTED_IRON_SOURCE
            if (medation == mediation.IRON_SOURCE)
            {
                return true;
            }
#elif EXISTED_MAX
            if (medation == mediation.MAX)
            {
                return true;
            }
#endif

            return false;
        }
    }
#endif
}