using MelonLoader;
using Il2Cpp;
using UnityEngine;
using WinterWishlist.Cheats;

[assembly: MelonInfo(typeof(WinterWishlist.WinterWishlistMod), "Winter Wishlist", "0.2.3", "barsuk")]
[assembly: MelonGame("Hinterland", "TheLongDark")]

namespace WinterWishlist
{
    public sealed class WinterWishlistMod : MelonMod
    {
        internal static MelonLogger.Instance Log;

        internal static MelonPreferences_Category Prefs;
        internal static MelonPreferences_Entry<string> PrefHotkey;

        internal static MelonPreferences_Entry<int> PrefWidth;
        internal static MelonPreferences_Entry<int> PrefHeight;

        public override void OnInitializeMelon()
        {
            Log = LoggerInstance;

            Prefs = MelonPreferences.CreateCategory("WinterWishlist", "Winter Wishlist");
            PrefHotkey = Prefs.CreateEntry("Hotkey", "F7", "Клавиша открытия окна");

            PrefWidth = Prefs.CreateEntry("WindowWidth", 940, "Ширина окна");
            PrefHeight = Prefs.CreateEntry("WindowHeight", 680, "Высота окна");
            Prefs.SaveToFile();

            MainWindow.LoadPreferences();
            Log.Msg("Winter Wishlist 0.2.3 загружен. Клавиша: " + PrefHotkey.Value);
        }

        public override void OnDeinitializeMelon()
        {
            try
            {
                CheatState.ResetAll();
            }
            catch (System.Exception e)
            {
                Log.Warning("Сброс читов: " + e.Message);
            }

            GameInput.SetBlocked(false);
            Prefs?.SaveToFile();
        }

        public override void OnUpdate()
        {
            MainWindow.HandleInput();
            MainWindow.HandleCursor();
            CheatState.Tick();
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            try
            {
                if (MainWindow.IsOpen)
                    Log.Msg("Окно открыто во время смены сцены — каталог остаётся прежним");
            }
            catch (System.Exception e)
            {
                Log.Warning("OnSceneWasInitialized: " + e.Message);
            }
        }

        public override void OnGUI()
        {
            MainWindow.Draw();
        }
    }
}
