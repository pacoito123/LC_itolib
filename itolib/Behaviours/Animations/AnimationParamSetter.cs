using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Animations
{
    /// <summary>
    ///     Targets a specific <c>Animator</c> parameter of any type (which <i>can</i> be switched) to allow setting its value from an event call, while syncing it across clients.
    /// </summary>
    public class AnimationParamSetter : NetworkBehaviour
    {
        /// <summary>
        ///     <c>Animator</c> with a parameter of any type to target and set.
        /// </summary>
        [Header("Animation Param Setter")]
        [Tooltip("Animator with a parameter of any type to target and set.")]
        [SerializeField] private Animator animator = null!;

        /// <summary>
        ///     Name of the <c>Animator</c> parameter to target and set by default.
        /// </summary>
        [Tooltip("Name of the Animator parameter to target and set by default.")]
        [SerializeField] private string defaultParameterName = string.Empty;

        /// <summary>
        ///     Hash of the <c>Animator</c> parameter to target and set.
        /// </summary>
        private int targetedParamID;

        /// <summary>
        ///     Whether there is currently a valid <c>Animator</c> parameter target or not.
        /// </summary>
        private bool hasValidParameter;

        /// <summary>
        ///     Make sure there is an <c>Animator</c> component to target, and hash the default target <c>Animator</c> parameter.
        /// </summary>
        private void OnEnable()
        {
            if (animator == null && !TryGetComponent(out animator))
            {
                Plugin.Logger.LogWarning($"Could not find Animator for AnimationParamSetter component in GameObject '{gameObject.name}'.");
                hasValidParameter = false;
                enabled = false;

                return;
            }

            if (!hasValidParameter)
            {
                SwitchParamLocal(defaultParameterName);
            }
        }

        /// <summary>
        ///     Switch targeted <c>Animator</c> parameter.
        /// </summary>
        /// <param name="paramName">Name of the <c>Animator</c> parameter to target.</param>
        public void SwitchParam(string paramName)
        {
            int paramID = Animator.StringToHash(paramName);

            SwitchParam(paramID);
        }

        /// <summary>
        ///     Switch targeted <c>Animator</c> parameter.
        /// </summary>
        /// <param name="paramID">Hash of the <c>Animator</c> parameter to target.</param>
        private void SwitchParam(int paramID)
        {
            SwitchParamLocal(paramID);

            if (hasValidParameter && IsSpawned)
            {
                SwitchParamRpc(paramID);
            }
        }

        /// <summary>
        ///     Switch targeted <c>Animator</c> parameter for all other clients.
        /// </summary>
        /// <param name="paramID">Hash of the <c>Animator</c> parameter to target.</param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void SwitchParamRpc(int paramID)
        {
            SwitchParamLocal(paramID);
        }

        /// <summary>
        ///     Switch targeted <c>Animator</c> parameter for the local client.
        /// </summary>
        /// <param name="paramName">Name of the <c>Animator</c> parameter to target.</param>
        public void SwitchParamLocal(string paramName)
        {
            int paramID = Animator.StringToHash(paramName);

            SwitchParamLocal(paramID);
        }

        /// <summary>
        ///     Switch targeted <c>Animator</c> parameter for the local client.
        /// </summary>
        /// <param name="paramID">Hash of the <c>Animator</c> parameter to target.</param>
        private void SwitchParamLocal(int paramID)
        {
            if (hasValidParameter && paramID == targetedParamID)
            {
                return;
            }

            for (int i = 0; i < animator.parameters.Length; i++)
            {
                if (animator.parameters[i].nameHash == paramID)
                {
                    targetedParamID = paramID;
                    hasValidParameter = true;

                    return;
                }
            }

            Plugin.Logger.LogWarning($"Could not find Animator parameter with hash '{paramID}' for AnimationParamSetter component in GameObject '{gameObject.name}'.");
        }

        /// <summary>
        ///     Set value for the targeted <c>Bool</c> <c>Animator</c> parameter.
        /// </summary>
        /// <param name="value">Value to set the targeted <c>Bool</c> <c>Animator</c> parameter to.</param>
        public void SetBool(bool value)
        {
            if (hasValidParameter && animator.GetBool(targetedParamID) != value)
            {
                animator.SetBool(targetedParamID, value);

                if (IsSpawned)
                {
                    SetBoolRpc(targetedParamID, value);
                }
            }
        }

        /// <summary>
        ///     Set value for the given <c>Bool</c> <c>Animator</c> parameter for all other clients.
        /// </summary>
        /// <param name="paramID">Hash of the <c>Bool</c> <c>Animator</c> parameter to target.</param>
        /// <param name="value">Value to set the targeted <c>Bool</c> <c>Animator</c> parameter to.</param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void SetBoolRpc(int paramID, bool value)
        {
            if (paramID != targetedParamID)
            {
                SwitchParamLocal(paramID);
            }

            SetBoolLocal(value);
        }

        /// <summary>
        ///     Set value for the targeted <c>Bool</c> <c>Animator</c> parameter for the local client.
        /// </summary>
        /// <param name="value">Value to set the targeted <c>Bool</c> <c>Animator</c> parameter to.</param>
        public void SetBoolLocal(bool value)
        {
            if (hasValidParameter && animator.GetBool(targetedParamID) != value)
            {
                animator.SetBool(targetedParamID, value);
            }
        }

        /// <summary>
        ///     Toggle value for the targeted <c>Bool</c> <c>Animator</c> parameter.
        /// </summary>
        public void ToggleBool()
        {
            if (hasValidParameter)
            {
                SetBool(!animator.GetBool(targetedParamID));
            }
        }

        /// <summary>
        ///     Toggle value for the targeted <c>Bool</c> <c>Animator</c> parameter for the local client.
        /// </summary>
        public void ToggleBoolLocal()
        {
            if (hasValidParameter)
            {
                SetBoolLocal(!animator.GetBool(targetedParamID));
            }
        }

        /// <summary>
        ///     Set value for the given <c>Float</c> <c>Animator</c> parameter.
        /// </summary>
        /// <param name="value">Value to set the targeted <c>Float</c> <c>Animator</c> parameter to.</param>
        public void SetFloat(float value)
        {
            if (hasValidParameter && animator.GetFloat(targetedParamID) != value)
            {
                animator.SetFloat(targetedParamID, value);

                if (IsSpawned)
                {
                    SetFloatRpc(targetedParamID, value);
                }
            }
        }

        /// <summary>
        ///     Set value for the given <c>Float</c> <c>Animator</c> parameter for all other clients.
        /// </summary>
        /// <param name="paramID">Hash of the <c>Float</c> <c>Animator</c> parameter to target.</param>
        /// <param name="value">Value to set the targeted <c>Float</c> <c>Animator</c> parameter to.</param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void SetFloatRpc(int paramID, float value)
        {
            if (paramID != targetedParamID)
            {
                SwitchParamLocal(paramID);
            }

            SetFloatLocal(value);
        }

        /// <summary>
        ///     Set value for the targeted <c>Float</c> <c>Animator</c> parameter for the local client.
        /// </summary>
        /// <param name="value">Value to set the targeted <c>Float</c> <c>Animator</c> parameter to.</param>
        public void SetFloatLocal(float value)
        {
            if (hasValidParameter && animator.GetFloat(targetedParamID) != value)
            {
                animator.SetFloat(targetedParamID, value);
            }
        }

        /// <summary>
        ///     Increment value of the targeted <c>Float</c> <c>Animator</c> parameter.
        /// </summary>
        /// <param name="value">Value to increment the targeted <c>Float</c> <c>Animator</c> parameter by.</param>
        public void IncrementFloat(float value)
        {
            if (hasValidParameter)
            {
                SetFloat(animator.GetFloat(targetedParamID) + value);
            }
        }

        /// <summary>
        ///     Increment value of the targeted <c>Float</c> <c>Animator</c> parameter for the local client.
        /// </summary>
        /// <param name="value">Value to increment the targeted <c>Float</c> <c>Animator</c> parameter by.</param>
        public void IncrementFloatLocal(float value)
        {
            if (hasValidParameter)
            {
                SetFloatLocal(animator.GetFloat(targetedParamID) + value);
            }
        }

        /// <summary>
        ///     Multiply value of the targeted <c>Float</c> <c>Animator</c> parameter.
        /// </summary>
        /// <param name="value">Value to multiply the targeted <c>Float</c> <c>Animator</c> parameter by.</param>
        public void MultiplyFloat(float value)
        {
            if (hasValidParameter)
            {
                SetFloat(animator.GetFloat(targetedParamID) * value);
            }
        }

        /// <summary>
        ///     Multiply value of the targeted <c>Float</c> <c>Animator</c> parameter for the local client.
        /// </summary>
        /// <param name="value">Value to multiply the targeted <c>Float</c> <c>Animator</c> parameter by.</param>
        public void MultiplyFloatLocal(float value)
        {
            if (hasValidParameter)
            {
                SetFloatLocal(animator.GetFloat(targetedParamID) * value);
            }
        }

        /// <summary>
        ///     Set value for the given <c>Integer</c> <c>Animator</c> parameter.
        /// </summary>
        /// <param name="value">Value to set the targeted <c>Integer</c> <c>Animator</c> parameter to.</param>
        public void SetInt(int value)
        {
            if (hasValidParameter && animator.GetInteger(targetedParamID) != value)
            {
                animator.SetInteger(targetedParamID, value);

                if (IsSpawned)
                {
                    SetIntRpc(targetedParamID, value);
                }
            }
        }

        /// <summary>
        ///     Set value for the given <c>Integer</c> <c>Animator</c> parameter for all other clients.
        /// </summary>
        /// <param name="paramID">Hash of the <c>Integer</c> <c>Animator</c> parameter to target.</param>
        /// <param name="value">Value to set the targeted <c>Integer</c> <c>Animator</c> parameter to.</param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void SetIntRpc(int paramID, int value)
        {
            if (paramID != targetedParamID)
            {
                SwitchParamLocal(paramID);
            }

            SetIntLocal(value);
        }

        /// <summary>
        ///     Set value for the targeted <c>Integer</c> <c>Animator</c> parameter for the local client.
        /// </summary>
        /// <param name="value">Value to set the targeted <c>Integer</c> <c>Animator</c> parameter to.</param>
        public void SetIntLocal(int value)
        {
            if (hasValidParameter && animator.GetInteger(targetedParamID) != value)
            {
                animator.SetInteger(targetedParamID, value);
            }
        }

        /// <summary>
        ///     Increment value of the targeted <c>Integer</c> <c>Animator</c> parameter.
        /// </summary>
        /// <param name="value">Value to increment the targeted <c>Integer</c> <c>Animator</c> parameter by.</param>
        public void IncrementInt(int value)
        {
            if (hasValidParameter)
            {
                SetInt(animator.GetInteger(targetedParamID) + value);
            }
        }

        /// <summary>
        ///     Increment value of the targeted <c>Integer</c> <c>Animator</c> parameter for the local client.
        /// </summary>
        /// <param name="value">Value to increment the targeted <c>Integer</c> <c>Animator</c> parameter by.</param>
        public void IncrementIntLocal(int value)
        {
            if (hasValidParameter)
            {
                SetInt(animator.GetInteger(targetedParamID) + value);
            }
        }

        /// <summary>
        ///     Multiply value of the targeted <c>Integer</c> <c>Animator</c> parameter.
        /// </summary>
        /// <param name="value">Value to multiply the targeted <c>Integer</c> <c>Animator</c> parameter by.</param>
        public void MultiplyInt(float value)
        {
            if (hasValidParameter)
            {
                SetInt((int)(animator.GetInteger(targetedParamID) * value));
            }
        }

        /// <summary>
        ///     Multiply value of the targeted <c>Integer</c> <c>Animator</c> parameter for the local client.
        /// </summary>
        /// <param name="value">Value to multiply the targeted <c>Integer</c> <c>Animator</c> parameter by.</param>
        public void MultiplyIntLocal(float value)
        {
            if (hasValidParameter)
            {
                SetInt((int)(animator.GetInteger(targetedParamID) * value));
            }
        }

        /// <summary>
        ///     Set or reset the targeted <c>Trigger</c> <c>Animator</c> parameter.
        /// </summary>
        /// <param name="reset">Whether to reset the targeted <c>Trigger</c> <c>Animator</c> parameter or not.</param>
        public void SetTrigger(bool reset)
        {
            if (hasValidParameter)
            {
                SetTriggerLocal(reset);

                if (IsSpawned)
                {
                    SetTriggerRpc(targetedParamID, reset);
                }
            }
        }

        /// <summary>
        ///     Set or reset the targeted <c>Trigger</c> <c>Animator</c> parameter for all other clients.
        /// </summary>
        /// <param name="paramID">Hash of the <c>Trigger</c> <c>Animator</c> parameter to target.</param>
        /// <param name="reset">Whether to reset the targeted <c>Trigger</c> <c>Animator</c> parameter or not.</param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void SetTriggerRpc(int paramID, bool reset)
        {
            if (paramID != targetedParamID)
            {
                SwitchParamLocal(paramID);
            }

            SetTriggerLocal(reset);
        }

        /// <summary>
        ///     Set or reset the targeted <c>Trigger</c> <c>Animator</c> parameter for the local client.
        /// </summary>
        /// <param name="reset">Whether to reset the targeted <c>Trigger</c> <c>Animator</c> parameter or not.</param>
        public void SetTriggerLocal(bool reset)
        {
            if (!reset)
            {
                animator.SetTrigger(targetedParamID);
            }
            else
            {
                animator.ResetTrigger(targetedParamID);
            }
        }
    }
}