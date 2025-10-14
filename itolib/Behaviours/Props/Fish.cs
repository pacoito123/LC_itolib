using DunGen;
using itolib.Enums;
using itolib.Extensions;
using itolib.Interfaces;
using System.Collections.Generic;
using UnityEngine;

namespace itolib.Behaviours.Props
{
    /// <summary>
    ///     Fish.
    /// </summary>
    public class Fish : MonoBehaviour, IActivationScript, ISeededScript<Fish>
    {
        /// <summary>
        ///     Fish.
        /// </summary>
        public static UnlockableItem? GoldfishUnlockableItem
        {
            get
            {
                if (field == null && StartOfRound.Instance != null)
                {
                    field = StartOfRound.Instance.unlockablesList.unlockables.Find(unlockable =>
                        unlockable.unlockableName.CompareOrdinal("Goldfish"));
                }

                return field;
            }
        }

        /// <summary>
        ///     Fish.
        /// </summary>
        public static GameObject? FishBowl
        {
            get
            {
                if (field == null && GoldfishUnlockableItem != null)
                {
                    field = GoldfishUnlockableItem.prefabObject.transform.Find("FishBowl").gameObject;
                }

                return field;
            }
        }

        /// <summary>
        ///     Cached instance of the current <c>Fish</c> as an <c>IActivationScript</c>, to avoid having to cast.
        /// </summary>
        public IActivationScript ActivationSelf { get; }

        /// <summary>
        ///     Cached instance of the current <c>Fish</c> as an <c>ISeededScript</c>, to avoid having to cast.
        /// </summary>
        public ISeededScript<Fish> SeededSelf { get; }

        /// <summary>
        ///     Whether activation has already been performed or not.
        /// </summary>
        public bool PerformedActivation { get; set; }

        /// <summary>
        ///     Fish.
        /// </summary>
        [Header("Fish")]
        [Tooltip("Fish.")]
        [SerializeField] private bool includeBowl;

        /// <summary>
        ///     Fish.
        /// </summary>
        [Tooltip("Fish.")]
        [SerializeField] private Material?[]? materialReplacements;

        /// <summary>
        ///     Fish.
        /// </summary>
        [Tooltip("Fish.")]
        [SerializeField] private Mesh?[]? meshReplacements;

        /// <summary>
        ///     Fish.
        /// </summary>
        [Tooltip("Fish.")]
        [Min(-1)]
        [SerializeField] private int minFish = 1;

        /// <summary>
        ///     Fish.
        /// </summary>
        [Tooltip("Fish.")]
        [Min(-1)]
        [SerializeField] private int maxFish = 1;

        /// <summary>
        ///     Fish.
        /// </summary>
        [Tooltip("Fish.")]
        [Min(0.0f)]
        [SerializeField] private float minSize = 1.0f;

        /// <summary>
        ///     Fish.
        /// </summary>
        [Tooltip("Fish.")]
        [Min(0.0f)]
        [SerializeField] private float maxSize = 5.0f;

        /// <summary>
        ///     Fish.
        /// </summary>
        [Tooltip("Fish.")]
        [SerializeField] private bool randomizeAnimationStart = true;

        /// <summary>
        ///     Fish.
        /// </summary>
        [Space(10.0f)]
        [Tooltip("Fish.")]
        [SerializeField] private List<Transform?>? spawnLocations;

        /// <summary>
        ///     Fish.
        /// </summary>
        [Tooltip("Fish.")]
        [SerializeField] private bool includeChildren;

        /// <summary>
        ///     Fish.
        /// </summary>
        [Tooltip("Fish.")]
        [SerializeField] private bool exhaustiveLocations;

        /// <summary>
        ///     Fish.
        /// </summary>
        [Tooltip("Fish.")]
        [SerializeField] private List<BoxCollider?>? spawnAreas;

        /// <summary>
        ///     Fish.
        /// </summary>
        [Tooltip("Fish.")]
        [SerializeField] private bool exhaustiveAreas;

