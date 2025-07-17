using itolib.Compatibility;
using itolib.Extensions;
using System;

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
        public bool IsProgressive { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        public LevelWeatherType CurrentWeather { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void Start()
        {
            if (CrowdControlCompatibility.Enabled)
            {
                CrowdControlCompatibility.CurrentWeather = TimeOfDay.Instance.currentLevel.currentWeather;
                CrowdControlCompatibility.OnCCWeatherChanged += ApplyConditional;
            }

            if (WeatherRegistryCompatibility.Enabled && WeatherRegistryCompatibility.ApplyWeatherOverrides(ApplyConditional))
            {
                return;
            }

            base.Start();
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
            ApplyConditional(TimeOfDay.Instance.currentLevelWeather);
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
                CurrentWeather = objectToCheck;
            }

            string weatherName = $"{objectToCheck}";

            for (int i = 0; i < conditionalOverrides?.Length; i++)
            {
                ConditionalOverride overrideEntry = conditionalOverrides[i];

                if (weatherName.CompareOrdinal(overrideEntry.nameToSearch))
                {
                    overrideEntry.Apply(undo);

                    return;
                }
                else if (overrideEntry.alsoAppliesTo?.Length > 0)
                {
                    for (int j = 0; j < overrideEntry.alsoAppliesTo.Length; j++)
                    {
                        if (weatherName.CompareOrdinal(overrideEntry.alsoAppliesTo[j]))
                        {
                            overrideEntry.Apply(undo);

                            return;
                        }
                    }
                }
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
            if (weatherTypes.Length == 0)
            {
                return;
            }

            if (IsProgressive && CurrentWeather != LevelWeatherType.None)
            {
                ApplyConditional(CurrentWeather, weatherTypes[0]);

                return;
            }

            string incomingWeather = $"{weatherTypes[0]}";

            if (incomingWeather.Contains('>'))
            {
                IsProgressive = true;

                return;
            }
            else if (incomingWeather.Contains('+'))
            {
                foreach (string weatherName in incomingWeather.Split(" + "))
                {
                    if (Enum.TryParse(weatherName, out LevelWeatherType weather))
                    {
                        ApplyConditional(weather);
                    }
                }

                return;
            }

            ApplyConditional(weatherTypes[0]);
        }
    }
}