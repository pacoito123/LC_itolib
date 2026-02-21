using UnityEngine;

namespace itolib.Behaviours.Helpers
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class SaneReverbTrigger : AudioReverbTrigger
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Sane Reverb Trigger")]
        [Tooltip("")]
        [SerializeField] private float delayCheck;

        /// <summary>
        ///     TODO.
        /// </summary>
        [SerializeField] private bool onlyOnEnter;

        /// <summary>
        ///     TODO.
        /// </summary>
        private float timeSinceLastCheck;

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Reset()
        {
            if (TryGetComponent(out AudioReverbTrigger reverbTrigger))
            {
                reverbPreset = reverbTrigger.reverbPreset;
                usePreset = reverbTrigger.usePreset;
                audioChanges = [.. reverbTrigger.audioChanges];
                elevatorTriggerForProps = reverbTrigger.elevatorTriggerForProps;
                setInElevatorTrigger = reverbTrigger.setInElevatorTrigger;
                isShipRoom = reverbTrigger.isShipRoom;
                toggleLocalFog = reverbTrigger.toggleLocalFog;
                fogEnabledAmount = reverbTrigger.fogEnabledAmount;
                localFog = reverbTrigger.localFog;
                terrainObj = reverbTrigger.terrainObj;
                setInsideAtmosphere = reverbTrigger.setInsideAtmosphere;
                insideLighting = reverbTrigger.insideLighting;
                weatherEffect = reverbTrigger.weatherEffect;
                effectEnabled = reverbTrigger.effectEnabled;
                disableAllWeather = reverbTrigger.disableAllWeather;
                enableCurrentLevelWeather = reverbTrigger.enableCurrentLevelWeather;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Awake()
        {
            timeSinceLastCheck = delayCheck;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter(Collider other)
        {
            if (onlyOnEnter)
            {
                base.OnTriggerStay(other);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="other"></param>
        public new void OnTriggerStay(Collider other)
        {
            if (onlyOnEnter)
            {
                return;
            }

            if (delayCheck == 0)
            {
                base.OnTriggerStay(other);

                return;
            }

            if (timeSinceLastCheck <= delayCheck)
            {
                timeSinceLastCheck += Time.deltaTime;

                return;
            }

            timeSinceLastCheck = 0.0f;

            base.OnTriggerStay(other);
        }
    }
}