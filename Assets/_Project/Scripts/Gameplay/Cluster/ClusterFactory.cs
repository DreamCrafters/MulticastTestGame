using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WordPuzzle.Core.Architecture;
using WordPuzzle.Data.Models;

namespace WordPuzzle.Gameplay.Cluster
{
    /// <summary>
    /// Фабрика для создания кластеров из данных уровня
    /// Обеспечивает генерацию кластеров с правильными ID и валидацией
    /// </summary>
    public static class ClusterFactory
    {
        /// <summary>
        /// Создание списка кластеров из данных уровня
        /// </summary>
        /// <param name="levelData">Данные уровня</param>
        /// <returns>Список созданных кластеров</returns>
        public static List<ClusterData> CreateClustersFromLevel(LevelData levelData)
        {
            if (levelData == null)
            {
                GameLogger.LogError("ClusterFactory", "Cannot create clusters from null level data");
                return new List<ClusterData>();
            }

            if (levelData.AvailableClusters == null || levelData.AvailableClusters.Length == 0)
            {
                GameLogger.LogError("ClusterFactory", "Level data contains no available clusters");
                return new List<ClusterData>();
            }

            var clusters = new List<ClusterData>();

            GameLogger.LogInfo("ClusterFactory", $"Creating {levelData.AvailableClusters.Length} clusters from level {levelData.LevelId}");

            for (int i = 0; i < levelData.AvailableClusters.Length; i++)
            {
                var clusterText = levelData.AvailableClusters[i];
                var cluster = CreateSingleCluster(clusterText, i, levelData.LevelId);

                if (cluster != null)
                {
                    clusters.Add(cluster);
                }
                else
                {
                    GameLogger.LogWarning("ClusterFactory", $"Failed to create cluster for text '{clusterText}' at index {i}");
                }
            }

            // Валидация созданных кластеров
            ValidateCreatedClusters(clusters, levelData);

            GameLogger.LogInfo("ClusterFactory", $"Successfully created {clusters.Count} clusters");
            return clusters;
        }

        /// <summary>
        /// Создание одного кластера
        /// </summary>
        /// <param name="clusterText">Текст кластера</param>
        /// <param name="index">Индекс в массиве кластеров</param>
        /// <param name="levelId">ID уровня для логирования</param>
        /// <returns>Созданный кластер или null</returns>
        public static ClusterData CreateSingleCluster(string clusterText, int index, int levelId = -1)
        {
            if (string.IsNullOrEmpty(clusterText))
            {
                GameLogger.LogWarning("ClusterFactory", $"Skipping empty cluster text at index {index}");
                return null;
            }

            // Очистка и валидация текста
            string cleanText = CleanClusterText(clusterText);

            if (IsValidClusterText(cleanText) == false)
            {
                GameLogger.LogError("ClusterFactory", $"Invalid cluster text '{clusterText}' at index {index}");
                return null;
            }

            var cluster = ClusterData.FromString(cleanText, index);

            // Дополнительная валидация созданного кластера
            if (cluster == null || cluster.IsValid() == false)
            {
                GameLogger.LogError("ClusterFactory", $"Failed to create valid cluster from text '{cleanText}'");
                return null;
            }

            GameLogger.LogInfo("ClusterFactory", $"Created cluster '{cleanText}' (ID: {index}) for level {levelId}");
            return cluster;
        }

        /// <summary>
        /// Создание кластеров из массива строк (для тестирования)
        /// </summary>
        /// <param name="clusterTexts">Массив текстов кластеров</param>
        /// <param name="startId">Начальный ID для кластеров</param>
        /// <returns>Список созданных кластеров</returns>
        public static List<ClusterData> CreateClustersFromTexts(string[] clusterTexts, int startId = 0)
        {
            if (clusterTexts == null || clusterTexts.Length == 0)
            {
                GameLogger.LogWarning("ClusterFactory", "Cannot create clusters from empty array");
                return new List<ClusterData>();
            }

            var clusters = new List<ClusterData>();

            for (int i = 0; i < clusterTexts.Length; i++)
            {
                var cluster = CreateSingleCluster(clusterTexts[i], startId + i);
                if (cluster != null)
                {
                    clusters.Add(cluster);
                }
            }

            GameLogger.LogInfo("ClusterFactory", $"Created {clusters.Count} clusters from text array");
            return clusters;
        }

