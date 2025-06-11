using GameNetcodeStuff;
using itolib.Extensions;
using System.Linq;
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
        public int TargetedParamID { get; private set; } = -1;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Animation Param Setter")]
        [Tooltip("")]
        public Animator animator = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public string defaultParameterName = "";

        /// <summary>
        ///     TODO.
        /// </summary>
        public void Awake()
        {
            TargetedParamID = Animator.StringToHash(defaultParameterName);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void OnEnable()
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
            if (animator.parameters.Any(param => param.nameHash == paramID))
            {
                SwitchParamLocal(paramID);

                if (IsSpawned)
                {
                    SwitchParamServerRpc(GameNetworkManager.Instance.localPlayerController, paramID);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="paramID"></param>
        [ServerRpc(RequireOwnership = false)]
        public void SwitchParamServerRpc(NetworkBehaviourReference playerReference, int paramID)
        {
            SwitchParamClientRpc(playerReference, paramID);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="paramID"></param>
        [ClientRpc]
        public void SwitchParamClientRpc(NetworkBehaviourReference playerReference, int paramID)
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
        private void SwitchParamLocal(int paramID)
        {
            TargetedParamID = paramID;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="value"></param>
        public void SetBool(bool value)
        {
            if (TargetedParamID != -1 && animator.GetBool(TargetedParamID) != value)
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
        public void SetBoolServerRpc(NetworkBehaviourReference playerReference, bool value)
        {
            SetBoolClientRpc(playerReference, value);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="value"></param>
        [ClientRpc]
        public void SetBoolClientRpc(NetworkBehaviourReference playerReference, bool value)
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
        private void SetBoolLocal(bool value)
        {
            animator.SetBool(TargetedParamID, value);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="value"></param>
        public void SetFloat(float value)
        {
            if (TargetedParamID != -1 && animator.GetFloat(TargetedParamID) != value)
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
        public void SetFloatServerRpc(NetworkBehaviourReference playerReference, float value)
        {
            SetFloatClientRpc(playerReference, value);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="value"></param>
        [ClientRpc]
        public void SetFloatClientRpc(NetworkBehaviourReference playerReference, float value)
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
        private void SetFloatLocal(float value)
        {
            animator.SetFloat(TargetedParamID, value);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="value"></param>
        public void SetInt(int value)
        {
            if (TargetedParamID != -1 && animator.GetInteger(TargetedParamID) != value)
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
        public void SetIntServerRpc(NetworkBehaviourReference playerReference, int value)
        {
            SetIntClientRpc(playerReference, value);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="value"></param>
        [ClientRpc]
        public void SetIntClientRpc(NetworkBehaviourReference playerReference, int value)
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
        private void SetIntLocal(int value)
        {
            animator.SetInteger(TargetedParamID, value);
        }
    }
}