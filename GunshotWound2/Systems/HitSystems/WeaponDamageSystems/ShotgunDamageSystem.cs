using System;
using GunshotWoundEcs.Components.DamageComponents.WeaponDamageComponents;
using GunshotWoundEcs.Components.UiComponents;
using GunshotWoundEcs.Components.WoundComponents.CriticalWoundComponents;
using LeopotamGroup.Ecs;
using Weighted_Randomizer;

namespace GunshotWoundEcs.Systems.HitSystems.WeaponDamageSystems
{
    [EcsInject]
    public class ShotgunDamageSystem : BaseWeaponDamageSystem<ShotgunDamageComponent>
    {
        public ShotgunDamageSystem()
        {
            WeaponClass = "Shotgun";

            DefaultAction = GrazeInjury;
            
            HeadActions = new StaticWeightedRandomizer<Action<int>>
            {
                {GrazeInjury, 1},
                {HeadCase1, 1},
                {HeadCase2, 1},
                {HeadCase3, 1},
                {HeadCase4, 1},
            };
            
            NeckActions = new StaticWeightedRandomizer<Action<int>>
            {
                {GrazeInjury, 1},
                {LongShallow, 1},
                {Fragmentation, 2},
                {ArterySevered, 2},
                {NeckCase1, 2},
            };
            
            UpperBodyActions = new StaticWeightedRandomizer<Action<int>>
            {
                {LongShallow, 1},
                {InternalBleeding, 2},
                {UpperBodyCase1, 3},
                {UpperBodyCase2, 3},
                {UpperBodyCase3, 3},
            };
            
            LowerBodyActions = new StaticWeightedRandomizer<Action<int>>
            {
                {LongShallow, 1},
                {InternalBleeding, 2},
                {LowerBodyCase1, 3},
                {LowerBodyCase2, 3},
                {LowerBodyCase3, 3},
            };
            
            ArmActions = new StaticWeightedRandomizer<Action<int>>
            {
                {GrazeInjury, 1},
                {LongShallow, 1},
                {ThroughAndThrough, 1},
                {ArterySevered, 2},
                {ArmCase1, 3},
            };
            
            LegActions = new StaticWeightedRandomizer<Action<int>>
            {
                {GrazeInjury, 1},
                {LongShallow, 1},
                {ThroughAndThrough, 1},
                {ArterySevered, 2},
                {LegCase1, 3},
            };
        }

        private void GrazeInjury(int entity)
        {
            InstantDamage(entity, 10f);
            CreateBleeding(entity, 0.5f, "Graze Injury");
            SendMessage(entity, "Graze injury from ricochet/fragment");
        }

        private void LongShallow(int entity)
        {
            InstantDamage(entity, 10f);
            CreateBleeding(entity, 0.8f, "Long Shallow");
            SendMessage(entity, "Long shallow GSW, superficial damage");
        }

        private void InternalBleeding(int entity)
        {
            InstantDamage(entity, 20f);
            CreateBleeding(entity, 1.5f, "Internal Bleeding");
            SendMessage(entity, "Internal bleeding from GSW path");
        }

        private void ThroughAndThrough(int entity)
        {
            InstantDamage(entity, 15f);
            CreateBleeding(entity, 1.8f, "Hole");
            SendMessage(entity, "Through-and-through small caliber gunshot wound");
        }

        private void Fragmentation(int entity)
        {
            InstantDamage(entity, 25f);
            CreateBleeding(entity, 1.2f);
            SendMessage(entity, "Spalling/Fragmentation risk detected");
        }

        private void ArterySevered(int entity)
        {
            InstantDamage(entity, 20f);
            CreateBleeding(entity, 3f, "Artery");
            SendMessage(entity, "Artery severed", NotifyLevels.EMERGENCY);
        }

        #region HeadCases

        private void HeadCase1(int pedEntity)
        {
            InstantDamage(pedEntity, 10f);
            CreateBleeding(pedEntity, 1f, "Lost ear");
            SendMessage(pedEntity, "Part of ear sails off away");
        }
        private void HeadCase2(int pedEntity)
        {
            InstantDamage(pedEntity, 70f);
            CreateBleeding(pedEntity, 3f);
            SendMessage(pedEntity, "Bullet fly through your head", NotifyLevels.EMERGENCY);
        }
        private void HeadCase3(int pedEntity)
        {
            InstantDamage(pedEntity, 70f);
            CreateBleeding(pedEntity, 3f);
            SendMessage(pedEntity, "Small caliber bullet torn apart your brain", NotifyLevels.EMERGENCY);
        }
        private void HeadCase4(int pedEntity)
        {
            InstantDamage(pedEntity, 70f);
            CreateBleeding(pedEntity, 3f);
            SendMessage(pedEntity, "Heavy brain damage detected", NotifyLevels.EMERGENCY);
        }

        #endregion

        #region NeckCases

        private void NeckCase1(int entity)
        {
            InstantDamage(entity, 35f);
            CreateBleeding(entity, 2.0f, "Bullet in the neck");
            SendMessage(entity, "Small caliber bullet stuck in your neck", NotifyLevels.ALERT);
        }

        #endregion

        #region UpperBodyCases

        private void UpperBodyCase1(int entity)
        {
            InstantDamage(entity, 30f);
            CreateBleeding(entity, 1.5f);
            SendMessage(entity, "Possible nerves damage");
            EcsWorld.CreateEntityWith<NervesCriticalComponent>().PedEntity = entity;
        }
        private void UpperBodyCase2(int entity)
        {
            InstantDamage(entity, 35f);
            CreateBleeding(entity, 1.5f, "Destroyed lung");
            SendMessage(entity, "Punctured or collapsed lung detected");
            EcsWorld.CreateEntityWith<LungsCriticalComponent>().PedEntity = entity;
        }
        private void UpperBodyCase3(int entity)
        {
            InstantDamage(entity, 45f);
            CreateBleeding(entity, 3.5f, "Destroyed Heart");
            SendMessage(entity, "Punctured heart detected");
            EcsWorld.CreateEntityWith<HeartCriticalComponent>().PedEntity = entity;
        }

        #endregion

        #region LowerBodyCases

        private void LowerBodyCase1(int entity)
        {
            InstantDamage(entity, 30f);
            CreateBleeding(entity, 1.5f, "Damaged Stomach");
            SendMessage(entity, "Punctured stomach detected");
        }
        private void LowerBodyCase2(int entity)
        {
            InstantDamage(entity, 30f);
            CreateBleeding(entity, 1.5f, "Damaged Guts");
            SendMessage(entity, "Punctured guts detected");
        }
        private void LowerBodyCase3(int entity)
        {
            InstantDamage(entity, 30f);
            CreateBleeding(entity, 1.5f, "Lost balls");
            SendMessage(entity, "Bullet torn apart your balls");
        }

        #endregion

        #region ArmCases

        private void ArmCase1(int entity)
        {
            InstantDamage(entity, 20f);
            CreateBleeding(entity, 1.1f, "Wound on arm");
            SendMessage(entity, "Arm bone was broken");
            EcsWorld.CreateEntityWith<ArmsCriticalComponent>().PedEntity = entity;
        }

        #endregion

        #region LegCases

        private void LegCase1(int entity)
        {
            InstantDamage(entity, 20f);
            CreateBleeding(entity, 1.1f, "Wound on leg");
            SendMessage(entity, "Leg bone was broken");
            EcsWorld.CreateEntityWith<LegsCriticalComponent>().PedEntity = entity;
        }

        #endregion
    }
}