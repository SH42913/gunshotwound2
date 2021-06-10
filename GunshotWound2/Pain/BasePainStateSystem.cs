using GunshotWound2.Configs;
using GunshotWound2.Effects;
using GunshotWound2.GUI;
using GunshotWound2.HitDetection;
using GunshotWound2.Utils;
using Leopotam.Ecs;

namespace GunshotWound2.Pain
{
    [EcsInject]
    public abstract class BasePainStateSystem<TStateEvent> : IEcsRunSystem
        where TStateEvent : BaseChangePainStateEvent, new()
    {
        protected EcsWorld EcsWorld;
        protected EcsFilter<TStateEvent> Events;

        protected EcsFilterSingle<MainConfig> Config;
        protected EcsFilterSingle<LocaleConfig> Locale;

        protected PainStates CurrentState;

        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(BasePainStateSystem<TStateEvent>);
#endif

            for (var i = 0; i < Events.EntitiesCount; i++)
            {
                var changeEvent = Events.Components1[i];
                var pedEntity = changeEvent.Entity;
                if (!EcsWorld.IsEntityExists(pedEntity)) continue;

                var woundedPed = EcsWorld.GetComponent<WoundedPedComponent>(pedEntity);
                if (woundedPed == null || woundedPed.IsDead) continue;
                if (woundedPed.PainState == CurrentState && !changeEvent.ForceUpdate) continue;

                ExecuteState(woundedPed, pedEntity);
                woundedPed.PainState = CurrentState;
            }

            Events.CleanFilter();
        }

        protected virtual void ExecuteState(WoundedPedComponent woundedPed, int pedEntity)
        {
#if DEBUG
            SendMessage($"{pedEntity} switch to {CurrentState} pain state", NotifyLevels.DEBUG);
#endif
        }

        protected void SendPedToRagdoll(int pedEntity, RagdollStates ragdollType)
        {
            EcsWorld.CreateEntityWith(out SetPedToRagdollEvent ragdoll);
            ragdoll.Entity = pedEntity;
            ragdoll.RagdollState = ragdollType;
        }

        protected void ChangeMoveSet(int pedEntity, string[] moveSets)
        {
            EcsWorld.CreateEntityWith(out SwitchMoveSetRequest request);
            request.Entity = pedEntity;
            request.AnimationName = moveSets != null && moveSets.Length > 0
                ? moveSets[GunshotWound2.Random.Next(0, moveSets.Length)]
                : null;
        }

        protected void ResetMoveSet(int pedEntity)
        {
            ChangeMoveSet(pedEntity, null);
        }

        protected void SendMessage(string message, NotifyLevels level = NotifyLevels.COMMON)
        {
#if !DEBUG
            if (level == NotifyLevels.DEBUG) return;
#endif

            var notification = EcsWorld.CreateEntityWith<ShowNotificationEvent>();
            notification.Level = level;
            notification.StringToShow = message;
        }
    }
}