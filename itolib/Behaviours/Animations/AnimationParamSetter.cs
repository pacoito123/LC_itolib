using GameNetcodeStuff;
using itolib.Extensions;
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
        [SerializeField] private Animator animator = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private string defaultParameterName = string.Empty;

        /// <summary>
        ///     TODO.
        /// </summary>
        private int targetedParamID = -1;

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
            if (animator == null)
            {
                Plugin.StaticLogger.LogWarning($"Could not find Animator for AnimationParamSetter component in GameObject '{gameObject.name}'.");
                enabled = false;

                return;
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
                        SwitchParamServerRpc(GameNetworkManager.Instance.localPlayerController, paramID);
                    }

                    break;
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="paramID"></param>
        [ServerRpc(RequireOwnership = false)]
        private void SwitchParamServerRpc(NetworkBehaviourReference playerReference, int paramID)
        {
            SwitchParamClientRpc(playerReference, paramID);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="paramID"></param>
        [ClientRpc]
        private void SwitchParamClientRpc(NetworkBehaviourReference playerReference, int paramID)
        {
            if (playerReference.TryGet(out PlayerControllerB player) && !player.IsLocalClient())
            {
                SwitchParamLocal(paramID);
            }
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
            if (targetedParamID != -1 && animator.GetBool(targetedParamID) != value)
            {
                SetBoolLocal(value);

                if (IsSpawned)
                {
                    SetBoolServerRpc(GameNetworkManager.Instance.localPlayerController, value);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="value"></param>
        [ServerRpc(RequireOwnership = false)]
        private void SetBoolServerRpc(NetworkBehaviourReference playerReference, bool value)
        {
            SetBoolClientRpc(playerReference, value);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="value"></param>
        [ClientRpc]
        private void SetBoolClientRpc(NetworkBehaviourReference playerReference, bool value)
        {
            if (playerReference.TryGet(out PlayerControllerB player) && !player.IsLocalClient())
            {
                SetBoolLocal(value);
            }
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
            if (targetedParamID != -1 && animator.GetFloat(targetedParamID) != value)
            {
                SetFloatLocal(value);

                if (IsSpawned)
                {
                    SetFloatServerRpc(GameNetworkManager.Instance.localPlayerController, value);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="value"></param>
        [ServerRpc(RequireOwnership = false)]
        private void SetFloatServerRpc(NetworkBehaviourReference playerReference, float value)
        {
            SetFloatClientRpc(playerReference, value);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="value"></param>
        [ClientRpc]
        private void SetFloatClientRpc(NetworkBehaviourReference playerReference, float value)
        {
            if (playerReference.TryGet(out PlayerControllerB player) && !player.IsLocalClient())
            {
                SetFloatLocal(value);
            }
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
            if (targetedParamID != -1 && animator.GetInteger(targetedParamID) != value)
            {
                SetIntLocal(value);

                if (IsSpawned)
                {
                    SetIntServerRpc(GameNetworkManager.Instance.localPlayerController, value);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="value"></param>
        [ServerRpc(RequireOwnership = false)]
        private void SetIntServerRpc(NetworkBehaviourReference playerReference, int value)
        {
            SetIntClientRpc(playerReference, value);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="value"></param>
        [ClientRpc]
        private void SetIntClientRpc(NetworkBehaviourReference playerReference, int value)
        {
            if (playerReference.TryGet(out PlayerControllerB player) && !player.IsLocalClient())
            {
                SetIntLocal(value);
            }
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
            if (targetedParamID != -1)
            {
                SetTriggerLocal(reset);

                if (IsSpawned)
                {
                    SetTriggerServerRpc(GameNetworkManager.Instance.localPlayerController, reset);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="reset"></param>
        [ServerRpc(RequireOwnership = false)]
        private void SetTriggerServerRpc(NetworkBehaviourReference playerReference, bool reset)
        {
            SetTriggerClientRpc(playerReference, reset);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="reset"></param>
        [ClientRpc]
        private void SetTriggerClientRpc(NetworkBehaviourReference playerReference, bool reset)
        {
            if (playerReference.TryGet(out PlayerControllerB player) && !player.IsLocalClient())
            {
                SetTriggerLocal(reset);
            }
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