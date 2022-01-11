#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace FMediation
{
    public static class FMediation_Utils
    {
        private static readonly BuildTargetGroup[] AllPlatform =
            {BuildTargetGroup.Android, BuildTargetGroup.iOS};

        /// <summary>
        /// Add the scripting define symbol on the platform if it not exists.
        /// </summary>
        /// <param name="symbol">Symbol.</param>
        /// <param name="Platform">Platform.</param>
        public static string AddDefine(string symbol)
        {
            foreach (var platform in AllPlatform)
            {
                string symbolStr = PlayerSettings.GetScriptingDefineSymbolsForGroup(platform);
                List<string> symbols = new List<string>(symbolStr.Split(';'));
                Debug.Log(symbolStr);
                if (!symbols.Contains(symbol))
                {
                    Debug.Log("Add scripting define: " + symbol);
                    symbols.Add(symbol);

                    StringBuilder sb = new StringBuilder();

                    for (int i = 0; i < symbols.Count; i++)
                    {
                        sb.Append(symbols[i]);
                        if (i < symbols.Count - 1)
                            sb.Append(";");
                    }

                    PlayerSettings.SetScriptingDefineSymbolsForGroup(platform, sb.ToString());
                }
            }

            return "ADD_DEFINE";
        }

        /// <summary>
        /// Removes the scripting define symbol on the platform if it exists.
        /// </summary>
        /// <param name="symbol">Symbol.</param>
        /// <param name="Platform">Platform.</param>
        public static string RemoveDefine(string symbol)
        {
            foreach (var platform in AllPlatform)
            {
                string symbolStr = PlayerSettings.GetScriptingDefineSymbolsForGroup(platform);
                List<string> symbols = new List<string>(symbolStr.Split(';'));

                if (symbols.Contains(symbol))
                {
                    Debug.Log("Remove scripting define: " + symbol);
                    symbols.Remove(symbol);

                    StringBuilder settings = new StringBuilder();

                    for (int i = 0; i < symbols.Count; i++)
                    {
                        settings.Append(symbols[i]);
                        if (i < symbols.Count - 1)
                            settings.Append(";");
                    }

                    PlayerSettings.SetScriptingDefineSymbolsForGroup(platform, settings.ToString());
                }
            }

            return "REMOVE_DEFINE";
        }
    }

    public class FMediation_EditorCorountine
    {
        readonly IEnumerator mRoutine;

        public static FMediation_EditorCorountine StartEditorCoroutine(IEnumerator routine)
        {
            FMediation_EditorCorountine coroutine = new FMediation_EditorCorountine(routine);
            coroutine.start();
            return coroutine;
        }

        FMediation_EditorCorountine(IEnumerator routine)
        {
            mRoutine = routine;
        }

        void start()
        {
            EditorApplication.update += update;
        }

        void update()
        {
            if (!mRoutine.MoveNext())
            {
                StopEditorCoroutine();
            }
        }

        public void StopEditorCoroutine()
        {
            EditorApplication.update -= update;
        }
    }
}
#endif