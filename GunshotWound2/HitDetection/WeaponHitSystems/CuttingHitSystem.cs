using GTA.Native;
using Leopotam.Ecs;

namespace GunshotWound2.HitDetection.WeaponHitSystems
{
    [EcsInject]
    public class CuttingHitSystem : BaseWeaponHitSystem
    {
        protected override uint[] GetWeaponHashes()
        {
            return new uint[]
            {
                //Animal    Cougar     BarbedWire
                4194021054, 148160082, 1223143800,
                (uint) WeaponHash.BattleAxe,
                (uint) WeaponHash.Bottle,
                (uint) WeaponHash.Dagger,
                (uint) WeaponHash.Hatchet,
                (uint) WeaponHash.Knife,
                (uint) WeaponHash.Machete,
                (uint) WeaponHash.SwitchBlade,
            };
        }

        protected override void CreateComponent(int pedEntity)
        {
            EcsWorld
                .CreateEntityWith<CuttingHitEvent>()
                .Entity = pedEntity;
        }
    }
}