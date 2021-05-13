using System;
using GTA;
using GTA.Native;
using GunshotWound2.GUI;
using Leopotam.Ecs;

namespace GunshotWound2.HitDetection
{
    [EcsInject]
    public sealed class BodyHitSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilter<CheckBodyHitEvent> _events;

        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(BodyHitSystem);
#endif

            for (int i = 0; i < _events.EntitiesCount; i++)
            {
                int pedEntity = _events.Components1[i].Entity;
                if (!_ecsWorld.IsEntityExists(pedEntity)) continue;

                var woundedPed = _ecsWorld.GetComponent<WoundedPedComponent>(pedEntity);
                if (woundedPed == null) continue;

                var bodyPart = GetDamagedBodyPart(woundedPed.ThisPed);

                var bodyDamage = _ecsWorld.CreateEntityWith<BodyPartWasHitEvent>();
                bodyDamage.Entity = pedEntity;
                bodyDamage.DamagedPart = bodyPart;
            }
        }

        private unsafe BodyParts GetDamagedBodyPart(Ped target)
        {
            if (target == null)
            {
                SendMessage("Target is null", NotifyLevels.DEBUG);
                return BodyParts.NOTHING;
            }

            int damagedBoneNum = 0;
            int* x = &damagedBoneNum;
            Function.Call(Hash.GET_PED_LAST_DAMAGE_BONE, target, x);

            Enum.TryParse(damagedBoneNum.ToString(), out Bone damagedBone);
            SendMessage("Damaged bone is " + damagedBone, NotifyLevels.DEBUG);

            switch (damagedBone)
            {
                case Bone.SkelHead:
                    return BodyParts.HEAD;
                case Bone.SkelNeck1:
                case Bone.SkelNeck2:
                    return BodyParts.NECK;
                case Bone.SkelSpine1:
                case Bone.SkelSpine2:
                case Bone.SkelSpine3:
                    return BodyParts.UPPER_BODY;
                case Bone.SkelRoot:
                case Bone.SkelSpineRoot:
                case Bone.SkelSpine0:
                case Bone.SkelPelvis:
                case Bone.SkelPelvis1:
                    return BodyParts.LOWER_BODY;
                case Bone.SkelLeftThigh:
                case Bone.SkelRightThigh:
                case Bone.SkelLeftToe0:
                case Bone.SkelLeftToe1:
                case Bone.SkelRightToe0:
                case Bone.SkelRightToe1:
                case Bone.SkelLeftFoot:
                case Bone.SkelRightFoot:
                case Bone.SkelLeftCalf:
                case Bone.SkelRightCalf:
                    return BodyParts.LEG;
                case Bone.SkelLeftUpperArm:
                case Bone.SkelRightUpperArm:
                case Bone.SkelLeftClavicle:
                case Bone.SkelRightClavicle:
                case Bone.SkelLeftForearm:
                case Bone.SkelRightForearm:
                case Bone.SkelLeftHand:
                case Bone.SkelRightHand:
                    return BodyParts.ARM;
            }

            SendMessage("WARNING! Nothing bone is " + damagedBone, NotifyLevels.DEBUG);

            return BodyParts.NOTHING;
        }

        private void SendMessage(string message, NotifyLevels level = NotifyLevels.COMMON)
        {
#if !DEBUG
            if(level == NotifyLevels.DEBUG) return;
#endif

            var notification = _ecsWorld.CreateEntityWith<ShowNotificationEvent>();
            notification.Level = level;
            notification.StringToShow = message;
        }
    }
}