        /// <summary>
        /// Создание тестовых кластеров для отладки
        /// </summary>
        /// <returns>Список тестовых кластеров</returns>
        public static List<ClusterData> CreateTestClusters()
        {
            var testTexts = new string[]
            {
                "КЛ", "АС", "ТЕР", "ПРО", "ЕКТ", "ЗА", "ДА", "ЧА", "ИГ", "РО", "КА"
            };

            GameLogger.LogInfo("ClusterFactory", "Creating test clusters for debugging");
            return CreateClustersFromTexts(testTexts);
        }

        /// <summary>
        /// Дублирование кластеров для больших уровней
        /// </summary>
        /// <param name="baseClusters">Базовые кластеры</param>
        /// <param name="duplicateCount">Количество дубликатов</param>
        /// <returns>Список кластеров с дубликатами</returns>
        public static List<ClusterData> DuplicateClusters(List<ClusterData> baseClusters, int duplicateCount)
        {
            if (baseClusters == null || baseClusters.Count == 0)
            {
                GameLogger.LogWarning("ClusterFactory", "Cannot duplicate empty cluster list");
                return new List<ClusterData>();
            }

            if (duplicateCount <= 0)
            {
                GameLogger.LogWarning("ClusterFactory", "Invalid duplicate count, returning original list");
                return new List<ClusterData>(baseClusters);
            }

            var duplicatedClusters = new List<ClusterData>();
            int currentId = baseClusters.Max(c => c.ClusterId) + 1;

            // Добавляем оригинальные кластеры
            duplicatedClusters.AddRange(baseClusters);

            // Создаем дубликаты
            for (int duplicate = 0; duplicate < duplicateCount; duplicate++)
            {
                foreach (var originalCluster in baseClusters)
                {
                    var duplicatedCluster = originalCluster.Clone();
                    duplicatedCluster.ClusterId = currentId++;
                    duplicatedClusters.Add(duplicatedCluster);
                }
            }

            GameLogger.LogInfo("ClusterFactory", $"Created {duplicatedClusters.Count} clusters with {duplicateCount} duplicates");
            return duplicatedClusters;
        }

        /// <summary>
        /// Перемешивание кластеров для случайного порядка
        /// </summary>
        /// <param name="clusters">Исходные кластеры</param>
        /// <param name="preserveIds">Сохранить ли оригинальные ID</param>
        /// <returns>Перемешанный список кластеров</returns>
        public static List<ClusterData> ShuffleClusters(List<ClusterData> clusters, bool preserveIds = true)
        {
            if (clusters == null || clusters.Count == 0)
            {
                return new List<ClusterData>();
            }

            var shuffledClusters = new List<ClusterData>(clusters);

            // Простое перемешивание Fisher-Yates
            for (int i = shuffledClusters.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                var temp = shuffledClusters[i];
                shuffledClusters[i] = shuffledClusters[randomIndex];
                shuffledClusters[randomIndex] = temp;
            }

            // Переназначаем ID если не нужно сохранять
            if (preserveIds == false)
            {
                for (int i = 0; i < shuffledClusters.Count; i++)
                {
                    shuffledClusters[i].ClusterId = i;
                }
            }

            GameLogger.LogInfo("ClusterFactory", $"Shuffled {shuffledClusters.Count} clusters (preserve IDs: {preserveIds})");
            return shuffledClusters;
        }

        /// <summary>
        /// Очистка текста кластера от лишних символов
        /// </summary>
        /// <param name="text">Исходный текст</param>
        /// <returns>Очищенный текст</returns>
        private static string CleanClusterText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            // Убираем лишние пробелы и переводим в верхний регистр
            string cleaned = text.Trim().ToUpper();

            // Убираем недопустимые символы (оставляем только буквы)
            var validChars = cleaned.Where(char.IsLetter).ToArray();
            string result = new string(validChars);

