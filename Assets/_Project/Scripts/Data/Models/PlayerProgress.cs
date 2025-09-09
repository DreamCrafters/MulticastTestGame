using System;
using System.Collections.Generic;
using System.Linq;

namespace WordPuzzle.Data.Models
{
    /// <summary>
    /// Модель прогресса игрока для сериализации и сохранения
    /// Содержит информацию о пройденных уровнях и достижениях
    /// </summary>
    [Serializable]
    public class PlayerProgress
    {
        /// <summary>
        /// Версия формата сохранения для обратной совместимости
        /// </summary>
        public int SaveVersion { get; set; } = 1;
        
        /// <summary>
        /// Общее количество пройденных уровней
        /// </summary>
        public int CompletedLevelsCount { get; set; } = 0;
        
        /// <summary>
        /// Время последнего сохранения
        /// </summary>
        public DateTime LastSaveTime { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Информация о пройденных уровнях
        /// Ключ - ID уровня, Значение - данные о прохождении
        /// </summary>
        public Dictionary<int, LevelCompletionData> CompletedLevels { get; set; }
        
        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public PlayerProgress()
        {
            CompletedLevels = new Dictionary<int, LevelCompletionData>();
        }
        
        /// <summary>
        /// Отметить уровень как пройденный
        /// </summary>
        /// <param name="levelId">ID уровня</param>
        /// <param name="completedWords">Порядок разгаданных слов</param>
        /// <param name="completionTime">Время прохождения в секундах</param>
        public void MarkLevelCompleted(int levelId, string[] completedWords, float completionTime = 0f)
        {
            var completionData = new LevelCompletionData
            {
                LevelId = levelId,
                CompletedWords = completedWords?.ToArray() ?? new string[0],
                CompletionTime = completionTime,
                CompletionDate = DateTime.Now
            };
            
            // Если уровень уже пройден, обновляем данные
            if (CompletedLevels.ContainsKey(levelId))
            {
                CompletedLevels[levelId] = completionData;
            }
            else
            {
                CompletedLevels.Add(levelId, completionData);
                CompletedLevelsCount = CompletedLevels.Count;
            }
            
            LastSaveTime = DateTime.Now;
        }
        
        /// <summary>
        /// Проверить, пройден ли уровень
        /// </summary>
        /// <param name="levelId">ID уровня</param>
        /// <returns>true если уровень пройден</returns>
        public bool IsLevelCompleted(int levelId)
        {
            return CompletedLevels.ContainsKey(levelId);
        }
        
        /// <summary>
        /// Получить данные прохождения уровня
        /// </summary>
        /// <param name="levelId">ID уровня</param>
        /// <returns>Данные прохождения или null</returns>
        public LevelCompletionData GetLevelCompletion(int levelId)
        {
            return CompletedLevels.TryGetValue(levelId, out var data) ? data : null;
        }
        
        /// <summary>
        /// Получить номер следующего уровня для прохождения
        /// </summary>
        /// <returns>Номер следующего уровня</returns>
        public int GetNextLevelNumber()
        {
            if (CompletedLevels.Count == 0) return 1;
            
            // Находим максимальный пройденный уровень и возвращаем следующий
            int maxCompletedLevel = CompletedLevels.Keys.Max();
            return maxCompletedLevel + 1;
        }
        
        /// <summary>
        /// Сброс всего прогресса
        /// </summary>
        public void ResetProgress()
        {
            CompletedLevels.Clear();
            CompletedLevelsCount = 0;
            LastSaveTime = DateTime.Now;
        }
        
        /// <summary>
        /// Валидация данных прогресса
        /// </summary>
        /// <returns>true если данные корректны</returns>
        public bool IsValid()
        {
            if (CompletedLevels == null) return false;
            if (CompletedLevelsCount < 0) return false;
            if (CompletedLevelsCount != CompletedLevels.Count) return false;
            
            // Проверяем валидность каждого пройденного уровня
            foreach (var kvp in CompletedLevels)
            {
                if (kvp.Key <= 0) return false;
                if (kvp.Value == null || !kvp.Value.IsValid()) return false;
            }
            
            return true;
        }
    }
    
    /// <summary>
    /// Данные о прохождении конкретного уровня
    /// </summary>
    [Serializable]
    public class LevelCompletionData
    {
        /// <summary>
        /// ID пройденного уровня
        /// </summary>
        public int LevelId { get; set; }
        
        /// <summary>
        /// Порядок разгаданных слов
        /// </summary>
        public string[] CompletedWords { get; set; } = new string[0];
        
        /// <summary>
        /// Время прохождения в секундах
        /// </summary>
        public float CompletionTime { get; set; } = 0f;
        
        /// <summary>
        /// Дата и время прохождения
        /// </summary>
        public DateTime CompletionDate { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Валидация данных прохождения
        /// </summary>
        /// <returns>true если данные корректны</returns>
        public bool IsValid()
        {
            return LevelId > 0 && 
                   CompletedWords != null && 
                   CompletedWords.Length > 0 && 
                   CompletionTime >= 0f;
        }
    }
}
