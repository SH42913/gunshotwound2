using GTA.Native;
using GunshotWound2.Configs;
using GunshotWound2.Effects;
using GunshotWound2.HitDetection;
using GunshotWound2.Player;
using GunshotWound2.Utils;
using Leopotam.Ecs;

namespace GunshotWound2.Pain
{
    [EcsInject]
    public sealed class IncreasePainSystem : IEcsRunSystem
    {
        private readonly EcsWorld _ecsWorld = null;
        private readonly EcsFilter<IncreasePainEvent> _events = null;
        private readonly EcsFilterSingle<MainConfig> _config = null;

        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(IncreasePainSystem);
#endif

            for (var i = 0; i < _events.EntitiesCount; i++)
            {
                var component = _events.Components1[i];
                var pedEntity = component.Entity;
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

                    var newPain = component.PainAmount;
                    var painDeviation = GunshotWound2.Random.NextFloat(
                        -_config.Data.WoundConfig.PainDeviation * newPain,
                        _config.Data.WoundConfig.PainDeviation * newPain);
                    pain.CurrentPain += _config.Data.WoundConfig.PainMultiplier * newPain + painDeviation;

                    var painAnimIndex = GunshotWound2.Random.Next(1, 6);
                    var animationDict = woundedPed.IsMale ? "facials@gen_male@base" : "facials@gen_female@base";
                    Function.Call(Hash.PLAY_FACIAL_ANIM, woundedPed.ThisPed, $"pain_{painAnimIndex.ToString()}", animationDict);

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