using UnityEngine;

namespace itolib.Behaviours.Materials
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class MaterialParamSetter : MonoBehaviour
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Material Param Setter")]
        [Tooltip("")]
        [SerializeField] private Material material;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private string defaultParameterName = string.Empty;

        /// <summary>
        ///     TODO.
        /// </summary>
        private int targetedParamID;

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Awake()
        {
            targetedParamID = Shader.PropertyToID(defaultParameterName);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void OnEnable()
        {
            if (material == null)
            {
                Plugin.StaticLogger.LogWarning($"Could not find Material for MaterialParamSetter component in GameObject '{gameObject.name}'.");
                enabled = false;

                return;
            }

            if (targetedParamID != 0 && !material.HasProperty(defaultParameterName))
            {
                Plugin.StaticLogger.LogWarning($"Could not find Material parameter '{defaultParameterName}' for Material '{material.name}' in MaterialParamSetter component in GameObject '{gameObject.name}'.");
                targetedParamID = 0;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="paramName"></param>
        public void SwitchParam(string paramName)
        {
            int paramID = Shader.PropertyToID(paramName);

            if (material.HasProperty(paramID))
            {
                targetedParamID = paramID;

                return;
            }

            Plugin.StaticLogger.LogWarning($"Could not find Material parameter '{paramName}' for Material '{material.name}' in MaterialParamSetter component in GameObject '{gameObject.name}'.");
            targetedParamID = 0;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="value"></param>
        public void SetFloat(float value)
        {
            if (targetedParamID != 0 && material.HasFloat(targetedParamID) && material.GetFloat(targetedParamID) != value)
            {
                material.SetFloat(targetedParamID, value);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="value"></param>
        public void SetInteger(int value)
        {
            if (targetedParamID != 0 && material.HasInteger(targetedParamID) && material.GetInteger(targetedParamID) != value)
            {
                material.SetInteger(targetedParamID, value);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="value"></param>
        public void SetTexture(Texture value)
        {
            if (targetedParamID != 0 && material.HasTexture(targetedParamID) && material.GetTexture(targetedParamID) != value)
            {
                material.SetTexture(targetedParamID, value);
            }
        }
    }
}