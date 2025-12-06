using itolib.Behaviours.Grabbables;
using itolib.Compatibility;
using itolib.Util;
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
        private static readonly int DimID = Animator.StringToHash("Dim");

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

                if (triggerFacilityMeltdown)
                {
                    OnDisconnectEarly.AddListener(() =>
                    {
                        if (bothPulled)
                        {
                            FacilityMeltdownCompatibility.InitiateMeltdown();
                        }
                    });
                }
            }

            if (PizzaTowerEscapeMusicCompatibility.Enabled)
            {
                OnDisconnectEarly.AddListener(() => PizzaTowerEscapeMusicCompatibility.SwitchApparatus(triggerEscapeMusic ? LongLostTwin : null));
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

                if (playDisconnectSFX)
                {
                    apparatusAudio.PlayOneShot(disconnectSFX, 0.7f);
                }
            }
            OnDisconnectEarly.Invoke();

            yield return Yielders.WaitForSeconds(0.1f);
            if (playSparkPFX)
            {
                sparkParticle.SetActive(true);
            }

            if (apparatusAudio != null && playRemoveFromMachineSFX)
            {
                apparatusAudio.PlayOneShot(removeFromMachineSFX);
            }

            if (modifyEnemySpawns && IsHost && Random.Range(0, 100) < 70 && roundManager.minEnemiesToSpawn < 2)
            {
                roundManager.minEnemiesToSpawn = bothPulled ? 2 : 1;
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
                roundManager.powerOffPermanently = shutOffPowerPermanently && bothPulled;
            }
            OnLightsOff.Invoke();

            yield return Yielders.WaitForSeconds(0.75f);

            if (!bothPulled) // TODO: Check if lights are on to begin with.
            {
                if (shutOffPower)
                {
                    roundManager.SwitchPower(true);
                    DimLights();
                }

                yield break;
            }

            if (displayRadiationWarning)
            {
                HUDManager.Instance.RadiationWarningHUD();
            }
            OnDisplayWarning.Invoke();

            if (awakenOldBirds && bothPulled && IsHost && radMechEnemyType != null)
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
                light.SetBool(DimID, true);
            }
        }
    }
}