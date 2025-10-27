using itolib.Extensions;
using LethalLevelLoader;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace itolib.ScriptableObjects
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [CreateAssetMenu(fileName = "ScriptableNetworkPrefab", menuName = "itolib/Networking/ScriptableNetworkPrefab")]
    public sealed class ScriptableNetworkPrefab : ScriptableObject
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        private static Dictionary<string, NetworkObject> RegisteredPrefabs { get; } = [];

        /// <summary>
        ///     TODO.
        /// </summary>
        [SerializeField] private NetworkObject[]? prefabsToRegister;

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Awake()
        {
            for (int i = 0; i < prefabsToRegister?.Length; i++)
            {
                NetworkObject? prefabToRegister = prefabsToRegister[i];

                if (prefabToRegister != null)
                {
                    LethalLevelLoaderNetworkManager.RegisterNetworkPrefab(prefabToRegister.gameObject);

                    if (!RegisteredPrefabs.TryAdd(prefabToRegister.name, prefabToRegister))
                    {
                        // TODO: Log warning.
                    }
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool TryGetPrefab(out NetworkObject prefab, string name)
        {
            prefab = null!;

            return !name.IsNullOrEmpty() && RegisteredPrefabs.TryGetValue(name, out prefab);
        }
    }
}