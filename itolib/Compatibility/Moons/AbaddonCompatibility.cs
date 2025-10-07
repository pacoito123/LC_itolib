using LethalLevelLoader;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace itolib.Compatibility.Moons
{
    internal sealed class AbaddonCompatibility
    {
        public static bool Registered { get; private set; } // Only needed for LLL < 1.5.0

        internal static void RegisterCompat()
        {
            AssetBundleLoader.AddOnExtendedModLoadedListener(abaddonExtended =>
            {
                if (Registered)
                {
                    return;
                }

                if (abaddonExtended.ExtendedLevels.Count > 0 && abaddonExtended.ExtendedLevels[0] != null)
                {
                    abaddonExtended.ExtendedLevels[0].LevelEvents.onLevelLoaded.AddListener(AbaddonDoorFix);

                    Registered = true;
                }
            }, "DemonMae_ABD", "Abaddon");
        }

        private static void AbaddonDoorFix()
        {
            Transform? environment = null;

            GameObject[]? rootObjects = SceneManager.GetSceneByName(StartOfRound.Instance.currentLevel.sceneName).GetRootGameObjects();
            for (int i = 0; i < rootObjects?.Length; i++)
            {
                if (rootObjects[i].CompareTag("OutsideLevelNavMesh"))
                {
                    environment = rootObjects[i].transform;
                    break;
                }
            }

            if (environment == null)
            {
                return;
            }

            Transform? door = environment.Find("Gate/Door/DoorMesh/Cube");
            if (door == null || !door.TryGetComponent(out DoorLock doorLock) || doorLock.navMeshObstacle != null)
            {
                return;
            }

            GameObject obstacleContainer = new("NavMeshObstacle", [typeof(NavMeshObstacle)])
            {
                layer = LayerMask.NameToLayer("Room")
            };

            Transform obstacleTransform = obstacleContainer.transform;
            obstacleTransform.SetParent(door);
            obstacleTransform.localPosition = new(-1.25f, -2.65f, -1.5f);
            obstacleTransform.localScale = new(6.0f, 2.5f, 0.325f);

            if (obstacleContainer.TryGetComponent(out NavMeshObstacle obstacle))
            {
                obstacle.center = new(0.0f, 0.0f, 0.0f);
                obstacle.size = new(1.0f, 1.0f, 1.0f);
            }

            doorLock.navMeshObstacle = obstacle;
        }
    }
}