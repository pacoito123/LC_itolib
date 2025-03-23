using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Helpers
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class PrefabSpawner : MonoBehaviour
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public NetworkObject? spawnPrefab;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool destroyWithScene = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public Vector3 positionOffset = Vector3.zero;

        private void Start()
        {
            if (!NetworkManager.Singleton.IsHost || spawnPrefab == null)
            {
                return;
            }

            if (Instantiate(spawnPrefab, transform.position + positionOffset, transform.rotation, transform)
                .TryGetComponent(out NetworkObject networkObject))
            {
                networkObject.Spawn(destroyWithScene);
            }

            Destroy(gameObject);
        }
    }
}