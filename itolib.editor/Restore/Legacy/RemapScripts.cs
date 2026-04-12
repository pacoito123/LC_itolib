using itolib.editor.Data;
using itolib.editor.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace itolib.editor.Restore.Legacy
{
    public static class RemapScripts
    {
        private static readonly Dictionary<string, MonoScript[]> scriptPaths = [];
        private static readonly HashSet<string> guidsToSkip = [];

        private static MonoScript? GetMonoScript(string guid, string typeName)
        {
            if (guidsToSkip.Contains(guid))
            {
                return null;
            }

            string scriptPath = AssetDatabase.GUIDToAssetPath(guid);

            if (!scriptPaths.TryGetValue(scriptPath, out MonoScript[] monoScripts))
            {
                UnityEngine.Object[] assetObjects = AssetDatabase.LoadAllAssetsAtPath(scriptPath);
                HashSet<MonoScript> uniqueMonoScripts = new(assetObjects.Length);

                for (int i = 0; i < assetObjects.Length; i++)
                {
                    if (assetObjects[i] is MonoScript script)
                    {
                        _ = uniqueMonoScripts.Add(script);
                    }
                }

                if (uniqueMonoScripts.Count == 0)
                {
                    _ = guidsToSkip.Add(guid);

                    return null;
                }

                MonoScript[] foundScripts = [.. uniqueMonoScripts];
                if (scriptPaths.TryAdd(scriptPath, foundScripts))
                {
                    monoScripts = foundScripts;
                }
            }

            for (int i = 0; i < monoScripts.Length; i++)
            {
                MonoScript? possibleScript = monoScripts[i];
                if (possibleScript != null && string.Equals(possibleScript.name, typeName, StringComparison.Ordinal))
                {
                    return possibleScript;
                }
            }

            return null;
        }

        [MenuItem("Assets/itolib/Restore/Legacy/Remap Scripts")]
        public static void RemapSelectedScripts()
        {
            string projectInfoPath = EditorUtility.OpenFilePanel("Select Extracted Project Information", string.Empty, "json");
            if (string.IsNullOrEmpty(projectInfoPath))
            {
                return;
            }

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

            string json = File.ReadAllText(projectInfoPath);
            ProjectInformation projectInformation = JsonUtility.FromJson<ProjectInformation>(json);

            stopwatch.Stop();
            Debug.Log($"[itolib] Opening JSON file took {stopwatch.Elapsed} ms");

            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < projectInformation.guids.Length; i++)
            {
                float progress = i / (float)projectInformation.guids.Length;
                EditorUtility.DisplayProgressBar("[itolib]", $"Parsing scripts from JSON file... {i}/{projectInformation.guids.Length} ({progress * 100}%)", progress);

                ProjectInformation.Guid scriptInfo = projectInformation.guids[i];
                MonoScript? script = null;

                string typeName = scriptInfo.fullTypeName.Split('.')[^1];
                string[] foundScriptGUIDs = AssetDatabase.FindAssets(typeName);
                if (foundScriptGUIDs.Length > 0)
                {
                    if (foundScriptGUIDs.Length == 1)
                    {
                        script = GetMonoScript(foundScriptGUIDs[0], typeName);
                    }
                    else
                    {
                        MonoScript[] foundScripts = [.. foundScriptGUIDs.Select(guid => GetMonoScript(guid, typeName)!)
                            .Where(script => script != null).OrderBy(x => x.GetInstanceID())];

                        if (foundScripts.Length > 0)
                        {
                            script = foundScripts[0];
                            Debug.LogWarning($"[itolib] Found {foundScripts.Length} scripts for type '{scriptInfo.fullTypeName}'. Using '{script.name}' from '{script.GetAssemblyName()}' with instance ID: {script.GetInstanceID()}");
                        }
                    }
                }

                if (script != null)
                {
                    if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(script, out string guid, out long fileId))
                    {
                        RemapUtils.AddRemap(scriptInfo.originalGuid, guid, fileId);
                    }
                    else
                    {
                        Debug.LogWarning($"[itolib] GUID and fileID for type '{scriptInfo.fullTypeName}' not found! Skipping...");
                    }
                }
                else
                {
                    Debug.LogWarning($"[itolib] Type '{scriptInfo.fullTypeName}' not found! Skipping...");
                }
            }

            stopwatch.Stop();
            Debug.Log($"[itolib] Parsing JSON file took {stopwatch.Elapsed} ms");
            EditorUtility.ClearProgressBar();

            if (!FileUtils.TryGetSelectedFiles(out string[] files, "*.prefab;*.asset;*.Unity", recursive: true))
            {
                RemapUtils.ClearRemaps();
                return;
            }

            RemapUtils.RemapFiles(files);
        }
    }
}
