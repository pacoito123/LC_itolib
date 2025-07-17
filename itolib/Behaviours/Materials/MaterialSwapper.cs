using DunGen;
using itolib.Enums;
using System;
using UnityEngine;

namespace itolib.Behaviours.Materials
{
    /// <summary>
    ///     Represents a material swap to perform on activation.
    /// </summary>
    [Serializable]
    public struct MaterialSwap
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public string searchKeyword = string.Empty;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public Material? replacementMaterial = null;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public GameObject[]? affectedObjects;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool affectChildren = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        public MaterialSwap() { }
    }

    /// <summary>
    ///     Swaps materials based on a given word search. Can perform multiple at a time, on various different objects.
    /// </summary>
    public class MaterialSwapper : MonoBehaviour, IDungeonCompleteReceiver
    {
        /// <summary>
        ///     List of material swaps to perform.
        /// </summary>
        [Header("Material Swapper")]
        [Tooltip("List of material swaps to perform.")]
        [SerializeField] private MaterialSwap[]? materialSwaps;

        /// <summary>
        ///     The number of swaps done at a time per activation. If set to a value of <b>1</b> (for example) it'll sequentially go down the
        ///     list of swaps and perform them one by one, each time it's activated. If left at the default value of <b>0</b> it'll perform all swaps at once.
        /// </summary>
        [Tooltip("The number of swaps done at a time per activation. If set to a value of 1 (for example) it'll sequentially go down the "
            + "list of swaps and perform them one by one, each time it's activated. If left at the default value of 0 it'll perform all swaps at once.")]
        [Min(0)]
        [SerializeField] private int swapsPerActivation;

        /// <summary>
        ///     Activation time for the automatic material swap.
        /// </summary>
        /// <remarks><b>NOTE:</b> Can be set to <c>Manual</c> to disable the automatic swap, but is not required for triggering manual swaps afterwards.</remarks>
        [Tooltip("Activation time for the automatic material swap. NOTE: Can be set to Manual to disable the automatic swap, but is not required for "
            + "triggering manual swaps afterwards.")]
        [SerializeField] private ActivationTime activationTime = ActivationTime.DungeonComplete;

        /// <summary>
        ///     Current 
        /// </summary>
        private int swapIndex;

        /// <summary>
        ///     TODO.
        /// </summary>
        public void Start()
        {
            if (activationTime is ActivationTime.Immediate)
            {
                SwapMaterials();
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void OnEnable()
        {
            if (activationTime is ActivationTime.StartOfRound && StartOfRound.Instance != null)
            {
                StartOfRound.Instance.StartNewRoundEvent.AddListener(SwapMaterials);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void OnDisable()
        {
            if (activationTime is ActivationTime.StartOfRound && StartOfRound.Instance != null)
            {
                StartOfRound.Instance.StartNewRoundEvent.RemoveListener(SwapMaterials);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void SwapMaterials()
        {
            if (materialSwaps == null || materialSwaps.Length == 0)
            {
                Plugin.StaticLogger.LogWarning($"Could not perform material swapping, as there are no swaps defined for MaterialSwapper component in "
                    + "GameObject '{gameObject.name}'.");

                return;
            }

            // 
            if (swapsPerActivation <= 0 || swapsPerActivation > materialSwaps.Length)
            {
                swapsPerActivation = materialSwaps.Length;
            }

            for (int i = 0; i < swapsPerActivation; i++, swapIndex++)
            {
                if (swapIndex >= materialSwaps.Length)
                {
                    swapIndex = 0;
                }

                MaterialSwap swap = materialSwaps[swapIndex];

                if (swap.replacementMaterial == null)
                {
                    continue;
                }

                for (int j = 0; j < swap.affectedObjects?.Length; j++)
                {
                    GameObject? affectedObject = swap.affectedObjects[j];

                    if (affectedObject == null)
                    {
                        continue;
                    }

                    if (swap.affectChildren)
                    {
                        foreach (Renderer renderer in affectedObject.GetComponentsInChildren<Renderer>())
                        {
                            PerformSwap(renderer, swap.searchKeyword, swap.replacementMaterial);
                        }
                    }
                    else if (affectedObject.TryGetComponent(out Renderer renderer))
                    {
                        PerformSwap(renderer, swap.searchKeyword, swap.replacementMaterial);
                    }
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="searchKeyword"></param>
        /// <param name="replacementMaterial"></param>
        private static void PerformSwap(Renderer renderer, string searchKeyword, Material replacementMaterial)
        {
            Material[] materials = renderer.sharedMaterials;

            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i].name.Contains(searchKeyword, StringComparison.OrdinalIgnoreCase))
                {
                    materials[i] = replacementMaterial;
                }
            }

            renderer.sharedMaterials = materials;
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