using GunshotWound2.Components.UiComponents;
using GunshotWound2.Components.WoundComponents;
using GunshotWound2.Configs;
using LeopotamGroup.Ecs;

namespace GunshotWound2.Systems.UiSystems
{
    [EcsInject]
    public class CheckSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilterSingle<MainConfig> _config;
        
        private EcsFilter<CheckPedComponent> _components;
        private EcsFilter<BleedingComponent> _wounds;
        
        public void Run()
        {
            GunshotWound2.LastSystem = nameof(CheckSystem);
            
            for (int i = 0; i < _components.EntitiesCount; i++)
            {
                int pedEntity = _components.Components1[i].PedEntity;
                var woundedPed = _ecsWorld.GetComponent<WoundedPedComponent>(pedEntity);

                if (woundedPed != null)
                {
                    var healthPercent = !woundedPed.IsPlayer
                        ? (float) woundedPed.ThisPed.Health / woundedPed.ThisPed.MaxHealth
                        : (woundedPed.Health - _config.Data.PlayerConfig.MinimalHealth) / 
                          (_config.Data.PlayerConfig.MaximalHealth - _config.Data.PlayerConfig.MinimalHealth);
                    
                    if (healthPercent >= 0.8f)
                    {
                        SendMessage($"Health: ~g~{healthPercent * 100f:0.0}%~s~");
                    }
                    else if(healthPercent > 0.5f)
                    {
                        SendMessage($"Health: ~y~{healthPercent * 100f:0.0}%~s~");
                    }
                    else if(healthPercent > 0.2f)
                    {
                        SendMessage($"Health: ~o~{healthPercent * 100f:0.0}%~s~");
                    }
                    else if(healthPercent > 0f)
                    {
                        SendMessage($"Health: ~r~{healthPercent * 100f:0.0}%~s~");
                    }
                    else
                    {
                        SendMessage("~r~You are dead!~s~");
                    }
                    
                    var painPercent = woundedPed.PainMeter / woundedPed.MaximalPain;
                    if (painPercent > 1.5f)
                    {
                        SendMessage("~s~Pain Level: ~r~>150%~s~");
                    }
                    else if (painPercent > 1f)
                    {
                        SendMessage($"~s~Pain Level: ~r~{painPercent * 100f:0.0}%~s~");
                    }
                    else if (painPercent > 0.5f)
                    {
                        SendMessage($"~s~Pain Level: ~o~{painPercent * 100f:0.0}%~s~");
                    }
                    else if (painPercent > 0.2f)
                    {
                        SendMessage($"~s~Pain Level: ~y~{painPercent * 100f:0.0}%~s~");
                    }
                    else
                    {
                        SendMessage($"~s~Pain Level: ~g~{painPercent * 100f:0.0}%~s~");
                    }
                    
                    var armorPercent = woundedPed.Armor / 100f;
                    if (armorPercent > 0)
                    {
                        if (armorPercent > 0.6f)
                        {
                            SendMessage("~g~Your armor looks great~s~");
                        }
                        else if (armorPercent > 0.3f)
                        {
                            SendMessage("~y~Some scratches on your armor~s~");
                        }
                        else if (armorPercent > 0.1f)
                        {
                            SendMessage("~o~Large dents on your armor~s~");
                        }
                        else
                        {
                            SendMessage("~r~Your armor looks awful~s~");
                        }
                    }

                    string healthInfo = "";
                    if (woundedPed.DamagedParts > 0)
                    {
                        healthInfo += "Damaged body parts: ";
                    
                        if (woundedPed.DamagedParts.HasFlag(DamageTypes.NERVES_DAMAGED))
                        {
                            healthInfo += "~r~Nerves ";
                        }
                    
                        if (woundedPed.DamagedParts.HasFlag(DamageTypes.HEART_DAMAGED))
                        {
                            healthInfo += "~r~Heart ";
                        }
                    
                        if (woundedPed.DamagedParts.HasFlag(DamageTypes.LUNGS_DAMAGED))
                        {
                            healthInfo += "~r~Lungs ";
                        }
                    
                        if (woundedPed.DamagedParts.HasFlag(DamageTypes.STOMACH_DAMAGED))
                        {
                            healthInfo += "~r~Stomach ";
                        }
                    
                        if(woundedPed.DamagedParts.HasFlag(DamageTypes.GUTS_DAMAGED))
                        {
                            healthInfo += "~r~Guts ";
                        }
                    
                        if (woundedPed.DamagedParts.HasFlag(DamageTypes.ARMS_DAMAGED))
                        {
                            healthInfo += "~r~Arms ";
                        }
                    
                        if (woundedPed.DamagedParts.HasFlag(DamageTypes.LEGS_DAMAGED))
                        {
                            healthInfo += "~r~Legs ";
                        }

                        if (!string.IsNullOrEmpty(healthInfo))
                        {
                            SendMessage(healthInfo + "~s~");
                        }
                    }
                }

                string wounds = "";
                for (int woundIndex = 0; woundIndex < _wounds.EntitiesCount; woundIndex++)
                {
                    var wound = _wounds.Components1[woundIndex];
                    if(pedEntity != wound.PedEntity) continue;
                    
                    if (wound.BleedSeverity > _config.Data.WoundConfig.EmergencyBleedingLevel)
                    {
                        wounds += $"~r~{wound.Name}~s~\n";
                    }
                    else if (wound.BleedSeverity > _config.Data.WoundConfig.EmergencyBleedingLevel/2)
                    {
                        wounds += $"~o~{wound.Name}~s~\n";
                    }
                    else if (wound.BleedSeverity > _config.Data.WoundConfig.EmergencyBleedingLevel/4)
                    {
                        wounds += $"~y~{wound.Name}\n";
                    }
                    else
                    {
                        wounds += $"~s~{wound.Name}\n";
                    }
                }

                if (string.IsNullOrEmpty(wounds)) wounds = "~g~You have no wounds~s~";
                SendMessage($"Wounds:\n{wounds}");
                
                _ecsWorld.RemoveEntity(_components.Entities[i]);
            }
        }

        private void SendMessage(string message, NotifyLevels level = NotifyLevels.COMMON)
        {
            var notification = _ecsWorld.CreateEntityWith<NotificationComponent>();
            notification.Level = level;
            notification.StringToShow = message;
        }
    }
}