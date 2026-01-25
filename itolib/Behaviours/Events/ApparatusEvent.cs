using itolib.Compatibility;
using itolib.Patches;
using LethalLevelLoader;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Events
{
    /// <summary>
    /// 	Represents an event invoked by a <c>LungProp</c> being pulled.
    /// </summary>
    public class ApparatusEvent : MonoBehaviour
    {
        /// <summary>
        ///     Callback invoked when a <c>LungProp</c> is pulled by a player.
        /// </summary>
        [Header("Apparatus Event")]
        [Tooltip("Callback invoked when an Apparatus is pulled by a player.")]
        [SerializeField] private UnityEvent<GrabbableObject> onApparatusPull = new();

        /// <summary>
        ///     Callback invoked when the radiation warning is displayed to players.
        /// </summary>
        /// <remarks>This is also when Old Birds are woken up.</remarks>
        [Tooltip("Callback invoked when the radiation warning is displayed to players. This is also when Old Birds are woken up.")]
        [SerializeField] private UnityEvent onRadiationWarning = new();

        /// <summary>
        ///     Whether the event should be invoked only on the first pull or not, in case there are multiple.
        /// </summary>
        [Tooltip("Whether the event should be invoked only on the first pull or not, in case there are multiple.")]
        [SerializeField] private bool runOnce = true;

        /// <summary>
        ///     Callback invoked when <c>FacilityMeltdown</c> starts its countdown.
        /// </summary>
        [Space(5.0f)]
        [Header("Compatibility")]
        [Tooltip("Callback invoked when FacilityMeltdown starts its countdown.")]
        [SerializeField] private UnityEvent onFacilityMeltdown = new();

        /// <summary>
        ///     Whether a <c>LungProp</c> has already been pulled by a player or not.
        /// </summary>
        private bool hasBeenPulled;

        /// <summary>
        ///     Whether the radiation warning has already been displayed to players or not.
        /// </summary>
        private bool hasRadiationWarningShown;

        /// <summary>
        ///     Whether <c>FacilityMeltdown</c> has started counting down or not.
        /// </summary>
        private bool hasMeltdownStarted;

        /// <summary>
        ///     Subscribe to <c>LungProp</c>-related events.
        /// </summary>
        private void OnEnable()
        {
            LevelManager.GlobalLevelEvents.onApparatusTaken.AddListener(OnApparatusPull);
            ApparatusPatches.OnRadiationWarningHUD += OnRadiationWarning;

            if (FacilityMeltdownCompatibility.Enabled)
            {
                FacilityMeltdownCompatibility.RegisterMeltdownListener(OnMeltdownStart);
            }
        }

        /// <summary>
        ///     Unsubscribe from <c>LungProp</c>-related events.
        /// </summary>
        private void OnDisable()
        {
            LevelManager.GlobalLevelEvents.onApparatusTaken.RemoveListener(OnApparatusPull);
            ApparatusPatches.OnRadiationWarningHUD -= OnRadiationWarning;

            if (FacilityMeltdownCompatibility.Enabled)
            {
                FacilityMeltdownCompatibility.RegisterMeltdownListener(OnMeltdownStart, remove: true);
            }
        }

        /// <summary>
        ///     Handle invoking event upon a <c>LungProp</c> being pulled.
        /// </summary>
        /// <param name="apparatus"><c>LungProp</c> that was just pulled, as a <c>GrabbableObject</c>.</param>
        private void OnApparatusPull(GrabbableObject apparatus)
        {
            if (!runOnce || !hasBeenPulled)
            {
                onApparatusPull.Invoke(apparatus);
                hasBeenPulled = true;
            }
        }

        /// <summary>
        ///     Handle invoking event upon the radiation warning being displayed to players.
        /// </summary>
        private void OnRadiationWarning()
        {
            if (!runOnce || !hasRadiationWarningShown)
            {
                onRadiationWarning.Invoke();
                hasRadiationWarningShown = true;
            }
        }

        /// <summary>
        ///     Handle invoking event upon <c>FacilityMeltdown</c> starting its countdown.
        /// </summary>
        private void OnMeltdownStart()
        {
            if (!runOnce || !hasMeltdownStarted)
            {
                onFacilityMeltdown.Invoke();
                hasMeltdownStarted = true;
            }
        }
    }
}