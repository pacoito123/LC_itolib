using itolib.editor.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace itolib.editor.Util
{
    public static class RemapAssets
    {
        private static readonly Regex assetReference = new(@"fileID:(?!.*100100000).*guid:[^,]*", GameAssets.regexOptions);
        private static readonly Dictionary<string, string> assetRemaps = [];

        [MenuItem("Assets/itolib/Util/Legacy/Remap Assets")]
        public static void RemapSelectedAssets()
        {
            string projectInfoPath = EditorUtility.OpenFilePanel("Select Extracted Project Information", string.Empty, "json");
            if (string.IsNullOrEmpty(projectInfoPath))
            {
                return;
            }

            GameAssets.BuildGameFileCache();

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

            string json = File.ReadAllText(projectInfoPath);
            ProjectInformation projectInformation = JsonUtility.FromJson<ProjectInformation>(json);

            stopwatch.Stop();
            Debug.Log($"[itolib] Opening JSON file took {stopwatch.Elapsed} ms");

            stopwatch = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 0; i < projectInformation.assetGuids.Length; i++)
            {
                float progress = i / (float)projectInformation.assetGuids.Length;
                EditorUtility.DisplayProgressBar("[itolib]", $"Parsing assets from JSON file... {i}/{projectInformation.assetGuids.Length} ({progress * 100}%)", progress);

                ProjectInformation.AssetGuid assetInfo = projectInformation.assetGuids[i];
                if (assetInfo.assetPath.EndsWith(".cs", StringComparison.InvariantCultureIgnoreCase)
                    || assetInfo.assetPath.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                if (GameAssets.TryFindAsset(assetInfo.assetPath, out string guid, out long fileID))
                {
                    assetRemaps[assetInfo.originalGuid] = $"fileID: {fileID}, guid: {guid}";
                }
            }

            stopwatch.Stop();
            Debug.Log($"[itolib] Parsing JSON file took {stopwatch.Elapsed} ms");
            EditorUtility.ClearProgressBar();

            if (!FileUtils.TryGetSelectedFiles(out string[] files, "*.prefab;*.asset;*.Unity", recursive: true))
            {
                assetRemaps.Clear();
                return;
            }

            stopwatch = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 0; i < files.Length; i++)
            {
                float progress = i / (float)files.Length;
                EditorUtility.DisplayProgressBar("[itolib]", $"Remapping assets... {i}/{files.Length} ({progress * 100}%)", progress);

                string filePath = files[i];
                if (string.IsNullOrEmpty(filePath))
                {
                    continue;
                }

                string[] lines;
                try
                {
                    lines = File.ReadAllLines(filePath);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[itolib] Could not read file '{filePath}': {e}");
                    continue;
                }

                int remapsDone = 0;
                for (int j = 0; j < lines.Length; j++)
                {
                    string line = lines[j];
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }

                    ReadOnlySpan<char> span = line.AsSpan();
                    Match guidLineMatch = assetReference.Match(line);
                    if (guidLineMatch.Success)
                    {
                        string guid = guidLineMatch.Value.Split("guid: ")[^1];
                        if (assetRemaps.TryGetValue(guid, out string replacement))
                        {
                            string left = span[..guidLineMatch.Index].ToString(),
                                right = span[(guidLineMatch.Index + guidLineMatch.Length)..].ToString();

                            lines[j] = left + replacement + right;
                            remapsDone++;
                        }
                    }
                }
                if (remapsDone > 0)
                {
                    try
                    {
                        File.WriteAllLines(filePath, lines);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[itolib] Could not write file '{filePath}': {e}");
                    }
                }
            }
            stopwatch.Stop();
            Debug.Log($"[itolib] Remapping took {stopwatch.Elapsed} ms");
            EditorUtility.ClearProgressBar();

            AssetDatabase.Refresh();
            assetRemaps.Clear();
            // guidsToSkip.Clear();
        }
    }
}
