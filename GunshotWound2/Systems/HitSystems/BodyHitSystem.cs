using System;
using GTA;
using GTA.Native;
using GunshotWound2.Components.Events.BodyHitEvents;
using GunshotWound2.Components.Events.GuiEvents;
using GunshotWound2.Components.StateComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.HitSystems
{
    [EcsInject]
    public class BodyHitSystem : IEcsRunSystem
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
                if(!_ecsWorld.IsEntityExists(pedEntity)) continue;
                
                var woundedPed = _ecsWorld.GetComponent<WoundedPedComponent>(pedEntity);
                if(woundedPed == null) continue;

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
                case Bone.SKEL_Head:
                    return BodyParts.HEAD;
                case Bone.SKEL_Neck_1:
                    return BodyParts.NECK;
                case Bone.SKEL_Spine2:
                case Bone.SKEL_Spine3:
                    return BodyParts.UPPER_BODY;
                case Bone.SKEL_ROOT:
                case Bone.SKEL_Spine_Root:
                case Bone.SKEL_Spine0:
                case Bone.SKEL_Spine1:
                case Bone.SKEL_Pelvis:
                    return BodyParts.LOWER_BODY;
                case Bone.SKEL_L_Thigh:
                case Bone.SKEL_R_Thigh:
                case Bone.SKEL_L_Toe0:
                case Bone.SKEL_R_Toe0:
                case Bone.SKEL_R_Foot:
                case Bone.SKEL_L_Foot:
                case Bone.SKEL_L_Calf:
                case Bone.SKEL_R_Calf:
                    return BodyParts.LEG;
                case Bone.SKEL_L_UpperArm:
                case Bone.SKEL_R_UpperArm:
                case Bone.SKEL_L_Clavicle:
                case Bone.SKEL_R_Clavicle:
                case Bone.SKEL_L_Forearm:
                case Bone.SKEL_R_Forearm:
                case Bone.SKEL_L_Hand:
                case Bone.SKEL_R_Hand:
                    return BodyParts.ARM;
            }
            SendMessage("WARNING! Nothing bone is " + damagedBone, NotifyLevels.ALERT);

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