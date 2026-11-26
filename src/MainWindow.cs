using System;
using System.Collections.Generic;
using UnityEngine;
using WinterWishlist.Cheats;

namespace WinterWishlist
{
    internal static class MainWindow
    {
        private const float RowHeight = 44f;
        private const float IconSize = 40f;
        private const int IconCacheLimit = 400;

        private static bool _open;
        private static Rect _rect = new Rect(60f, 60f, 940f, 680f);
        private static int _tab;
        private static bool _dragging;
        private static bool _resizing;

        // вкладка «Предметы»
        private static string _query = string.Empty;
        private static Vector2 _scroll;
        private static readonly List<ItemEntry> Filtered = new List<ItemEntry>();
        private static readonly Dictionary<string, Texture2D> IconCache = new Dictionary<string, Texture2D>();
        private static readonly Queue<string> IconOrder = new Queue<string>();
        private static GUIStyle _nameStyle;
        private static GUIStyle _idStyle;
        private static int _count = 1;
        private static float _condition = 1f;
        private static bool _toFeet;

        // прочие вкладки
        private static Vector2 _cheatScroll;
        private static float _timeOfDay = 0.5f;
        private static List<string> _afflictions = new List<string>();
        private static float _afflictionsNext;

        private static string _status = string.Empty;
        private static float _statusUntil;

        private static KeyCode _hotkey = KeyCode.F7;
        private static float _lastErrorLog;

        internal static bool IsOpen => _open;

        internal static void LoadPreferences()
        {
            _rect.width = Mathf.Clamp(WinterWishlistMod.PrefWidth.Value, 640, 1600);
            _rect.height = Mathf.Clamp(WinterWishlistMod.PrefHeight.Value, 360, 1200);
            if (!Enum.TryParse(WinterWishlistMod.PrefHotkey.Value, true, out _hotkey))
                _hotkey = KeyCode.F7;
        }

        private static string[] TabNames()
        {
            return new[]
            {
                UiLang.T("Предметы", "Items"),
                UiLang.T("Скиллы", "Skills"),
                UiLang.T("Игрок", "Player"),
                UiLang.T("Движение", "Movement"),
                UiLang.T("Мир", "World"),
                UiLang.T("Болезни", "Ailments"),
            };
        }

        /********* Ввод *********/

        internal static void HandleInput()
        {
            if (Input.GetKeyDown(_hotkey))
            {
                SetOpen(!_open);
                return;
            }

            if (!_open)
                return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SetOpen(false);
                return;
            }

            if (_tab != 0)
                return;

            string typed = Input.inputString;
            if (string.IsNullOrEmpty(typed))
                return;

            bool changed = false;
            foreach (char c in typed)
            {
                if (c == '\b')
                {
                    if (_query.Length > 0)
                    {
                        _query = _query.Substring(0, _query.Length - 1);
                        changed = true;
                    }
                }
                else if (c == '\n' || c == '\r' || c == '\t')
                {
                    // игнорируем
                }
                else if (!char.IsControl(c) && _query.Length < 48)
                {
                    _query += c;
                    changed = true;
                }
            }

            if (changed)
            {
                FilterItems();
                _scroll = Vector2.zero;
            }
        }

        internal static void SetOpen(bool open)
        {
            if (open == _open)
                return;

            _open = open;
            GameInput.SetBlocked(open);

            if (open)
            {
                ItemCatalog.EnsureBuilt();
                FilterItems();
                _scroll = Vector2.zero;
            }
            else
            {
                SavePreferences();
            }
        }

        internal static void SavePreferences()
        {
            WinterWishlistMod.PrefWidth.Value = Mathf.RoundToInt(_rect.width);
            WinterWishlistMod.PrefHeight.Value = Mathf.RoundToInt(_rect.height);
            WinterWishlistMod.Prefs.SaveToFile();
        }

