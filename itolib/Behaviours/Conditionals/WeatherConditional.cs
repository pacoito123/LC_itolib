using itolib.Compatibility;
using itolib.Extensions;

namespace itolib.Behaviours.Conditionals
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class WeatherConditional : BaseConditional<LevelWeatherType>
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public LevelWeatherType LastWeather { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void Awake()
        {
            if (CrowdControlCompatibility.Enabled && TimeOfDay.Instance != null && TimeOfDay.Instance.currentLevel != null)
            {
                CrowdControlCompatibility.CurrentWeather = TimeOfDay.Instance.currentLevel.currentWeather;
                CrowdControlCompatibility.OnCCWeatherChanged += ApplyConditional;
            }

            if (WeatherRegistryCompatibility.Enabled && WeatherRegistryCompatibility.ApplyWeatherOverrides(ApplyConditional))
            {
                return;
            }

            base.Awake();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void OnDestroy()
        {
            if (WeatherRegistryCompatibility.Enabled)
            {
                WeatherRegistryCompatibility.OnWeatherEffectsApply -= ApplyConditional;
            }

            if (CrowdControlCompatibility.Enabled)
            {
                CrowdControlCompatibility.OnCCWeatherChanged -= ApplyConditional;
            }

            base.OnDestroy();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="undo"></param>
        public override void ApplyConditional(bool undo)
        {
            if (TimeOfDay.Instance != null)
            {
                ApplyConditional(TimeOfDay.Instance.currentLevelWeather, undo);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="objectToCheck"></param>
        /// <param name="undo"></param>
        public override void ApplyConditional(LevelWeatherType objectToCheck, bool undo)
        {
            if (!undo)
            {
                LastWeather = objectToCheck;
            }

            string weatherName = $"{objectToCheck}";
            bool foundMatch = false;

            for (int i = 0; i < conditionalOverrides?.Length; i++)
            {
                ConditionalOverride overrideEntry = conditionalOverrides[i];

                if (weatherName.CompareOrdinal(overrideEntry.nameToSearch))
                {
                    overrideEntry.Apply(undo);
                    foundMatch = true;

                    continue;
                }
                else if (overrideEntry.alsoAppliesTo?.Length > 0)
                {
                    for (int j = 0; j < overrideEntry.alsoAppliesTo.Length; j++)
                    {
                        if (weatherName.CompareOrdinal(overrideEntry.alsoAppliesTo[j]))
                        {
                            overrideEntry.Apply(undo);
                            foundMatch = true;

                            continue;
                        }
                    }
                }

                if (!undo)
                {
                    overrideEntry.onConditionalFail.Invoke();
                }
            }

            if (!foundMatch && WeatherTweaksCompatibility.Enabled
                && WeatherTweaksCompatibility.IsCombinedWeather(out LevelWeatherType[]? allWeathers, objectToCheck))
            {
                for (int i = 0; i < allWeathers?.Length; i++)
                {
                    ApplyConditional(allWeathers[i]);
                }

                LastWeather = objectToCheck;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="previousWeather"></param>
        /// <param name="incomingWeather"></param>
        public void ApplyConditional(LevelWeatherType previousWeather, LevelWeatherType incomingWeather)
        {
            ApplyConditional(previousWeather, undo: true);
            ApplyConditional(incomingWeather);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="weatherTypes"></param>
        public void ApplyConditional(LevelWeatherType[] weatherTypes)
        {
            if (LastWeather != LevelWeatherType.None
                && WeatherTweaksCompatibility.Enabled && !WeatherTweaksCompatibility.IsProgressingWeather(LastWeather))
            {
                ApplyConditional(LastWeather, undo: true);
            }

            for (int i = 0; i < weatherTypes?.Length; i++)
            {
                ApplyConditional(weatherTypes[i]);
            }
        }
    }
}