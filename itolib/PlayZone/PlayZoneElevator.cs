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
        public UnityEvent<bool>? onDoorsOpen;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<bool>? onDoorsClose;

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

            switch (CurrentState)
            {
                case ElevatorState.IdleUp:
                case ElevatorState.IdleDown:
                case ElevatorState.GoingUp:
                    if (elevatorAudioFinish != null && elevatorSource != null)
                    {
                        elevatorSource.PlayOneShot(elevatorAudioFinish);
                    }

                    if (callElevatorUpper != null && callElevatorLower != null)
                    {
                        callElevatorUpper.hoverTip = "Open Door";
                        callElevatorLower.hoverTip = "Call Elevator";
                    }

                    onElevatorTravelFinish?.Invoke(true);
                    break;
                case ElevatorState.GoingDown:
                    if (elevatorAudioFinish != null && elevatorSource != null)
                    {
                        elevatorSource.PlayOneShot(elevatorAudioFinish);
                    }

                    if (callElevatorUpper != null && callElevatorLower != null)
                    {
                        callElevatorUpper.hoverTip = "Call Elevator";
                        callElevatorLower.hoverTip = "Open Door";
                    }

                    onElevatorTravelFinish?.Invoke(true);
                    break;
                case ElevatorState.Stuck:
                default:
                    break;
            }

            CurrentState = newState;
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
                    if (up)
                    {
                        OpenDoors();
                    }
                    else
                    {
                        MoveElevator(up);
                    }

                    break;
                case ElevatorState.IdleDown:
                    if (!up)
                    {
                        OpenDoors();
                    }
                    else
                    {
                        MoveElevator(up);
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
        /// <param name="up"></param>
        public void MoveElevator(bool up)
        {
            switch (CurrentState)
            {
                case ElevatorState.IdleUp:
                    if (!up && elevatorAnimator?.GetBool("ElevatorGoingUp") == true)
                    {
                        if (elevatorAudioFinish != null && elevatorSource != null)
                        {
                            elevatorSource.Play();
                        }

                        elevatorAnimator.SetBool("ElevatorGoingUp", false);
                        onElevatorTravelStart?.Invoke(true);
                    }

                    break;
                case ElevatorState.IdleDown:
                    if (up && elevatorAnimator?.GetBool("ElevatorGoingUp") == false)
                    {
                        if (elevatorAudioFinish != null && elevatorSource != null)
                        {
                            elevatorSource.Play();
                        }

                        elevatorAnimator.SetBool("ElevatorGoingUp", true);
                        onElevatorTravelStart?.Invoke(false);
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