using itolib.Compatibility;
using itolib.Patches;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Conditionals
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [Serializable]
    public struct WeatherOverride
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public string weatherName = "";

        /// <summary>
        ///     TODO.
        /// </summary>
        public List<string> alsoAppliesTo;

        /// <summary>
        ///     TODO.
        /// </summary>
        public UnityEvent? onWeatherChange;

        /// <summary>
        ///     TODO.
        /// </summary>
        public List<GameObject> effectsToEnable;

        /// <summary>
        ///     TODO.
        /// </summary>
        public List<GameObject> effectsToDisable;

        /// <summary>
        ///     TODO.
        /// </summary>
        public WeatherOverride()
        {
            alsoAppliesTo = [];
            effectsToEnable = [];
            effectsToDisable = [];
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="undo"></param>
        public readonly void Apply(bool undo = false)
        {
            effectsToEnable?.ForEach(weatherEffect => weatherEffect?.SetActive(!undo));
            effectsToDisable?.ForEach(weatherEffect => weatherEffect?.SetActive(undo));

            if (!undo)
            {
                onWeatherChange?.Invoke();
            }
        }
    }

    /// <summary>
    ///     TODO.
    /// </summary>
    public class WeatherControl : MonoBehaviour
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public List<WeatherOverride> weatherOverrides = [];

        /* /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Rainy")]
        [Tooltip("")]
        public bool disableRainDuringRainy = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool disableRainAmbienceDuringRainy = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Stormy")]
        [Tooltip("")]
        public bool disableRainDuringStormy = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool disableRainAmbienceDuringStormy = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Foggy")]
        [Tooltip("")]
        public bool disableFogDuringFoggy = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public float overrideFogDensity = 4.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Flooded")]
        [Tooltip("")]
        public bool disableRainDuringFlooded = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool disableRainAmbienceDuringFlooded = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool disableWaterDuringFlooded = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool disableWaterAmbienceDuringFlooded = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Eclipsed")]
        [Tooltip("")]
        public bool disableMusicDuringEclipsed = false; */

        /// <summary>
        ///     TODO.
        /// </summary>
        public bool IsProgressive { get; private set; } = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        public string CurrentWeather { get; private set; } = "";

        private void Awake()
        {
            // ModifyVanillaEffects();

            if (CrowdControlCompatibility.Enabled)
            {
                CrowdControlCompatibility.CurrentWeather = TimeOfDay.Instance.currentLevel.currentWeather;
                CrowdControlCompatibility.OnCCWeatherChanged += ModifyWeather;
            }

            if (WeatherRegistryCompatibility.Enabled && WeatherRegistryCompatibility.ApplyWeatherOverrides(ModifyWeather))
            {
                return;
            }

            LoadPatch.OnFinishGeneratingLevelPost += ModifyWeather;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void ModifyWeather()
        {
            ModifyWeather($"{TimeOfDay.Instance.currentLevelWeather}");
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void ModifyWeather(string weatherType)
        {
            CurrentWeather = weatherType;

            foreach (WeatherOverride weather in weatherOverrides)
            {
                if (string.CompareOrdinal(weatherType, weather.weatherName) == 0
                    || (weather.alsoAppliesTo.Count > 0 && weather.alsoAppliesTo.Contains(weatherType)))
                {
                    weather.Apply();
                    return;
                }
            }

            Plugin.StaticLogger.LogWarning($"Unknown weather '{weatherType}' found, there might be weird stuff going on!");
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="previousWeather"></param>
        /// <param name="incomingWeather"></param>
        public void ModifyWeather(string previousWeather, string incomingWeather)
        {
            foreach (WeatherOverride weather in weatherOverrides)
            {
                if (string.CompareOrdinal(previousWeather, weather.weatherName) == 0
                    || (weather.alsoAppliesTo.Count > 0 && weather.alsoAppliesTo.Contains(previousWeather)))
                {
                    weather.Apply(undo: true);
                    break;
                }
            }

            ModifyWeather(incomingWeather);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void ModifyWeather(LevelWeatherType[] weatherTypes)
        {
            if (weatherTypes.Length == 0)
            {
                return;
            }

            string incomingWeather = $"{weatherTypes[0]}";

            if (IsProgressive && CurrentWeather.Length > 0)
            {
                ModifyWeather(CurrentWeather, incomingWeather);
                return;
            }

            if (incomingWeather.Contains('>'))
            {
                IsProgressive = true;
                return;
            }
            else if (incomingWeather.Contains('+'))
            {
                foreach (string weather in incomingWeather.Split(" + "))
                {
                    ModifyWeather(weather);
                }
                return;
            }

            ModifyWeather($"{incomingWeather}");
        }

        /* /// <summary>
        ///     TODO.
        /// </summary>
        public void ModifyVanillaEffects()
        {
            // TODO: Disable Systems/Rendering/PlayerHUDHelmetModel/ScavengerHelmet/Plane?

            // Obtain parent of all (or most) vanilla weather effects.
            Transform weatherContainer = TimeOfDay.Instance.transform;

            // Disable vanilla Rainy effects.
            Transform rain = weatherContainer.Find("RainParticleContainer");

            rain?.GetChild(0)?.gameObject.SetActive(!disableRainDuringRainy);
            rain?.GetChild(1)?.gameObject.SetActive(!disableRainAmbienceDuringRainy);

            // Disable vanilla Stormy effects.
            Transform stormyRain = weatherContainer.Find("StormyRainParticleContainer");

            stormyRain?.GetChild(0)?.gameObject.SetActive(!disableRainDuringStormy);
            stormyRain?.GetChild(1)?.gameObject.SetActive(!disableRainAmbienceDuringStormy);

            // Disable vanilla Foggy effects.
            if (weatherContainer.Find("Foggy")?.TryGetComponent(out LocalVolumetricFog fog) == true)
            {
                fog.enabled = !disableFogDuringFoggy;

                if (overrideFogDensity >= 0.0f)
                {
                    fog.parameters.meanFreePath = overrideFogDensity;
                }
            }

            // Disable vanilla Flooded effects.
            Transform flood = weatherContainer.Find("Flooding");

            if (flood.TryGetComponent(out FloodWeather floodWeather))
            {
                floodWeather.enabled = !disableWaterDuringFlooded && !disableWaterAmbienceDuringFlooded;
            }

            flood?.GetChild(0)?.gameObject.SetActive(!disableWaterDuringFlooded);
            flood?.GetChild(1)?.gameObject.SetActive(!disableWaterDuringFlooded);
            flood?.GetChild(2)?.gameObject.SetActive(!disableWaterAmbienceDuringFlooded);

            Transform floodRain = weatherContainer.Find("RainParticleContainer");

            floodRain?.GetChild(0)?.gameObject.SetActive(!disableRainDuringFlooded);
            floodRain?.GetChild(1)?.gameObject.SetActive(!disableRainAmbienceDuringFlooded);

            // Disable vanilla Eclipsed effects.
            TimeOfDay.Instance.transform.Find("Eclipse")?.GetChild(0)?.gameObject.SetActive(!disableMusicDuringEclipsed);
        } */

        private void OnDestroy()
        {
            LoadPatch.OnFinishGeneratingLevelPost -= ModifyWeather;

            if (WeatherRegistryCompatibility.Enabled)
            {
                WeatherRegistryCompatibility.OnWeatherEffectsApply -= ModifyWeather;
            }

            if (CrowdControlCompatibility.Enabled)
            {
                CrowdControlCompatibility.OnCCWeatherChanged -= ModifyWeather;
            }

            /* if (TimeOfDay.Instance == null)
            {
                return;
            }

            // Obtain parent of all (or most) vanilla weather effects.
            Transform weatherContainer = TimeOfDay.Instance.transform;

            // Reenable vanilla Rainy effects.
            Transform rain = weatherContainer.Find("RainParticleContainer");
            for (int i = 0; i < 2; i++)
            {
                rain?.GetChild(i)?.gameObject.SetActive(true);
            }

            // Reenable vanilla Stormy effects.
            Transform stormyRain = weatherContainer.Find("StormyRainParticleContainer");
            for (int i = 0; i < 2; i++)
            {
                stormyRain?.GetChild(i)?.gameObject.SetActive(true);
            }

            // Reenable vanilla Foggy effects.
            if (weatherContainer.Find("Foggy")?.TryGetComponent(out LocalVolumetricFog fog) == true)
            {
                fog.enabled = true;
                fog.parameters.meanFreePath = 4;
            }

            // Reenable vanilla Flooded effects.
            Transform flood = weatherContainer.Find("Flooding");
            for (int i = 0; i < 3; i++)
            {
                flood?.GetChild(i)?.gameObject.SetActive(true);
            }

            // Reenable vanilla Eclipsed effects.
            weatherContainer.Find("Eclipse")?.GetChild(0)?.gameObject.SetActive(true); */
        }
    }
}