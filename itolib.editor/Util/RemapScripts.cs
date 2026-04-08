using itolib.editor.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace itolib.editor.Util
{
    public static class RemapScripts
    {
        private static readonly Dictionary<Regex, string> scriptRemaps = [];
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

        [MenuItem("Assets/itolib/Util/Legacy/Remap Scripts")]
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
                System.Diagnostics.Stopwatch infoStopwatch = System.Diagnostics.Stopwatch.StartNew();

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
                    if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(script, out string guid, out long fileID))
                    {
                        Regex regex = new(string.Format(CultureInfo.InvariantCulture, @"fileID:.*?{0}", Regex.Escape(scriptInfo.originalGuid)));
                        scriptRemaps[regex] = $"fileID: {fileID}, guid: {guid}";
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

                infoStopwatch.Stop();
                Debug.Log($"[itolib] Searching for '{scriptInfo.fullTypeName}' took {infoStopwatch.Elapsed} ms. Succeeded: {script != null}");
            }

            stopwatch.Stop();
            Debug.Log($"[itolib] Parsing JSON file took {stopwatch.Elapsed} ms");

            string[] extensions = ["*.prefab", "*.asset", "*.Unity"];
            HashSet<string> allFiles = [];

            for (int i = 0; i < extensions.Length; i++)
            {
                if (!ReserializeAssets.TryGetFiles(out string[] files, extensions[i], recursive: true))
                {
                    continue;
                }

                allFiles.UnionWith(files);
            }

            if (allFiles == null || allFiles.Count == 0)
            {
                return;
            }

            stopwatch = System.Diagnostics.Stopwatch.StartNew();

            foreach (string file in allFiles)
            {
                int remapsMade = 0;
                string assetText = string.Empty;

                try
                {
                    assetText = File.ReadAllText(file);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[itolib] Could not read asset file: {e}");
                    continue;
                }

                foreach (Regex key in scriptRemaps.Keys)
                {
                    Debug.Log($"[itolib] Remapping: \"{key}\" -> \"{scriptRemaps[key]}\"");

                    assetText = key.Replace(assetText, match =>
                    {
                        remapsMade++;
                        return scriptRemaps[key];
                    });
                }

                if (remapsMade > 0)
                {
                    try
                    {
                        File.WriteAllText(file, assetText);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[itolib] Could not write asset file: {e}");
                    }
                }
            }
            stopwatch.Stop();
            Debug.Log($"[itolib] Remapping took {stopwatch.Elapsed} ms");

            AssetDatabase.Refresh();
            scriptRemaps.Clear();
            // scriptPaths.Clear();
            // guidsToSkip.Clear();
        }
    }
}
