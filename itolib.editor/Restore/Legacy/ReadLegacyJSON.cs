using itolib.editor.Data;
using itolib.editor.Util;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace itolib.editor.Restore.Legacy
{
    internal static class ReadLegacyJSON
    {
        private static bool TryReadLegacyJSON(out ProjectInformation projectInformation)
        {
            projectInformation = default;

            string projectInfoPath = EditorUtility.OpenFilePanel("Select Extracted Project Information", string.Empty, "json");
            if (string.IsNullOrEmpty(projectInfoPath))
            {
                Debug.LogError($"[itolib] No file selected to read.");

                return false;
            }

            try
            {
                string json = File.ReadAllText(projectInfoPath);
                projectInformation = JsonUtility.FromJson<ProjectInformation>(json);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[itolib] Could not read project information from selected file '{projectInfoPath}': {e}");
            }

            return false;
        }

        internal static int ParseGameAssets()
        {
            if (!TryReadLegacyJSON(out ProjectInformation projectInformation))
            {
                return 0;
            }

            GameAssetUtils.BuildGameFileCache();

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
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

            return RemapUtils.GuidRemaps.Count;
        }

        internal static int ParseGameScripts()
        {
            if (!TryReadLegacyJSON(out ProjectInformation projectInformation))
            {
                return 0;
            }

            ScriptUtils.BuildGameScriptCache();

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < projectInformation.guids.Length; i++)
            {
                float progress = i / (float)projectInformation.guids.Length;
                EditorUtility.DisplayProgressBar("[itolib]", $"Parsing scripts from JSON file... {i}/{projectInformation.guids.Length} ({progress * 100}%)", progress);

                ProjectInformation.Guid scriptInfo = projectInformation.guids[i];
                if (ScriptUtils.TryFindScript(scriptInfo.fullTypeName, out string guid, out long fileId))
                {
                    RemapUtils.AddRemap(scriptInfo.originalGuid, guid, fileId);
                }
            }

            stopwatch.Stop();
            Debug.Log($"[itolib] Parsing JSON file took {stopwatch.Elapsed} ms");
            EditorUtility.ClearProgressBar();

            return RemapUtils.GuidRemaps.Count;
        }
    }
}