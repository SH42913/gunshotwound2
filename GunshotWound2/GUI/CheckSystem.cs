using GunshotWound2.Configs;
using GunshotWound2.Damage;
using GunshotWound2.HitDetection;
using GunshotWound2.Pain;
using GunshotWound2.Utils;
using Leopotam.Ecs;

namespace GunshotWound2.GUI
{
    [EcsInject]
    public sealed class CheckSystem : IEcsRunSystem
    {
        private readonly EcsWorld _ecsWorld = null;
        private readonly EcsFilterSingle<MainConfig> _config = null;
        private readonly EcsFilterSingle<LocaleConfig> _locale = null;
        private readonly EcsFilter<ShowHealthStateEvent> _events = null;
        private readonly EcsFilter<BleedingComponent> _wounds = null;

        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(CheckSystem);
#endif

            for (var i = 0; i < _events.EntitiesCount; i++)
            {
                var pedEntity = _events.Components1[i].Entity;
                if (!_ecsWorld.IsEntityExists(pedEntity)) continue;

                var woundedPed = _ecsWorld.GetComponent<WoundedPedComponent>(pedEntity);
                if (woundedPed == null) continue;

                ShowHealth(woundedPed);
                ShowArmor(woundedPed);
                ShowPain(woundedPed, pedEntity);
                ShowCrits(woundedPed);
                ShowBleedings(woundedPed, pedEntity);
            }

            _events.CleanFilter();
        }

        private void ShowHealth(WoundedPedComponent woundedPed)
        {
            var playerConfig = _config.Data.PlayerConfig;
            var healthPercent = !woundedPed.IsPlayer
                ? woundedPed.PedHealth / woundedPed.PedMaxHealth
                : (woundedPed.Health - playerConfig.MinimalHealth) /
                  (playerConfig.MaximalHealth - playerConfig.MinimalHealth);

            var healthString = (healthPercent * 100f).ToString("0.0");
            if (healthPercent >= 0.7f)
            {
                SendMessage($"~s~{_locale.Data.Health}: ~g~{healthString}%~s~");
            }
            else if (healthPercent > 0.5f)
            {
                SendMessage($"~s~{_locale.Data.Health}: ~y~{healthString}%~s~");
            }
            else if (healthPercent > 0.2f)
            {
                SendMessage($"~s~{_locale.Data.Health}: ~o~{healthString}%~s~");
            }
            else if (healthPercent > 0f)
            {
                SendMessage($"~s~{_locale.Data.Health}: ~r~{healthString}%~s~");
            }
            else
            {
                SendMessage($"~r~{_locale.Data.YouAreDead}~s~");
            }
        }

        private void ShowPain(WoundedPedComponent woundedPed, int pedEntity)
        {
            var pain = _ecsWorld.GetComponent<PainComponent>(pedEntity);
            if (pain == null || woundedPed.IsDead) return;

            var painPercent = pain.CurrentPain / woundedPed.MaximalPain;
            if (painPercent > 3f)
            {
                SendMessage($"~s~{_locale.Data.Pain}: ~r~>300%~s~");
            }
            else
            {
                var painString = (painPercent * 100f).ToString("0.0");
                if (painPercent > 1f)
                {
                    SendMessage($"~s~{_locale.Data.Pain}: ~r~{painString}%~s~");
                }
                else if (painPercent > 0.5f)
                {
                    SendMessage($"~s~{_locale.Data.Pain}: ~o~{painString}%~s~");
                }
                else if (painPercent > 0.2f)
                {
                    SendMessage($"~s~{_locale.Data.Pain}: ~y~{painString}%~s~");
                }
                else if (painPercent > 0f)
                {
                    SendMessage($"~s~{_locale.Data.Pain}: ~g~{painString}%~s~");
                }
            }
        }

