using itolib.Extensions;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Notifications
{
    /// <summary>
    ///     TODO.
    /// </summary>
    internal sealed class AlertSignal : NetworkBehaviour
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public static UnlockableItem? SignalTranslatorUnlockableItem
        {
            get
            {
                if (field == null && StartOfRound.Instance != null)
                {
                    field = StartOfRound.Instance.unlockablesList.unlockables.Find(unlockable =>
                        unlockable.unlockableName.CompareOrdinal("Signal translator"));
                }

                return field;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public static Transform? SignalTranslatorContainer
        {
            get
            {
                if (field == null && SignalTranslatorUnlockableItem != null && SignalTranslatorUnlockableItem.prefabObject != null)
                {
                    field = SignalTranslatorUnlockableItem.prefabObject.transform.GetChild(2);
                }

                return field;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public static AudioClip[]? TypeTextClips
        {
            get
            {
                if (field == null && SignalTranslatorContainer != null && SignalTranslatorContainer.TryGetComponent(out SignalTranslator signal))
                {
                    field = signal.typeTextClips;
                }

                return field;
            }
        }
    }
}