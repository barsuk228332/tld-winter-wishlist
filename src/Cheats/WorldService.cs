using System;
using Il2Cpp;
using Il2CppTLD.Scenes;
using UnityEngine;

namespace WinterWishlist.Cheats
{
    /// <summary>Карта, время и погода.</summary>
    internal static class WorldService
    {
        internal static Panel_Map Map()
        {
            try
            {
                return InterfaceManager.GetPanel<Panel_Map>();
            }
            catch (Exception e)
            {
                WinterWishlistMod.Log.Warning("Panel_Map: " + e.Message);
                return null;
            }
        }

        internal static string RevealCurrentScene()
        {
            try
            {
                var map = Map();
                if (map == null)
                    return UiLang.T("Карта не загружена — открой её (M) и повтори", "Map not loaded — open it (M) and retry");

                map.RevealCurrentScene();
                return UiLang.T("Туман карты раскрыт", "Fog of war revealed");
            }
            catch (Exception e)
            {
                WinterWishlistMod.Log.Warning("RevealCurrentScene: " + e);
                return UiLang.T("Ошибка: ", "Error: ") + e.Message;
            }
        }

        internal static string UnlockCurrentScene()
        {
            try
            {
                var map = Map();
                if (map == null)
                    return UiLang.T("Карта не загружена — открой её (M) и повтори", "Map not loaded — open it (M) and retry");

                map.UnlockMapCurrentScene();
                return UiLang.T("Карта региона разблокирована", "Region map unlocked");
            }
            catch (Exception e)
            {
                WinterWishlistMod.Log.Warning("UnlockMapCurrentScene: " + e);
                return UiLang.T("Ошибка: ", "Error: ") + e.Message;
            }
        }

        internal static string UnlockAllRegions()
        {
            try
            {
                var map = Map();
                if (map == null)
                    return UiLang.T("Карта не загружена — открой её (M) и повтори", "Map not loaded — open it (M) and retry");

                var specs = Resources.FindObjectsOfTypeAll<ZoneSpecification>();
                int done = 0;
                if (specs != null)
                {
                    foreach (var spec in specs)
                    {
                        if (spec == null)
                            continue;
                        try
                        {
                            map.UnlockRegionMap(spec);
                            map.RevealFogForScene(spec);
                            done++;
                        }
                        catch (Exception e)
                        {
                            WinterWishlistMod.Log.Warning("Регион " + spec.ZoneName + ": " + e.Message);
                        }
                    }
                }

                return done > 0
                    ? UiLang.T("Разблокировано регионов: ", "Regions unlocked: ") + done
                    : UiLang.T("Регионы не найдены — открой карту мира", "No regions found — open the world map");
            }
            catch (Exception e)
            {
                WinterWishlistMod.Log.Warning("UnlockAllRegions: " + e);
                return UiLang.T("Ошибка: ", "Error: ") + e.Message;
            }
        }

        internal static string SetTime(float normalized)
        {
            try
            {
                var tod = GameManager.GetTimeOfDayComponent();
                if (tod == null)
                    return UiLang.T("Время недоступно", "Time unavailable");

                tod.SetNormalizedTime(Mathf.Clamp01(normalized));
                return UiLang.T("Время установлено", "Time set");
            }
            catch (Exception e)
            {
                WinterWishlistMod.Log.Warning("SetNormalizedTime: " + e);
                return UiLang.T("Ошибка: ", "Error: ") + e.Message;
            }
        }

        internal static string WarmWeather()
        {
            try
            {
                var weather = GameManager.GetWeatherComponent();
                if (weather == null)
                    return UiLang.T("Погода недоступна", "Weather unavailable");

                weather.WarmWeather();
                return UiLang.T("Погода сброшена на тёплую", "Weather warmed up");
            }
            catch (Exception e)
            {
                WinterWishlistMod.Log.Warning("WarmWeather: " + e);
                return UiLang.T("Ошибка: ", "Error: ") + e.Message;
            }
        }
    }
}
