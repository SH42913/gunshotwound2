using GTA;
using GunshotWound2.Effects;
using GunshotWound2.GUI;
using GunshotWound2.HitDetection;
using GunshotWound2.Player;
using GunshotWound2.Utils;
using Leopotam.Ecs;

namespace GunshotWound2.Pain
{
    [EcsInject]
    public sealed class UnbearablePainStateSystem : BasePainStateSystem<UnbearablePainChangeStateEvent>
    {
        public UnbearablePainStateSystem()
        {
            CurrentState = PainStates.UNBEARABLE;
        }

        protected override void ExecuteState(WoundedPedComponent woundedPed, int pedEntity)
        {
            base.ExecuteState(woundedPed, pedEntity);

            SendPedToRagdoll(pedEntity, RagdollStates.PERMANENT);

            if (woundedPed.IsPlayer && Config.Data.PlayerConfig.CanDropWeapon)
            {
                woundedPed.ThisPed.Weapons.Drop();
            }
            else if (!woundedPed.IsPlayer)
            {
                woundedPed.ThisPed.Weapons.Drop();
            }

            var speech = GunshotWound2.Random.IsTrueWithProbability(0.5f) ? "DYING_HELP" : "DYING_MOAN";
            woundedPed.ThisPed.PlayAmbientSpeech(speech, SpeechModifier.ShoutedClear);

            if (!woundedPed.IsPlayer) return;
            Game.Player.IgnoredByEveryone = true;
            EcsWorld.CreateEntityWith<ChangeSpecialAbilityEvent>().Lock = true;

            if (Config.Data.PlayerConfig.PoliceCanForgetYou) Game.Player.WantedLevel = 0;

            if (woundedPed.Crits.Has(CritTypes.NERVES_DAMAGED) || woundedPed.IsDead) return;
            SendMessage(Locale.Data.UnbearablePainMessage, NotifyLevels.WARNING);
        }
    }
}