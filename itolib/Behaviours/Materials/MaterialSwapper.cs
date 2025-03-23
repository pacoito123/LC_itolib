using itolib.Patches;
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
    public class MaterialSwapper : MonoBehaviour
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public List<MaterialSwap> materialSwaps = [];

        /// <summary>
        ///     TODO.
        /// </summary>
        public bool activateImmediately = true;

        private void Awake()
        {
            if (activateImmediately)
            {
                SwapMaterials();
            }
            else
            {
                LoadPatch.OnFinishGeneratingNewLevelClientRpcPost += SwapMaterials;
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
                    foreach (MeshRenderer renderer in affectedObject.GetComponentsInChildren<MeshRenderer>())
                    {
                        Material[] materials = renderer.sharedMaterials;

                        for (int i = 0; i < materials.Length; i++)
                        {
                            if (materials[i].name.Contains(swap.searchKeyword))
                            {
                                materials[i] = swap.replacementMaterial;
                            }
                        }

                        renderer.sharedMaterials = materials;
                    }
                }
            }

            if (!activateImmediately)
            {
                LoadPatch.OnFinishGeneratingNewLevelClientRpcPost -= SwapMaterials;
            }
        }
    }
}