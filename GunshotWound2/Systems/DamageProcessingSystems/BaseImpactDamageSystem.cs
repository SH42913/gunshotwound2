using GunshotWound2.Components.Events.WeaponHitEvents;
using GunshotWound2.Components.StateComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.DamageProcessingSystems
{
    [EcsInject]
    public abstract class BaseImpactDamageSystem<T> : BaseDamageSystem<T>
        where T : BaseWeaponHitEvent, new()
    {
        protected void DefaultGrazeWound(int entity)
        {
            CreateWound(Locale.Data.LightBruise, entity, DamageMultiplier * 5f,
                -1f, PainMultiplier * 15f);
        }

        protected void AbrasionWoundOn(string position, int entity)
        {
            CreateWound($"{Locale.Data.AbrazionWoundOn} {position}", entity, DamageMultiplier * 5f,
                BleeedingMultiplier * 0.2f, PainMultiplier * 15f);
        }

        protected void LightBruiseWound(string position, int entity)
        {
            CreateWound($"{Locale.Data.LightBruiseOn} {position}", entity, DamageMultiplier * 5f,
                -1f, PainMultiplier * 15f);
        }

        protected void MediumBruiseWound(string position, int entity)
        {
            CreateWound($"{Locale.Data.MediumBruiseOn} {position}", entity, DamageMultiplier * 10f,
                -1f, PainMultiplier * 30f, 0.05f);
        }

        protected void HeavyBruiseWound(string position, int entity, params DamageTypes?[] crits)
        {
            CreateWound($"{Locale.Data.HeavyBruiseOn} {position}", entity, DamageMultiplier * 15f,
                -1f, PainMultiplier * 45f, 0.1f, crits);
        }

        protected void WindedFromImpact(int entity)
        {
            CreateWound(Locale.Data.WindedFromImpact, entity, DamageMultiplier * 15f, -1, PainMultiplier * 40f);
        }
    }
}