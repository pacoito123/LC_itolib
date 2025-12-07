using DunGen;
using itolib.Enums;
using itolib.Interfaces;
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
    public class MaterialSwapper : MonoBehaviour, IActivationScript
    {
        /// <summary>
        ///     Cached instance of the current <c>MaterialSwapper</c> as an <c>IActivationScript</c>, to avoid having to cast.
        /// </summary>
        public IActivationScript ActivationSelf { get; }

        /// <summary>
        ///     Whether activation has already been performed or not.
        /// </summary>
        public bool PerformedActivation { get; set; }

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
        ///     Desired <c>ActivationTime</c> for the automatic material swap.
        /// </summary>
        /// <remarks><b>NOTE:</b> Can be set to <c>Manual</c> to disable the automatic swap, but it's not required for triggering manual swaps afterwards.</remarks>
        [field: Tooltip("Desired activation time for the automatic material swap. NOTE: Can be set to 'Manual' to disable the automatic swap, but it's not "
            + "required for triggering manual swaps afterwards.")]
        [field: SerializeField] public ActivationTime ActivationTime { get; set; } = ActivationTime.DungeonComplete;

        /// <summary>
        ///     Current swap index. TODO: Use a NetworkVariable.
        /// </summary>
        private int swapIndex;

        /// <summary>
        ///     Cache already-cast <c>IActivationScript</c> instance.
        /// </summary>
        private MaterialSwapper()
        {
            ActivationSelf = this;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Awake()
        {
            ActivationSelf.Initialize();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void OnDestroy()
        {
            ActivationSelf.UnsubscribeFromEvents();
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
        ///     Perform script activation at the specified <c>ActivationTime</c>.
        /// </summary>
        /// <param name="activationTime"><c>ActivationTime</c> set for the script.</param>
        public virtual void PerformActivation(ActivationTime activationTime)
        {
            SwapMaterials();
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
        ///     <c>DunGen</c> listener called when the Dungeon finishes generating.
        /// </summary>
        /// <param name="dungeon">Dungeon that just finished generating.</param>
        public void OnDungeonComplete(Dungeon dungeon)
        {
            ActivationSelf.OnDungeonComplete();
        }
    }
}