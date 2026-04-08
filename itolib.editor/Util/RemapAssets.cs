using itolib.editor.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace itolib.editor.Util
{
    public static class RemapAssets
    {
        private static readonly Regex gameAsset = new(@"Assets\/LethalCompany\/Game.*\.(?!cs).*");

        private static readonly Dictionary<Regex, string> assetRemaps = [];
        private static readonly HashSet<string> guidsToSkip = [];

        private static UnityEngine.Object? GetAsset(string guid, string assetName)
        {
            if (guidsToSkip.Contains(guid))
            {
                return null;
            }

            string scriptPath = AssetDatabase.GUIDToAssetPath(guid);

            if (!gameAsset.IsMatch(scriptPath))
            {
                _ = guidsToSkip.Add(guid);

                return null;
            }

            return string.Equals(assetName, scriptPath.Split('/')[^1], StringComparison.Ordinal)
                ? AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(scriptPath) : null;
        }

        [MenuItem("Assets/itolib/Util/Legacy/Remap Assets")]
        public static void RemapSelectedAssets()
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
            for (int i = 0; i < projectInformation.assetGuids.Length; i++)
            {
                ProjectInformation.AssetGuid assetInfo = projectInformation.assetGuids[i];
                if (!gameAsset.IsMatch(assetInfo.assetPath))
                {
                    continue;
                }

                // System.Diagnostics.Stopwatch infoStopwatch = System.Diagnostics.Stopwatch.StartNew();

                UnityEngine.Object? asset = null;
                string assetName = assetInfo.assetPath.Split('/')[^1];
                int lastPeriod = assetName.LastIndexOf('.');

                string[] foundAssetGUIDs = AssetDatabase.FindAssets((lastPeriod != -1) ? assetName[..assetName.LastIndexOf('.')] : assetName, ["Assets/LethalCompany/Game"]);
                if (foundAssetGUIDs.Length > 0)
                {
                    if (foundAssetGUIDs.Length == 1)
                    {
                        asset = GetAsset(foundAssetGUIDs[0], assetName);
                    }
                    else
                    {
                        for (int j = 0; j < foundAssetGUIDs.Length; j++)
                        {
                            asset = GetAsset(foundAssetGUIDs[j], assetName);
                            if (asset != null)
                            {
                                break;
                            }
                        }
                    }
                }

                if (asset != null)
                {
                    if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string guid, out long fileID))
                    {
                        Regex regex = new(string.Format(CultureInfo.InvariantCulture, @"fileID:(?!.*100100000).*?{0}", Regex.Escape(assetInfo.originalGuid)));
                        assetRemaps[regex] = $"fileID: {fileID}, guid: {guid}";
                    }
                    else
                    {
                        Debug.LogWarning($"[itolib] GUID and fileID for asset at '{assetInfo.assetPath}' not found! Skipping...");
                    }
                }
                else
                {
                    Debug.LogWarning($"[itolib] Asset at '{assetInfo.assetPath}' not found! Skipping...");
                }

                // infoStopwatch.Stop();
                // Debug.Log($"[itolib] Searching for '{assetName}' took {infoStopwatch.Elapsed} ms. Succeeded: {asset != null}");
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

                foreach (Regex key in assetRemaps.Keys)
                {
                    Debug.Log($"[itolib] Remapping: \"{key}\" -> \"{assetRemaps[key]}\"");

                    assetText = key.Replace(assetText, match =>
                    {
                        remapsMade++;
                        return assetRemaps[key];
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
            assetRemaps.Clear();
            // guidsToSkip.Clear();
        }
    }
}
