using itolib.Structs;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace itolib.Behaviours.Scenes
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [Serializable]
    public struct ReverbTriggerAdjustment
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Reverb Trigger Adjustment")]
        [Tooltip("")]
        public string path = string.Empty;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool disableTrigger = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public Vector3 positionOffset = Vector3.zero;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public Vector3 rotationOffset = Vector3.zero;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool overrideScale;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public Vector3 triggerScale = Vector3.one;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public AudioReverbTrigger? overrideTrigger;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool ignoreAudioChanges;

        /// <summary>
        ///     TODO.
        /// </summary>
        public ReverbTriggerAdjustment() { }
    }

    /// <summary>
    ///     TODO.
    /// </summary>
    public class ReverbTriggerAdjuster : MonoBehaviour
    {
        [Header("Reverb Trigger Adjuster")]
        [Tooltip("")]
        [SerializeField] private ReverbTriggerAdjustment[]? triggerAdjustments;

        private ReverbTriggerInfo[]? foundTriggers;

        private void Awake()
        {
            if (triggerAdjustments == null || triggerAdjustments.Length == 0)
            {
                return;
            }

            GameObject[]? rootObjects = SceneManager.GetSceneByName("SampleSceneRelay").GetRootGameObjects();

            if (rootObjects == null || rootObjects.Length == 0)
            {
                return;
            }

            Transform[] rootTransforms = new Transform[rootObjects.Length];

            for (int i = 0; i < rootObjects.Length; i++)
            {
                rootTransforms[i] = rootObjects[i].transform;
            }

            foundTriggers = new ReverbTriggerInfo[triggerAdjustments.Length];

            for (int i = 0; i < triggerAdjustments.Length; i++)
            {
                for (int j = 0; j < rootTransforms.Length; j++)
                {
                    Transform? triggerTransform = rootTransforms[j].Find(triggerAdjustments[i].path);

                    if (triggerTransform != null && triggerTransform.TryGetComponent(out AudioReverbTrigger reverbTrigger)
                        && triggerTransform.TryGetComponent(out BoxCollider triggerCollider))
                    {
                        foundTriggers[i] = new(reverbTrigger, triggerCollider);

                        break;
                    }
                }
            }
        }

        private void OnEnable()
        {
            if (foundTriggers == null || foundTriggers.Length == 0
                || triggerAdjustments == null || triggerAdjustments.Length == 0)
            {
                return;
            }

            for (int i = 0; i < foundTriggers.Length; i++)
            {
                AudioReverbTrigger reverbTrigger = foundTriggers[i].reverbTrigger;
                Transform reverbTriggerTransform = reverbTrigger.transform;

                if (reverbTrigger.TryGetComponent(out BoxCollider triggerCollider))
                {
                    AudioReverbTrigger? overrideTrigger = triggerAdjustments[i].overrideTrigger;
                    if (overrideTrigger != null)
                    {
                        ReverbTriggerInfo overrideInfo = new(overrideTrigger, triggerCollider);
                        overrideInfo.ApplyValuesTo(reverbTrigger);

                        if (!triggerAdjustments[i].ignoreAudioChanges)
                        {
                            reverbTrigger.audioChanges = [.. foundTriggers[i].audioChanges, .. reverbTrigger.audioChanges];
                        }

                        reverbTriggerTransform.SetLocalPositionAndRotation(foundTriggers[i].localPosition, foundTriggers[i].localRotation);
                        reverbTriggerTransform.localScale = foundTriggers[i].localScale;
                    }

                    triggerCollider.enabled = !triggerAdjustments[i].disableTrigger;

                    reverbTriggerTransform.SetLocalPositionAndRotation(reverbTriggerTransform.localPosition + triggerAdjustments[i].positionOffset,
                        reverbTriggerTransform.localRotation * Quaternion.Euler(triggerAdjustments[i].rotationOffset));

                    if (triggerAdjustments[i].overrideScale)
                    {
                        reverbTriggerTransform.localScale = triggerAdjustments[i].triggerScale;
                    }
                }
            }
        }

        private void OnDisable()
        {
            if (foundTriggers == null || foundTriggers.Length == 0)
            {
                return;
            }

            for (int i = 0; i < foundTriggers.Length; i++)
            {
                foundTriggers[i].ApplyValuesTo(foundTriggers[i].reverbTrigger);
            }
        }
    }
}