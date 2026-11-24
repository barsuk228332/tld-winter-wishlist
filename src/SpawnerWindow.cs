using System;
using System.Collections.Generic;
using UnityEngine;

namespace WinterWishlist
{
    internal static class SpawnerWindow
    {
        private const float RowHeight = 44f;
        private const float IconSize = 40f;
        private const int IconCacheLimit = 400;

        private static bool _open;
        private static Rect _rect = new Rect(60f, 60f, 760f, 560f);

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

        private static string _status = string.Empty;
        private static float _statusUntil;

        private static KeyCode _hotkey = KeyCode.F7;
        private static float _lastErrorLog;

        internal static bool IsOpen => _open;

        internal static void LoadPreferences()
        {
            _rect.width = Mathf.Clamp(WinterWishlistMod.PrefWidth.Value, 420, 1600);
            _rect.height = Mathf.Clamp(WinterWishlistMod.PrefHeight.Value, 260, 1200);
            if (!Enum.TryParse(WinterWishlistMod.PrefHotkey.Value, true, out _hotkey))
                _hotkey = KeyCode.F7;
        }

        internal static void HandleInput()
        {
            if (Input.GetKeyDown(_hotkey))
            {
                _open = !_open;
                if (_open)
                {
                    ItemCatalog.EnsureBuilt();
                    FilterItems();
                    _scroll = Vector2.zero;
                }
                else
                {
                    SavePreferences();
                }
                return;
            }

            if (!_open)
                return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _open = false;
                SavePreferences();
                return;
            }

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

        internal static void Draw()
        {
            if (!_open)
                return;

            try
            {
                GUI.depth = 0;
                GUI.Box(_rect, GUIContent.none);
                GUILayout.BeginArea(new Rect(_rect.x + 8f, _rect.y + 8f, _rect.width - 16f, _rect.height - 16f));

                DrawHeader();
                DrawToolbar();
                DrawList();
                DrawSpawnPanel();
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

        private static void DrawHeader()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("<b>Item Spawner</b>");
            GUILayout.Label(ItemCatalog.LastReport);
            GUILayout.Label(UiLang.Current);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("⟳", GUILayout.Width(28f)))
            {
                ItemCatalog.Rebuild();
                FilterItems();
            }
            if (GUILayout.Button("X", GUILayout.Width(24f)))
            {
                _open = false;
                SavePreferences();
            }
            GUILayout.EndHorizontal();
        }

        private static void DrawToolbar()
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
            float listHeight = Mathf.Max(80f, _rect.height - 16f - 24f - 26f - 34f - 46f);
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
            string name = (entry.FromMod ? "★ " : string.Empty) + entry.Display;
            GUI.Label(new Rect(textX, row.y + 4f, row.width - (textX - row.x) - 8f, 20f), name, _nameStyle);

            if (WinterWishlistMod.PrefShowIds.Value)
            {
                GUI.Label(new Rect(textX, row.y + 24f, row.width - (textX - row.x) - 8f, 16f),
                    entry.Id, _idStyle);
            }

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
                alignment = TextAnchor.MiddleLeft,
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
