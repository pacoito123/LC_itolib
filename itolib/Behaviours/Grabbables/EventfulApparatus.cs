using GameNetcodeStuff;
using itolib.Behaviours.Helpers;
using itolib.Compatibility;
using itolib.Extensions;
using itolib.Interfaces;
using itolib.Util;
using LethalLevelLoader;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Grabbables
{
    /// <summary>
    ///     Represents an <i>eventful</i> <c>LungProp</c>, with event callbacks for everything that would otherwise require overriding from an inheriting class.
    /// </summary>
    public class EventfulApparatus : LungProp, IEventfulItem
    {
        /// <summary>
        ///     <c>AudioSource</c> for the Apparatus' sound effects.
        /// </summary>
        [Space(5.0f)]
        [Header("Eventful Apparatus")]
        [Tooltip("Audio source for the Apparatus' sound effects.")]
        [SerializeField] protected AudioSource? apparatusAudio;

        /// <summary>
        ///     Whether to play the disconnect sound effect or not.
        /// </summary>
        [Space(5.0f)]
        [Header("Apparatus Sequence")]
        [Tooltip("Whether to play the disconnect sound effect or not.")]
        [SerializeField] protected bool playDisconnectSFX = true;

        /// <summary>
        ///     Whether to play the spark particle effect or not, if one is set.
        /// </summary>
        [Tooltip("Whether to play the spark particle effect or not, if one is set.")]
        [SerializeField] protected bool playSparkPFX = true;

        /// <summary>
        ///     Whether to play the machine shutdown sound effect or not.
        /// </summary>
        [Tooltip("Whether to play machine shutdown sound effect or not.")]
        [SerializeField] protected bool playRemoveFromMachineSFX = true;

        /// <summary>
        ///     Whether to affect enemy spawns after being pulled or not.
        /// </summary>
        [Tooltip("Whether to affect enemy spawns after being pulled or not.")]
        [SerializeField] protected bool modifyEnemySpawns = true;

        /// <summary>
        ///     Whether to flicker interior lights after being pulled or not.
        /// </summary>
        [Tooltip("Whether to flicker interior lights after being pulled or not.")]
        [SerializeField] protected bool flickerLights = true;

        /// <summary>
        ///     Whether to shut off power after being pulled or not.
        /// </summary>
        [Tooltip("Whether to shut off power after being pulled or not.")]
        [SerializeField] protected bool shutOffPower = true;

        /// <summary>
        ///     Whether power should be shut off permanently or not.
        /// </summary>
        [Tooltip("Whether power should be shut off permanently.")]
        [SerializeField] protected bool shutOffPowerPermanently = true;

        /// <summary>
        ///     Whether the radiation HUD warning should play or not.
        /// </summary>
        [Tooltip("Whether the radiation HUD warning should play or not.")]
        [SerializeField] protected bool displayRadiationWarning = true;

        /// <summary>
        ///     Whether Old Birds should wake up after being pulled or not.
        /// </summary>
        [Tooltip("Whether Old Birds should wake up after being pulled or not.")]
        [SerializeField] protected bool awakenOldBirds = true;

        /// <summary>
        ///     Whether to trigger a full meltdown or not, if <c>FacilityMeltdown</c> is installed.
        /// </summary>
        [Tooltip("Whether to trigger a full meltdown or not, if 'FacilityMeltdown' is installed.")]
        [SerializeField] protected bool triggerFacilityMeltdown = true;

        /// <summary>
        ///     Whether to trigger escape music or not, with <c>PizzaTowerEscapeMusic</c> is installed.
        /// </summary>
        [Tooltip("Whether to trigger escape music or not, with 'PizzaTowerEscapeMusic' is installed.")]
        [SerializeField] protected bool triggerEscapeMusic = true;

        /// <summary>
        ///     Callback invoked when the Apparatus is activated, at the start of the round.
        /// </summary>
        [field: Space(5.0f)]
        [field: Header("Apparatus Events")]
        [field: Tooltip("Callback invoked when the Apparatus is activated, at the start of the round.")]
        [field: SerializeField] public UnityEvent OnApparatusActivate { get; private set; } = new();

        /// <summary>
        ///     Callback invoked immediately before the Apparatus is unplugged by a player (not yet holding the item).
        /// </summary>
        [field: Tooltip("Callback invoked immediately before the Apparatus is unplugged by a player (not yet holding the item).")]
        [field: SerializeField] public UnityEvent OnDisconnectEarly { get; private set; } = new();

        /// <summary>
        ///     Callback invoked when the Apparatus is unplugged by a player (now holding the item).
        /// </summary>
        [field: Tooltip("Callback invoked when the Apparatus is unplugged by a player (now holding the item).")]
        [field: SerializeField] public UnityEvent OnDisconnect { get; private set; } = new();

        /// <summary>
        ///     Callback invoked when the facility's lights flicker.
        /// </summary>
        [field: Tooltip("Callback invoked when the facility's lights flicker.")]
        [field: SerializeField] public UnityEvent OnLightsFlicker { get; private set; } = new();

        /// <summary>
        ///     Callback invoked when the facility's lights shut off permanently.
        /// </summary>
        [field: Tooltip("Callback invoked when the facility's lights shut off permanently.")]
        [field: SerializeField] public UnityEvent OnLightsOff { get; private set; } = new();

        /// <summary>
        ///     Callback invoked when the radiation HUD warning is played.
        /// </summary>
        [field: Tooltip("Callback invoked when the radiation HUD warning is played.")]
        [field: SerializeField] public UnityEvent OnDisplayWarning { get; private set; } = new();

        /// <summary>
        ///     Whether the item should keep its material variant when reloading the save file or not.
        /// </summary>
        [field: Space(5.0f)]
        [field: Header("Item Grabbable")]
        [field: Tooltip("Whether the item should keep its material variant when reloading the save file or not.")]
        [field: SerializeField] public bool SaveMaterialVariant { get; set; }

        /// <summary>
        ///     Whether the item should keep its mesh variant when reloading the save file or not.
        /// </summary>
        [field: SerializeField] public bool SaveMeshVariant { get; set; }

        /// <summary>
        ///     Whether to hide the item when pocketed or not.
        /// </summary>
        [field: Tooltip("Whether to hide the item when pocketed or not.")]
        [field: SerializeField] public bool HideOnPocket { get; set; } = true;

        /// <summary>
        ///     Callback invoked when the item is activated (<c>LMB</c>), with the first parameter being whether the item has already been used up or not, and the
        ///     second parameter being whether the button is being held down or not.
        /// </summary>
        [field: Space(5.0f)]
        [field: Header("Grabbable Events")]
        [field: Tooltip("Callback invoked when the item is activated (LMB), with the first parameter being whether the item has already been used up or not, "
            + "and the second parameter being whether the button is being held down or not.")]
        [field: SerializeField] public UnityEvent<bool, bool> OnActivate { get; set; } = new();

        /// <summary>
        ///     Callback invoked when the item's <c>GrabbableObjectPhysicsTrigger</c> is triggered by a player or an enemy, with the <c>Collider</c> in question
        ///     given as parameter.
        /// </summary>
        [field: Tooltip("Callback invoked when the item's 'GrabbableObjectPhysicsTrigger' is triggered by a player or an enemy, with the collider in question "
            + "given as parameter.")]
        [field: SerializeField] public UnityEvent<Collider> OnActivatePhysicsTrigger { get; set; } = new();

        /// <summary>
        ///     Callback invoked when the item's battery is charged OR when the item's battery charge is synced with clients after being discarded.
        /// </summary>
        [field: Tooltip("Callback invoked when the item's battery is charged OR when the item's battery charge is synced with clients after being discarded.")]
        [field: SerializeField] public UnityEvent OnBatteryCharge { get; set; } = new();

        /// <summary>
        ///     Callback invoked when the item's battery is being used up.
        /// </summary>
        /// <remarks><b>NOTE:</b> Called every frame.</remarks>
        [field: Tooltip("Callback invoked when the item's battery is being used up. NOTE: Called every frame")]
        [field: SerializeField] public UnityEvent OnBatteryDrain { get; set; } = new();

        /// <summary>
        ///     Callback invoked when the item is brought back to the ship.
        /// </summary>
        [field: Tooltip("Callback invoked when the item is brought back to the ship.")]
        [field: SerializeField] public UnityEvent OnCollect { get; set; } = new();

        /// <summary>
        ///     Callback invoked when the item is brought back to the ship, but before the radar map icon is destroyed.
        /// </summary>
        [field: Tooltip("Callback invoked when the item is brought back to the ship, but before the radar map icon is destroyed.")]
        [field: SerializeField] public UnityEvent OnCollectEarly { get; set; } = new();

        /// <summary>
        ///     Callback invoked immediately before the item is destroyed while being held by a player.
        /// </summary>
        [field: Tooltip("Callback invoked immediately before the item is destroyed while being held by a player.")]
        [field: SerializeField] public UnityEvent<PlayerControllerB> OnDestroyHeldEarly { get; set; } = new();

        /// <summary>
        ///     Callback invoked when the item is dropped.
        /// </summary>
        [field: Tooltip("Callback invoked when the item is dropped.")]
        [field: SerializeField] public UnityEvent OnDiscard { get; set; } = new();

        /// <summary>
        ///     Callback invoked when the item is dropped, but before the reference to the player holding it is cleared.
        /// </summary>
        [field: Tooltip("Callback invoked when the item is dropped, but before the reference to the player holding it is cleared.")]
        [field: SerializeField] public UnityEvent OnDiscardEarly { get; set; } = new();

        /// <summary>
        ///     Callback invoked when the item plays its dropped sound effect.
        /// </summary>
        [field: Tooltip("Callback invoked when the item plays its dropped sound effect.")]
        [field: SerializeField] public UnityEvent OnDiscardSFX { get; set; } = new();

        /// <summary>
        ///     Callback invoked immediately before the item plays its dropped sound effect.
        /// </summary>
        [field: Tooltip("Callback invoked immediately before the item plays its dropped sound effect.")]
        [field: SerializeField] public UnityEvent OnDiscardSFXEarly { get; set; } = new();

        /// <summary>
        ///     Callback invoked when the item is grabbed by an enemy.
        /// </summary>
        [field: Tooltip("Callback invoked when the item is grabbed by an enemy.")]
        [field: SerializeField] public UnityEvent<EnemyAI> OnEnemyGrab { get; set; } = new();

        /// <summary>
        ///     Callback invoked when the item is dropped by an enemy.
        /// </summary>
        [field: Tooltip("Callback invoked when the item is dropped by an enemy.")]
        [field: SerializeField] public UnityEvent OnEnemyDiscard { get; set; } = new();

        /// <summary>
        ///     Callback invoked when the item is equipped by a player.
        /// </summary>
        [field: Tooltip("Callback invoked when the item is equipped by a player.")]
        [field: SerializeField] public UnityEvent OnEquip { get; set; } = new();

        /// <summary>
        ///     Callback invoked immediately before the item is equipped by a player.
        /// </summary>
        [field: Tooltip("Callback invoked immediately before the item is equipped by a player.")]
        [field: SerializeField] public UnityEvent OnEquipEarly { get; set; } = new();

        /// <summary>
        ///     Callback invoked when the item is grabbed by a player.
        /// </summary>
        [field: Tooltip("Callback invoked when the item is grabbed by a player.")]
        [field: SerializeField] public UnityEvent OnGrab { get; set; } = new();

        /// <summary>
        ///     Callback invoked when the item finishes falling.
        /// </summary>
        [field: Tooltip("Callback invoked when the item finishes falling.")]
        [field: SerializeField] public UnityEvent OnGroundReached { get; set; } = new();

        /// <summary>
        ///     Callback invoked when the item finishes falling, with the specific variant index given as parameter.
        /// </summary>
        [field: Tooltip("Callback invoked when the item finishes falling, with the specific variant index given as parameter.")]
        [field: SerializeField] public UnityEvent<int> OnGroundReachedVariant { get; set; } = new();

        /// <summary>
        ///     Callback invoked when the item is inspected.
        /// </summary>
        [field: Tooltip("Callback invoked when the item is inspected.")]
        [field: SerializeField] public UnityEvent OnInspect { get; set; } = new();

        /// <summary>
        ///     Callback invoked immediately before the item is inspected.
        /// </summary>
        [field: Tooltip("Callback invoked immediately before the item is inspected.")]
        [field: SerializeField] public UnityEvent OnInspectEarly { get; set; } = new();

        /// <summary>
        ///     Callback invoked when <c>E</c> is pressed on the item, regardless of whether it was successfully picked up or not.
        /// </summary>
        [field: Tooltip("Callback invoked when 'E' is pressed on the item, regardless of whether it was successfully picked up or not.")]
        [field: SerializeField] public UnityEvent OnInteract { get; set; } = new();

        /// <summary>
        ///     Callback invoked when <c>Q</c> is pressed while the item is held.
        /// </summary>
        [field: Tooltip("Callback invoked when 'Q' is pressed while the item is held.")]
        [field: SerializeField] public UnityEvent OnInteractLeft { get; set; } = new();

        /// <summary>
        ///     Callback invoked when <c>E</c> is pressed while the item is held.
        /// </summary>
        [field: Tooltip("Callback invoked when 'E' is pressed while the item is held.")]
        [field: SerializeField] public UnityEvent OnInteractRight { get; set; } = new();

        /// <summary>
        ///     Callback invoked when the item is placed onto a <c>PlaceableObjectsSurface</c>.
        /// </summary>
        [field: Tooltip("Callback invoked when the item is placed onto a 'PlaceableObjectsSurface'.")]
        [field: SerializeField] public UnityEvent OnPlace { get; set; } = new();

        /// <summary>
        ///     Callback invoked when the item is pocketed.
        /// </summary>
        [field: Tooltip("Callback invoked when the item is pocketed.")]
        [field: SerializeField] public UnityEvent OnPocket { get; set; } = new();

        /// <summary>
        ///     Callback invoked immediately before the item is pocketed.
        /// </summary>
        [field: Tooltip("Callback invoked immediately before the item is pocketed.")]
        [field: SerializeField] public UnityEvent OnPocketEarly { get; set; } = new();

        /// <summary>
        ///     Callback invoked when the item is placed on a <c>DepositItemsDesk</c>.
        /// </summary>
        [field: Tooltip("Callback invoked when the item is placed on a 'DepositItemsDesk'.")]
        [field: SerializeField] public UnityEvent OnReactToSellCounter { get; set; } = new();

        /// <summary>
        ///     Function to override the item's fall curve logic with, if one is set.
        /// </summary>
        public Action? FallWithCurveOverride { get; set; }

        /// <summary>
        ///     Index corresponding to the item's mesh and/or material variant.
        /// </summary>
        public int VariantIndex { get; set; } = -1; // TODO: Use a NetworkVariable for this.

        /// <summary>
        ///     Activate Apparatus upon being created.
        /// </summary>
        public override void Start()
        {
            if (IsHost)
            {
                Activate();
            }

            base.Start();

            // Enable FacilityMeltdown and PizzaTowerEscapeMusic compatibility.
            HandleCompatibility();
        }

        /// <summary>
        ///     Activate Apparatus upon being spawned.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (!IsHost)
            {
                Activate();
            }
        }

        /// <summary>
        ///     Handle the Apparatus being activated or plugged in.
        /// </summary>
        protected virtual void Activate()
        {
            // if (!isInShipRoom && transform.GetParent() == roundManager.mapPropsContainer)
            if (!isInShipRoom)
            {
                isLungDocked = true;
                isLungPowered = true;

                // Set actual Old Bird enemy type reference.
                radMechEnemyType = ActivateApparatus.OldBirdEnemyType;

                if (apparatusAudio != null)
                {
                    apparatusAudio.loop = true;
                    apparatusAudio.Play();
                }

                OnApparatusActivate.Invoke();
            }
            else
            {
                OnBroughtToShip();
            }
        }

        /// <summary>
        ///     <c>Coroutine</c> to handle the Apparatus being disconnected or unplugged.
        /// </summary>
        protected virtual IEnumerator HandleDisconnect()
        {
            if (apparatusAudio != null)
            {
                apparatusAudio.Stop();

                if (playDisconnectSFX)
                {
                    apparatusAudio.PlayOneShot(disconnectSFX, 0.7f);
                }
            }
            OnDisconnectEarly.Invoke();

            yield return Yielders.WaitForSeconds(0.1f);
            if (playSparkPFX && sparkParticle != null)
            {
                sparkParticle.SetActive(true);
            }

            if (playRemoveFromMachineSFX && apparatusAudio != null)
            {
                apparatusAudio.PlayOneShot(removeFromMachineSFX);
            }

            if (modifyEnemySpawns && IsHost && UnityEngine.Random.Range(0, 100) < 70 && roundManager.minEnemiesToSpawn < 2)
            {
                roundManager.minEnemiesToSpawn = 2;
            }
            OnDisconnect.Invoke();

            yield return Yielders.WaitForSeconds(1.0f);
            if (flickerLights)
            {
                roundManager.FlickerLights(false, false);
            }
            OnLightsFlicker.Invoke();

            yield return Yielders.WaitForSeconds(2.5f);
            if (shutOffPower)
            {
                roundManager.SwitchPower(false);
                roundManager.powerOffPermanently = shutOffPowerPermanently;
            }
            OnLightsOff.Invoke();

            yield return Yielders.WaitForSeconds(0.75f);
            if (displayRadiationWarning)
            {
                HUDManager.Instance.RadiationWarningHUD();
            }
            OnDisplayWarning.Invoke();

            if (awakenOldBirds && IsHost && radMechEnemyType != null)
            {
                EnemyAINestSpawnObject[] enemyNests = FindObjectsByType<EnemyAINestSpawnObject>(FindObjectsSortMode.None);
                for (int i = 0; i < enemyNests.Length; i++)
                {
                    if (enemyNests[i].enemyType == radMechEnemyType)
                    {
                        _ = roundManager.SpawnEnemyGameObject(roundManager.outsideAINodes[i].transform.position,
                            0f, -1, radMechEnemyType);
                    }
                }
            }
        }

        /// <summary>
        ///     Enable compatibility with <c>FacilityMeltdown</c> and <c>PizzaTowerEscapeMusic</c>.
        /// </summary>
        protected virtual void HandleCompatibility()
        {
            if (triggerFacilityMeltdown && FacilityMeltdownCompatibility.Enabled)
            {
                OnDisconnectEarly.AddListener(FacilityMeltdownCompatibility.InitiateMeltdown);
            }

            if (PizzaTowerEscapeMusicCompatibility.Enabled)
            {
                OnDisconnectEarly.AddListener(() => PizzaTowerEscapeMusicCompatibility.SwitchApparatus(triggerEscapeMusic ? this : null));
            }
        }

        /// <summary>
        ///     Save the item's mesh or material variant index.
        /// </summary>
        /// <returns>The item's variant index, or <c>-1</c> if no variants are set.</returns>
        public override int GetItemDataToSave()
        {
            if ((SaveMeshVariant || SaveMaterialVariant) && mainObjectRenderer == null)
            {
                Plugin.StaticLogger.LogWarning($"Main object renderer not set for item '{name}', its variant will not be saved!");
                return -1;
            }

            // Return the mesh variant index, if set to save mesh variant.
            if (SaveMeshVariant && mainObjectRenderer.TryGetComponent(out MeshFilter itemMesh))
            {
                for (int i = 0; i < itemProperties.meshVariants.Length; i++)
                {
                    if (itemProperties.meshVariants[i] == itemMesh.sharedMesh)
                    {
                        return i;
                    }
                }
            }

            // Return the material variant index, if set to save material variant. Gets overridden by mesh index.
            if (SaveMaterialVariant)
            {
                for (int i = 0; i < itemProperties.materialVariants.Length; i++)
                {
                    if (itemProperties.materialVariants[i] == mainObjectRenderer.sharedMaterial)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        ///     Load the item's mesh or material variant index.
        /// </summary>
        /// <param name="saveData">Value present in the save file, corresponding to the item's variant index.</param>
        public override void LoadItemSaveData(int saveData)
        {
            if (saveData < 0 || mainObjectRenderer == null)
            {
                return;
            }

            // Load the mesh variant index, if saved.
            if (SaveMeshVariant && saveData < itemProperties.meshVariants.Length
                && mainObjectRenderer.TryGetComponent(out MeshFilter itemMesh))
            {
                itemMesh.sharedMesh = itemProperties.meshVariants[saveData];
            }

            // Load the material variant index, if saved.
            if (SaveMaterialVariant && saveData < itemProperties.materialVariants.Length)
            {
                mainObjectRenderer.sharedMaterial = itemProperties.materialVariants[saveData];
            }
        }

        /// <summary>
        ///     Handle the item being activated (<c>LMB</c>) by a player.
        /// </summary>
        /// <param name="used">Whether the item has already been used or not.</param>
        /// <param name="buttonDown">Whether the button is being held down or not.</param>
        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            OnActivate.Invoke(used, buttonDown);
        }

        /// <summary>
        ///     Handle the item's <c>GrabbableObjectPhysicsTrigger</c> being triggered by a player or an enemy.
        /// </summary>
        /// <param name="other">Collider that triggered the physics trigger.</param>
        public override void ActivatePhysicsTrigger(Collider other)
        {
            base.ActivatePhysicsTrigger(other);
            OnActivatePhysicsTrigger.Invoke(other);
        }

        /// <summary>
        ///     Handle the item's battery being charged.
        /// </summary>
        public override void ChargeBatteries()
        {
            base.ChargeBatteries();
            OnBatteryCharge.Invoke();
        }

        /// <summary>
        ///     Handle the item's battery being used up.
        /// </summary>
        /// <remarks>Called every frame.</remarks>
        public override void UseUpBatteries()
        {
            base.UseUpBatteries();
            OnBatteryDrain.Invoke();
        }

        /// <summary>
        ///     Handle the item being brought back to the ship.
        /// </summary>
        public override void OnBroughtToShip()
        {
            OnCollectEarly.Invoke();
            base.OnBroughtToShip();
            OnCollect.Invoke();
        }

        /// <summary>
        ///     Handle the item being destroyed from the player's hand.
        /// </summary>
        /// <param name="playerHolding">Player holding the item.</param>
        public override void DestroyObjectInHand(PlayerControllerB playerHolding)
        {
            OnDestroyHeldEarly.Invoke(playerHolding);
            base.DestroyObjectInHand(playerHolding);
        }

        /// <summary>
        ///     Handle the item being dropped.
        /// </summary>
        public override void DiscardItem()
        {
            if (playerHeldBy != null)
            {
                // Set player as no longer holding an item with a left/right interact.
                playerHeldBy.equippedUsableItemQE = false;
                isBeingUsed = false;
            }

            OnDiscardEarly.Invoke();
            base.DiscardItem();
            OnDiscard.Invoke();
        }

        /// <summary>
        ///     Handle the item playing its drop sound effect.
        /// </summary>
        public override void PlayDropSFX()
        {
            OnDiscardSFXEarly.Invoke();
            base.PlayDropSFX();
            OnDiscardSFX.Invoke();
        }

        /// <summary>
        ///     Handle the item being grabbed by an enemy.
        /// </summary>
        /// <param name="enemy">Enemy that grabbed the item.</param>
        public override void GrabItemFromEnemy(EnemyAI enemy)
        {
            base.GrabItemFromEnemy(enemy);
            OnEnemyGrab.Invoke(enemy);
        }

        /// <summary>
        ///     Handle the item being dropped by an enemy.
        /// </summary>
        public override void DiscardItemFromEnemy()
        {
            base.DiscardItemFromEnemy();
            OnEnemyDiscard.Invoke();
        }

        /// <summary>
        ///     Handle the item being equipped in your hand, and starting the Apparatus sequence.
        /// </summary>
        public override void EquipItem()
        {
            if (isLungDocked)
            {
                isLungDocked = false;
                isLungPowered = false;

                if (disconnectAnimation != null)
                {
                    StopCoroutine(disconnectAnimation);
                }

                disconnectAnimation = StartCoroutine(HandleDisconnect());

                // Invoke LLL Apparatus pull events.
                if (DungeonManager.CurrentExtendedDungeonFlow != null)
                {
                    DungeonManager.CurrentExtendedDungeonFlow.DungeonEvents.onApparatusTaken.Invoke(this);
                    DungeonManager.GlobalDungeonEvents.onApparatusTaken.Invoke(this);
                }
                if (LevelManager.CurrentExtendedLevel != null)
                {
                    LevelManager.CurrentExtendedLevel.LevelEvents.onApparatusTaken.Invoke(this);
                    LevelManager.GlobalLevelEvents.onApparatusTaken.Invoke(this);
                }
            }

            OnEquipEarly.Invoke();

            base.EquipItem();

            if (playerHeldBy != null)
            {
                // Set player as holding an item with a left/right interact, to allow their use.
                playerHeldBy.equippedUsableItemQE = true;
            }

            OnEquip.Invoke();
        }

        /// <summary>
        ///     Handle the item being grabbed by a player.
        /// </summary>
        public override void GrabItem()
        {
            base.GrabItem();
            OnGrab.Invoke();
        }

        /// <summary>
        ///     Handle the item landing on the ground.
        /// </summary>
        public override void OnHitGround()
        {
            base.OnHitGround();

            if (VariantIndex < 0)
            {
                OnGroundReached.Invoke();
            }
            else
            {
                OnGroundReachedVariant.Invoke(VariantIndex);
            }
        }

        /// <summary>
        ///     Handle the item being inspected by a player.
        /// </summary>
        public override void InspectItem()
        {
            OnInspectEarly.Invoke();
            base.InspectItem();
            OnInspect.Invoke();
        }

        /// <summary>
        ///     Handle <c>E</c> being pressed on the item by a player when trying to pick it up.
        /// </summary>
        public override void InteractItem()
        {
            base.InteractItem();
            OnInteract.Invoke();
        }

        /// <summary>
        ///     Handle <c>Q</c> or <c>E</c> being pressed by a player while holding the item.
        /// </summary>
        /// <param name="right">Whether right interact was triggered or not.</param>
        public override void ItemInteractLeftRight(bool right)
        {
            base.ItemInteractLeftRight(right);

            if (right)
            {
                OnInteractRight.Invoke();
            }
            else
            {
                OnInteractLeft.Invoke();
            }
        }

        /// <summary>
        ///     Handle the item being placed onto a <c>PlaceableObjectsSurface</c>.
        /// </summary>
        public override void OnPlaceObject()
        {
            base.OnPlaceObject();
            OnPlace.Invoke();
        }

        /// <summary>
        ///     Handle the item being pocketed by the player holding it.
        /// </summary>
        public override void PocketItem()
        {
            if (playerHeldBy != null && playerHeldBy.IsLocalClient())
            {
                // Set player as no longer holding an item with a left/right interact.
                playerHeldBy.equippedUsableItemQE = false;
                isBeingUsed = false;
            }

            OnPocketEarly.Invoke();

            if (HideOnPocket)
            {
                base.PocketItem();
            }

            OnPocket.Invoke();
        }

        /// <summary>
        ///     Handle the item reacting to being placed on a <c>DepositItemsDesk</c>.
        /// </summary>
        public override void ReactToSellingItemOnCounter()
        {
            OnReactToSellCounter.Invoke();
        }

        /// <summary>
        ///     Handle the item's falling curve, or its override if one is set.
        /// </summary>
        public override void FallWithCurve()
        {
            if (FallWithCurveOverride != null)
            {
                FallWithCurveOverride();
            }
            else
            {
                base.FallWithCurve();
            }
        }

        /// <summary>
        ///     Sync the item's current mesh or material variant with other clients, if it has one.
        /// </summary>
        public void SyncItemVariant()
        {
            if (VariantIndex < 0 && (SaveMeshVariant || SaveMaterialVariant))
            {
                SyncItemVariantRpc(GetItemDataToSave());
            }
        }

        /// <summary>
        ///     Send the item's current mesh or material variant to everyone.
        /// </summary>
        /// <param name="variantIndex">Index of the item's mesh or material variant.</param>
        [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
        private void SyncItemVariantRpc(int variantIndex)
        {
            VariantIndex = variantIndex;
        }
    }
}