            if (result != cleaned)
            {
                GameLogger.LogInfo("ClusterFactory", $"Cleaned cluster text: '{text}' -> '{result}'");
            }

            return result;
        }

        /// <summary>
        /// Валидация текста кластера
        /// </summary>
        /// <param name="text">Текст для проверки</param>
        /// <returns>true если текст валиден</returns>
        private static bool IsValidClusterText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            // Проверяем длину (1-4 символа согласно ТЗ)
            if (text.Length < 1 || text.Length > 4)
            {
                GameLogger.LogWarning("ClusterFactory", $"Invalid cluster length: '{text}' (length: {text.Length})");
                return false;
            }

            // Проверяем что все символы - буквы
            if (text.All(char.IsLetter) == false)
            {
                GameLogger.LogWarning("ClusterFactory", $"Invalid cluster contains non-letters: '{text}'");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Валидация созданных кластеров против данных уровня
        /// </summary>
        /// <param name="clusters">Созданные кластеры</param>
        /// <param name="levelData">Данные уровня</param>
        private static void ValidateCreatedClusters(List<ClusterData> clusters, LevelData levelData)
        {
            GameLogger.LogInfo("ClusterFactory", "Validating created clusters against level data...");

            // Проверяем количество кластеров
            if (clusters.Count != levelData.AvailableClusters.Length)
            {
                GameLogger.LogWarning("ClusterFactory", 
                    $"Cluster count mismatch: created {clusters.Count}, expected {levelData.AvailableClusters.Length}");
            }

            // Проверяем уникальность ID
            var uniqueIds = clusters.Select(c => c.ClusterId).Distinct().Count();
            if (uniqueIds != clusters.Count)
            {
                GameLogger.LogWarning("ClusterFactory", "Some clusters have duplicate IDs");
            }

            // Проверяем что все кластеры могут составить целевые слова
            ValidateClustersForTargetWords(clusters, levelData.TargetWords);

            GameLogger.LogInfo("ClusterFactory", "Cluster validation completed");
        }

        /// <summary>
        /// Проверка что кластеры могут составить целевые слова
        /// </summary>
        /// <param name="clusters">Список кластеров</param>
        /// <param name="targetWords">Целевые слова</param>
        private static void ValidateClustersForTargetWords(List<ClusterData> clusters, WordData[] targetWords)
        {
            var clusterTexts = clusters.Select(c => c.Text).ToList();

            foreach (var wordData in targetWords)
            {
                bool canFormWord = true;
                var requiredClusters = new List<string>(wordData.Clusters);

                foreach (var requiredCluster in requiredClusters)
                {
                    if (clusterTexts.Contains(requiredCluster) == false)
                    {
                        GameLogger.LogError("ClusterFactory", 
                            $"Missing cluster '{requiredCluster}' required for word '{wordData.Word}'");
                        canFormWord = false;
                    }
                }

                if (canFormWord)
                {
                    GameLogger.LogInfo("ClusterFactory", $"Word '{wordData.Word}' can be formed from available clusters");
                }
                else
                {
                    GameLogger.LogError("ClusterFactory", $"Word '{wordData.Word}' CANNOT be formed from available clusters");
                }
            }
        }

        /// <summary>
        /// Получение статистики кластеров
        /// </summary>
        /// <param name="clusters">Список кластеров</param>
        /// <returns>Строка со статистикой</returns>
        public static string GetClustersStatistics(List<ClusterData> clusters)
        {
            if (clusters == null || clusters.Count == 0)
            {
                return "No clusters to analyze";
            }

            var lengthGroups = clusters.GroupBy(c => c.Text.Length).OrderBy(g => g.Key);
            var lengthStats = string.Join(", ", lengthGroups.Select(g => $"{g.Key}-letter: {g.Count()}"));

            var placedCount = clusters.Count(c => c.IsPlaced);
            var availableCount = clusters.Count - placedCount;

            return $"Clusters Statistics:\n" +
                   $"Total: {clusters.Count}\n" +
                   $"By length: {lengthStats}\n" +
                   $"Placed: {placedCount}, Available: {availableCount}";
        }
    }
}