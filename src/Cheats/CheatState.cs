using System;
using Il2Cpp;
using Il2CppTLD.IntBackedUnit;
using UnityEngine;

namespace WinterWishlist.Cheats
{
    /// <summary>
    /// Состояние читов и периодическое применение «поддерживающих» эффектов
    /// (здоровье, голод, жажда, усталость, тепло, выносливость).
    /// </summary>
    internal static class CheatState
    {
        internal const float InfiniteCarryCapacity = 500f;
        private const float TickSeconds = 0.5f;

        internal static bool God;
        internal static bool KeepHealth;
        internal static bool NoHunger;
        internal static bool NoThirst;
        internal static bool NoFatigue;
        internal static bool NoFreezing;

        internal static bool UnlimitedSprint;
        internal static bool UnlimitedStamina;
        internal static bool UnlimitedCarry;
        internal static bool NoSprains;

        internal static bool SuppressFrostbite;
        internal static bool SuppressHypothermia;

        private static float _nextTick;
        private static bool _carryCaptured;
        private static ItemWeight _carryOriginal;
        private static bool _errorLogged;

        internal static void Tick()
        {
            if (Time.realtimeSinceStartup < _nextTick)
                return;
            _nextTick = Time.realtimeSinceStartup + TickSeconds;
            Apply();
        }

        /// <summary>Применяет включённые эффекты. Вызывается часто, поэтому всё в try/catch.</summary>
        internal static void Apply()
        {
            try
            {
                if (God)
                    ApplyGod(true);

                if (KeepHealth)
                {
                    var condition = GameManager.GetConditionComponent();
                    if (condition != null)
                        condition.m_CurrentHP = condition.m_MaxHP;
                }

                if (NoHunger)
                {
                    var hunger = GameManager.GetHungerComponent();
                    if (hunger != null)
                        hunger.m_CurrentReserveCalories = hunger.m_MaxReserveCalories;
                }

                if (NoThirst)
                {
                    var thirst = GameManager.GetThirstComponent();
                    if (thirst != null)
                        thirst.m_CurrentThirst = 0f;
                }

                if (NoFatigue)
                {
                    var fatigue = GameManager.GetFatigueComponent();
                    if (fatigue != null)
                        fatigue.m_CurrentFatigue = 0f;
                }

                if (NoFreezing)
                {
                    var freezing = GameManager.GetFreezingComponent();
                    if (freezing != null)
                        freezing.m_CurrentFreezing = 0f;
                }

                if (UnlimitedSprint || UnlimitedStamina || NoSprains)
                {
                    var movement = GameManager.GetPlayerMovementComponent();
                    if (movement != null)
                    {
                        if (UnlimitedSprint)
                            PlayerMovement.m_UnlimitedSprint = true;
                        if (UnlimitedStamina)
                            movement.m_SprintStamina = movement.m_MaxSprintStamina;
                        if (NoSprains)
                            movement.m_ForceNoRandomSprain = true;
                    }
                }

                if (UnlimitedCarry)
                {
                    var encumber = GameManager.GetEncumberComponent();
                    if (encumber != null)
                    {
                        if (!_carryCaptured)
                        {
                            _carryOriginal = encumber.m_MaxCarryCapacity;
                            _carryCaptured = true;
                        }
                        encumber.m_MaxCarryCapacity = ItemWeight.FromKilograms(InfiniteCarryCapacity);
                    }
                }

                if (SuppressFrostbite)
                {
                    var frostbite = GameManager.GetFrostbiteComponent();
                    if (frostbite != null)
                        frostbite.SetSuppressFrostbite(true);
                }

                if (SuppressHypothermia)
                {
                    var hypothermia = GameManager.GetHypothermiaComponent();
                    if (hypothermia != null)
                        hypothermia.SetSuppressHypothermia(true);
                }
            }
            catch (Exception e)
            {
                if (!_errorLogged)
                {
                    _errorLogged = true;
                    WinterWishlistMod.Log.Warning("Читы: " + e.Message);
                }
            }
        }

        private static void ApplyGod(bool on)
        {
            var condition = GameManager.GetConditionComponent();
            if (condition != null)
            {
                condition.m_Invulnerable = on;
                condition.m_NeverDie = on;
            }

            var player = GameManager.GetPlayerManagerComponent();
            if (player != null)
                player.m_God = on;
        }

        /**** Setters (вызываются из UI) ****/

        internal static void SetGod(bool on)
        {
            God = on;
            if (!on)
            {
                try
                {
                    ApplyGod(false);
                }
                catch (Exception e)
                {
                    WinterWishlistMod.Log.Warning("Отключение бессмертия: " + e.Message);
                }
            }
        }

        internal static void SetUnlimitedCarry(bool on)
        {
            UnlimitedCarry = on;
            if (!on)
            {
                try
                {
                    var encumber = GameManager.GetEncumberComponent();
                    if (encumber != null)
                        encumber.m_MaxCarryCapacity = _carryOriginal;
                    _carryCaptured = false;
                }
                catch (Exception e)
                {
                    WinterWishlistMod.Log.Warning("Отключение грузоподъёмности: " + e.Message);
                }
            }
        }

        internal static void SetUnlimitedSprint(bool on)
        {
            UnlimitedSprint = on;
            if (!on)
            {
                try
                {
                    PlayerMovement.m_UnlimitedSprint = false;
                }
                catch (Exception e)
                {
                    WinterWishlistMod.Log.Warning("Отключение спринта: " + e.Message);
                }
            }
        }

        internal static void SetNoSprains(bool on)
        {
            NoSprains = on;
            if (!on)
            {
                try
                {
                    var movement = GameManager.GetPlayerMovementComponent();
                    if (movement != null)
                        movement.m_ForceNoRandomSprain = false;
                }
                catch (Exception e)
                {
                    WinterWishlistMod.Log.Warning("Отключение защиты от растяжений: " + e.Message);
                }
            }
        }

        internal static void SetSuppressFrostbite(bool on)
        {
            SuppressFrostbite = on;
            try
            {
                var frostbite = GameManager.GetFrostbiteComponent();
                if (frostbite != null)
                    frostbite.SetSuppressFrostbite(on);
            }
            catch (Exception e)
            {
                WinterWishlistMod.Log.Warning("Подавление обморожения: " + e.Message);
            }
        }

        internal static void SetSuppressHypothermia(bool on)
        {
            SuppressHypothermia = on;
            try
            {
                var hypothermia = GameManager.GetHypothermiaComponent();
                if (hypothermia != null)
                    hypothermia.SetSuppressHypothermia(on);
            }
            catch (Exception e)
            {
                WinterWishlistMod.Log.Warning("Подавление гипотермии: " + e.Message);
            }
        }

        /// <summary>Сбрасывает все читы (вызывается при выгрузке мода).</summary>
        internal static void ResetAll()
        {
            SetGod(false);
            SetUnlimitedCarry(false);
            SetUnlimitedSprint(false);
            SetNoSprains(false);
            SetSuppressFrostbite(false);
            SetSuppressHypothermia(false);
        }
    }
}
