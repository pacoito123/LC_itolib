using DunGen;
using itolib.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace itolib.Behaviours.Materials
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [Serializable]
    public struct MaterialSwap
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public string searchKeyword;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public Material? replacementMaterial;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public List<GameObject> affectedObjects;

        /// <summary>
        ///     TODO.
        /// </summary>
        public MaterialSwap()
        {
            searchKeyword = "";
            replacementMaterial = null;
            affectedObjects = [];
        }
    }

    /// <summary>
    ///     TODO.
    /// </summary>
    public class MaterialSwapper : MonoBehaviour, IDungeonCompleteReceiver
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Material Swapper")]
        [Tooltip("")]
        public List<MaterialSwap> materialSwaps = [];

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public ActivationTime activationTime = ActivationTime.DungeonComplete;

        private void Start()
        {
            if (activationTime is ActivationTime.Immediate)
            {
                SwapMaterials();
            }
        }

        private void OnEnable()
        {
            if (activationTime is ActivationTime.StartOfRound)
            {
                StartOfRound.Instance?.StartNewRoundEvent.AddListener(SwapMaterials);
            }
        }

        private void OnDisable()
        {
            // TODO: Switch to regular C# events?
            if (activationTime is ActivationTime.StartOfRound)
            {
                StartOfRound.Instance?.StartNewRoundEvent.RemoveListener(SwapMaterials);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void SwapMaterials()
        {
            foreach (MaterialSwap swap in materialSwaps)
            {
                if (swap.replacementMaterial == null)
                {
                    continue;
                }

                foreach (GameObject affectedObject in swap.affectedObjects)
                {
                    if (affectedObject == null)
                    {
                        continue;
                    }

                    foreach (MeshRenderer renderer in affectedObject.GetComponentsInChildren<MeshRenderer>())
                    {
                        Material[] materials = renderer.sharedMaterials;

                        for (int i = 0; i < materials.Length; i++)
                        {
                            if (materials[i].name.Contains(swap.searchKeyword, StringComparison.OrdinalIgnoreCase))
                            {
                                materials[i] = swap.replacementMaterial;
                            }
                        }

                        renderer.sharedMaterials = materials;
                    }
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="dungeon"></param>
        public void OnDungeonComplete(Dungeon dungeon)
        {
            if (activationTime is ActivationTime.DungeonComplete)
            {
                SwapMaterials();
            }
        }
    }
}