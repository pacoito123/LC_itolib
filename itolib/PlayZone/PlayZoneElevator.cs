using itolib.Enums;
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
        [SerializeField] private Animator? elevatorAnimator;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private Animator? doorAnimatorUpper;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private Animator? doorAnimatorLower;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Buttons")]
        [Tooltip("")]
        [SerializeField] private InteractTrigger? callElevatorUpper;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private InteractTrigger? callElevatorLower;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private InteractTrigger? openDoors;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private InteractTrigger? closeDoors;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Audio")]
        [Tooltip("")]
        [SerializeField] private AudioSource? elevatorSource;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private AudioSource? doorSourceUpper;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private AudioSource? doorSourceLower;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private AudioClip? elevatorAudioTravel;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private AudioClip? elevatorAudioFinish;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private AudioClip? doorAudioOpen;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private AudioClip? doorAudioClose;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header(header: "Events")]
        [Tooltip("")]
        [SerializeField] private UnityEvent<bool> onElevatorTravelStart = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent<bool> onElevatorTravelFinish = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent onTopReached = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent onBottomReached = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent onDeactivate = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent<bool> onDoorsOpen = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent<bool> onDoorsClose = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="newState"></param>
        public void SwitchState(ElevatorState newState)
        {
            if (CurrentState != newState && CurrentState is not ElevatorState.Deactivated)
            {
                SwitchStateLocal(newState);

                if (IsSpawned)
                {
                    SwitchStateRpc(newState);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="newState"></param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void SwitchStateRpc(ElevatorState newState)
        {
            SwitchStateLocal(newState);
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

                    if (elevatorAnimator != null)
                    {
                        elevatorAnimator.SetBool("ElevatorGoingUp", up);
                    }

                    onElevatorTravelStart.Invoke(up);

                    CurrentState = newState;

                    break;
                case ElevatorState.Deactivated:
                    if (elevatorAnimator != null)
                    {
                        elevatorAnimator.SetTrigger("Deactivated");
                    }

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

                if (IsSpawned)
                {
                    ToggleDoorsRpc(open);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="open"></param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void ToggleDoorsRpc(bool open)
        {
            ToggleDoorsLocal(open);
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
                    if (doorAnimatorUpper != null && doorAnimatorUpper.GetBool("Open") == !open)
                    {
                        if (doorSourceUpper != null && doorAudioOpen != null)
                        {
                            doorSourceUpper.PlayOneShot(doorAudioOpen);
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
                    if (doorAnimatorLower != null && doorAnimatorLower.GetBool("Open") == !open)
                    {
                        if (doorSourceLower != null && doorAudioOpen != null)
                        {
                            doorSourceLower.PlayOneShot(doorAudioOpen);
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

            if (doorAnimatorLower != null && doorAnimatorLower.GetBool("Open"))
            {
                doorAnimatorLower.SetBool("Open", false);

                if (doorSourceLower != null && doorAudioOpen != null)
                {
                    doorSourceLower.PlayOneShot(doorAudioClose);
                }
            }

            if (doorAnimatorUpper != null && doorAnimatorUpper.GetBool("Open"))
            {
                doorAnimatorUpper.SetBool("Open", false);

                if (doorSourceUpper != null && doorAudioOpen != null)
                {
                    doorSourceUpper.PlayOneShot(doorAudioClose);
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

            if (doorSourceLower != null && doorAudioOpen != null)
            {
                doorSourceLower.PlayOneShot(doorAudioOpen);
            }

            if (doorAnimatorLower != null && doorAnimatorUpper != null)
            {
                doorAnimatorLower.SetBool("Open", true);
                doorAnimatorUpper.SetBool("Open", false);
            }
        }
    }
}