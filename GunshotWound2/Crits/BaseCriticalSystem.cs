using GTA.Native;
using GunshotWound2.Configs;
using GunshotWound2.Damage;
using GunshotWound2.Effects;
using GunshotWound2.GUI;
using GunshotWound2.HitDetection;
using GunshotWound2.Pain;
using GunshotWound2.Utils;
using Leopotam.Ecs;

namespace GunshotWound2.Crits
{
    public abstract class BaseCriticalWoundEvent : ComponentWithEntity
    {
    }

    [EcsInject]
    public abstract class BaseCriticalSystem<T> : IEcsRunSystem where T : ComponentWithEntity, new()
    {
        protected readonly EcsWorld EcsWorld = null;
        protected readonly EcsFilter<T> Events = null;
        protected readonly EcsFilterSingle<MainConfig> Config = null;
        protected readonly EcsFilterSingle<LocaleConfig> Locale = null;

        protected abstract CritTypes CurrentCrit { get; }

        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(BaseCriticalSystem<T>);
#endif

            for (var i = 0; i < Events.EntitiesCount; i++)
            {
                var pedEntity = Events.Components1[i].Entity;
                var woundedPed = EcsWorld.GetComponent<WoundedPedComponent>(pedEntity);

                if (woundedPed == null)
                {
                    EcsWorld.RemoveEntity(Events.Entities[i]);
                    continue;
                }

                if (woundedPed.Crits.Has(CurrentCrit))
                {
                    EcsWorld.RemoveEntity(Events.Entities[i]);
                    continue;
                }

                woundedPed.Crits |= CurrentCrit;
                if (woundedPed.IsPlayer)
                    ActionForPlayer(woundedPed, pedEntity);
                else
                    ActionForNpc(woundedPed, pedEntity);

                woundedPed.ThisPed.IsPainAudioEnabled = true;
                var pain = GunshotWound2.Random.IsTrueWithProbability(0.5f) ? 6 : 7;
                Function.Call(Hash.PLAY_PAIN, woundedPed.ThisPed, pain, 0, 0);
                EcsWorld.RemoveEntity(Events.Entities[i]);
            }
        }

        protected abstract void ActionForPlayer(WoundedPedComponent pedComponent, int pedEntity);
        protected abstract void ActionForNpc(WoundedPedComponent pedComponent, int pedEntity);

        protected void SendMessage(string message, NotifyLevels level = NotifyLevels.COMMON)
        {
#if !DEBUG
            if (level == NotifyLevels.DEBUG) return;
#endif

            var notification = EcsWorld.CreateEntityWith<ShowNotificationEvent>();
            notification.Level = level;
            notification.StringToShow = message;
        }

        protected void CreatePain(int entity, float amount)
        {
            var pain = EcsWorld.CreateEntityWith<IncreasePainEvent>();
            pain.Entity = entity;
            pain.PainAmount = amount;
        }

        protected void CreateInternalBleeding(int entity, float amount)
        {
            var bleed = EcsWorld.CreateEntityWith<BleedingComponent>();
            bleed.Entity = entity;
            bleed.BleedSeverity = amount;
            bleed.Name = Locale.Data.InternalBleeding;
        }

        protected void SendPedToRagdoll(int pedEntity, RagdollStates ragdollType)
        {
            EcsWorld.CreateEntityWith(out SetPedToRagdollEvent ragdoll);
            ragdoll.Entity = pedEntity;
            ragdoll.RagdollState = ragdollType;
        }

        protected static void StartPostFx(string animation, int argument)
        {
            Function.Call(Hash.ANIMPOSTFX_PLAY, animation, argument, true);
        }
    }
}