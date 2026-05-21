using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace itolib.editor.Util
{
    internal static class RemapUtils
    {
        private static readonly Regex guidReference = new(@"fileID:(?!.*100100000).*guid:[^,]*", GameAssetUtils.regexOptions);
        internal static Dictionary<string, string> GuidRemaps { get; } = [];

        internal static void RemapFiles(string[] files)
        {
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 0; i < files.Length; i++)
            {
                float progress = i / (float)files.Length;
                EditorUtility.DisplayProgressBar("[itolib]", $"Remapping files... {i}/{files.Length} ({progress * 100}%)", progress);

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
                    Match guidLineMatch = guidReference.Match(line);
                    if (guidLineMatch.Success)
                    {
                        string guid = guidLineMatch.Value.Split("guid: ")[^1];
                        if (GuidRemaps.TryGetValue(guid, out string replacement))
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

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            GuidRemaps.Clear();
        }

        internal static void AddRemap(string key, string guid, long fileId, bool overwrite = true)
        {
            string value = $"fileID: {fileId}, guid: {guid}";
            if (GuidRemaps.TryAdd(key, value))
            {
                return;
            }

            Debug.Log($"[itolib] Duplicate key '{key}' found! Overwriting value: {overwrite}");
            if (overwrite)
            {
                GuidRemaps[key] = value;
            }
        }
    }
}