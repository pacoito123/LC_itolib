using System;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace itolib.Behaviours.Groupings
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class LightGroup : ComponentGroup<HDAdditionalLightData>
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        private enum LightActions : byte // TODO: Fade Distance, Volumetrics, Shadows
        {
            SetIntensity,
            IncrementIntensity,
            SetIntensityMultiplier,
            IncrementIntensityMultiplier,
            SetColorTemperature,
            IncrementColorTemperature,
            SetRange,
            IncrementRange,
            SetRadius,
            IncrementRadius,
            SetSpotOuterAngle,
            IncrementSpotOuterAngle,
            SetSpotInnerAngle,
            IncrementSpotInnerAngle,
            SetAreaRectangleShapeX,
            IncrementAreaRectangleShapeX,
            SetAreaRectangleShapeY,
            IncrementAreaRectangleShapeY,
            SetAreaTubeLength,
            IncrementAreaTubeLength,
        }

        /// <inheritdoc/>
        protected override void PerformSingleAction(HDAdditionalLightData component, Enum actionID, object? parameter = null)
        {
            if (actionID is not LightActions lightActionID)
            {
                return;
            }

            float floatValue = 0.0f;
            if (parameter is float num)
            {
                floatValue = num;
            }

            Light light = component.legacyLight;
            switch (lightActionID)
            {
                case LightActions.SetIntensity:
                    component.intensity = ClampBasedOnUnit(floatValue, component.lightUnit);
                    break;
                case LightActions.IncrementIntensity:
                    component.intensity = ClampBasedOnUnit(component.intensity + floatValue, component.lightUnit);
                    break;
                case LightActions.SetIntensityMultiplier:
                    component.lightDimmer = Mathf.Clamp(floatValue, 0.0f, 16.0f);
                    break;
                case LightActions.IncrementIntensityMultiplier:
                    component.lightDimmer = Mathf.Clamp(component.lightDimmer + floatValue, 0.0f, 16.0f);
                    break;
                case LightActions.SetColorTemperature:
                    light.useColorTemperature = true;
                    light.colorTemperature = Mathf.Clamp(floatValue, 1500.0f, 20000.0f);
                    break;
                case LightActions.IncrementColorTemperature:
                    light.useColorTemperature = true;
                    light.colorTemperature = Mathf.Clamp(light.colorTemperature + floatValue, 1500.0f, 20000.0f);
                    break;
                case LightActions.SetRange:
                    component.range = (floatValue < 0.001f) ? 0.001f : floatValue;
                    break;
                case LightActions.IncrementRange:
                    float range = component.range + floatValue;
                    component.range = (range < 0.001f) ? 0.001f : range;
                    break;
                case LightActions.SetRadius:
                    component.shapeRadius = (floatValue < 0.0f) ? 0.0f : floatValue;
                    break;
                case LightActions.IncrementRadius:
                    float radius = component.shapeRadius + floatValue;
                    component.shapeRadius = (radius < 0.0f) ? 0.0f : radius;
                    break;
                case LightActions.SetSpotOuterAngle:
                    light.spotAngle = Mathf.Clamp(floatValue, 1.0f, 179.0f);
                    break;
                case LightActions.IncrementSpotOuterAngle:
                    light.spotAngle = Mathf.Clamp(light.spotAngle + floatValue, 1.0f, 179.0f);
                    break;
                case LightActions.SetSpotInnerAngle:
                    light.innerSpotAngle = Mathf.Clamp(floatValue, 0.0f, 100.0f);
                    break;
                case LightActions.IncrementSpotInnerAngle:
                    light.innerSpotAngle = Mathf.Clamp(light.innerSpotAngle + floatValue, 0.0f, 100.0f);
                    break;
                case LightActions.SetAreaRectangleShapeX or LightActions.SetAreaTubeLength:
                    component.shapeWidth = (floatValue < 0.01f) ? 0.01f : floatValue;
                    break;
                case LightActions.IncrementAreaRectangleShapeX or LightActions.IncrementAreaTubeLength:
                    float width = component.shapeWidth + floatValue;
                    component.shapeWidth = (width < 0.01f) ? 0.01f : width;
                    break;
                case LightActions.SetAreaRectangleShapeY:
                    component.shapeHeight = (floatValue < 0.01f) ? 0.01f : floatValue;
                    break;
                case LightActions.IncrementAreaRectangleShapeY:
                    float height = component.shapeHeight + floatValue;
                    component.shapeHeight = (height < 0.01f) ? 0.01f : height;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="intensity"></param>
        public void SetIntensityAll(float intensity)
        {
            PerformGroupAction(LightActions.SetIntensity, intensity);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="intensity"></param>
        public void IncrementIntensityAll(float intensity)
        {
            PerformGroupAction(LightActions.IncrementIntensity, intensity);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="multiplier"></param>
        public void SetIntensityMultiplierAll(float multiplier)
        {
            PerformGroupAction(LightActions.SetIntensityMultiplier, multiplier);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="multiplier"></param>
        public void IncrementIntensityMultiplierAll(float multiplier)
        {
            PerformGroupAction(LightActions.IncrementIntensityMultiplier, multiplier);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="temperature"></param>
        public void SetColorTemperatureAll(float temperature)
        {
            PerformGroupAction(LightActions.SetColorTemperature, temperature);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="temperature"></param>
        public void IncrementColorTemperatureAll(float temperature)
        {
            PerformGroupAction(LightActions.IncrementColorTemperature, temperature);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="range"></param>
        public void SetRangeAll(float range)
        {
            PerformGroupAction(LightActions.SetRange, range);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="range"></param>
        public void IncrementRangeAll(float range)
        {
            PerformGroupAction(LightActions.IncrementRange, range);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="radius"></param>
        public void SetRadiusAll(float radius)
        {
            PerformGroupAction(LightActions.SetRadius, radius);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="radius"></param>
        public void IncrementRadiusAll(float radius)
        {
            PerformGroupAction(LightActions.IncrementRadius, radius);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="angle"></param>
        public void SetSpotOuterAngleAll(float angle)
        {
            PerformGroupAction(LightActions.SetSpotOuterAngle, angle);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="angle"></param>
        public void IncrementSpotOuterAngleAll(float angle)
        {
            PerformGroupAction(LightActions.IncrementSpotOuterAngle, angle);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="angle"></param>
        public void SetSpotInnerAngleAll(float angle)
        {
            PerformGroupAction(LightActions.SetSpotInnerAngle, angle);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="angle"></param>
        public void IncrementSpotInnerAngleAll(float angle)
        {
            PerformGroupAction(LightActions.IncrementSpotInnerAngle, angle);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="width"></param>
        public void SetAreaRectangleShapeXAll(float width)
        {
            PerformGroupAction(LightActions.SetAreaRectangleShapeX, width);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="width"></param>
        public void IncrementAreaRectangleShapeXAll(float width)
        {
            PerformGroupAction(LightActions.IncrementAreaRectangleShapeX, width);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="height"></param>
        public void SetAreaRectangleShapeYAll(float height)
        {
            PerformGroupAction(LightActions.SetAreaRectangleShapeY, height);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="height"></param>
        public void IncrementAreaRectangleShapeYAll(float height)
        {
            PerformGroupAction(LightActions.IncrementAreaRectangleShapeY, height);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="length"></param>
        public void SetAreaTubeLengthAll(float length)
        {
            PerformGroupAction(LightActions.SetAreaTubeLength, length);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="length"></param>
        public void IncrementAreaTubeLengthAll(float length)
        {
            PerformGroupAction(LightActions.IncrementAreaTubeLength, length);
        }

        /// <inheritdoc/>
        protected override void EnableSingleComponent(HDAdditionalLightData component, bool enabled)
        {
            component.enabled = enabled;
        }

        /// <inheritdoc/>
        protected override void ToggleSingleComponent(HDAdditionalLightData component)
        {
            component.enabled = !component.enabled;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="lightType"></param>
        /// <returns></returns>
        private static float ClampBasedOnUnit(float value, LightUnit lightType)
        {
            return lightType switch
            {
                LightUnit.Lumen or LightUnit.Nits => Mathf.Clamp(value, 0.0f, 40000.0f),
                LightUnit.Candela or LightUnit.Lux => Mathf.Clamp(value, 0, 3183.099f),
                LightUnit.Ev100 => (value < 14.63622f) ? value : 14.63622f,
                _ => 0.0f,
            };
        }
    }
}