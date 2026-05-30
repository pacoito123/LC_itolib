using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Animations
{
    /// <summary>
    /// 	TODO.
    /// </summary>
    public class AnimationParamSetter : NetworkBehaviour
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Animation Param Setter")]
        [Tooltip("")]
        [SerializeField] private Animator animator;

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
            targetedParamID = Animator.StringToHash(defaultParameterName);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void OnEnable()
        {
            if (animator == null && !TryGetComponent(out animator))
            {
                Plugin.StaticLogger.LogWarning($"Could not find Animator for AnimationParamSetter component in GameObject '{gameObject.name}'.");
                enabled = false;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="paramName"></param>
        public void SwitchParam(string paramName)
        {
            int paramID = Animator.StringToHash(paramName);

            for (int i = 0; i < animator.parameters.Length; i++)
            {
                if (animator.parameters[i].nameHash == paramID)
                {
                    SwitchParamLocal(paramID);

                    if (IsSpawned)
                    {
                        SwitchParamRpc(paramID);
                    }

                    break;
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="paramID"></param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void SwitchParamRpc(int paramID)
        {
            SwitchParamLocal(paramID);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="paramID"></param>
        public void SwitchParamLocal(int paramID)
        {
            targetedParamID = paramID;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="value"></param>
        public void SetBool(bool value)
        {
            if (targetedParamID != 0 && animator.GetBool(targetedParamID) != value)
            {
                SetBoolLocal(value);

                if (IsSpawned)
                {
                    SetBoolRpc(value);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="value"></param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void SetBoolRpc(bool value)
        {
            SetBoolLocal(value);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="value"></param>
        public void SetBoolLocal(bool value)
        {
            animator.SetBool(targetedParamID, value);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="value"></param>
        public void SetFloat(float value)
        {
            if (targetedParamID != 0 && animator.GetFloat(targetedParamID) != value)
            {
                SetFloatLocal(value);

                if (IsSpawned)
                {
                    SetFloatRpc(value);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="value"></param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void SetFloatRpc(float value)
        {
            SetFloatLocal(value);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="value"></param>
        public void SetFloatLocal(float value)
        {
            animator.SetFloat(targetedParamID, value);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="value"></param>
        public void SetInt(int value)
        {
            if (targetedParamID != 0 && animator.GetInteger(targetedParamID) != value)
            {
                SetIntLocal(value);

                if (IsSpawned)
                {
                    SetIntRpc(value);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="value"></param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void SetIntRpc(int value)
        {
            SetIntLocal(value);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="value"></param>
        public void SetIntLocal(int value)
        {
            animator.SetInteger(targetedParamID, value);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="reset"></param>
        public void SetTrigger(bool reset)
        {
            if (targetedParamID != 0)
            {
                SetTriggerLocal(reset);

                if (IsSpawned)
                {
                    SetTriggerRpc(reset);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="reset"></param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void SetTriggerRpc(bool reset)
        {
            SetTriggerLocal(reset);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="reset"></param>
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