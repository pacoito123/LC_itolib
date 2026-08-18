using itolib.Compatibility;
using itolib.Patches;
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
        ///     Callback invoked when <c>FacilityMeltdown</c> turns outside lights on.
        /// </summary>
        [Tooltip("Callback invoked when FacilityMeltdown turns outside lights on.")]
        [SerializeField] private UnityEvent onFacilityMeltdownLightsOn = new();

        /// <summary>
        ///     Callback invoked when <c>FacilityMeltdown</c> turns outside lights off.
        /// </summary>
        [Tooltip("Callback invoked when FacilityMeltdown turns outside lights off.")]
        [SerializeField] private UnityEvent onFacilityMeltdownLightsOff = new();

        /// <summary>
        ///     Subscribe to <c>LungProp</c>-related events.
        /// </summary>
        private void OnEnable()
        {
            ApparatusPatches.OnApparatusPulled += OnApparatusPull;
            ApparatusPatches.OnRadiationWarningHUD += OnRadiationWarning;

            if (FacilityMeltdownCompatibility.Enabled)
            {
                FacilityMeltdownCompatibility.RegisterMeltdownListener(OnMeltdownStart);
                FacilityMeltdownCompatibility.OnMeltdownLightsOn += OnMeltdownLightsOn;
                FacilityMeltdownCompatibility.OnMeltdownLightsOff += OnMeltdownLightsOff;
            }
        }

        /// <summary>
        ///     Unsubscribe from <c>LungProp</c>-related events.
        /// </summary>
        private void OnDisable()
        {
            ApparatusPatches.OnApparatusPulled -= OnApparatusPull;
            ApparatusPatches.OnRadiationWarningHUD -= OnRadiationWarning;

            if (FacilityMeltdownCompatibility.Enabled)
            {
                FacilityMeltdownCompatibility.RegisterMeltdownListener(OnMeltdownStart, remove: true);
                FacilityMeltdownCompatibility.OnMeltdownLightsOn -= OnMeltdownLightsOn;
                FacilityMeltdownCompatibility.OnMeltdownLightsOff -= OnMeltdownLightsOff;
            }
        }

        /// <summary>
        ///     Handle invoking event upon a <c>LungProp</c> being pulled.
        /// </summary>
        /// <param name="apparatus"><c>LungProp</c> that was just pulled, as a <c>GrabbableObject</c>.</param>
        private void OnApparatusPull(GrabbableObject apparatus)
        {
            onApparatusPull.Invoke(apparatus);

            if (runOnce)
            {
                ApparatusPatches.OnApparatusPulled -= OnApparatusPull;
            }
        }

        /// <summary>
        ///     Handle invoking event upon the radiation warning being displayed to players.
        /// </summary>
        private void OnRadiationWarning()
        {
            onRadiationWarning.Invoke();

            if (runOnce)
            {
                ApparatusPatches.OnRadiationWarningHUD -= OnRadiationWarning;
            }
        }

        /// <summary>
        ///     Handle invoking event upon <c>FacilityMeltdown</c> starting its countdown.
        /// </summary>
        private void OnMeltdownStart()
        {
            onFacilityMeltdown.Invoke();

            if (runOnce)
            {
                FacilityMeltdownCompatibility.RegisterMeltdownListener(OnMeltdownStart, remove: true);
            }
        }

        /// <summary>
        ///     Handle invoking event upon <c>FacilityMeltdown</c> turning outside lights on.
        /// </summary>
        private void OnMeltdownLightsOn()
        {
            onFacilityMeltdownLightsOn.Invoke();

            if (runOnce)
            {
                FacilityMeltdownCompatibility.OnMeltdownLightsOn -= OnMeltdownLightsOn;
            }
        }

        /// <summary>
        ///     Handle invoking event upon <c>FacilityMeltdown</c> turning outside lights off.
        /// </summary>
        private void OnMeltdownLightsOff()
        {
            onFacilityMeltdownLightsOff.Invoke();

            if (runOnce)
            {
                FacilityMeltdownCompatibility.OnMeltdownLightsOff -= OnMeltdownLightsOff;
            }
        }
    }
}