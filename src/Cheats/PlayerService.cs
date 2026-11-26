using System;
using Il2Cpp;

namespace WinterWishlist.Cheats
{
    /// <summary>Одноразовые действия с игроком.</summary>
    internal static class PlayerService
    {
        internal static bool FullHealth(out string error)
        {
            error = null;
            try
            {
                var condition = GameManager.GetConditionComponent();
                if (condition == null)
                {
                    error = UiLang.T("нет данных об игроке", "no player data");
                    return false;
                }

                condition.m_CurrentHP = condition.m_MaxHP;
                return true;
            }
            catch (Exception e)
            {
                error = e.Message;
                WinterWishlistMod.Log.Warning("Здоровье: " + e);
                return false;
            }
        }

        internal static bool RestoreAll(out string error)
        {
            error = null;
            try
            {
                var fatigue = GameManager.GetFatigueComponent();
                if (fatigue != null)
                    fatigue.m_CurrentFatigue = 0f;

                var hunger = GameManager.GetHungerComponent();
                if (hunger != null)
                    hunger.m_CurrentReserveCalories = hunger.m_MaxReserveCalories;

                var thirst = GameManager.GetThirstComponent();
                if (thirst != null)
                    thirst.m_CurrentThirst = 0f;

                var freezing = GameManager.GetFreezingComponent();
                if (freezing != null)
                    freezing.m_CurrentFreezing = 0f;

                var movement = GameManager.GetPlayerMovementComponent();
                if (movement != null)
                    movement.m_SprintStamina = movement.m_MaxSprintStamina;

                return true;
            }
            catch (Exception e)
            {
                error = e.Message;
                WinterWishlistMod.Log.Warning("Восстановление: " + e);
                return false;
            }
        }
    }
}