        /// <summary>
        ///     Fish.
        /// </summary>
        [Tooltip("Fish.")]
        [SerializeField] private bool skipInactive = true;

        /// <summary>
        ///     Fish.
        /// </summary>
        [Tooltip("Fish.")]
        [SerializeField] private bool useLocalRotation = true;

        /// <summary>
        ///     Fish.
        /// </summary>
        [Tooltip("Fish.")]
        [SerializeField] private bool isSeededRandom = true;

        /// <summary>
        ///     Desired <c>ActivationTime</c> for the fish.
        /// </summary>
        [field: Tooltip("Desired activation time for the fish.")]
        [field: SerializeField] public ActivationTime ActivationTime { get; set; } = ActivationTime.StartOfRound;

        /// <summary>
        ///     Fish.
        /// </summary>
        private static readonly int fishAnimationID = Animator.StringToHash("Fishbowl");

        /// <summary>
        ///     Fish.
        /// </summary>
        public void SummonFish()
        {
            int spawnAmount = isSeededRandom ? SeededSelf.GetSeededRandom().Next(minFish, maxFish + 1)
                : Random.RandomRangeInt(minFish, maxFish + 1);

            if (spawnAmount == 0)
            {
                return;
            }

            _ = spawnLocations?.RemoveAll(spawnLocation => spawnLocation == null || (skipInactive && !spawnLocation.gameObject.activeInHierarchy));
            _ = spawnAreas?.RemoveAll(spawnArea => spawnArea == null || (skipInactive && !spawnArea.gameObject.activeInHierarchy));

            if (spawnLocations?.Count > 0)
            {
                if (spawnAmount == -1)
                {
                    spawnAmount = spawnLocations.Count;
                }

                for (int i = 0; i < spawnAmount && spawnLocations.Count > 0; i++)
                {
                    int locationIndex = isSeededRandom ? SeededSelf.GetSeededRandom().Next(0, spawnLocations.Count)
                        : Random.RandomRangeInt(0, spawnLocations.Count);

                    SummonFish(spawnLocations[locationIndex]!);

                    if (exhaustiveLocations)
                    {
                        spawnLocations.RemoveAt(locationIndex);
                    }
                }
            }
            else if (spawnAreas?.Count > 0)
            {
                if (spawnAmount == -1)
                {
                    spawnAmount = spawnAreas.Count;
                }

                for (int i = 0; i < spawnAmount && spawnAreas.Count > 0; i++)
                {
                    int areaIndex = isSeededRandom ? SeededSelf.GetSeededRandom().Next(0, spawnAreas.Count)
                        : Random.RandomRangeInt(0, spawnAreas.Count);

                    SummonFish(spawnAreas[areaIndex]!);

                    if (exhaustiveAreas)
                    {
                        spawnAreas.RemoveAt(areaIndex);
                    }
                }
            }
            else if (!skipInactive)
            {
                SummonFish(transform);
            }
        }

        /// <summary>
        ///     Fish.
        /// </summary>
        /// <param name="spawnLocation"></param>
        private void SummonFish(Transform spawnLocation)
        {
            SummonFish(spawnLocation.position, !useLocalRotation ? spawnLocation.rotation : spawnLocation.localRotation);
        }

        /// <summary>
        ///     Fish.
        /// </summary>
        /// <param name="spawnArea"></param>
        private void SummonFish(BoxCollider spawnArea)
        {
            Vector3 point = spawnArea.GetPointWithin(isSeededRandom ? SeededSelf.GetSeededRandom() : null);

            Transform spawnTransform = spawnArea.transform;
            Vector3 spawnPosition = spawnTransform.TransformPoint(point + spawnArea.center);

            SummonFish(spawnPosition, !useLocalRotation ? spawnTransform.rotation : spawnTransform.localRotation);
        }

