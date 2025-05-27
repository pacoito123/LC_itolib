using UnityEngine;

namespace itolib.Behaviours.Effects
{
    /// <summary>
    ///     Disables v67's interior fog, if it's rolled for the current round.
    /// </summary>
    public class SpookyDefog : MonoBehaviour
    {
        /// <summary>
        ///     Wait until the round starts to deactivate interior fog.
        /// </summary>
        public void Start()
        {
            StartOfRound.Instance?.StartNewRoundEvent.AddListener(DeactivateSpookyFog);
        }

        /// <summary>
        ///     Unsubscribe from the StartNewRound event when destroyed.
        /// </summary>
        public void OnDestroy()
        {
            StartOfRound.Instance?.StartNewRoundEvent.RemoveListener(DeactivateSpookyFog);
        }

        /// <summary>
        ///     Simply turn off the 'SpookyFog' GameObject.
        /// </summary>
        public void DeactivateSpookyFog()
        {
            RoundManager.Instance?.indoorFog?.gameObject.SetActive(false);
        }
    }
}