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

        internal static string Current => Lang == "ru" ? "RU" : "EN";

        internal static string Search => Lang == "ru" ? "Поиск:" : "Search:";
        internal static string SearchHint => Lang == "ru" ? "печатай название или ID…" : "type name or ID…";
        internal static string Count => Lang == "ru" ? "Кол-во:" : "Count:";
        internal static string Condition => Lang == "ru" ? "Состояние:" : "Condition:";
        internal static string ToFeet => Lang == "ru" ? "под ноги (не в инвентарь)" : "at feet (not inventory)";
        internal static string Added => Lang == "ru" ? "Добавлено" : "Added";
        internal static string Dropped => Lang == "ru" ? "Выброшено" : "Dropped";
        internal static string Failed => Lang == "ru" ? "Ошибка" : "Failed";
    }
}
