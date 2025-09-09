using System;
using System.Collections.Generic;
using System.Linq;

namespace WordPuzzle.Data.Models
{
    /// <summary>
    /// Модель игровой сессии для отслеживания прогресса в текущем уровне
    /// </summary>
    public class GameSession
    {
        /// <summary>
        /// Данные текущего уровня
        /// </summary>
        public LevelData LevelData { get; private set; }
        
        /// <summary>
        /// Доступные кластеры для размещения
        /// </summary>
        public List<ClusterData> AvailableClusters { get; private set; }
        
        /// <summary>
        /// Размещенные кластеры на игровом поле
        /// </summary>
        public List<ClusterData> PlacedClusters { get; private set; }
        
        /// <summary>
        /// Время начала уровня
        /// </summary>
        public DateTime StartTime { get; private set; }
        
        /// <summary>
        /// Порядок разгадывания слов
        /// </summary>
        public List<string> CompletedWordsOrder { get; private set; }
        
        /// <summary>
        /// Завершена ли сессия
        /// </summary>
        public bool IsCompleted { get; private set; }
        
        /// <summary>
        /// Время завершения сессии
        /// </summary>
        public DateTime? CompletionTime { get; private set; }
        
        /// <summary>
        /// Создание новой игровой сессии
        /// </summary>
        public GameSession(LevelData levelData)
        {
            LevelData = levelData ?? throw new ArgumentNullException(nameof(levelData));
            
            // Создаем кластеры из данных уровня
            AvailableClusters = new List<ClusterData>();
            for (int i = 0; i < levelData.AvailableClusters.Length; i++)
            {
                AvailableClusters.Add(ClusterData.FromString(levelData.AvailableClusters[i], i));
            }
            
            PlacedClusters = new List<ClusterData>();
            CompletedWordsOrder = new List<string>();
            StartTime = DateTime.Now;
            IsCompleted = false;
        }
        
        /// <summary>
        /// Размещение кластера на игровом поле
        /// </summary>
        public bool PlaceCluster(int clusterId, int wordIndex, int startCellIndex)
        {
            var cluster = AvailableClusters.FirstOrDefault(c => c.ClusterId == clusterId && !c.IsPlaced);
            if (cluster == null) return false;
            
            // Проверяем возможность размещения
            if (!CanPlaceCluster(cluster, wordIndex, startCellIndex))
                return false;
            
            // Размещаем кластер
            cluster.PlaceAt(wordIndex, startCellIndex);
            PlacedClusters.Add(cluster);
            
            // Проверяем завершение слова
            CheckWordCompletion(wordIndex);
            
            // Проверяем завершение уровня
            CheckLevelCompletion();
            
            return true;
        }
        
        /// <summary>
        /// Убрать кластер с игрового поля
        /// </summary>
        public bool RemoveCluster(int clusterId)
        {
            var cluster = PlacedClusters.FirstOrDefault(c => c.ClusterId == clusterId);
            if (cluster == null) return false;
            
            cluster.RemoveFromField();
            PlacedClusters.Remove(cluster);
            
            return true;
        }
        
        /// <summary>
        /// Проверка возможности размещения кластера
        /// </summary>
        private bool CanPlaceCluster(ClusterData cluster, int wordIndex, int startCellIndex)
        {
            // Проверяем границы
            if (wordIndex < 0 || wordIndex >= LevelData.TargetWords.Length) return false;
            if (startCellIndex < 0 || startCellIndex + cluster.Text.Length > 6) return false;
            
            // Проверяем пересечения с другими кластерами
            for (int i = 0; i < cluster.Text.Length; i++)
            {
                int cellIndex = startCellIndex + i;
                if (IsCellOccupied(wordIndex, cellIndex)) return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Проверка занятости ячейки
        /// </summary>
        private bool IsCellOccupied(int wordIndex, int cellIndex)
        {
            return PlacedClusters.Any(cluster => 
                cluster.Position.WordIndex == wordIndex &&
                cellIndex >= cluster.Position.StartCellIndex &&
                cellIndex < cluster.Position.StartCellIndex + cluster.Text.Length);
        }
        
        /// <summary>
        /// Проверка завершения слова
        /// </summary>
        private void CheckWordCompletion(int wordIndex)
        {
            var targetWord = LevelData.TargetWords[wordIndex];
            var wordClusters = PlacedClusters
                .Where(c => c.Position.WordIndex == wordIndex)
                .OrderBy(c => c.Position.StartCellIndex)
                .ToList();
            
            // Проверяем что все кластеры слова размещены подряд
            string reconstructedWord = string.Join("", wordClusters.Select(c => c.Text));
            
            if (reconstructedWord == targetWord.Word && !CompletedWordsOrder.Contains(targetWord.Word))
            {
                CompletedWordsOrder.Add(targetWord.Word);
            }
        }
        
        /// <summary>
        /// Проверка завершения уровня
        /// </summary>
        private void CheckLevelCompletion()
        {
            // Уровень завершен если:
            // 1. Все кластеры размещены
            // 2. Все слова собраны правильно
            bool allClustersPlaced = AvailableClusters.All(c => c.IsPlaced);
            bool allWordsCompleted = CompletedWordsOrder.Count == LevelData.TargetWords.Length;
            
            if (allClustersPlaced && allWordsCompleted && !IsCompleted)
            {
                IsCompleted = true;
                CompletionTime = DateTime.Now;
            }
        }
        
        /// <summary>
        /// Получение времени прохождения в секундах
        /// </summary>
        public float GetCompletionTimeInSeconds()
        {
            if (!IsCompleted || !CompletionTime.HasValue) return 0f;
            
            return (float)(CompletionTime.Value - StartTime).TotalSeconds;
        }
        
        /// <summary>
        /// Получение текущего состояния игрового поля
        /// </summary>
        public string[,] GetGameFieldState()
        {
            var field = new string[4, 6]; // 4 слова по 6 букв
            
            foreach (var cluster in PlacedClusters)
            {
                for (int i = 0; i < cluster.Text.Length; i++)
                {
                    int wordIndex = cluster.Position.WordIndex;
                    int cellIndex = cluster.Position.StartCellIndex + i;
                    
                    if (wordIndex >= 0 && wordIndex < 4 && cellIndex >= 0 && cellIndex < 6)
                    {
                        field[wordIndex, cellIndex] = cluster.Text[i].ToString();
                    }
                }
            }
            
            return field;
        }
    }
}