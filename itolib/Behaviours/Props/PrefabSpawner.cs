using itolib.Behaviours.Networking;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Props
{
    /// <summary>
    /// 	TODO.
    /// </summary>
    public class PrefabSpawner : NetworkedSpawner<NetworkObject>
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Prefab Spawner")]
        [Tooltip("")]
        public NetworkObject? prefabToSpawn;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private Transform? parentTransform;

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <returns></returns>
        public override NetworkObject? GetPrefabToSpawn()
        {
            return prefabToSpawn;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="spawnPosition"></param>
        /// <param name="spawnRotation"></param>
        /// <returns></returns>
        protected override bool AdditionalProcessing(NetworkObject prefab, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            _ = StartCoroutine(ReparentPrefabOnSpawn(prefab));

            return true;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <returns></returns>
        private IEnumerator ReparentPrefabOnSpawn(NetworkObject prefab)
        {
            float startTime = Time.realtimeSinceStartup;
            while (!prefab.IsSpawned && Time.realtimeSinceStartup - startTime < 8f)
            {
                yield return new WaitForSeconds(0.03f); // TODO: Replace with better method.
            }

            yield return new WaitForEndOfFrame();

            if (parentTransform != null)
            {
                _ = prefab.TrySetParent(parentTransform);
            }
            else if (RoundManager.Instance != null && RoundManager.Instance.mapPropsContainer != null)
            {
                _ = prefab.TrySetParent(RoundManager.Instance.mapPropsContainer.transform);
            }
        }
    }
}