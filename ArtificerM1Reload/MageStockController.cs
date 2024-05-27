using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ArtificerM1Reload
{
    public class MageStockController : MonoBehaviour
    {
        public static float graceDuration = 0.4f;    //Used when there's still stocks in the mag
        public static float defaultReloadTime = 1f;

        private CharacterBody body;
        private SkillLocator skills;

        private float reloadStopwatch;
        private float delayStopwatch;

        private bool rightMuzzle;

        public static GameObject fireMuzzleflashEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mage/MuzzleflashMageFire.prefab").WaitForCompletion();
        public static GameObject lightningMuzzleflashEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mage/MuzzleflashMageLightning.prefab").WaitForCompletion();
        public static GameObject iceMuzzleflashEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Junk/Mage/MuzzleflashMageIce.prefab").WaitForCompletion();

        public static Dictionary<Type, SkillInfo> StatePairs = new Dictionary<Type, SkillInfo>();

        private void Awake()
        {
            body = base.GetComponent<CharacterBody>();
            skills = base.GetComponent<SkillLocator>();

            reloadStopwatch = 0f;
            delayStopwatch = 0f;
            rightMuzzle = true;
        }

        private void FixedUpdate()
        {
            if (!skills.hasAuthority) return;
            if (skills.primary.stock < skills.primary.maxStock && StatePairs.ContainsKey(skills.primary.activationState.stateType))
            {
                if (skills.primary.stock <= 0) delayStopwatch = 0f;
                if (delayStopwatch > 0f)
                {
                    delayStopwatch -= Time.fixedDeltaTime;
                }
                else
                {
                    reloadStopwatch -= Time.fixedDeltaTime;
                    if (reloadStopwatch <= 0f)
                    {
                        reloadStopwatch += GetReloadDuration() / body.attackSpeed;

                        skills.primary.stock += GetRechargeStock();
                        skills.primary.rechargeStopwatch = 0f;

                        Util.PlaySound("Play_railgunner_m2_reload_basic", base.gameObject);
                        ShowReloadVFX();

                        if (skills.primary.stock > skills.primary.maxStock)
                        {
                            skills.primary.stock = skills.primary.maxStock;
                        }
                    }
                }
            }
            else
            {
                reloadStopwatch = GetReloadDuration() / body.attackSpeed;
            }
        }

        public void FireSkill(float duration)
        {
            delayStopwatch = graceDuration;  //Duration is already scaled to attack speed. InitialDelay is simply for inputs, and is ignored if the mag is empty.
            reloadStopwatch = GetReloadDuration() / body.attackSpeed;// + (skills.primary.stock <= 0 ? duration : 0f);
        }

        private void ShowReloadVFX()
        {
            SkillInfo currentSkill = null;
            StatePairs.TryGetValue(skills.primary.activationState.stateType, out currentSkill);

            if (currentSkill != null && currentSkill.reloadEffectPrefab != null)
            {
                EffectManager.SimpleMuzzleFlash(currentSkill.reloadEffectPrefab, base.gameObject, rightMuzzle ? "MuzzleRight" : "MuzzleLeft", true);
            }

            rightMuzzle = !rightMuzzle;
        }

        private float GetReloadDuration()
        {
            SkillInfo currentSkill = null;
            StatePairs.TryGetValue(skills.primary.activationState.stateType, out currentSkill);

            if (currentSkill != null)
            {
                return currentSkill.baseReloadDuration;
            }
            else
            {
                return defaultReloadTime;
            }
        }

        private int GetRechargeStock()
        {
            SkillInfo currentSkill = null;
            StatePairs.TryGetValue(skills.primary.activationState.stateType, out currentSkill);

            if (currentSkill != null)
            {
                return currentSkill.rechargeStock;
            }
            else
            {
                return 1;
            }
        }

        public class SkillInfo
        {
            public GameObject reloadEffectPrefab;
            public float baseReloadDuration;
            public int rechargeStock;

            public SkillInfo()
            {
                reloadEffectPrefab = fireMuzzleflashEffectPrefab;
                baseReloadDuration = defaultReloadTime;
                rechargeStock = 1;
            }

            public SkillInfo(GameObject reloadEffectPrefab, float baseCooldown, int rechargeStock)
            {
                this.reloadEffectPrefab = reloadEffectPrefab;
                this.baseReloadDuration = baseCooldown;
                this.rechargeStock = rechargeStock;
            }
        }
    }
}
