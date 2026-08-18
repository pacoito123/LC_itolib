/* using itolib.Extensions;
using LethalLevelLoader;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace itolib.Compatibility.Moons
{
    internal sealed class BerunahCompatibility
    {
        internal static void RegisterCompat()
        {
            AssetBundleLoader.AddOnExtendedModLoadedListener(extendedMod =>
            {
                int berunahIndex = extendedMod.ExtendedLevels.FindIndex(level => level.name.CompareOrdinal("BerunahExtended"));
                if (berunahIndex != -1)
                {
                    extendedMod.ExtendedLevels[berunahIndex].LevelEvents.onLevelLoaded.AddListener(NukeDevCamera);
                }
            }, "MagicWesley", "MagicWesleysMod");
        }

        private static void NukeDevCamera()
        {
            Transform? cutsceneStuff = null;
            GameObject[]? rootObjects = SceneManager.GetSceneByName(StartOfRound.Instance.currentLevel.sceneName).GetRootGameObjects();
            for (int i = 0; i < rootObjects?.Length; i++)
            {
                if (rootObjects[i].name == "Cutscenestuff")
                {
                    cutsceneStuff = rootObjects[i].transform;
                    break;
                }
            }

            if (cutsceneStuff == null)
            {
                return;
            }

            Transform? cameraContainer = cutsceneStuff.Find("Cameranode/Camera (1)");
            if (cameraContainer != null && cameraContainer.TryGetComponent(out Camera devCamera) && devCamera.enabled)
            {
                // Nuke camera.
                Object.Destroy(devCamera.gameObject);
            }
        }
    }
} */