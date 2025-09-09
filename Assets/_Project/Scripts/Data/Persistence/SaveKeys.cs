namespace WordPuzzle.Data.Persistence
{
    /// <summary>
    /// Константы ключей для сохранения в PlayerPrefs
    /// Централизованное управление ключами для избежания коллизий
    /// </summary>
    public static class SaveKeys
    {
        /// <summary>
        /// Префикс для всех ключей игры
        /// </summary>
        private const string GamePrefix = "WordPuzzle_";
        
        /// <summary>
        /// Основные ключи прогресса
        /// </summary>
        public static class Progress
        {
            /// <summary>
            /// JSON данные прогресса игрока
            /// </summary>
            public const string PlayerProgressData = GamePrefix + "PlayerProgress";
            
            /// <summary>
            /// Количество пройденных уровней (резервный ключ)
            /// </summary>
            public const string CompletedLevelsCount = GamePrefix + "CompletedLevels";
            
            /// <summary>
            /// Номер текущего уровня (резервный ключ)
            /// </summary>
            public const string CurrentLevel = GamePrefix + "CurrentLevel";
            
            /// <summary>
            /// Время последнего сохранения
            /// </summary>
            public const string LastSaveTime = GamePrefix + "LastSave";
        }
        
        /// <summary>
        /// Настройки игры
        /// </summary>
        public static class Settings
        {
            /// <summary>
            /// Версия сохранения для миграции данных
            /// </summary>
            public const string SaveVersion = GamePrefix + "SaveVersion";
            
            /// <summary>
            /// Первый запуск игры
            /// </summary>
            public const string FirstLaunch = GamePrefix + "FirstLaunch";
        }
        
        /// <summary>
        /// Отладочные ключи
        /// </summary>
        public static class Debug
        {
            /// <summary>
            /// Флаг отладочного режима
            /// </summary>
            public const string DebugMode = GamePrefix + "DebugMode";
            
            /// <summary>
            /// Количество запусков для аналитики
            /// </summary>
            public const string LaunchCount = GamePrefix + "LaunchCount";
        }
        
        /// <summary>
        /// Получить ключ для конкретного уровня
        /// </summary>
        /// <param name="levelId">ID уровня</param>
        /// <returns>Ключ для данных уровня</returns>
        public static string GetLevelKey(int levelId)
        {
            return $"{GamePrefix}Level_{levelId:D3}";
        }
        
        /// <summary>
        /// Получить ключ для времени прохождения уровня
        /// </summary>
        /// <param name="levelId">ID уровня</param>
        /// <returns>Ключ для времени прохождения</returns>
        public static string GetLevelTimeKey(int levelId)
        {
            return $"{GamePrefix}LevelTime_{levelId:D3}";
        }
    }
}