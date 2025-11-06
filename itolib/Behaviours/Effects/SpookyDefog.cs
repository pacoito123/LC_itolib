using System.Collections;
using itolib.Util;
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
        private void Start()
        {
            if (StartOfRound.Instance != null)
            {
                StartOfRound.Instance.StartNewRoundEvent.AddListener(DeactivateSpookyFog);
            }
        }

        /// <summary>
        ///     Unsubscribe from the StartNewRound event when destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (StartOfRound.Instance != null)
            {
                StartOfRound.Instance.StartNewRoundEvent.RemoveListener(DeactivateSpookyFog);
            }
        }

        /// <summary>
        ///     Simply turn off the 'SpookyFog' GameObject.
        /// </summary>
        private void DeactivateSpookyFog()
        {
            if (RoundManager.Instance != null && RoundManager.Instance.indoorFog != null)
            {
                _ = StartCoroutine(DeactivateSpookyFogDelayed());
            }
        }

        /// <summary>
        ///     Coroutine to turn off the 'SpookyFog' GameObject after 5 seconds.
        /// </summary>
        private static IEnumerator DeactivateSpookyFogDelayed() // TODO: Handle through SimulateAnomaly.
        {
            yield return Yielders.WaitForSeconds(5.0f);
            RoundManager.Instance.indoorFog.gameObject.SetActive(false);
        }
    }
}