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
    [EcsInject]
    public abstract class BaseCriticalSystem<T> : IEcsRunSystem where T : ComponentWithEntity, new()
    {
        protected EcsWorld EcsWorld;
        protected EcsFilter<T> Events;

        protected EcsFilterSingle<MainConfig> Config;
        protected EcsFilterSingle<LocaleConfig> Locale;

        protected CritTypes CurrentCrit;

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

                if (woundedPed.Crits.HasFlag(CurrentCrit))
                {
                    EcsWorld.RemoveEntity(Events.Entities[i]);
                    continue;
                }

                woundedPed.Crits = woundedPed.Crits | CurrentCrit;
                if (woundedPed.IsPlayer)
                {
                    ActionForPlayer(woundedPed, pedEntity);
                }
                else
                {
                    ActionForNpc(woundedPed, pedEntity);
                }

                EcsWorld.RemoveEntity(Events.Entities[i]);
            }
        }

        protected abstract void ActionForPlayer(WoundedPedComponent pedComponent, int pedEntity);
        protected abstract void ActionForNpc(WoundedPedComponent pedComponent, int pedEntity);

        protected void SendMessage(string message, NotifyLevels level = NotifyLevels.COMMON)
        {
#if !DEBUG
            if(level == NotifyLevels.DEBUG) return;
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
            SetPedToRagdollEvent ragdoll;
            EcsWorld.CreateEntityWith(out ragdoll);
            ragdoll.Entity = pedEntity;
            ragdoll.RagdollState = ragdollType;
        }
    }
}