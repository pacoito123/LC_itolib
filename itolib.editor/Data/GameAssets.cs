using itolib.editor.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace itolib.editor.Data
{
    internal static class GameAssets
    {
        internal const RegexOptions regexOptions = RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.CultureInvariant;

        private static readonly Regex gameAssetScriptOrScene = new(@"Assets(\/|\\)LethalCompany(\/|\\)Game(\/|\\)(?=Scripts|Scenes|Plugins).*", regexOptions);
        private static readonly Regex metaDirOrScript = new(@"(^|\/|\\)([^\.]*|(.*\.cs)|(.*\.dll)|(.*\.mixer))\.meta", regexOptions);
        private static readonly Regex metaPath = new(@"Assets(\/|\\).*(?=\.meta)", regexOptions);
        private static readonly Regex metaName = new(@"(?!.*(\/|\\)).*\..*(?=\.meta)", regexOptions);
        private static readonly Regex metaGuid = new(@"(?<=guid: ).*", regexOptions);
        private static readonly Regex metaFileId = new(@"(?<=mainObjectFileID: ).*", regexOptions);
        private static readonly Regex assetName = new(@"(?!.*(\/|\\)).+", regexOptions);

        private static readonly Dictionary<string, (string, long)> fileCache = [];

        internal static void BuildGameFileCache()
        {
            if (fileCache.Count > 0)
            {
                return;
            }

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

            string gameAssetsPath = Path.Combine(Application.dataPath, "LethalCompany", "Game");
            if (!FileUtils.TryGetFiles(out string[] files, gameAssetsPath, "*.meta", recursive: true) || files.Length == 0)
            {
                stopwatch.Stop();
                return;
            }

            stopwatch.Stop();
            Debug.Log($"[itolib] Getting project files ({files.Length}) took {stopwatch.Elapsed} ms");

            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < files.Length; i++)
            {
                float progress = i / (float)files.Length;
                EditorUtility.DisplayProgressBar("[itolib]", $"Creating game asset cache... {i}/{files.Length} ({progress * 100}%)", progress);

                string filePath = files[i];
                if (gameAssetScriptOrScene.IsMatch(filePath) || metaDirOrScript.IsMatch(filePath))
                {
                    continue;
                }

                using StreamReader metaReader = new(filePath);
                string metaText = string.Empty;
                try
                {
                    metaText = metaReader.ReadToEnd();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[itolib] Could not read file '{filePath}': {e}");
                    continue;
                }

                string assetPath = metaPath.Match(filePath).Value,
                    assetName = metaName.Match(filePath).Value,
                    assetGuid = metaGuid.Match(metaText).Value;
                if (string.IsNullOrEmpty(assetGuid))
                {
                    Debug.LogWarning($"[itolib] File '{assetPath}' has no GUID! Skipping...");
                    continue;
                }

                string assetFileId = metaFileId.Match(metaText).Value;
                if (string.IsNullOrEmpty(assetFileId))
                {
                    using StreamReader fileReader = new(filePath[..^5]);
                    for (int j = 0; j < 3; j++)
                    {
                        string currentLine = fileReader.ReadLine();
                        if (currentLine == null)
                        {
                            break;
                        }

                        if (j == 2 && currentLine.StartsWith("--- !u!", StringComparison.Ordinal))
                        {
                            assetFileId = currentLine.Split('&', StringSplitOptions.RemoveEmptyEntries)[^1];
                        }
                    }
                }

                if (string.IsNullOrEmpty(assetFileId) || !long.TryParse(assetFileId, out long assetFileIdValue))
                {
                    UnityEngine.Object? possibleObject = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                    if (possibleObject == null)
                    {
                        possibleObject = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
                        if (possibleObject == null)
                        {
                            possibleObject = AssetDatabase.LoadMainAssetAtPath(assetPath);
                        }
                    }
                    if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(possibleObject, out string _, out assetFileIdValue))
                    {
                        Debug.LogWarning($"[itolib] Could not find fileID for file '{assetPath}'! Skipping...");
                        continue;
                    }
                }

                (string, long) assetEntry = (assetGuid, assetFileIdValue);
                if (!fileCache.TryAdd(assetName, assetEntry))
                {
                    if (!fileCache.TryAdd(assetPath.Replace('\\', '/'), assetEntry))
                    {
                        Debug.LogWarning($"[itolib] File '{assetPath}' already cached!");
                    }
                }
            }

            stopwatch.Stop();
            Debug.Log($"[itolib] Preparing project files took {stopwatch.Elapsed} ms");
            EditorUtility.ClearProgressBar();
        }

        internal static bool TryFindAsset(string assetPath, out string guid, out long fileId)
        {
            guid = string.Empty;
            fileId = default;

            if (!fileCache.TryGetValue(assetPath, out (string, long) asset)
                && !fileCache.TryGetValue(assetName.Match(assetPath).Value, out asset))
            {
                return false;
            }

            guid = asset.Item1;
            fileId = asset.Item2;

            return true;
        }
    }
}