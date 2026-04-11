using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace itolib.editor.Util
{
    internal static class FileUtils
    {
        internal static bool TryGetSelectedFiles(out string[] files, string pattern, bool recursive = false)
        {
            return TryGetSelectedFiles(out files, pattern.Split(';', StringSplitOptions.RemoveEmptyEntries), recursive);
        }

        internal static bool TryGetSelectedFiles(out string[] files, string[] patterns, bool recursive = false)
        {
            files = null!;

            // Try get selected object.
            string activePath = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (string.IsNullOrEmpty(activePath))
            {
                if (!ProjectWindowUtil.TryGetActiveFolderPath(out activePath))
                {
                    Debug.LogWarning($"[itolib] Could not get current folder path or selection.");

                    return false;
                }
            }

            return TryGetFiles(out files, activePath, patterns, recursive);
        }

        internal static bool TryGetFiles(out string[] files, string path, string pattern, bool recursive = false)
        {
            return TryGetFiles(out files, path, pattern.Split(';', StringSplitOptions.RemoveEmptyEntries), recursive);
        }

        internal static bool TryGetFiles(out string[] files, string path, string[] patterns, bool recursive = false)
        {
            HashSet<string> allFiles = [];

            for (int i = 0; i < patterns?.Length; i++)
            {
                string[]? foundFiles = null;

                try
                {
                    foundFiles = Directory.GetFiles(path, patterns[i], recursive ? SearchOption.AllDirectories
                        : SearchOption.TopDirectoryOnly);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[itolib] Could not get list of files: {e}");
                }

                if (foundFiles?.Length > 0)
                {
                    allFiles.UnionWith(foundFiles);
                }
            }
            files = [.. allFiles];

            return files.Length > 0;
        }

        internal static string GetNameFromPath(string path, bool extension = true)
        {
            string name = path.Split('/')[^1];

            if (!extension)
            {
                int lastPeriod = name.LastIndexOf('.');

                if (lastPeriod != -1)
                {
                    name = name[..lastPeriod];
                }
            }

            return name;
        }
    }
}