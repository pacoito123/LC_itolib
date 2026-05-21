using itolib.editor.Util;
using UnityEditor;
using UnityEngine;

namespace itolib.editor.Restore.Legacy
{
    public static class RemapGUIDs
    {
        [MenuItem("Assets/itolib/Restore/Legacy/Remap Assets")]
        public static void RemapSelectedAssets()
        {
            int remapsToPerform = ReadLegacyJSON.ParseGameAssets();
            if (remapsToPerform == 0)
            {
                return;
            }
            Debug.Log($"[itolib] Performing {remapsToPerform} asset remaps...");

            if (!FileUtils.TryGetSelectedFiles(out string[] files, "*.prefab;*.asset;*.Unity", recursive: true))
            {
                RemapUtils.GuidRemaps.Clear();
                return;
            }

            RemapUtils.RemapFiles(files);
        }

        [MenuItem("Assets/itolib/Restore/Legacy/Remap Scripts")]
        public static void RemapSelectedScripts()
        {
            int remapsToPerform = ReadLegacyJSON.ParseGameScripts();
            if (remapsToPerform == 0)
            {
                return;
            }
            Debug.Log($"[itolib] Performing {remapsToPerform} script remaps...");

            if (!FileUtils.TryGetSelectedFiles(out string[] files, "*.prefab;*.asset;*.Unity", recursive: true))
            {
                RemapUtils.GuidRemaps.Clear();
                return;
            }

            RemapUtils.RemapFiles(files);
        }
    }
}