        internal static void HandleCursor()
        {
            if (!_open)
                return;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        /********* Отрисовка *********/

        internal static void Draw()
        {
            if (!_open)
                return;

            try
            {
                GUI.depth = 0;
                HandleDrag();
                GUI.Box(_rect, GUIContent.none);
                GUILayout.BeginArea(new Rect(_rect.x + 8f, _rect.y + 8f, _rect.width - 16f, _rect.height - 16f));

                DrawHeader();
                _tab = GUILayout.Toolbar(_tab, TabNames());

                switch (_tab)
                {
                    case 0:
                        DrawItemsTab();
                        break;
                    case 1:
                        DrawSkillsTab();
                        break;
                    case 2:
                        DrawPlayerTab();
                        break;
                    case 3:
                        DrawMovementTab();
                        break;
                    case 4:
                        DrawWorldTab();
                        break;
                    default:
                        DrawAilmentsTab();
                        break;
                }

                DrawStatus();
                GUILayout.EndArea();
            }
            catch (Exception e)
            {
                if (Time.realtimeSinceStartup - _lastErrorLog > 5f)
                {
                    _lastErrorLog = Time.realtimeSinceStartup;
                    WinterWishlistMod.Log.Error("Отрисовка окна: " + e);
                }
            }
        }

        private static void HandleDrag()
        {
            var e = Event.current;
            if (e == null)
                return;

            if (e.type == EventType.MouseUp)
            {
                _dragging = false;
                _resizing = false;
                return;
            }

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                if (new Rect(_rect.x, _rect.y, _rect.width, 26f).Contains(e.mousePosition))
                    _dragging = true;
                else if (new Rect(_rect.x + _rect.width - 18f, _rect.y + _rect.height - 18f, 18f, 18f).Contains(e.mousePosition))
                    _resizing = true;
                return;
            }

            if (e.type != EventType.MouseDrag)
                return;

            if (_dragging)
            {
                _rect.x += e.delta.x;
                _rect.y += e.delta.y;
            }
            else if (_resizing)
            {
                _rect.width = Mathf.Clamp(_rect.width + e.delta.x, 640f, 1600f);
                _rect.height = Mathf.Clamp(_rect.height + e.delta.y, 360f, 1200f);
            }
        }

