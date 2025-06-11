using GameNetcodeStuff;
using itolib.Enums;
using itolib.Extensions;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.PlayZone
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class PlayZoneElevator : NetworkBehaviour
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public ElevatorState CurrentState { get; private set; } = ElevatorState.IdleDown;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("PlayZone Elevator")]
        [Tooltip("")]
        public Animator? elevatorAnimator;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public Animator? doorAnimatorUpper;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public Animator? doorAnimatorLower;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Buttons")]
        [Tooltip("")]
        public InteractTrigger? callElevatorUpper;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public InteractTrigger? callElevatorLower;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public InteractTrigger? openDoors;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public InteractTrigger? closeDoors;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Audio")]
        [Tooltip("")]
        public AudioSource? elevatorSource;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public AudioSource? doorSourceUpper;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public AudioSource? doorSourceLower;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public AudioClip? elevatorAudioTravel;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public AudioClip? elevatorAudioFinish;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public AudioClip? doorAudioOpen;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public AudioClip? doorAudioClose;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header(header: "Events")]
        [Tooltip("")]
        public UnityEvent<bool> onElevatorTravelStart = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<bool> onElevatorTravelFinish = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent onTopReached = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent onBottomReached = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent onDeactivate = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<bool> onDoorsOpen = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<bool> onDoorsClose = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="newState"></param>
        public void SwitchState(ElevatorState newState)
        {
            if (CurrentState != newState && CurrentState is not ElevatorState.Deactivated)
            {
                SwitchStateLocal(newState);
                SwitchStateServerRpc(GameNetworkManager.Instance.localPlayerController, newState);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="newState"></param>
        [ServerRpc(RequireOwnership = false)]
        public void SwitchStateServerRpc(NetworkBehaviourReference playerReference, ElevatorState newState)
        {
            SwitchStateClientRpc(playerReference, newState);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="newState"></param>
        [ClientRpc]
        public void SwitchStateClientRpc(NetworkBehaviourReference playerReference, ElevatorState newState)
        {
            if (playerReference.TryGet(out PlayerControllerB player) && !player.IsLocalClient())
            {
                SwitchStateLocal(newState);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="newState"></param>
        public void SwitchStateLocal(ElevatorState newState)
        {
            if (CurrentState == newState)
            {
                return;
            }

            bool up;
            switch (newState)
            {
                case ElevatorState.IdleUp:
                case ElevatorState.IdleDown:
                    up = newState is ElevatorState.IdleUp;

                    if (elevatorAudioFinish != null && elevatorSource != null)
                    {
                        elevatorSource.Stop();
                        elevatorSource.PlayOneShot(elevatorAudioFinish);
                    }

                    if (callElevatorUpper != null && callElevatorLower != null)
                    {
                        callElevatorUpper.hoverTip = up ? "Open door : [LMB]" : "Call : [LMB]";
                        callElevatorLower.hoverTip = up ? "Call : [LMB]" : "Open door : [LMB]";
                    }

                    CurrentState = newState;

                    if (up)
                    {
                        onTopReached.Invoke();
                    }
                    else
                    {
                        onBottomReached.Invoke();
                    }

                    onElevatorTravelFinish.Invoke(up);

                    break;
                case ElevatorState.GoingUp:
                case ElevatorState.GoingDown:
                    up = newState is ElevatorState.GoingUp;

                    if (elevatorAudioFinish != null && elevatorSource != null)
                    {
                        elevatorSource.Play();
                    }

                    elevatorAnimator?.SetBool("ElevatorGoingUp", up);
                    onElevatorTravelStart.Invoke(up);

                    CurrentState = newState;

                    break;
                case ElevatorState.Deactivated:
                    elevatorAnimator?.SetTrigger("Deactivated");

                    CurrentState = newState;

                    break;
                default:
                    break;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="up"></param>
        public void CallElevator(bool up)
        {
            switch (CurrentState)
            {
                case ElevatorState.IdleUp:
                    if (!up)
                    {
                        SwitchState(ElevatorState.GoingDown);
                    }
                    else
                    {
                        ToggleDoors(open: true);
                    }
                    break;
                case ElevatorState.IdleDown:
                    if (up)
                    {
                        SwitchState(ElevatorState.GoingUp);
                    }
                    else
                    {
                        ToggleDoors(open: true);
                    }
                    break;
                case ElevatorState.GoingUp:
                case ElevatorState.GoingDown:
                case ElevatorState.Deactivated:
                default:
                    break;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="up"></param>
        public void CallElevatorLocal(bool up)
        {
            switch (CurrentState)
            {
                case ElevatorState.IdleUp:
                    if (!up)
                    {
                        SwitchStateLocal(ElevatorState.GoingDown);
                    }
                    else
                    {
                        ToggleDoorsLocal(open: true);
                    }
                    break;
                case ElevatorState.IdleDown:
                    if (up)
                    {
                        SwitchStateLocal(ElevatorState.GoingUp);
                    }
                    else
                    {
                        ToggleDoorsLocal(open: true);
                    }
                    break;
                case ElevatorState.GoingUp:
                case ElevatorState.GoingDown:
                case ElevatorState.Deactivated:
                default:
                    break;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="open"></param>
        public void ToggleDoors(bool open)
        {
            if (CurrentState is not ElevatorState.Deactivated)
            {
                ToggleDoorsLocal(open);
                ToggleDoorsServerRpc(GameNetworkManager.Instance.localPlayerController, open);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="open"></param>
        [ServerRpc(RequireOwnership = false)]
        public void ToggleDoorsServerRpc(NetworkBehaviourReference playerReference, bool open)
        {
            ToggleDoorsClientRpc(playerReference, open);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="open"></param>
        [ClientRpc]
        public void ToggleDoorsClientRpc(NetworkBehaviourReference playerReference, bool open)
        {
            if (playerReference.TryGet(out PlayerControllerB player) && !player.IsLocalClient())
            {
                ToggleDoorsLocal(open);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="open"></param>
        public void ToggleDoorsLocal(bool open)
        {
            switch (CurrentState)
            {
                case ElevatorState.IdleUp:
                    if (doorAnimatorUpper?.GetBool("Open") == !open)
                    {
                        if (doorAudioOpen != null)
                        {
                            doorSourceUpper?.PlayOneShot(doorAudioOpen);
                        }

                        doorAnimatorUpper.SetBool("Open", open);

                        if (open)
                        {
                            onDoorsOpen.Invoke(true);
                        }
                        else
                        {
                            onDoorsClose.Invoke(true);
                        }
                    }

                    break;
                case ElevatorState.IdleDown:
                    if (doorAnimatorLower?.GetBool("Open") == !open)
                    {
                        if (doorAudioOpen != null)
                        {
                            doorSourceLower?.PlayOneShot(doorAudioOpen);
                        }

                        doorAnimatorLower.SetBool("Open", open);

                        if (open)
                        {
                            onDoorsOpen.Invoke(false);
                        }
                        else
                        {
                            onDoorsClose.Invoke(false);
                        }
                    }

                    break;
                case ElevatorState.GoingUp:
                case ElevatorState.GoingDown:
                case ElevatorState.Deactivated:
                default:
                    break;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void DeactivateElevator()
        {
            switch (CurrentState)
            {
                case ElevatorState.IdleUp:
                    SwitchState(ElevatorState.GoingDown);
                    break;
                case ElevatorState.IdleDown:
                    SwitchState(ElevatorState.GoingUp);
                    break;
                case ElevatorState.GoingUp:
                case ElevatorState.GoingDown:
                case ElevatorState.Deactivated:
                default:
                    break;
            }

            if (doorAnimatorLower?.GetBool("Open") == true)
            {
                doorAnimatorLower?.SetBool("Open", false);

                if (doorAudioOpen != null)
                {
                    doorSourceLower?.PlayOneShot(doorAudioClose);
                }
            }

            if (doorAnimatorUpper?.GetBool("Open") == true)
            {
                doorAnimatorUpper?.SetBool("Open", false);

                if (doorAudioOpen != null)
                {
                    doorSourceUpper?.PlayOneShot(doorAudioClose);
                }
            }

            _ = StartCoroutine(DeactivateElevatorDelayed());
        }

        private IEnumerator DeactivateElevatorDelayed()
        {
            yield return new WaitForSeconds(1.5f);
            SwitchState(ElevatorState.Deactivated);
            yield break;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void OnDeactivate()
        {
            onDeactivate.Invoke();

            if (elevatorAudioFinish != null && elevatorSource != null)
            {
                elevatorSource.Stop();
                elevatorSource.PlayOneShot(elevatorAudioFinish);
            }

            if (doorAudioOpen != null)
            {
                doorSourceLower?.PlayOneShot(doorAudioOpen);
            }

            doorAnimatorLower?.SetBool("Open", true);
            doorAnimatorUpper?.SetBool("Open", false);
        }
    }
}