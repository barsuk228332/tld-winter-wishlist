using System;
using Il2Cpp;

namespace WinterWishlist
{
    internal static class SpawnService
    {
        internal static bool LastToFeet { get; private set; }

        internal static GearItem Spawn(ItemEntry entry, int count, float condition, bool toFeet, out string error)
        {
            error = null;
            try
            {
                var pm = GameManager.GetPlayerManagerComponent();
                if (pm == null)
                {
                    error = "нет активного игрока";
                    return null;
                }

                if (count < 1)
                    count = 1;

                if (toFeet)
                {
                    var prefab = ResolvePrefab(entry);
                    if (prefab == null)
                    {
                        error = "префаб не найден";
                        return null;
                    }
                    LastToFeet = true;
                    var dropped = pm.InstantiateItemAtPlayersFeet(prefab, count);
                    if (dropped != null && condition < 0.999f)
                    {
                        try
                        {
                            dropped.SetNormalizedHP(condition, false);
                        }
                        catch (Exception e)
                        {
                            WinterWishlistMod.Log.Warning("Состояние предмета под ногами: " + e.Message);
                        }
                    }
                    return dropped;
                }

                if (entry.Prefab != null)
                {
                    LastToFeet = false;
                    return pm.InstantiateItemInPlayerInventory(entry.Prefab, count, condition);
                }

                var item = pm.AddItemCONSOLE(entry.Id, count, condition);
                LastToFeet = false;
                if (item == null)
                    error = "AddItemCONSOLE вернул null";
                return item;
            }
            catch (Exception e)
            {
                error = e.GetType().Name + ": " + e.Message;
                WinterWishlistMod.Log.Error("Спавн '" + entry.Id + "': " + e);
                return null;
            }
        }

        private static GearItem ResolvePrefab(ItemEntry entry)
        {
            if (entry.Prefab != null)
                return entry.Prefab;
            return GearItem.LoadGearItemPrefab(entry.Id);
        }
    }
}
