using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace itolib.editor.Util
{
    /// <summary>
    ///     Reserialization utilities.
    /// </summary>
    public static class ReserializeAssets
    {
        /// <summary>
        ///     Force save prefab assets in the current directory.
        /// </summary>
        [MenuItem("Assets/itolib/Util/Reserialize/Prefabs")]
        public static void ReserializePrefabs()
        {
            ReserializeAssetsOfType<GameObject>("*.prefab");
        }

        /// <summary>
        ///     Force save <c>AnimationClip</c> assets in the current directory.
        /// </summary>
        [MenuItem("Assets/itolib/Util/Reserialize/AnimationClips")]
        public static void ReserializeAnimationClips()
        {
            ReserializeAssetsOfType<AnimationClip>("*.anim");
        }

        /// <summary>
        ///     Force save <c>AnimatorController</c> assets in the current directory.
        /// </summary>
        [MenuItem("Assets/itolib/Util/Reserialize/AnimatorControllers")]
        public static void ReserializeAnimatorControllers()
        {
            ReserializeAssetsOfType<AnimatorController>("*.controller");
        }

        /// <summary>
        ///     Force save <c>AudioClip</c> assets in the current directory.
        /// </summary>
        [MenuItem("Assets/itolib/Util/Reserialize/AudioClips")]
        public static void ReserializeAudioClips()
        {
            ReserializeAssetsOfType<AudioClip>("*.mp3", "*.ogg", "*.wav");
        }

        /// <summary>
        ///     Force save <c>Material</c> assets in the current directory.
        /// </summary>
        [MenuItem("Assets/itolib/Util/Reserialize/Materials")]
        public static void ReserializeMaterials()
        {
            ReserializeAssetsOfType<Material>("*.mat");
        }

        /// <summary>
        ///     Force save <c>Mesh</c> assets in the current directory.
        /// </summary>
        [MenuItem("Assets/itolib/Util/Reserialize/Meshes")]
        public static void ReserializeMeshes()
        {
            ReserializeAssetsOfType<Mesh>("*.mesh", "*.fbx");
        }

        /// <summary>
        ///     Force save <c>ScriptableObject</c> assets in the current directory.
        /// </summary>
        [MenuItem("Assets/itolib/Util/Reserialize/ScriptableObjects")]
        public static void ReserializeScriptableObjects()
        {
            ReserializeAssetsOfType<ScriptableObject>("*.asset");
        }

        /// <summary>
        ///     Force save <c>Texture2D</c> assets in the current directory.
        /// </summary>
        [MenuItem("Assets/itolib/Util/Reserialize/Textures")]
        public static void ReserializeTextures()
        {
            ReserializeAssetsOfType<Texture2D>("*.png", "*.jpg", "*.jpeg", "*.tga");
        }

        private static void ReserializeAssetsOfType<T>(params string[] extensions) where T : UnityEngine.Object
        {
            bool recursive = EditorUtility.DisplayDialog($"Re-serialize {typeof(T).FullName} assets", $"Should {typeof(T).FullName} assets in the "
                + "current (or selected) directory be re-serialized recursively?", "Yes", "No");

            string[]? allFiles = null;

            for (int i = 0; i < extensions.Length; i++)
            {
                if (!TryGetFiles(out string[] files, extensions[i], recursive))
                {
                    continue;
                }

                allFiles = (allFiles?.Length > 0) ? [.. allFiles, .. files] : files;
            }

            if (allFiles == null)
            {
                _ = EditorUtility.DisplayDialog($"Re-serialize {typeof(T).FullName} assets", $"No {typeof(T).FullName} assets were found in the "
                    + "selected directory", "Close");
                return;
            }

            List<string> validPaths = new(allFiles.Length);

            for (int i = 0; i < allFiles.Length; i++)
            {
                T? asset = AssetDatabase.LoadAssetAtPath<T>(allFiles[i]);

                if (asset == null)
                {
                    continue;
                }

                if (asset is GameObject prefab)
                {
                    if (!PrefabUtility.IsPartOfImmutablePrefab(prefab))
                    {
                        _ = PrefabUtility.SavePrefabAsset(prefab);
                    }

                    continue;
                }

                validPaths.Add(allFiles[i]);
            }

            if (validPaths.Count > 0)
            {
                AssetDatabase.ForceReserializeAssets(validPaths);
            }

            _ = EditorUtility.DisplayDialog($"Re-serialize {typeof(T).FullName} assets", $"Finished!", "Close");
        }

        internal static bool TryGetFiles(out string[] files, string pattern, bool recursive = false)
        {
            files = null!;

            try
            {
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

                files = Directory.GetFiles(activePath, pattern, recursive ? SearchOption.AllDirectories
                    : SearchOption.TopDirectoryOnly);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[itolib] Could not get list of files: {e}");
            }

            return false;
        }
    }
}
