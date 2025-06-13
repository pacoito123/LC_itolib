using itolib.Behaviours.Helpers;
using itolib.Compatibility;
using LethalLevelLoader;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.PlayZone
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class TwinApparatus : LungProp
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public TwinApparatus? LongLostTwin { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        public static BreakerBox? BreakerBoxInstance { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Twin Apparatus")]
        [Tooltip("")]
        public AudioSource? apparatusAudio;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Events")]
        [Tooltip("")]
        public UnityEvent onActivate = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<bool> onDisconnectEarly = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<bool> onDisconnect = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<bool> onLightsFlicker = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<bool> onLightsOff = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<bool> onDisplayWarning = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void Start()
        {
            // BreakerBoxInstance ??= FindObjectOfType<BreakerBox>();

            if (IsHost)
            {
                Activate();
            }

            base.Start();

            if (FacilityMeltdownCompatibility.Enabled)
            {
                FacilityMeltdownCompatibility.HalveTwinValue(this);

                onDisconnectEarly.AddListener(FacilityMeltdownCompatibility.TwinMeltdown);
            }

            if (PizzaTowerEscapeMusicCompatibility.Enabled)
            {
                onDisconnectEarly.AddListener(_ => PizzaTowerEscapeMusicCompatibility.SwitchTwin(LongLostTwin));
            }
        }

        /// <summary>
        ///     TODO.
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
        ///     TODO.
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

                bool bothPulled = LongLostTwin != null && !LongLostTwin.isLungDocked && !LongLostTwin.isLungPowered;
                disconnectAnimation = StartCoroutine(HandleDisconnect(bothPulled));

                if (bothPulled) // Invoke LLL Apparatus pull events only when both are pulled.
                {
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
            }

            base.EquipItem();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void Activate()
        {
            // if (!isInShipRoom && transform.GetParent() == roundManager.mapPropsContainer)
            if (!isInShipRoom)
            {
                isLungDocked = true;
                isLungPowered = true;

                radMechEnemyType = ActivateApparatus.OldBirdEnemyType;

                if (apparatusAudio != null)
                {
                    apparatusAudio.loop = true;
                    apparatusAudio.Play();
                }

                onActivate.Invoke();
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="possibleTwin"></param>
        public void AssignTwin(GrabbableObject possibleTwin)
        {
            if (LongLostTwin == null && possibleTwin is TwinApparatus twin)
            {
                LongLostTwin = twin;
                twin.LongLostTwin = this;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void DimLights()
        {
            for (int i = 0; i < roundManager.allPoweredLightsAnimators.Count; i++)
            {
                Animator light = roundManager.allPoweredLightsAnimators[i];
                light.SetBool("Dim", true);
            }
        }

        private IEnumerator HandleDisconnect(bool bothPulled)
        {
            if (apparatusAudio != null)
            {
                apparatusAudio.Stop();
                apparatusAudio.PlayOneShot(disconnectSFX, 0.7f);
            }
            onDisconnectEarly.Invoke(bothPulled);

            yield return new WaitForSeconds(0.1f);
            sparkParticle.SetActive(true);
            if (apparatusAudio != null)
            {
                apparatusAudio.PlayOneShot(removeFromMachineSFX);
            }

            if (IsHost && Random.Range(0, 100) < 70 && roundManager.minEnemiesToSpawn < 2)
            {
                roundManager.minEnemiesToSpawn = bothPulled ? 2 : 1;
            }
            onDisconnect.Invoke(bothPulled);

            yield return new WaitForSeconds(1.0f);
            roundManager.FlickerLights(false, false);
            onLightsFlicker.Invoke(bothPulled);

            yield return new WaitForSeconds(2.5f);
            roundManager.SwitchPower(false);
            roundManager.powerOffPermanently = bothPulled;
            onLightsOff.Invoke(bothPulled);

            yield return new WaitForSeconds(0.75f);

            if (!bothPulled) // TODO: Check if lights are on to begin with.
            {
                roundManager.SwitchPower(!bothPulled);
                DimLights();

                yield break;
            }

            HUDManager.Instance.RadiationWarningHUD();
            onDisplayWarning.Invoke(bothPulled);

            if (bothPulled && IsHost && radMechEnemyType != null)
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
            yield break;
        }
    }
}