using System;
using System.Collections.Generic;
using Il2Cpp;

namespace WinterWishlist.Cheats
{
    internal static class AfflictionService
    {
        /// <summary>Список активных болезней/травм для отображения в окне.</summary>
        internal static List<string> Active()
        {
            var list = new List<string>();

            Try(list, "Ожоги", () => GameManager.GetBurnsComponent()?.HasBurns() == true);
            Try(list, "Электроожоги", () => GameManager.GetBurnsElectricComponent()?.HasBurnsElectric() == true);
            Try(list, "Кровотечение", () => GameManager.GetBloodLossComponent()?.HasBloodLoss() == true);
            Try(list, "Перелом ребра", () => GameManager.GetBrokenRibComponent()?.HasBrokenRib() == true);
            Try(list, "Головная боль", () => GameManager.GetHeadacheComponent()?.HasHeadache() == true);
            Try(list, "Гипотермия", () => GameManager.GetHypothermiaComponent()?.HasHypothermia() == true);
            Try(list, "Обморожение", () => GameManager.GetFrostbiteComponent()?.HasFrostbite() == true);
            Try(list, "Инфекция", () => GameManager.GetInfectionComponent()?.HasInfection() == true);
            Try(list, "Риск инфекции", () => GameManager.GetInfectionRiskComponent()?.HasInfectionRisk() == true);
            Try(list, "Отравление", () => GameManager.GetFoodPoisoningComponent()?.HasFoodPoisoning() == true);
            Try(list, "Дизентерия", () => GameManager.GetDysenteryComponent()?.HasDysentery() == true);
            Try(list, "Паразиты", () => GameManager.GetIntestinalParasitesComponent()?.HasIntestinalParasites() == true);
            Try(list, "Растяжение ноги", () => GameManager.GetSprainedAnkleComponent()?.HasSprainedAnkle() == true);
            Try(list, "Растяжение руки", () => GameManager.GetSprainedWristComponent()?.HasSprainedWrist() == true);
            Try(list, "Удушье", () => GameManager.GetSuffocatingComponent()?.HasSuffocatingRisk() == true);

            return list;
        }

        private static void Try(List<string> list, string name, Func<bool> check)
        {
            try
            {
                if (check())
                    list.Add(name);
            }
            catch
            {
                // компонент недоступен — пропускаем
            }
        }

        internal static string CureAll()
        {
            int cured = 0;

            cured += Cure("Ожоги", () =>
            {
                var c = GameManager.GetBurnsComponent();
                if (c == null || !c.HasBurns())
                    return false;
                c.BurnsEnd(false);
                return true;
            });

            cured += Cure("Электроожоги", () =>
            {
                var c = GameManager.GetBurnsElectricComponent();
                if (c == null || !c.HasBurnsElectric())
                    return false;
                c.BurnsElectricEnd(false);
                return true;
            });

            cured += Cure("Кровотечение", () =>
            {
                var c = GameManager.GetBloodLossComponent();
                if (c == null || !c.HasBloodLoss())
                    return false;
                c.BloodLossEnd(0, AfflictionOptions.None);
                return true;
            });

            cured += Cure("Перелом ребра", () =>
            {
                var c = GameManager.GetBrokenRibComponent();
                if (c == null || !c.HasBrokenRib())
                    return false;
                c.BrokenRibEnd(0, true);
                return true;
            });

            cured += Cure("Перелом тела", () =>
            {
                var c = GameManager.GetBrokenBody();
                if (c == null)
                    return false;
                c.Cure(AfflictionOptions.None);
                return true;
            });

            cured += Cure("Головная боль", () =>
            {
                var c = GameManager.GetHeadacheComponent();
                if (c == null || !c.HasHeadache())
                    return false;
                c.Cure();
                return true;
            });

            cured += Cure("Гипотермия", () =>
            {
                var c = GameManager.GetHypothermiaComponent();
                if (c == null || !c.HasHypothermia())
                    return false;
                c.HypothermiaEnd(true);
                return true;
            });

            cured += Cure("Обморожение", () =>
            {
                var c = GameManager.GetFrostbiteComponent();
                if (c == null || !c.HasFrostbite())
                    return false;
                c.FrostbiteEnd();
                return true;
            });

            cured += Cure("Инфекция", () =>
            {
                var c = GameManager.GetInfectionComponent();
                if (c == null || !c.HasInfection())
                    return false;
                c.InfectionEnd(0);
                return true;
            });

            cured += Cure("Риск инфекции", () =>
            {
                var c = GameManager.GetInfectionRiskComponent();
                if (c == null || !c.HasInfectionRisk())
                    return false;
                c.InfectionRiskEnd(0, true);
                return true;
            });

            cured += Cure("Отравление", () =>
            {
                var c = GameManager.GetFoodPoisoningComponent();
                if (c == null || !c.HasFoodPoisoning())
                    return false;
                c.FoodPoisoningEnd(true);
                return true;
            });

            cured += Cure("Дизентерия", () =>
            {
                var c = GameManager.GetDysenteryComponent();
                if (c == null || !c.HasDysentery())
                    return false;
                c.DysenteryEnd(true);
                return true;
            });

            cured += Cure("Паразиты", () =>
            {
                var c = GameManager.GetIntestinalParasitesComponent();
                if (c == null || !c.HasIntestinalParasites())
                    return false;
                c.IntestinalParasitesEnd(true);
                return true;
            });

            cured += Cure("Растяжение ноги", () =>
            {
                var c = GameManager.GetSprainedAnkleComponent();
                if (c == null || !c.HasSprainedAnkle())
                    return false;
                c.SprainedAnkleEnd(0, AfflictionOptions.None);
                return true;
            });

            cured += Cure("Растяжение руки", () =>
            {
                var c = GameManager.GetSprainedWristComponent();
                if (c == null || !c.HasSprainedWrist())
                    return false;
                c.SprainedWristEnd(0, AfflictionOptions.None);
                return true;
            });

            cured += Cure("Удушье", () =>
            {
                var c = GameManager.GetSuffocatingComponent();
                if (c == null)
                    return false;
                c.StopSuffocating();
                return true;
            });

            cured += Cure("Стимулятор", () =>
            {
                var c = GameManager.GetEmergencyStimComponent();
                if (c == null)
                    return false;
                c.ResetEmergencyStim();
                return true;
            });

            cured += Cure("Смертельная травма", () =>
            {
                var c = GameManager.GetCheatDeathAffliction();
                if (c == null)
                    return false;
                c.Cure(AfflictionOptions.None);
                return true;
            });

            return cured > 0
                ? UiLang.T("Вылечено: ", "Cured: ") + cured
                : UiLang.T("Активных болезней нет", "No active afflictions");
        }

        private static int Cure(string name, Func<bool> action)
        {
            try
            {
                return action() ? 1 : 0;
            }
            catch (Exception e)
            {
                WinterWishlistMod.Log.Warning("Лечение (" + name + "): " + e.Message);
                return 0;
            }
        }
    }
}
