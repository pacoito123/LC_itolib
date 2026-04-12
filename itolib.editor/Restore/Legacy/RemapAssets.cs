using itolib.editor.Data;
using itolib.editor.Util;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace itolib.editor.Restore.Legacy
{
    public static class RemapAssets
    {
        [MenuItem("Assets/itolib/Restore/Legacy/Remap Assets")]
        public static void RemapSelectedAssets()
        {
            string projectInfoPath = EditorUtility.OpenFilePanel("Select Extracted Project Information", string.Empty, "json");
            if (string.IsNullOrEmpty(projectInfoPath))
            {
                return;
            }

            GameAssetUtils.BuildGameFileCache();

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

                if (GameAssetUtils.TryFindAsset(assetInfo.assetPath, out string guid, out long fileId))
                {
                    RemapUtils.AddRemap(assetInfo.originalGuid, guid, fileId);
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
