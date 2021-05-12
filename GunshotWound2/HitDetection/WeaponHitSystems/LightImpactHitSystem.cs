﻿using GTA;
using Leopotam.Ecs;

namespace GunshotWound2.HitDetection.WeaponHitSystems
{
    [EcsInject]
    public sealed class LightImpactHitSystem : BaseWeaponHitSystem
    {
        protected override uint[] GetWeaponHashes()
        {
            return new uint[]
            {
                //GarbageBug Briefcase  Briefcase2
                3794977420, 2294779575, 28811031,
                //Fall      WaterCannon
                3452007600, 3425972830,
                (uint) WeaponHash.Ball,
                (uint) WeaponHash.Flashlight,
                (uint) WeaponHash.KnuckleDuster,
                (uint) WeaponHash.Nightstick,
                (uint) WeaponHash.Snowball,
                (uint) WeaponHash.Unarmed,
                (uint) WeaponHash.Parachute,
                (uint) WeaponHash.NightVision,
            };
        }

        protected override void CreateComponent(int pedEntity)
        {
            EcsWorld
                .CreateEntityWith<LightImpactHitEvent>()
                .Entity = pedEntity;
        }
    }
}