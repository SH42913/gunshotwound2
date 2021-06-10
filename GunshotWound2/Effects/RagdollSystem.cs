using System;
using GTA;
using GTA.Native;
using GunshotWound2.HitDetection;
using Leopotam.Ecs;

namespace GunshotWound2.Effects
{
    [EcsInject]
    public sealed class RagdollSystem : IEcsRunSystem
    {
        private readonly EcsWorld _ecsWorld = null;
        private readonly EcsFilter<SetPedToRagdollEvent> _events = null;

        private static readonly InputArgument[] PedArgument = new InputArgument[1];
        private static readonly InputArgument[] RagdollArguments = {0, 0, 0, 0, 1, 1, 0};
        private static readonly InputArgument[] NaturalMotionArguments = {true, 0};

        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(RagdollSystem);
#endif
            for (var i = 0; i < _events.EntitiesCount; i++)
            {
                var pedEntity = _events.Components1[i].Entity;
                if (!_ecsWorld.IsEntityExists(pedEntity))
                {
                    RemoveEvent();
                    continue;
                }

                var woundedPed = _ecsWorld.GetComponent<WoundedPedComponent>(pedEntity);
                if (woundedPed == null || woundedPed.ThisPed.IsDead)
                {
                    RemoveEvent();
                    continue;
                }

                var skipRemoving = false;
                switch (_events.Components1[i].RagdollState)
                {
                    case RagdollStates.WAKE_UP:
                        WakeUpFromRagdoll(woundedPed, pedEntity);
                        break;
                    case RagdollStates.PERMANENT:
                        skipRemoving = StartPermanentRagdoll(woundedPed);
                        break;
                    case RagdollStates.SHORT:
                        skipRemoving = StartShortRagdoll(woundedPed);
                        break;
                    case RagdollStates.LONG:
                        skipRemoving = StartLongRagdoll(woundedPed);
                        break;
                    case RagdollStates.LEG_DAMAGE:
                        skipRemoving = StartLegDamageRagdoll(woundedPed);
                        break;
                    case RagdollStates.HEART_DAMAGE:
                        skipRemoving = StartHeartDamageRagdoll(woundedPed);
                        break;
                    case RagdollStates.GUTS_DAMAGE:
                        skipRemoving = StartGutsDamageRagdoll(woundedPed);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (!skipRemoving)
                {
                    RemoveEvent();
                }

                void RemoveEvent()
                {
                    _ecsWorld.RemoveEntity(_events.Entities[i]);
                }
            }
        }

        private bool StartLegDamageRagdoll(WoundedPedComponent woundedPed)
        {
            if (woundedPed.InPermanentRagdoll) return false;
            if (woundedPed.ThisPed.IsRagdoll) return true;

            SetToRagdoll(woundedPed.ThisPed, 3000, 4);
            PlayNaturalMotion(woundedPed.ThisPed, 0, true);
            PlayNaturalMotion(woundedPed.ThisPed, 1025);
            PlayNaturalMotion(woundedPed.ThisPed, 169);
            return false;
        }

        private bool StartLongRagdoll(WoundedPedComponent woundedPed)
        {
            if (woundedPed.InPermanentRagdoll) return false;
            if (woundedPed.ThisPed.IsRagdoll) return true;

            SetToRagdoll(woundedPed.ThisPed, 4000, 0);
            return false;
        }

        private bool StartShortRagdoll(WoundedPedComponent woundedPed)
        {
            if (woundedPed.InPermanentRagdoll) return false;
            if (woundedPed.ThisPed.IsRagdoll) return true;

            SetToRagdoll(woundedPed.ThisPed, 2000, 0);
            return false;
        }

        private void WakeUpFromRagdoll(WoundedPedComponent woundedPed, int pedEntity)
        {
            if (woundedPed.Crits.Has(CritTypes.NERVES_DAMAGED)) return;

            if (woundedPed.InPermanentRagdoll || woundedPed.ThisPed.IsRagdoll)
            {
                SetToRagdoll(woundedPed.ThisPed, 1, 1);
                woundedPed.InPermanentRagdoll = false;
            }

            RemoveAllPermanentEventsForPed(pedEntity);
        }

        private bool StartPermanentRagdoll(WoundedPedComponent woundedPed)
        {
            if (woundedPed.InPermanentRagdoll) return false;
            if (woundedPed.ThisPed.IsRagdoll) return true;

            SetToRagdoll(woundedPed.ThisPed, -1, 0);
            woundedPed.InPermanentRagdoll = true;
            return false;
        }

        private bool StartHeartDamageRagdoll(WoundedPedComponent woundedPed)
        {
            if (woundedPed.InPermanentRagdoll) return false;
            if (woundedPed.ThisPed.IsRagdoll) return true;

            SetToRagdoll(woundedPed.ThisPed, 6000, 1);
            PlayNaturalMotion(woundedPed.ThisPed, 0, true);
            PlayNaturalMotion(woundedPed.ThisPed, 1083);
            return false;
        }

        private bool StartGutsDamageRagdoll(WoundedPedComponent woundedPed)
        {
            if (woundedPed.InPermanentRagdoll) return false;
            if (woundedPed.ThisPed.IsRagdoll) return true;

            SetToRagdoll(woundedPed.ThisPed, 4000, 0);
            PlayNaturalMotion(woundedPed.ThisPed, 0, true);
            PlayNaturalMotion(woundedPed.ThisPed, 1119);
            return false;
        }

        private static void PlayNaturalMotion(Ped ped, int clip, bool immediately = false)
        {
            NaturalMotionArguments[0] = immediately;
            NaturalMotionArguments[1] = clip;
            Function.Call(Hash.CREATE_NM_MESSAGE, NaturalMotionArguments);

            PedArgument[0] = ped;
            Function.Call(Hash.GIVE_PED_NM_MESSAGE, PedArgument);
            PedArgument[0] = null;
        }

        private static void SetToRagdoll(Ped ped, int length, int type)
        {
            RagdollArguments[0] = ped;
            RagdollArguments[1] = length;
            RagdollArguments[2] = length;
            RagdollArguments[3] = type;
            Function.Call(Hash.SET_PED_TO_RAGDOLL, RagdollArguments);
            RagdollArguments[0] = null;
        }

        private void RemoveAllPermanentEventsForPed(int entity)
        {
            for (var i = 0; i < _events.EntitiesCount; i++)
            {
                var ragdollEvent = _events.Components1[i];
                if (ragdollEvent.RagdollState == RagdollStates.PERMANENT && ragdollEvent.Entity == entity)
                    _ecsWorld.RemoveEntity(_events.Entities[i]);
            }
        }
    }
}