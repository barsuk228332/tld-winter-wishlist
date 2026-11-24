using System;
using System.Collections.Generic;
using System.Reflection;
using Il2Cpp;
using UnityEngine;

namespace WinterWishlist
{
    internal sealed class ItemEntry
    {
        internal string Id;
        internal string Display;
        internal GearItem Prefab;
        internal bool FromMod;
        internal string Search;

        internal void RefreshSearch()
        {
            Search = ((Display ?? string.Empty) + " " + (Id ?? string.Empty)).ToLowerInvariant();
        }
    }

    internal static class ItemCatalog
    {
        internal static readonly List<ItemEntry> Items = new List<ItemEntry>();
        internal static bool Built { get; private set; }
        internal static int ModCount { get; private set; }
        internal static string LastReport { get; private set; } = "не собран";

        private static readonly HashSet<string> Skipped = new HashSet<string>();
        private static bool _samplesLogged;

        internal static void EnsureBuilt()
        {
            if (!Built)
                Rebuild();
        }

        internal static string Rebuild()
        {
            Items.Clear();
            Skipped.Clear();
            ModCount = 0;
            Built = false;

            int vanilla = 0;
            try
            {
                vanilla = AddVanilla();
            }
            catch (Exception e)
            {
                WinterWishlistMod.Log.Error("Сканирование ванильных предметов: " + e);
            }

            try
            {
                AddModded();
            }
            catch (Exception e)
            {
                WinterWishlistMod.Log.Error("Сканирование модовых предметов: " + e);
            }

            Items.Sort((a, b) => string.Compare(a.Display, b.Display, StringComparison.CurrentCultureIgnoreCase));

            Built = Items.Count > 0;
            LastReport = ModCount > 0
                ? $"всего {Items.Count}, из них модовых {ModCount}"
                : $"всего {Items.Count} (включая модовые)";
            WinterWishlistMod.Log.Msg("Каталог: " + LastReport + $" [ванильный источник {vanilla}]");
            if (!_samplesLogged)
            {
                _samplesLogged = true;
                LogSamples(6);
            }
            return LastReport;
        }

        private static int AddVanilla()
        {
            var dict = ConsoleManager.m_SearchStringToGearNames;
            if (dict == null || dict.Count == 0)
            {
                TryStoreAutoComplete();
                dict = ConsoleManager.m_SearchStringToGearNames;
            }

            int before = Items.Count;
            if (dict == null)
            {
                WinterWishlistMod.Log.Warning("Список автокомплита консоли недоступен");
                return 0;
            }

            var seen = new HashSet<string>();
            foreach (var pair in dict)
            {
                string id = pair.Value;
                if (string.IsNullOrEmpty(id) || !seen.Add(id))
                    continue;
                AddEntry(id, null, false);
            }

            return Items.Count - before;
        }

        private static void TryStoreAutoComplete()
        {
            try
            {
                var mi = typeof(ConsoleManager).GetMethod("StoreAutoCompleteGearNames",
                    BindingFlags.NonPublic | BindingFlags.Static);
                mi?.Invoke(null, null);
            }
            catch (Exception e)
            {
                WinterWishlistMod.Log.Warning("StoreAutoCompleteGearNames: " + e.Message);
            }
        }

        private static void AddModded()
        {
            var asm = FindAssembly("ModComponent");
            if (asm == null)
            {
                WinterWishlistMod.Log.Msg("ModComponent не найден — модовые предметы не сканируются");
                return;
            }

            var t = asm.GetType("ModComponent.AssetLoader.ModAssetBundleManager");
            if (t == null)
            {
                WinterWishlistMod.Log.Warning("ModComponent: не найден ModAssetBundleManager");
                return;
            }

            var bundleField = t.GetField("knownAssetNames", BindingFlags.NonPublic | BindingFlags.Static);
            var mappedField = t.GetField("knownAssetMappedNames", BindingFlags.NonPublic | BindingFlags.Static);
            var bundles = bundleField?.GetValue(null) as Dictionary<string, AssetBundle>;
            var mapped = mappedField?.GetValue(null) as Dictionary<string, string>;

            if (bundles == null || mapped == null)
            {
                WinterWishlistMod.Log.Warning(
                    $"ModComponent: поля недоступны (bundles={(bundles != null)}, mapped={(mapped != null)})");
                return;
            }

            WinterWishlistMod.Log.Msg($"ModComponent: ассетов {bundles.Count}, отображений {mapped.Count}");
            var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var e in Items)
                existing.Add(e.Id);

