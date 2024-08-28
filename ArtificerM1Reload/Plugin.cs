using BepInEx;
using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace R2API.Utils
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class ManualNetworkRegistrationAttribute : Attribute
    {
    }
}

namespace ArtificerM1Reload
{
    [BepInPlugin("com.Moffein.ArtificerM1Reload", "ArtificerM1Reload", "1.1.1")]
    public class ArtificerM1Reload : BaseUnityPlugin
    {

        public void Awake()
        {
            MageStockController.graceDuration = base.Config.Bind<float>(new ConfigDefinition("Stats", "Grace Duration"), 0.4f, new ConfigDescription("Time after firing before the reload starts. Ignored when out of stocks.")).Value;
            MageStockController.defaultReloadTime = base.Config.Bind<float>(new ConfigDefinition("Stats", "Reload Time"), 1f, new ConfigDescription("Time it takes to reload a shot. Modded skills may have a different value from this.")).Value;

            GameObject bodyPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/MageBody");
            SkillDef fireBoltDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Mage/MageBodyFireFirebolt.asset").WaitForCompletion();
            SkillDef plasmaBoltDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Mage/MageBodyFireLightningBolt.asset").WaitForCompletion();

            MageStockController.SkillInfo fireBoltInfo = new MageStockController.SkillInfo(MageStockController.fireMuzzleflashEffectPrefab, MageStockController.defaultReloadTime, 1);
            MageStockController.SkillInfo plasmaBoltInfo = new MageStockController.SkillInfo(MageStockController.lightningMuzzleflashEffectPrefab, MageStockController.defaultReloadTime, 1);

            MageStockController.StatePairs.Add(typeof(EntityStates.Mage.Weapon.FireFireBolt), fireBoltInfo);
            MageStockController.StatePairs.Add(typeof(EntityStates.Mage.Weapon.FireLightningBolt), plasmaBoltInfo);
            bodyPrefab.AddComponent<MageStockController>();

            fireBoltDef.rechargeStock = 0;
            plasmaBoltDef.rechargeStock = 0;

            //FireLightningBolt inherits from this
            On.EntityStates.Mage.Weapon.FireFireBolt.OnEnter += (orig, self) =>
            {
                orig(self);
                MageStockController msc = self.gameObject.GetComponent<MageStockController>();
                if (msc)
                {
                    msc.FireSkill(self.duration);
                }
            };
        }
    }
}
