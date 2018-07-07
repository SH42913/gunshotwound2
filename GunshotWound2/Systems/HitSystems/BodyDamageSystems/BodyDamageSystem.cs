using System;
using GTA;
using GTA.Native;
using GunshotWoundEcs.Components.DamageComponents.BodyDamageComponents;
using GunshotWoundEcs.Components.UiComponents;
using GunshotWoundEcs.Components.WoundComponents;
using LeopotamGroup.Ecs;

namespace GunshotWoundEcs.Systems.HitSystems.BodyDamageSystems
{
    [EcsInject]
    public class BodyDamageSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilter<BodyDamageRequestComponent> _requests;
        
        public void Run()
        {
            GunshotWoundScript.LastSystem = nameof(BodyDamageSystem);
            for (int i = 0; i < _requests.EntitiesCount; i++)
            {
                int pedEntity = _requests.Components1[i].PedEntity;
                Ped ped = _ecsWorld
                    .GetComponent<WoundedPedComponent>(pedEntity)
                    .ThisPed;

                var bodyPart = GetDamagedBodyPart(ped);
                if (bodyPart == BodyParts.NOTHING) continue;

                var bodyDamage = _ecsWorld.CreateEntityWith<BodyDamageComponent>();
                bodyDamage.PedEntity = pedEntity;
                bodyDamage.DamagedPart = bodyPart;
            }
        }

        private unsafe BodyParts GetDamagedBodyPart(Ped target)
        {
            int damagedBoneNum = 0;
            int* x = &damagedBoneNum;
            Function.Call(Hash.GET_PED_LAST_DAMAGE_BONE, target, x);

            if (damagedBoneNum != 0)
            {
                Enum.TryParse(damagedBoneNum.ToString(), out Bone damagedBone);
                SendDebug($"It was {damagedBone}");

                switch (damagedBone)
                {
                    case Bone.SKEL_Head:
                        SendDebug($"You got {BodyParts.HEAD}");
                        return BodyParts.HEAD;
                    case Bone.SKEL_Neck_1:
                        SendDebug($"You got {BodyParts.NECK}");
                        return BodyParts.NECK;
                    case Bone.SKEL_Spine2:
                    case Bone.SKEL_Spine3:
                        SendDebug($"You got {BodyParts.UPPER_BODY}");
                        return BodyParts.UPPER_BODY;
                    case Bone.SKEL_Pelvis:
                    case Bone.SKEL_Spine_Root:
                    case Bone.SKEL_Spine0:
                    case Bone.SKEL_Spine1:
                    case Bone.SKEL_ROOT:
                        SendDebug($"You got {BodyParts.LOWER_BODY}");
                        return BodyParts.LOWER_BODY;
                    case Bone.SKEL_L_Thigh:
                    case Bone.SKEL_R_Thigh:
                    case Bone.SKEL_L_Toe0:
                    case Bone.SKEL_R_Toe0:
                    case Bone.SKEL_R_Foot:
                    case Bone.SKEL_L_Foot:
                    case Bone.SKEL_L_Calf:
                    case Bone.SKEL_R_Calf:
                        SendDebug($"You got {BodyParts.LEG}");
                        return BodyParts.LEG;
                    case Bone.SKEL_L_UpperArm:
                    case Bone.SKEL_R_UpperArm:
                    case Bone.SKEL_L_Clavicle:
                    case Bone.SKEL_R_Clavicle:
                    case Bone.SKEL_L_Forearm:
                    case Bone.SKEL_R_Forearm:
                    case Bone.SKEL_L_Hand:
                    case Bone.SKEL_R_Hand:
                        SendDebug($"You got {BodyParts.ARM}");
                        return BodyParts.ARM;
                }
            }

            return BodyParts.NOTHING;
        }

        private void SendDebug(string message)
        {
            var notification = _ecsWorld.CreateEntityWith<NotificationComponent>();
            notification.Level = NotifyLevels.DEBUG;
            notification.StringToShow = message;
        }
    }
}