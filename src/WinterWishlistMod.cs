using MelonLoader;
using Il2Cpp;
using UnityEngine;

[assembly: MelonInfo(typeof(WinterWishlist.WinterWishlistMod), "Winter Wishlist", "0.1.0", "barsuk")]
[assembly: MelonGame("Hinterland", "TheLongDark")]

namespace WinterWishlist
{
    public sealed class WinterWishlistMod : MelonMod
    {
        internal static MelonLogger.Instance Log;

        internal static MelonPreferences_Category Prefs;
        internal static MelonPreferences_Entry<string> PrefHotkey;
        internal static MelonPreferences_Entry<bool> PrefShowIds;
        internal static MelonPreferences_Entry<int> PrefWidth;
        internal static MelonPreferences_Entry<int> PrefHeight;

        public override void OnInitializeMelon()
        {
            Log = LoggerInstance;

            Prefs = MelonPreferences.CreateCategory("WinterWishlist", "Winter Wishlist");
            PrefHotkey = Prefs.CreateEntry("Hotkey", "F7", "Клавиша открытия окна");
            PrefShowIds = Prefs.CreateEntry("ShowIds", false, "Показывать внутренние ID предметов");
            PrefWidth = Prefs.CreateEntry("WindowWidth", 760, "Ширина окна");
            PrefHeight = Prefs.CreateEntry("WindowHeight", 560, "Высота окна");
            Prefs.SaveToFile();

            SpawnerWindow.LoadPreferences();
            Log.Msg("Item Spawner 0.1.0 загружен. Клавиша: " + PrefHotkey.Value);
        }

        public override void OnDeinitializeMelon()
        {
            Prefs?.SaveToFile();
        }

        public override void OnUpdate()
        {
            SpawnerWindow.HandleInput();
            SpawnerWindow.HandleCursor();
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            try
            {
                WinterWishlistMod.Log.Msg("Сцена инициализирована: " + sceneName);
                if (SpawnerWindow.IsOpen)
                    WinterWishlistMod.Log.Msg("Окно открыто во время смены сцены — каталог остаётся прежним");
            }
            catch (System.Exception e)
            {
                Log.Warning("OnSceneWasInitialized: " + e.Message);
            }
        }

        public override void OnGUI()
        {
            SpawnerWindow.Draw();
        }
    }
}
