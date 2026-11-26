namespace WinterWishlist
{
    internal static class UiLang
    {
        private static string _lang;

        private static string Lang
        {
            get
            {
                if (_lang == null)
                    _lang = ItemCatalog.CurrentGameLanguage();
                return _lang;
            }
        }

        internal static bool Ru => Lang == "ru";

        internal static string T(string ru, string en) => Ru ? ru : en;

        internal static string Current => Ru ? "RU" : "EN";

        internal static string Search => T("Поиск:", "Search:");
        internal static string SearchHint => T("печатай название или ID…", "type name or ID…");
        internal static string Count => T("Кол-во:", "Count:");
        internal static string Condition => T("Состояние:", "Condition:");
        internal static string ToFeet => T("под ноги (не в инвентарь)", "at feet (not inventory)");
        internal static string Added => T("Добавлено", "Added");
        internal static string Dropped => T("Выброшено", "Dropped");
        internal static string Failed => T("Ошибка", "Failed");
        internal static string Done => T("Готово", "Done");
    }
}