        private static void DrawHeader()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("<b>Winter Wishlist</b>");
            GUILayout.Label(ItemCatalog.LastReport);
            GUILayout.Label(UiLang.Current);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("⟳", GUILayout.Width(28f)))
            {
                ItemCatalog.Rebuild();
                FilterItems();
            }
            if (GUILayout.Button("X", GUILayout.Width(24f)))
                SetOpen(false);
            GUILayout.EndHorizontal();
        }

        /********* Вкладка «Предметы» *********/

        private static void DrawItemsTab()
        {
            DrawSearchRow();
            DrawList();
            DrawSpawnPanel();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(UiLang.T("Починить всё в инвентаре", "Repair inventory")))
                SetStatus(ItemService.RepairAll());
            if (GUILayout.Button(UiLang.T("Набор выживальщика", "Survival kit")))
                SetStatus(ItemService.GiveSurvivalKit());
            GUILayout.EndHorizontal();
        }

        private static void DrawSearchRow()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(UiLang.Search, GUILayout.Width(58f));

            var boxRect = GUILayoutUtility.GetRect(240f, 22f, GUILayout.ExpandWidth(true));
            GUI.Box(boxRect, GUIContent.none);
            string shown = string.IsNullOrEmpty(_query) ? UiLang.SearchHint : _query + "▌";
            GUI.Label(new Rect(boxRect.x + 6f, boxRect.y + 3f, boxRect.width - 12f, boxRect.height - 4f), shown);

            GUILayout.Label(Filtered.Count + " / " + ItemCatalog.Items.Count, GUILayout.Width(90f));

            if (GUILayout.Button("✕", GUILayout.Width(24f)))
            {
                _query = string.Empty;
                FilterItems();
            }
            GUILayout.EndHorizontal();
        }

        private static void DrawList()
        {
            float listHeight = Mathf.Max(80f, _rect.height - 16f - 24f - 28f - 26f - 26f - 30f - 20f);
            _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(listHeight));

            int total = Filtered.Count;
            int first = Mathf.Max(0, Mathf.FloorToInt(_scroll.y / RowHeight));
            int visible = Mathf.CeilToInt(listHeight / RowHeight) + 1;
            int last = Mathf.Min(total, first + visible);

            if (first > 0)
                GUILayout.Space(first * RowHeight);

            for (int i = first; i < last; i++)
                DrawRow(Filtered[i]);

            if (last < total)
                GUILayout.Space((total - last) * RowHeight);

            GUILayout.EndScrollView();
        }

        private static void DrawRow(ItemEntry entry)
        {
            EnsureStyles();

            Rect row = GUILayoutUtility.GetRect(0f, RowHeight, GUILayout.ExpandWidth(true));
            bool clicked = GUI.Button(row, GUIContent.none);

            var iconRect = new Rect(row.x + 2f, row.y + 2f, IconSize, IconSize);
            var icon = GetIcon(entry.Id);
            if (icon != null)
                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);

            float textX = row.x + IconSize + 8f;
            float innerWidth = row.width - (textX - row.x) - 10f;
            string name = (entry.FromMod ? "★ " : string.Empty) + entry.Display;

            GUI.Label(new Rect(textX, row.y + 5f, innerWidth, 18f), name, _nameStyle);
            GUI.Label(new Rect(textX, row.y + 23f, innerWidth, 15f), entry.Id, _idStyle);
            if (clicked)
                Spawn(entry);
        }

        private static void EnsureStyles()
        {
            if (_nameStyle != null)
                return;

            _nameStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 14,
                richText = true,
            };
            _idStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleRight,
                fontSize = 11,
                richText = false,
            };
            _idStyle.normal.textColor = new Color(0.62f, 0.62f, 0.62f);
        }

        private static Texture2D GetIcon(string id)
        {
            if (IconCache.TryGetValue(id, out var cached))
                return cached;

            Texture2D tex = null;
            try
            {
                tex = Il2Cpp.Utils.GetInventoryIconTextureFromPrefabName(id);
            }
            catch (Exception e)
            {
                if (Time.realtimeSinceStartup - _lastErrorLog > 10f)
                {
                    _lastErrorLog = Time.realtimeSinceStartup;
                    WinterWishlistMod.Log.Warning("Иконка для " + id + ": " + e.Message);
                }
            }

            if (IconCache.Count >= IconCacheLimit && IconOrder.Count > 0)
                IconCache.Remove(IconOrder.Dequeue());

            IconCache[id] = tex;
            IconOrder.Enqueue(id);
            return tex;
        }

        private static void DrawSpawnPanel()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(UiLang.Count, GUILayout.Width(58f));
            if (GUILayout.Button("−", GUILayout.Width(24f)))
                _count = Mathf.Max(1, _count - 1);
            _count = Mathf.RoundToInt(GUILayout.HorizontalSlider(_count, 1f, 50f, GUILayout.Width(110f)));
            GUILayout.Label(_count.ToString(), GUILayout.Width(28f));
            if (GUILayout.Button("+", GUILayout.Width(24f)))
                _count = Mathf.Min(50, _count + 1);

            GUILayout.Space(10f);
            GUILayout.Label(UiLang.Condition, GUILayout.Width(78f));
            _condition = GUILayout.HorizontalSlider(_condition, 0f, 1f, GUILayout.Width(110f));
            GUILayout.Label(Mathf.RoundToInt(_condition * 100f) + "%", GUILayout.Width(42f));

            GUILayout.FlexibleSpace();
            _toFeet = GUILayout.Toggle(_toFeet, " " + UiLang.ToFeet);
            GUILayout.EndHorizontal();
        }

        /********* Вкладка «Скиллы» *********/

        private static void DrawSkillsTab()
        {
            _cheatScroll = GUILayout.BeginScrollView(_cheatScroll, GUILayout.Height(_rect.height - 100f));

            GUILayout.Label("<b>" + UiLang.T("Уровни навыков", "Skill levels") + "</b>");
            GUILayout.Space(4f);

            foreach (var type in SkillService.All)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(SkillService.Name(type), GUILayout.Width(240f));
                GUILayout.Label(SkillService.TierName(type), GUILayout.Width(160f));
                if (GUILayout.Button("MAX", GUILayout.Width(70f)))
                {
                    if (SkillService.MaxOne(type))
                        SetStatus(UiLang.T("Навык прокачан: ", "Skill maxed: ") + SkillService.Name(type));
                    else
                        SetStatus(UiLang.T("Навык недоступен (загрузи игру)", "Skill unavailable (load a game)"));
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10f);
            if (GUILayout.Button(UiLang.T("Прокачать все навыки на максимум", "Max out all skills")))
            {
                int done = SkillService.MaxAll();
                SetStatus(done > 0
                    ? UiLang.T("Прокачано навыков: ", "Skills maxed: ") + done
                    : UiLang.T("Навыки недоступны (загрузи игру)", "Skills unavailable (load a game)"));
            }

            GUILayout.EndScrollView();
        }

        /********* Вкладка «Игрок» *********/

        private static void DrawPlayerTab()
        {
            _cheatScroll = GUILayout.BeginScrollView(_cheatScroll, GUILayout.Height(_rect.height - 100f));

            Section(UiLang.T("Постоянные эффекты", "Persistent effects"));
            Toggle(UiLang.T("Бессмертие (неуязвимость + игнор ИИ)", "God mode (invulnerable + ignored by AI)"),
                CheatState.God, CheatState.SetGod);
            Toggle(UiLang.T("Держать здоровье на максимуме", "Keep health full"),
                CheatState.KeepHealth, v => CheatState.KeepHealth = v);
            Toggle(UiLang.T("Не голодать", "No hunger"),
                CheatState.NoHunger, v => CheatState.NoHunger = v);
            Toggle(UiLang.T("Не испытывать жажду", "No thirst"),
                CheatState.NoThirst, v => CheatState.NoThirst = v);
            Toggle(UiLang.T("Не уставать", "No fatigue"),
                CheatState.NoFatigue, v => CheatState.NoFatigue = v);
            Toggle(UiLang.T("Не замерзать", "Never freeze"),
                CheatState.NoFreezing, v => CheatState.NoFreezing = v);

            GUILayout.Space(10f);
            Section(UiLang.T("Разово", "One-shot"));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(UiLang.T("Полное здоровье", "Full health")))
                SetStatus(PlayerService.FullHealth(out string error)
                    ? UiLang.T("Здоровье восстановлено", "Health restored")
                    : UiLang.Failed + ": " + error);
            if (GUILayout.Button(UiLang.T("Восстановить силы", "Restore needs")))
                SetStatus(PlayerService.RestoreAll(out string error2)
                    ? UiLang.T("Голод, жажда, усталость и тепло восстановлены", "Needs restored")
                    : UiLang.Failed + ": " + error2);
            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();
        }

        /********* Вкладка «Движение» *********/

        private static void DrawMovementTab()
        {
            _cheatScroll = GUILayout.BeginScrollView(_cheatScroll, GUILayout.Height(_rect.height - 100f));

            Section(UiLang.T("Движение", "Movement"));
            Toggle(UiLang.T("Бесконечный спринт", "Unlimited sprint"),
                CheatState.UnlimitedSprint, CheatState.SetUnlimitedSprint);
            Toggle(UiLang.T("Бесконечная выносливость", "Infinite stamina"),
                CheatState.UnlimitedStamina, v => CheatState.UnlimitedStamina = v);
            Toggle(UiLang.T("Бесконечная грузоподъёмность", "Infinite carry weight"),
                CheatState.UnlimitedCarry, CheatState.SetUnlimitedCarry);
            Toggle(UiLang.T("Без растяжений и вывихов", "No sprains"),
                CheatState.NoSprains, CheatState.SetNoSprains);

            GUILayout.Space(10f);
            Section(UiLang.T("Полёт (экспериментально)", "Flight (experimental)"));
            if (GUILayout.Button(MovementService.Flying
                    ? UiLang.T("Выключить полёт", "Disable flight")
                    : UiLang.T("Включить полёт", "Enable flight")))
            {
                SetStatus(MovementService.ToggleFly());
            }

            GUILayout.EndScrollView();
        }

        /********* Вкладка «Мир» *********/

        private static void DrawWorldTab()
        {
            _cheatScroll = GUILayout.BeginScrollView(_cheatScroll, GUILayout.Height(_rect.height - 100f));

            Section(UiLang.T("Карта", "Map"));
            if (GUILayout.Button(UiLang.T("Раскрыть туман текущего региона", "Reveal current region fog")))
                SetStatus(WorldService.RevealCurrentScene());
            if (GUILayout.Button(UiLang.T("Разблокировать карту текущего региона", "Unlock current region map")))
                SetStatus(WorldService.UnlockCurrentScene());
            if (GUILayout.Button(UiLang.T("Разблокировать и раскрыть все регионы", "Unlock and reveal all regions")))
                SetStatus(WorldService.UnlockAllRegions());

            GUILayout.Space(10f);
            Section(UiLang.T("Время суток", "Time of day"));
            GUILayout.BeginHorizontal();
            _timeOfDay = GUILayout.HorizontalSlider(_timeOfDay, 0f, 1f, GUILayout.ExpandWidth(true));
            GUILayout.Label(Mathf.RoundToInt(_timeOfDay * 24f).ToString("00") + ":00", GUILayout.Width(50f));
            if (GUILayout.Button(UiLang.T("Установить", "Apply"), GUILayout.Width(110f)))
                SetStatus(WorldService.SetTime(_timeOfDay));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(UiLang.T("Рассвет", "Dawn")))
                SetStatus(WorldService.SetTime(0.25f));
            if (GUILayout.Button(UiLang.T("Полдень", "Noon")))
                SetStatus(WorldService.SetTime(0.5f));
            if (GUILayout.Button(UiLang.T("Закат", "Dusk")))
                SetStatus(WorldService.SetTime(0.75f));
            if (GUILayout.Button(UiLang.T("Полночь", "Midnight")))
                SetStatus(WorldService.SetTime(0.0f));
            GUILayout.EndHorizontal();

            GUILayout.Space(10f);
            Section(UiLang.T("Погода", "Weather"));
            if (GUILayout.Button(UiLang.T("Сделать тепло", "Make it warm")))
                SetStatus(WorldService.WarmWeather());

            GUILayout.EndScrollView();
        }

        /********* Вкладка «Болезни» *********/

        private static void DrawAilmentsTab()
        {
            _cheatScroll = GUILayout.BeginScrollView(_cheatScroll, GUILayout.Height(_rect.height - 100f));

            if (Time.realtimeSinceStartup > _afflictionsNext)
            {
                _afflictionsNext = Time.realtimeSinceStartup + 1f;
                _afflictions = AfflictionService.Active();
            }

            Section(UiLang.T("Активные болезни и травмы", "Active ailments"));
            if (_afflictions.Count == 0)
                GUILayout.Label(UiLang.T("Активных болезней нет", "No active ailments"));
            else
            {
                foreach (var name in _afflictions)
                    GUILayout.Label("• " + name);
            }

            GUILayout.Space(10f);
            if (GUILayout.Button(UiLang.T("Вылечить всё", "Cure everything")))
            {
                SetStatus(AfflictionService.CureAll());
                _afflictionsNext = 0f;
            }

            GUILayout.Space(10f);
            Section(UiLang.T("Постоянная защита", "Permanent protection"));
            Toggle(UiLang.T("Подавить обморожение", "Suppress frostbite"),
                CheatState.SuppressFrostbite, CheatState.SetSuppressFrostbite);
            Toggle(UiLang.T("Подавить гипотермию", "Suppress hypothermia"),
                CheatState.SuppressHypothermia, CheatState.SetSuppressHypothermia);

            GUILayout.EndScrollView();
        }

        /********* Общее *********/

        private static void Section(string title)
        {
            GUILayout.Space(6f);
            GUILayout.Label("<b>" + title + "</b>");
        }

        private static void Toggle(string label, bool current, Action<bool> set)
        {
            bool now = GUILayout.Toggle(current, "  " + label);
            if (now != current)
                set(now);
        }

        private static void DrawStatus()
        {
            if (string.IsNullOrEmpty(_status))
                return;
            if (Time.realtimeSinceStartup > _statusUntil)
            {
                _status = string.Empty;
                return;
            }
            GUILayout.Label(_status);
        }

        private static void FilterItems()
        {
            Filtered.Clear();
            string q = (_query ?? string.Empty).Trim().ToLowerInvariant();
            if (q.Length == 0)
            {
                Filtered.AddRange(ItemCatalog.Items);
                return;
            }

            foreach (var item in ItemCatalog.Items)
            {
                if (item.Search != null && item.Search.Contains(q))
                    Filtered.Add(item);
            }
        }

        private static void Spawn(ItemEntry entry)
        {
            var item = SpawnService.Spawn(entry, _count, _condition, _toFeet, out string error);
            if (item != null)
                SetStatus((_toFeet ? UiLang.Dropped : UiLang.Added) + ": " + entry.Display + " ×" + _count);
            else
                SetStatus(UiLang.Failed + ": " + entry.Display + " — " + error);
        }

        private static void SetStatus(string text)
        {
            _status = text;
            _statusUntil = Time.realtimeSinceStartup + 4f;
        }
    }
}
