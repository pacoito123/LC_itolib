using itolib.Extensions;
using itolib.Patches;
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
        internal static Dictionary<string, NetworkObject> RegisteredPrefabs { get; } = [];

        [SerializeField] private NetworkObject[]? prefabsToRegister;

        private void Awake()
        {
            if (GameNetworkManagerPatch.networkStarted)
            {
                Plugin.StaticLogger.LogError($"ScriptableNetworkPrefab '{name}' initializing after network has already started. Prefabs won't be registered!");

                return;
            }

            for (int i = 0; i < prefabsToRegister?.Length; i++)
            {
                NetworkObject? prefabToRegister = prefabsToRegister[i];

                if (prefabToRegister == null)
                {
                    Plugin.StaticLogger.LogWarning($"ScriptableNetworkPrefab '{name}' attempts to register a missing prefab.");

                    continue;
                }

                if (!RegisteredPrefabs.TryAdd(prefabToRegister.name, prefabToRegister))
                {
                    Plugin.StaticLogger.LogWarning($"ScriptableNetworkPrefab could not register duplicate prefab '{name}'.");
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