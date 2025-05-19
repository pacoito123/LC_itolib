using GameNetcodeStuff;
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
        public Animator? animator;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public string defaultParameterName = "";

        /// <summary>
        ///     TODO.
        /// </summary>
        public void Start()
        {
            TargetedParamID = Animator.StringToHash(defaultParameterName);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="paramName"></param>
        public void SwitchParam(string paramName)
        {
            int paramID = Animator.StringToHash(paramName);
            if (animator?.parameters.Any(param => param.nameHash == paramID) == true)
            {
                SwitchParamLocal(paramID);
                SwitchParamServerRpc(GameNetworkManager.Instance.localPlayerController.GetComponent<NetworkObject>(), paramID);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="paramID"></param>
        [ServerRpc(RequireOwnership = false)]
        public void SwitchParamServerRpc(NetworkObjectReference playerReference, int paramID)
        {
            SwitchParamClientRpc(playerReference, paramID);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="paramID"></param>
        [ClientRpc]
        public void SwitchParamClientRpc(NetworkObjectReference playerReference, int paramID)
        {
            if (playerReference.TryGet(out NetworkObject playerNetworkObject)
                && playerNetworkObject.TryGetComponent(out PlayerControllerB player)
                && player.actualClientId != GameNetworkManager.Instance.localPlayerController.actualClientId)
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
            if (TargetedParamID != -1)
            {
                SetBoolLocal(value);
                SetBoolServerRpc(GameNetworkManager.Instance.localPlayerController.GetComponent<NetworkObject>(), value);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="value"></param>
        [ServerRpc(RequireOwnership = false)]
        public void SetBoolServerRpc(NetworkObjectReference playerReference, bool value)
        {
            SetBoolClientRpc(playerReference, value);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="value"></param>
        [ClientRpc]
        public void SetBoolClientRpc(NetworkObjectReference playerReference, bool value)
        {
            if (playerReference.TryGet(out NetworkObject playerNetworkObject)
                && playerNetworkObject.TryGetComponent(out PlayerControllerB player)
                && player.actualClientId != GameNetworkManager.Instance.localPlayerController.actualClientId)
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
            animator?.SetBool(TargetedParamID, value);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="value"></param>
        public void SetFloat(float value)
        {
            if (TargetedParamID != -1)
            {
                SetFloatLocal(value);
                SetFloatServerRpc(GameNetworkManager.Instance.localPlayerController.GetComponent<NetworkObject>(), value);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="value"></param>
        [ServerRpc(RequireOwnership = false)]
        public void SetFloatServerRpc(NetworkObjectReference playerReference, float value)
        {
            SetFloatClientRpc(playerReference, value);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="value"></param>
        [ClientRpc]
        public void SetFloatClientRpc(NetworkObjectReference playerReference, float value)
        {
            if (playerReference.TryGet(out NetworkObject playerNetworkObject)
                && playerNetworkObject.TryGetComponent(out PlayerControllerB player)
                && player.actualClientId != GameNetworkManager.Instance.localPlayerController.actualClientId)
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
            animator?.SetFloat(TargetedParamID, value);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="value"></param>
        public void SetInt(int value)
        {
            if (TargetedParamID != -1)
            {
                SetIntLocal(value);
                SetIntServerRpc(GameNetworkManager.Instance.localPlayerController.GetComponent<NetworkObject>(), value);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="value"></param>
        [ServerRpc(RequireOwnership = false)]
        public void SetIntServerRpc(NetworkObjectReference playerReference, int value)
        {
            SetIntClientRpc(playerReference, value);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="value"></param>
        [ClientRpc]
        public void SetIntClientRpc(NetworkObjectReference playerReference, int value)
        {
            if (playerReference.TryGet(out NetworkObject playerNetworkObject)
                && playerNetworkObject.TryGetComponent(out PlayerControllerB player)
                && player.actualClientId != GameNetworkManager.Instance.localPlayerController.actualClientId)
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
            animator?.SetInteger(TargetedParamID, value);
        }
    }
}