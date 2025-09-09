using System;
using Newtonsoft.Json;

namespace WordPuzzle.Data.Models
{
    /// <summary>
    /// Полная модель данных уровня
    /// Заменяет заглушку из ILevelService.cs
    /// </summary>
    [Serializable]
    public class LevelData
    {
        [JsonProperty("levelId")]
        public int LevelId { get; set; }
        
        [JsonProperty("targetWords")]
        public WordData[] TargetWords { get; set; } = new WordData[0];
        
        [JsonProperty("availableClusters")]
        public string[] AvailableClusters { get; set; } = new string[0];
        
        /// <summary>
        /// Валидация данных уровня
        /// </summary>
        public bool IsValid()
        {
            if (LevelId <= 0) return false;
            if (TargetWords == null || TargetWords.Length == 0) return false;
            if (AvailableClusters == null || AvailableClusters.Length == 0) return false;
            
            // Проверяем валидность каждого слова
            foreach (var word in TargetWords)
            {
                if (word == null || !word.IsValid()) return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Получение всех целевых слов как строкового массива
        /// Для обратной совместимости с существующим кодом
        /// </summary>
        public string[] GetTargetWordsAsStrings()
        {
            var result = new string[TargetWords.Length];
            for (int i = 0; i < TargetWords.Length; i++)
            {
                result[i] = TargetWords[i].Word;
            }
            return result;
        }
    }
}
