using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace itolib.editor.Util
{
    internal static class ScriptUtils
    {
        private static readonly Dictionary<string, (string, long, int)> scriptCache = [];

        internal static void BuildGameScriptCache()
        {
            if (scriptCache.Count > 0)
            {
                return;
            }

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

            string[] possibleScripts = AssetDatabase.FindAssets("t:MonoScript", ["Assets"]);

            stopwatch.Stop();
            Debug.Log($"[itolib] Getting project scripts ({possibleScripts.Length}) took {stopwatch.Elapsed} ms");

            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < possibleScripts.Length; i++)
            {
                float progress = i / (float)possibleScripts.Length;
                EditorUtility.DisplayProgressBar("[itolib]", $"Creating game script cache... {i}/{possibleScripts.Length} ({progress * 100}%)", progress);

                string scriptPath = AssetDatabase.GUIDToAssetPath(possibleScripts[i]);

                UnityEngine.Object[] scriptObjects = AssetDatabase.LoadAllAssetsAtPath(scriptPath);
                HashSet<MonoScript> uniqueMonoScripts = new(scriptObjects.Length);

                for (int j = 0; j < scriptObjects.Length; j++)
                {
                    if (scriptObjects[j] is MonoScript script)
                    {
                        _ = uniqueMonoScripts.Add(script);
                    }
                }

                foreach (MonoScript script in uniqueMonoScripts)
                {
                    int scriptInstanceId = script.GetInstanceID();

                    Type scriptType = script.GetClass();
                    if (scriptType == null)
                    {
                        continue;
                    }

                    if (scriptCache.TryGetValue(scriptType.FullName, out (string, long, int) existingEntry)
                        && scriptInstanceId > existingEntry.Item3) // Assumes a lower ID is prioritized for duplicate scripts...
                    {
                        continue;
                    }

                    if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(script, out string scriptGuid, out long scriptFileId))
                    {
                        Debug.LogWarning($"[itolib] Could not find fileID for script '{scriptType.FullName}'! Skipping...");
                        continue;
                    }

                    (string, long, int) scriptEntry = (scriptGuid, scriptFileId, scriptInstanceId);
                    scriptCache[scriptType.FullName] = scriptEntry;
                }
            }

            stopwatch.Stop();
            Debug.Log($"[itolib] Preparing project files took {stopwatch.Elapsed} ms");
            EditorUtility.ClearProgressBar();
        }

        internal static bool TryFindScript(string scriptFullName, out string guid, out long fileId)
        {
            guid = string.Empty;
            fileId = default;

            if (!scriptCache.TryGetValue(scriptFullName, out (string, long, int) entry))
            {
                return false;
            }

            guid = entry.Item1;
            fileId = entry.Item2;

            return true;
        }
    }
}