using System;
using GTA;
using GTA.Native;
using GunshotWound2.Components.Events.PedEvents;
using GunshotWound2.Components.Events.PlayerEvents;
using GunshotWound2.Components.Events.WoundEvents;
using GunshotWound2.Components.StateComponents;
using GunshotWound2.Configs;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.WoundSystems
{
    [EcsInject]
    public class IncreasePainSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilter<IncreasePainEvent> _events;

        private EcsFilterSingle<MainConfig> _config;

        private static readonly Random Random = new Random();

        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(IncreasePainSystem);
#endif

            for (int i = 0; i < _events.EntitiesCount; i++)
            {
                var component = _events.Components1[i];
                int pedEntity = component.Entity;
                if (!_ecsWorld.IsEntityExists(pedEntity))
                {
                    _ecsWorld.RemoveEntity(_events.Entities[i]);
                    continue;
                }

                var woundedPed = _ecsWorld.GetComponent<WoundedPedComponent>(pedEntity);
                if (woundedPed != null && component.PainAmount > 0f)
                {
                    var pain = _ecsWorld.EnsureComponent<PainComponent>(pedEntity, out var firstPain);
                    if (firstPain)
                    {
                        pain.CurrentPain = 0f;
                    }

                    float newPain = component.PainAmount;
                    var painDeviation = Random.NextFloat(
                        -_config.Data.WoundConfig.PainDeviation * newPain,
                        _config.Data.WoundConfig.PainDeviation * newPain);
                    pain.CurrentPain += _config.Data.WoundConfig.PainMultiplier * newPain + painDeviation;

                    int painAnimIndex = Random.Next(1, 6);
                    if (woundedPed.IsMale)
                    {
                        Function.Call(Hash.PLAY_FACIAL_ANIM, woundedPed.ThisPed, "pain_" + painAnimIndex, "facials@gen_male@base");
                    }
                    else
                    {
                        Function.Call(Hash.PLAY_FACIAL_ANIM, woundedPed.ThisPed, "pain_" + painAnimIndex, "facials@gen_female@base");
                    }

                    if (newPain > _config.Data.WoundConfig.PainfulWoundValue / 2)
                    {
                        if (woundedPed.IsPlayer)
                        {
                            _ecsWorld.CreateEntityWith<AddCameraShakeEvent>().Length = CameraShakeLength.ONE_TIME;
                        }
                    }

                    if (newPain > _config.Data.WoundConfig.PainfulWoundValue)
                    {
                        if (_config.Data.WoundConfig.RagdollOnPainfulWound)
                        {
                            _ecsWorld.CreateEntityWith(out SetPedToRagdollEvent ragdoll);
                            ragdoll.Entity = pedEntity;
                            ragdoll.RagdollState = RagdollStates.SHORT;
                        }

                        if (woundedPed.IsPlayer)
                        {
                            _ecsWorld.CreateEntityWith<AddFlashEvent>();
                        }
                    }
                }

                _ecsWorld.RemoveEntity(_events.Entities[i]);
            }
        }
    }
}