        /// <summary>
        ///     Fish.
        /// </summary>
        /// <param name="spawnPosition"></param>
        /// <param name="spawnRotation"></param>
        private void SummonFish(Vector3 spawnPosition, Quaternion spawnRotation)
        {
            if (FishBowl != null)
            {
                GameObject fishBowl = Instantiate(FishBowl, spawnPosition, spawnRotation * Quaternion.Euler(-90.0f, 0.0f, 0.0f),
                    (RoundManager.Instance != null && RoundManager.Instance.mapPropsContainer != null) ? RoundManager.Instance.mapPropsContainer.transform : null);

                if (!includeBowl && fishBowl.TryGetComponent(out MeshFilter fishBowlMesh)
                    && fishBowl.TryGetComponent(out MeshRenderer fishBowlRenderer))
                {
                    Destroy(fishBowlMesh);
                    Destroy(fishBowlRenderer);
                }

                if (randomizeAnimationStart && fishBowl.TryGetComponent(out Animator fishAnimator))
                {
                    float animationStart = isSeededRandom ? (float)SeededSelf.GetSeededRandom().NextDouble()
                        : Random.Range(0.0f, 1.0f);

                    fishAnimator.Play(fishAnimationID, 0, animationStart);
                }

                Transform? fishContainer = fishBowl.transform.Find("FishContainer");
                if (fishContainer == null)
                {
                    return;
                }

                if (minSize != 1.0f || maxSize != 1.0f)
                {
                    float fishScale = (minSize == maxSize) ? minSize : (isSeededRandom ? SeededSelf.GetSeededRandom().Next(minSize, maxSize)
                            : Random.Range(minSize, maxSize));

                    fishContainer.localScale = new(fishScale, fishScale, fishScale);
                }

                Transform? fish = fishContainer.Find("Fish");
                if (fish == null)
                {
                    return;
                }

                if (meshReplacements?.Length > 0 && fish.TryGetComponent(out MeshFilter fishMesh))
                {
                    int mesh = isSeededRandom ? SeededSelf.GetSeededRandom().Next(0, meshReplacements.Length)
                        : Random.RandomRangeInt(0, meshReplacements.Length);

                    fishMesh.sharedMesh = meshReplacements[mesh];
                }

                if (materialReplacements?.Length > 0 && fish.TryGetComponent(out MeshRenderer fishRenderer))
                {
                    int material = isSeededRandom ? SeededSelf.GetSeededRandom().Next(0, materialReplacements.Length)
                        : Random.RandomRangeInt(0, materialReplacements.Length);

                    fishRenderer.sharedMaterial = materialReplacements[material];
                }
            }
        }

        /// <summary>
        ///     Perform script activation at the specified <c>ActivationTime</c>.
        /// </summary>
        /// <param name="activationTime"><c>ActivationTime</c> set for the script.</param>
        public void PerformActivation(ActivationTime activationTime)
        {
            SummonFish();
        }

        /// <summary>
        ///     Cache already-cast <c>IActivationScript</c> and <c>ISeededScript</c> instances.
        /// </summary>
        private Fish()
        {
            ActivationSelf = this;
            SeededSelf = this;
        }

        /// <summary>
        ///     Fish.
        /// </summary>
        private void Awake()
        {
            if (includeChildren && spawnLocations?.Count > 0)
            {
                HashSet<Transform> childLocations = [];

                for (int i = 0; i < spawnLocations.Count; i++)
                {
                    Transform? locationRoot = spawnLocations[i];

                    if (locationRoot == null || locationRoot.childCount == 0)
                    {
                        continue;
                    }

                    for (int j = 0; j < locationRoot.childCount; j++)
                    {
                        Transform locationChild = locationRoot.GetChild(j);

                        if (!spawnLocations.Contains(locationChild))
                        {
                            _ = childLocations.Add(locationChild);
                        }
                    }
                }

                spawnLocations.AddRange(childLocations);
            }

            ActivationSelf.Initialize();
        }

        /// <summary>
        ///     Fish.
        /// </summary>
        private void OnDestroy()
        {
            ActivationSelf.UnsubscribeFromEvents();
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