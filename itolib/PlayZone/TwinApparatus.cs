using itolib.Behaviours.Grabbables;
using itolib.Compatibility;
using LethalLevelLoader;
using System.Collections;
using UnityEngine;

namespace itolib.PlayZone
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class TwinApparatus : EventfulApparatus
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public TwinApparatus? LongLostTwin { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        private bool bothPulled;

        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void HandleCompatibility()
        {
            if (FacilityMeltdownCompatibility.Enabled)
            {
                FacilityMeltdownCompatibility.HalveTwinValue(this);

                OnDisconnectEarly.AddListener(() =>
                {
                    if (bothPulled)
                    {
                        FacilityMeltdownCompatibility.InitiateMeltdown();
                    }
                });
            }

            if (PizzaTowerEscapeMusicCompatibility.Enabled)
            {
                OnDisconnectEarly.AddListener(() => PizzaTowerEscapeMusicCompatibility.SwitchTwin(LongLostTwin));
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <returns></returns>
        protected override IEnumerator HandleDisconnect()
        {
            if (apparatusAudio != null)
            {
                apparatusAudio.Stop();
                apparatusAudio.PlayOneShot(disconnectSFX, 0.7f);
            }
            OnDisconnectEarly.Invoke();

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
            OnDisconnect.Invoke();

            yield return new WaitForSeconds(1.0f);
            roundManager.FlickerLights(false, false);
            OnLightsFlicker.Invoke();

            yield return new WaitForSeconds(2.5f);
            roundManager.SwitchPower(false);
            roundManager.powerOffPermanently = bothPulled;
            OnLightsOff.Invoke();

            yield return new WaitForSeconds(0.75f);

            if (!bothPulled) // TODO: Check if lights are on to begin with.
            {
                roundManager.SwitchPower(!bothPulled);
                DimLights();

                yield break;
            }

            HUDManager.Instance.RadiationWarningHUD();
            OnDisplayWarning.Invoke();

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

                bothPulled = LongLostTwin != null && !LongLostTwin.isLungDocked && !LongLostTwin.isLungPowered;
                disconnectAnimation = StartCoroutine(HandleDisconnect());

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
        private void DimLights()
        {
            for (int i = 0; i < roundManager.allPoweredLightsAnimators.Count; i++)
            {
                Animator light = roundManager.allPoweredLightsAnimators[i];
                light.SetBool("Dim", true);
            }
        }
    }
}