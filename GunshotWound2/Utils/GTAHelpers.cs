namespace GunshotWound2.Utils {
    using GTA;
    using GTA.Math;
    using GTA.Native;

    public static class GTAHelpers {
        private const string DEATH_ANIM_NAME = "die";
        private static readonly CrClipAsset DRIVER_DEATH_CLIP = new("veh@std@ds@base", DEATH_ANIM_NAME);
        private static readonly CrClipAsset PASSENGER_DEATH_CLIP = new("veh@std@ps@base", DEATH_ANIM_NAME);
        private static readonly uint WEAPON_UNARMED_HASH = StringHash.AtStringHashUtf8("WEAPON_UNARMED");
        private static readonly uint SLOT_UNARMED_HASH = StringHash.AtStringHashUtf8("SLOT_UNARMED");

        public static bool TryGetClosestPed(out Ped closestPed, float radius) {
            Ped playerPed = Game.Player.Character;
            Ped[] closestPeds = World.GetNearbyPeds(playerPed, radius);
            if (closestPeds.Length < 1) {
                closestPed = null;
                return false;
            }

            if (closestPeds.Length == 1) {
                closestPed = closestPeds[0];
                return true;
            }

            var closestPedIndex = 0;
            var minDistance = float.MaxValue;
            Vector3 playerPos = playerPed.Position;
            for (int i = 0; i < closestPeds.Length; i++) {
                Ped ped = closestPeds[i];
                float distance = World.GetDistance(playerPos, ped.Position);
                if (distance < minDistance) {
                    closestPedIndex = i;
                    minDistance = distance;
                }
            }

            closestPed = closestPeds[closestPedIndex];
            return true;
        }

        public static int ConvertToMilliSec(this float timeInSec) {
            return (int)(timeInSec * 1000);
        }

        public static int GetPropTextureCount(Ped ped, PedPropAnchorPoint anchorPoint, int propIndex) {
            return Function.Call<int>(Hash.GET_NUMBER_OF_PED_PROP_TEXTURE_VARIATIONS, ped, (int)anchorPoint, propIndex);
        }

        public static bool IsValid(this Entity entity) {
            return entity != null && entity.Exists();
        }

        public static bool IsValidWeapon(uint hash) {
            return Function.Call<bool>(Hash.IS_WEAPON_VALID, hash);
        }

        public static bool StandsStill(this Ped ped) {
            bool isMoving = ped.IsRagdoll
                            || ped.IsWalking
                            || ped.IsRunning
                            || ped.IsSprinting
                            || ped.IsShooting
                            || ped.IsJumping
                            || ped.IsAiming
                            || ped.IsReloading
                            || ped.IsSwimming;

            return !isMoving;
        }

        public static DamageType GetWeaponDamageType(uint weaponHash) {
            return (DamageType)Function.Call<int>(Hash.GET_WEAPON_DAMAGE_TYPE, weaponHash);
        }

        public static Vector3 GetPedBoneCoords(Ped ped, Bone bone, Vector3 offset = default) {
            return Function.Call<Vector3>(Hash.GET_PED_BONE_COORDS, ped, (int)bone, offset.X, offset.Y, offset.Z);
        }

        public static int CreateParticleEffectAtCoord(string effectName,
                                                      Vector3 position,
                                                      Vector3 rotation = default,
                                                      float scale = 1f) {
            return Function.Call<int>(Hash.START_PARTICLE_FX_LOOPED_AT_COORD, effectName,
                                      position.X, position.Y, position.Z,
                                      rotation.X, rotation.Y, rotation.Z,
                                      scale, false, false, false, false);
        }

        public static void SetParticleEffectAlpha(int handle, float alpha) {
            Function.Call(Hash.SET_PARTICLE_FX_LOOPED_ALPHA, handle, alpha);
        }

        public static void RemoveParticleEffect(int handle) {
            Function.Call(Hash.REMOVE_PARTICLE_FX, handle);
        }

        public static void PlayDeathAnimationInVehicle(Ped ped) {
            CrClipAsset? vehDeathClip;
            switch (ped.SeatIndex) {
                case VehicleSeat.Driver:    vehDeathClip = DRIVER_DEATH_CLIP; break;
                case VehicleSeat.Passenger: vehDeathClip = PASSENGER_DEATH_CLIP; break;
                default:                    vehDeathClip = null; break;
            }

            if (!vehDeathClip.HasValue) {
                return;
            }

            const AnimationFlags flags = AnimationFlags.StayInEndFrame;
            AnimationBlendDelta blendIn = AnimationBlendDelta.NormalBlendIn;
            AnimationBlendDelta blendOut = AnimationBlendDelta.NormalBlendOut;
            CrClipAsset clip = vehDeathClip.Value;
            ped.Task.PlayAnimation(clip, blendIn, blendOut, duration: -1, flags, startPhase: 0f);
        }

        public static bool IsHumanWeapon(uint hash) {
            var slotHash = Function.Call<uint>(Hash.GET_WEAPONTYPE_SLOT, hash);
            if (slotHash == 0) {
                return false;
            }

            return slotHash != SLOT_UNARMED_HASH || hash == WEAPON_UNARMED_HASH;
        }
    }
}