using System;
using System.Collections.Generic;
using Il2Cpp;

namespace WinterWishlist.Cheats
{
    internal static class ItemService
    {
        private sealed class KitItem
        {
            internal string Token;
            internal int Count;

            internal KitItem(string token, int count)
            {
                Token = token;
                Count = count;
            }
        }

        private static readonly KitItem[] SurvivalKit =
        {
            new KitItem("Matches", 2),
            new KitItem("Firestriker", 1),
            new KitItem("Knife", 1),
            new KitItem("Hatchet", 1),
            new KitItem("Hacksaw", 1),
            new KitItem("Prybar", 1),
            new KitItem("BedRoll", 1),
            new KitItem("RifleAmmo", 20),
            new KitItem("RevolverAmmo", 20),
            new KitItem("Arrow", 10),
            new KitItem("Bandage", 5),
            new KitItem("PainKillers", 5),
            new KitItem("Antibiotics", 5),
            new KitItem("SewingKit", 1),
            new KitItem("Whetstone", 1),
            new KitItem("Water", 4),
        };

        private static readonly KitItem[] BestClothing =
        {
            new KitItem("GEAR_Balaclava", 1),
            new KitItem("GEAR_Toque", 1),
            new KitItem("GEAR_PremiumWinterCoat", 2), // Expedition Parka («Походная парка»)
            new KitItem("GEAR_FishermanSweater", 2),
            new KitItem("GEAR_RabbitSkinMittens", 1),
            new KitItem("GEAR_CombatPants", 2),
            new KitItem("GEAR_ClimbingSocks", 2),
            new KitItem("GEAR_SkiBoots", 1),
            new KitItem("GEAR_LongUnderwearWool", 2),
        };

        /// <summary>Ремонтирует все ремонтируемые предметы в инвентаре.</summary>
        internal static string RepairAll()
        {
            try
            {
                var inventory = GameManager.GetInventoryComponent();
                if (inventory?.m_Items == null)
                    return UiLang.T("Инвентарь недоступен", "Inventory unavailable");

                int repaired = 0;
                foreach (var wrapped in inventory.m_Items)
                {
                    try
                    {
                        var gear = wrapped.m_GearItem;
                        if (gear == null || !gear.m_Repairable)
                            continue;
                        if (gear.GetNormalizedCondition() >= 0.999f)
                            continue;

                        gear.SetNormalizedHP(1f, false);
                        repaired++;
                    }
                    catch
                    {
                        // отдельные предметы могут не поддерживать ремонт — пропускаем
                    }
                }

                return UiLang.T("Починено предметов: ", "Items repaired: ") + repaired;
            }
            catch (Exception e)
            {
                WinterWishlistMod.Log.Warning("RepairAll: " + e);
                return UiLang.T("Ошибка: ", "Error: ") + e.Message;
            }
        }

        /// <summary>Выдаёт базовый набор выживальщика.</summary>
        internal static string GiveSurvivalKit()
        {
            try
            {
                ItemCatalog.EnsureBuilt();

                int given = 0;
                var missing = new List<string>();
                foreach (var kitItem in SurvivalKit)
                {
                    var entry = Find(kitItem.Token);
                    if (entry == null)
                    {
                        missing.Add(kitItem.Token);
                        continue;
                    }

                    var spawned = SpawnService.Spawn(entry, kitItem.Count, 1f, false, out string error);
                    if (spawned != null)
                        given++;
                    else
                        missing.Add(kitItem.Token + ": " + error);
                }

                string text = UiLang.T("Выдано наборов: ", "Kit items given: ") + given;
                if (missing.Count > 0)
                    text += UiLang.T(" • не найдено: ", " • missing: ") + string.Join(", ", missing);
                return text;
            }
            catch (Exception e)
            {
                WinterWishlistMod.Log.Warning("GiveSurvivalKit: " + e);
                return UiLang.T("Ошибка: ", "Error: ") + e.Message;
            }
        }

        /// <summary>Выдаёт полный комплект лучшей одежды (состояние 100%).</summary>
        internal static string GiveBestClothing()
        {
            try
            {
                ItemCatalog.EnsureBuilt();

                int total = 0;
                var missing = new List<string>();
                foreach (var kitItem in BestClothing)
                {
                    var entry = FindExact(kitItem.Token);
                    if (entry == null)
                    {
                        missing.Add(kitItem.Token);
                        continue;
                    }

                    // Одежда не стакается: AddItemCONSOLE игнорирует count > 1,
                    // поэтому создаём каждый предмет отдельно.
                    for (int i = 0; i < kitItem.Count; i++)
                    {
                        var spawned = SpawnService.Spawn(entry, 1, 1f, false, out string error);
                        if (spawned != null)
                        {
                            total++;
                            continue;
                        }

                        missing.Add(entry.Display + ": " + error);
                        break;
                    }
                }

                string text = UiLang.T("Выдано одежды: ", "Clothing items given: ") + total;
                if (missing.Count > 0)
                    text += UiLang.T(" • не найдено: ", " • missing: ") + string.Join(", ", missing);
                return text;
            }
            catch (Exception e)
            {
                WinterWishlistMod.Log.Warning("GiveBestClothing: " + e);
                return UiLang.T("Ошибка: ", "Error: ") + e.Message;
            }
        }

        /// <summary>Ищет предмет по точному ID, иначе — по подстроке.</summary>
        private static ItemEntry FindExact(string id)
        {
            foreach (var entry in ItemCatalog.Items)
            {
                if (string.Equals(entry.Id, id, StringComparison.OrdinalIgnoreCase))
                    return entry;
            }
            return Find(id);
        }

        private static ItemEntry Find(string token)
        {
            foreach (var entry in ItemCatalog.Items)
            {
                if (entry.Id != null && entry.Id.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                    return entry;
            }
            return null;
        }
    }
}
