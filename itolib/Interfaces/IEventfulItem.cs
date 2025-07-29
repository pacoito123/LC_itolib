using GameNetcodeStuff;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Interfaces
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public interface IEventfulItem
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        UnityEvent<bool, bool> OnActivate { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        UnityEvent<Collider> OnActivatePhysicsTrigger { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        UnityEvent OnBatteryCharge { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        UnityEvent OnBatteryDrain { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        UnityEvent OnCollect { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        UnityEvent OnCollectEarly { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        UnityEvent<PlayerControllerB> OnDestroyHeldEarly { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        UnityEvent OnDiscard { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        UnityEvent OnDiscardEarly { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        UnityEvent OnDiscardSFX { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        UnityEvent OnDiscardSFXEarly { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        UnityEvent<EnemyAI> OnEnemyGrab { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        UnityEvent OnEnemyDiscard { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        UnityEvent OnEquip { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        UnityEvent OnEquipEarly { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        UnityEvent OnGrab { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        UnityEvent OnGroundReached { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        UnityEvent OnInspect { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        UnityEvent OnInspectEarly { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        UnityEvent OnInteract { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        UnityEvent OnInteractLeft { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        UnityEvent OnInteractRight { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        UnityEvent OnPlace { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        UnityEvent OnPocket { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        UnityEvent OnPocketEarly { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        Action? FallWithCurveOverride { get; set; }
    }
}