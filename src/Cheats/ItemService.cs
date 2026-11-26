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
