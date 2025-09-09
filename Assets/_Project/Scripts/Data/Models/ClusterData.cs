using System;

namespace WordPuzzle.Data.Models
{
    /// <summary>
    /// Модель данных для кластера букв
    /// Используется в игровом процессе для отслеживания состояния кластеров
    /// </summary>
    [Serializable]
    public class ClusterData
    {
        /// <summary>
        /// Текст кластера (например, "КЛ", "АС", "ТЕР")
        /// </summary>
        public string Text { get; set; } = string.Empty;
        
        /// <summary>
        /// Уникальный идентификатор кластера в уровне
        /// </summary>
        public int ClusterId { get; set; }
        
        /// <summary>
        /// Размещен ли кластер на игровом поле
        /// </summary>
        public bool IsPlaced { get; set; } = false;
        
        /// <summary>
        /// Позиция кластера на игровом поле (если размещен)
        /// </summary>
        public ClusterPosition Position { get; set; }
        
        /// <summary>
        /// Создание кластера из строки
        /// </summary>
        public static ClusterData FromString(string text, int clusterId)
        {
            return new ClusterData
            {
                Text = text ?? string.Empty,
                ClusterId = clusterId,
                IsPlaced = false,
                Position = new ClusterPosition()
            };
        }
        
        /// <summary>
        /// Валидация данных кластера
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Text) && Text.Length >= 1 && Text.Length <= 4;
        }
        
        /// <summary>
        /// Размещение кластера на позицию
        /// </summary>
        public void PlaceAt(int wordIndex, int startCellIndex)
        {
            Position = new ClusterPosition
            {
                WordIndex = wordIndex,
                StartCellIndex = startCellIndex
            };
            IsPlaced = true;
        }
        
        /// <summary>
        /// Убрать кластер с игрового поля
        /// </summary>
        public void RemoveFromField()
        {
            IsPlaced = false;
            Position = new ClusterPosition();
        }
        
        /// <summary>
        /// Создание копии кластера
        /// </summary>
        public ClusterData Clone()
        {
            return new ClusterData
            {
                Text = this.Text,
                ClusterId = this.ClusterId,
                IsPlaced = this.IsPlaced,
                Position = this.Position.Clone()
            };
        }
    }
    
    /// <summary>
    /// Позиция кластера на игровом поле
    /// </summary>
    [Serializable]
    public class ClusterPosition
    {
        /// <summary>
        /// Индекс слова (строки) на игровом поле (0-3)
        /// </summary>
        public int WordIndex { get; set; } = -1;
        
        /// <summary>
        /// Начальный индекс ячейки в слове (0-5)
        /// </summary>
        public int StartCellIndex { get; set; } = -1;
        
        /// <summary>
        /// Валидна ли позиция
        /// </summary>
        public bool IsValid()
        {
            return WordIndex >= 0 && StartCellIndex >= 0;
        }
        
        /// <summary>
        /// Создание копии позиции
        /// </summary>
        public ClusterPosition Clone()
        {
            return new ClusterPosition
            {
                WordIndex = this.WordIndex,
                StartCellIndex = this.StartCellIndex
            };
        }
    }
}