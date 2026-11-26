using System;
using System.Reflection;
using Il2Cpp;

namespace WinterWishlist.Cheats
{
    internal static class MovementService
    {
        internal static bool Flying { get; private set; }

        /// <summary>
        /// Переключает режим полёта. Сначала пробуем напрямую через FlyMode,
        /// если не вышло — через встроенную консольную команду игры.
        /// </summary>
        internal static string ToggleFly()
        {
            try
            {
                if (Flying)
                    FlyMode.Exit();
                else
                    FlyMode.Enter();

                Flying = !Flying;
                return Flying
                    ? UiLang.T("Полёт включён", "Fly mode on")
                    : UiLang.T("Полёт выключен", "Fly mode off");
            }
            catch (Exception e)
            {
                WinterWishlistMod.Log.Warning("FlyMode: " + e.Message);
            }

            try
            {
                var method = typeof(ConsoleManager).GetMethod(
                    "CONSOLE_fly",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                if (method != null)
                {
                    method.Invoke(null, null);
                    Flying = !Flying;
                    return Flying
                        ? UiLang.T("Полёт включён (консоль)", "Fly mode on (console)")
                        : UiLang.T("Полёт выключен (консоль)", "Fly mode off (console)");
                }
            }
            catch (Exception e)
            {
                WinterWishlistMod.Log.Warning("CONSOLE_fly: " + e.Message);
            }

            return UiLang.T("Режим полёта недоступен", "Fly mode unavailable");
        }
    }
}
