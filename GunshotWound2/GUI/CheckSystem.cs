using GunshotWound2.Configs;
using GunshotWound2.Damage;
using GunshotWound2.HitDetection;
using GunshotWound2.Pain;
using Leopotam.Ecs;

namespace GunshotWound2.GUI
{
    [EcsInject]
    public class CheckSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilterSingle<MainConfig> _config;
        private EcsFilterSingle<LocaleConfig> _locale;

        private EcsFilter<ShowHealthStateEvent> _events;
        private EcsFilter<BleedingComponent> _wounds;

        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(CheckSystem);
#endif

            for (int i = 0; i < _events.EntitiesCount; i++)
            {
                int pedEntity = _events.Components1[i].Entity;
                if (!_ecsWorld.IsEntityExists(pedEntity)) continue;

                var woundedPed = _ecsWorld.GetComponent<WoundedPedComponent>(pedEntity);
                if (woundedPed == null) continue;

                ShowHealth(woundedPed);
                ShowArmor(woundedPed);
                ShowPain(woundedPed, pedEntity);
                ShowCrits(woundedPed);
                ShowBleedings(woundedPed, pedEntity);
            }

            _events.RemoveAllEntities();
        }

        private void ShowHealth(WoundedPedComponent woundedPed)
        {
            var healthPercent = !woundedPed.IsPlayer
                ? (float) woundedPed.ThisPed.Health / woundedPed.ThisPed.MaxHealth
                : (woundedPed.Health - _config.Data.PlayerConfig.MinimalHealth) /
                  (_config.Data.PlayerConfig.MaximalHealth - _config.Data.PlayerConfig.MinimalHealth);

            if (healthPercent >= 0.7f)
            {
                SendMessage($"~s~{_locale.Data.Health}: ~g~{healthPercent * 100f:0.0}%~s~");
            }
            else if (healthPercent > 0.5f)
            {
                SendMessage($"~s~{_locale.Data.Health}: ~y~{healthPercent * 100f:0.0}%~s~");
            }
            else if (healthPercent > 0.2f)
            {
                SendMessage($"~s~{_locale.Data.Health}: ~o~{healthPercent * 100f:0.0}%~s~");
            }
            else if (healthPercent > 0f)
            {
                SendMessage($"~s~{_locale.Data.Health}: ~r~{healthPercent * 100f:0.0}%~s~");
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
            else if (painPercent > 1f)
            {
                SendMessage($"~s~{_locale.Data.Pain}: ~r~{painPercent * 100f:0.0}%~s~");
            }
            else if (painPercent > 0.5f)
            {
                SendMessage($"~s~{_locale.Data.Pain}: ~o~{painPercent * 100f:0.0}%~s~");
            }
            else if (painPercent > 0.2f)
            {
                SendMessage($"~s~{_locale.Data.Pain}: ~y~{painPercent * 100f:0.0}%~s~");
            }
            else if (painPercent > 0f)
            {
                SendMessage($"~s~{_locale.Data.Pain}: ~g~{painPercent * 100f:0.0}%~s~");
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
            string healthInfo = "";
            if (woundedPed.Crits <= 0) return;

            healthInfo += $"~s~{_locale.Data.Crits} ~r~";

            if (woundedPed.Crits.HasFlag(CritTypes.NERVES_DAMAGED))
            {
                healthInfo += $"{_locale.Data.NervesCrit} ";
            }

            if (woundedPed.Crits.HasFlag(CritTypes.HEART_DAMAGED))
            {
                healthInfo += $"{_locale.Data.HeartCrit} ";
            }

            if (woundedPed.Crits.HasFlag(CritTypes.LUNGS_DAMAGED))
            {
                healthInfo += $"{_locale.Data.LungsCrit} ";
            }

            if (woundedPed.Crits.HasFlag(CritTypes.STOMACH_DAMAGED))
            {
                healthInfo += $"{_locale.Data.StomachCrit} ";
            }

            if (woundedPed.Crits.HasFlag(CritTypes.GUTS_DAMAGED))
            {
                healthInfo += $"{_locale.Data.GutsCrit} ";
            }

            if (woundedPed.Crits.HasFlag(CritTypes.ARMS_DAMAGED))
            {
                healthInfo += $"{_locale.Data.ArmsCrit} ";
            }

            if (woundedPed.Crits.HasFlag(CritTypes.LEGS_DAMAGED))
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

            string woundList = "";
            for (int woundIndex = 0; woundIndex < _wounds.EntitiesCount; woundIndex++)
            {
                BleedingComponent wound = _wounds.Components1[woundIndex];
                if (pedEntity != wound.Entity) continue;

                if (woundedPed.MostDangerBleedingEntity != null &&
                    woundedPed.MostDangerBleedingEntity == _wounds.Entities[woundIndex])
                {
                    woundList += "~g~->~s~";
                }

                if (wound.BleedSeverity > _config.Data.WoundConfig.EmergencyBleedingLevel)
                {
                    woundList += $"~r~{wound.Name}~s~\n";
                }
                else if (wound.BleedSeverity > _config.Data.WoundConfig.EmergencyBleedingLevel / 2)
                {
                    woundList += $"~o~{wound.Name}~s~\n";
                }
                else if (wound.BleedSeverity > _config.Data.WoundConfig.EmergencyBleedingLevel / 4)
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
            if(level == NotifyLevels.DEBUG) return;
#endif

            var notification = _ecsWorld.CreateEntityWith<ShowNotificationEvent>();
            notification.Level = level;
            notification.StringToShow = message;
        }
    }
}