        private void ShowArmor(WoundedPedComponent woundedPed)
        {
            if (woundedPed.Armor <= 0) return;
            var armorPercent = woundedPed.Armor / 100f;

            if (armorPercent > 0.7f)
            {
                SendMessage($"~g~{_locale.Data.ArmorLooksGreat} ~s~");
            }
            else if (armorPercent > 0.4f)
            {
                SendMessage($"~y~{_locale.Data.ScratchesOnArmor} ~s~");
            }
            else if (armorPercent > 0.15f)
            {
                SendMessage($"~o~{_locale.Data.DentsOnArmor} ~s~");
            }
            else
            {
                SendMessage($"~r~{_locale.Data.ArmorLooksAwful} ~s~");
            }
        }

        private void ShowCrits(WoundedPedComponent woundedPed)
        {
            var healthInfo = "";
            if (woundedPed.Crits <= 0) return;

            healthInfo += $"~s~{_locale.Data.Crits} ~r~";

            if (woundedPed.Crits.Has(CritTypes.NERVES_DAMAGED))
            {
                healthInfo += $"{_locale.Data.NervesCrit} ";
            }

            if (woundedPed.Crits.Has(CritTypes.HEART_DAMAGED))
            {
                healthInfo += $"{_locale.Data.HeartCrit} ";
            }

            if (woundedPed.Crits.Has(CritTypes.LUNGS_DAMAGED))
            {
                healthInfo += $"{_locale.Data.LungsCrit} ";
            }

            if (woundedPed.Crits.Has(CritTypes.STOMACH_DAMAGED))
            {
                healthInfo += $"{_locale.Data.StomachCrit} ";
            }

            if (woundedPed.Crits.Has(CritTypes.GUTS_DAMAGED))
            {
                healthInfo += $"{_locale.Data.GutsCrit} ";
            }

            if (woundedPed.Crits.Has(CritTypes.ARMS_DAMAGED))
            {
                healthInfo += $"{_locale.Data.ArmsCrit} ";
            }

            if (woundedPed.Crits.Has(CritTypes.LEGS_DAMAGED))
            {
                healthInfo += $"{_locale.Data.LegsCrit} ";
            }

            if (!string.IsNullOrEmpty(healthInfo))
            {
                SendMessage(healthInfo + "~s~");
            }
        }

        private void ShowBleedings(WoundedPedComponent woundedPed, int pedEntity)
        {
            if (woundedPed.BleedingCount <= 0) return;

            var woundList = "";
            for (var woundIndex = 0; woundIndex < _wounds.EntitiesCount; woundIndex++)
            {
                var wound = _wounds.Components1[woundIndex];
                if (pedEntity != wound.Entity) continue;

                if (woundedPed.MostDangerBleedingEntity != null &&
                    woundedPed.MostDangerBleedingEntity == _wounds.Entities[woundIndex])
                {
                    woundList += "~g~->~s~";
                }

                var woundConfig = _config.Data.WoundConfig;
                if (wound.BleedSeverity > woundConfig.EmergencyBleedingLevel)
                {
                    woundList += $"~r~{wound.Name}~s~\n";
                }
                else if (wound.BleedSeverity > woundConfig.EmergencyBleedingLevel / 2)
                {
                    woundList += $"~o~{wound.Name}~s~\n";
                }
                else if (wound.BleedSeverity > woundConfig.EmergencyBleedingLevel / 4)
                {
                    woundList += $"~y~{wound.Name}\n";
                }
                else
                {
                    woundList += $"~s~{wound.Name}\n";
                }
            }

            SendMessage($"~s~{_locale.Data.Wounds}:\n{woundList}");
        }

        private void SendMessage(string message, NotifyLevels level = NotifyLevels.COMMON)
        {
#if !DEBUG
            if (level == NotifyLevels.DEBUG) return;
#endif

            var notification = _ecsWorld.CreateEntityWith<ShowNotificationEvent>();
            notification.Level = level;
            notification.StringToShow = message;
        }
    }
}