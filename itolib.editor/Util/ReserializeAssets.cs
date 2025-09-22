using System.IO;
using UnityEditor;
using UnityEngine;

namespace itolib.editor.Util
{
    /// <summary>
    ///     Reserialization utilities.
    /// </summary>
    public static class ReserializeAssets
    {
        /// <summary>
        ///     Force save all prefabs in order to reserialize them.
        /// </summary>
        [MenuItem("Assets/itolib/Util/Reserialize Prefabs (recursively)")]
        public static void ReserializePrefabs()
        {
            string selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            string[] files = Directory.GetFiles(selectedPath, "*.prefab", SearchOption.AllDirectories);

            for (int i = 0; i < files.Length; i++)
            {
                GameObject _prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(files[i]);
                if (!PrefabUtility.IsPartOfImmutablePrefab(_prefabAsset))
                {
                    _ = PrefabUtility.SavePrefabAsset(_prefabAsset);
                }
            }
        }
    }
}