            int prefabs = 0;
            int added = 0;
            foreach (var kv in mapped)
            {
                if (!kv.Key.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (!kv.Key.StartsWith("gear_", StringComparison.OrdinalIgnoreCase))
                    continue;

                prefabs++;
                if (!bundles.TryGetValue(kv.Value, out var bundle) || bundle == null)
                    continue;

                GameObject go;
                try
                {
                    go = bundle.LoadAsset<GameObject>(kv.Value);
                }
                catch (Exception e)
                {
                    WinterWishlistMod.Log.Warning("ModComponent asset " + kv.Value + ": " + e.Message);
                    continue;
                }

                if (go == null)
                    continue;
                var gear = go.GetComponent<GearItem>();
                if (gear == null)
                    continue;
                if (!existing.Add(gear.name))
                    continue;

                AddEntry(gear.name, gear, true);
                added++;
            }

            WinterWishlistMod.Log.Msg($"ModComponent: gear-префабов {prefabs}, добавлено {added}");
        }
        private static Assembly FindAssembly(string name)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.GetName().Name == name)
                    return asm;
            }
            try
            {
                return Assembly.Load(name);
            }
            catch
            {
                return null;
            }
        }

        private static void AddEntry(string id, GearItem prefab, bool fromMod)
        {
            string display = ResolveName(id, prefab);
            var entry = new ItemEntry
            {
                Id = id,
                Display = display,
                Prefab = prefab,
                FromMod = fromMod,
            };
            entry.RefreshSearch();
            Items.Add(entry);
            if (fromMod)
                ModCount++;
        }

        private static string ResolveName(string id, GearItem prefab)
        {
            try
            {
                if (prefab != null)
                {
                    string viaPrefab = prefab.DisplayName;
                    if (!string.IsNullOrWhiteSpace(viaPrefab))
                        return viaPrefab;
                }

                string name = GearItem.GetGearDisplayName(id);
                if (!string.IsNullOrWhiteSpace(name))
                    return name;

                string key = id.StartsWith("GEAR_", StringComparison.OrdinalIgnoreCase) ? id.Substring(5) : id;
                name = Localization.Get("GAMEPLAY_" + key);
                if (!string.IsNullOrWhiteSpace(name))
                    return name;
            }
            catch (Exception e)
            {
                WinterWishlistMod.Log.Warning("Имя для " + id + ": " + e.Message);
            }

            return id;
        }

        internal static void LogSamples(int take)
        {
            try
            {
                WinterWishlistMod.Log.Msg("Язык игры (по ножу): " + GearItem.GetGearDisplayName("GEAR_Knife"));
                int n = Math.Min(take, Items.Count);
                for (int i = 0; i < n; i++)
                {
                    var e = Items[i];
                    WinterWishlistMod.Log.Msg($"  {e.Display}  [{e.Id}]{(e.FromMod ? " (мод)" : "")}");
                }

                int shown = 0;
                foreach (var e in Items)
                {
                    if (!e.FromMod)
                        continue;
                    WinterWishlistMod.Log.Msg("  модовый: " + e.Display + " [" + e.Id + "]");
                    if (++shown >= 5)
                        break;
                }
            }
            catch (Exception e)
            {
                WinterWishlistMod.Log.Warning("LogSamples: " + e.Message);
            }
        }

        internal static string CurrentGameLanguage()
        {
            try
            {
                string probe = GearItem.GetGearDisplayName("GEAR_Knife");
                if (!string.IsNullOrEmpty(probe))
                {
                    foreach (char c in probe)
                    {
                        if (c >= 'А' && c <= 'я')
                            return "ru";
                    }
                    return "en";
                }
            }
            catch
            {
                // игнорируем, ниже вернём en
            }
            return "en";
        }
    }
}
