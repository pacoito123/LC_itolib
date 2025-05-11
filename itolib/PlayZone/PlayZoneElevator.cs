using itolib.Enums;
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
        public UnityEvent<bool>? onElevatorTravelStart;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<bool>? onElevatorTravelFinish;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent? onTopReached;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent? onBottomReached;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<bool>? onDoorsOpen;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<bool>? onDoorsClose;

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="speedMultiplier"></param>
        public void ChangeSpeedLocal(float speedMultiplier)
        {
            if (elevatorAnimator != null && elevatorAnimator.GetFloat("speed") != speedMultiplier)
            {
                ChangeSpeed(speedMultiplier);
                ChangeSpeedServerRpc(speedMultiplier);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="speedMultiplier"></param>
        [ServerRpc(RequireOwnership = false)]
        public void ChangeSpeedServerRpc(float speedMultiplier)
        {
            ChangeSpeedClientRpc(speedMultiplier);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="speedMultiplier"></param>
        [ClientRpc]
        public void ChangeSpeedClientRpc(float speedMultiplier)
        {
            ChangeSpeed(speedMultiplier);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="speedMultiplier"></param>
        public void ChangeSpeed(float speedMultiplier)
        {
            if (elevatorAnimator != null && elevatorAnimator.GetFloat("speed") != speedMultiplier)
            {
                elevatorAnimator.SetFloat("speed", speedMultiplier);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="newState"></param>
        public void SwitchStateLocal(ElevatorState newState)
        {
            if (CurrentState != newState)
            {
                SwitchState(newState);
                SwitchStateServerRpc(newState);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="newState"></param>
        [ServerRpc(RequireOwnership = false)]
        public void SwitchStateServerRpc(ElevatorState newState)
        {
            SwitchStateClientRpc(newState);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="newState"></param>
        [ClientRpc]
        public void SwitchStateClientRpc(ElevatorState newState)
        {
            SwitchState(newState);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="newState"></param>
        public void SwitchState(ElevatorState newState)
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
                        onTopReached?.Invoke();
                    }
                    else
                    {
                        onBottomReached?.Invoke();
                    }

                    onElevatorTravelFinish?.Invoke(up);

                    break;
                case ElevatorState.GoingUp:
                case ElevatorState.GoingDown:
                    up = newState is ElevatorState.GoingUp;

                    if (elevatorAudioFinish != null && elevatorSource != null)
                    {
                        elevatorSource.Play();
                    }

                    elevatorAnimator?.SetBool("ElevatorGoingUp", up);
                    onElevatorTravelStart?.Invoke(up);

                    CurrentState = newState;

                    break;
                case ElevatorState.Stuck:
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
                        SwitchStateLocal(ElevatorState.GoingDown);
                    }
                    else
                    {
                        OpenDoors();
                    }
                    break;
                case ElevatorState.IdleDown:
                    if (up)
                    {
                        SwitchStateLocal(ElevatorState.GoingUp);
                    }
                    else
                    {
                        OpenDoors();
                    }
                    break;
                case ElevatorState.GoingUp:
                case ElevatorState.GoingDown:
                case ElevatorState.Stuck:
                default:
                    break;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void OpenDoors()
        {
            switch (CurrentState)
            {
                case ElevatorState.IdleUp:
                    if (doorAnimatorUpper?.GetBool("Open") == false)
                    {
                        if (doorAudioOpen != null)
                        {
                            doorSourceUpper?.PlayOneShot(doorAudioOpen);
                        }

                        doorAnimatorUpper.SetBool("Open", true);
                        onDoorsOpen?.Invoke(true);
                    }

                    break;
                case ElevatorState.IdleDown:
                    if (doorAnimatorLower?.GetBool("Open") == false)
                    {
                        if (doorAudioOpen != null)
                        {
                            doorSourceLower?.PlayOneShot(doorAudioOpen);
                        }

                        doorAnimatorLower.SetBool("Open", true);
                        onDoorsOpen?.Invoke(false);
                    }

                    break;
                case ElevatorState.GoingUp:
                case ElevatorState.GoingDown:
                case ElevatorState.Stuck:
                default:
                    break;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void CloseDoors()
        {
            switch (CurrentState)
            {
                case ElevatorState.IdleUp:
                    if (doorAnimatorUpper?.GetBool("Open") == true)
                    {
                        if (doorAudioOpen != null)
                        {
                            doorSourceUpper?.PlayOneShot(doorAudioClose);
                        }

                        doorAnimatorUpper.SetBool("Open", false);
                        onDoorsClose?.Invoke(true);
                    }

                    break;
                case ElevatorState.IdleDown:
                    if (doorAnimatorLower?.GetBool("Open") == true)
                    {
                        if (doorAudioOpen != null)
                        {
                            doorSourceLower?.PlayOneShot(doorAudioClose);
                        }

                        doorAnimatorLower.SetBool("Open", false);
                        onDoorsClose?.Invoke(false);
                    }

                    break;
                case ElevatorState.GoingUp:
                case ElevatorState.GoingDown:
                case ElevatorState.Stuck:
                default:
                    break;
            }
        }
    }
}