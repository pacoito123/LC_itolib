using System;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace itolib.Structs
{
    [Serializable]
    internal struct ReverbTriggerInfo(AudioReverbTrigger reverbTrigger, BoxCollider triggerCollider)
    {
        public AudioReverbTrigger reverbTrigger = reverbTrigger;

        public Vector3 localPosition = reverbTrigger.transform.localPosition;
        public Quaternion localRotation = reverbTrigger.transform.localRotation;
        public Vector3 localScale = reverbTrigger.transform.localScale;

        public bool triggerEnabled = triggerCollider.enabled;

        public ReverbPreset? reverbPreset = reverbTrigger.reverbPreset;
        public int usePreset = reverbTrigger.usePreset;
        public switchToAudio[] audioChanges = [.. reverbTrigger.audioChanges];

        public bool elevatorTriggerForProps = reverbTrigger.elevatorTriggerForProps;
        public bool setInElevatorTrigger = reverbTrigger.setInElevatorTrigger;
        public bool isShipRoom = reverbTrigger.isShipRoom;

        public bool toggleLocalFog = reverbTrigger.toggleLocalFog;
        public float fogEnabledAmount = reverbTrigger.fogEnabledAmount;
        public LocalVolumetricFog localFog = reverbTrigger.localFog;

        public Terrain terrainObj = reverbTrigger.terrainObj;
        public bool setInsideAtmosphere = reverbTrigger.setInsideAtmosphere;
        public bool insideLighting = reverbTrigger.insideLighting;

        public int weatherEffect = reverbTrigger.weatherEffect;
        public bool effectEnabled = reverbTrigger.effectEnabled;
        public bool disableAllWeather = reverbTrigger.disableAllWeather;
        public bool enableCurrentLevelWeather = reverbTrigger.enableCurrentLevelWeather;
        public bool spectatedClientTriggered = reverbTrigger.spectatedClientTriggered;

        public readonly void ApplyValuesTo(AudioReverbTrigger? reverbTrigger)
        {
            if (reverbTrigger == null)
            {
                return;
            }

            reverbTrigger.transform.SetLocalPositionAndRotation(localPosition, localRotation);
            reverbTrigger.transform.localScale = localScale;

            if (reverbTrigger.TryGetComponent(out BoxCollider triggerCollider))
            {
                triggerCollider.enabled = triggerEnabled;
            }

            reverbTrigger.reverbPreset = reverbPreset;
            reverbTrigger.usePreset = usePreset;
            reverbTrigger.audioChanges = audioChanges;

            reverbTrigger.elevatorTriggerForProps = elevatorTriggerForProps;
            reverbTrigger.setInElevatorTrigger = setInElevatorTrigger;
            reverbTrigger.isShipRoom = isShipRoom;

            reverbTrigger.toggleLocalFog = toggleLocalFog;
            reverbTrigger.fogEnabledAmount = fogEnabledAmount;
            reverbTrigger.localFog = localFog;

            reverbTrigger.terrainObj = terrainObj;
            reverbTrigger.setInsideAtmosphere = setInsideAtmosphere;
            reverbTrigger.insideLighting = insideLighting;

            reverbTrigger.weatherEffect = weatherEffect;
            reverbTrigger.effectEnabled = effectEnabled;
            reverbTrigger.disableAllWeather = disableAllWeather;
            reverbTrigger.enableCurrentLevelWeather = enableCurrentLevelWeather;
            reverbTrigger.spectatedClientTriggered = spectatedClientTriggered;
        }
    }
}