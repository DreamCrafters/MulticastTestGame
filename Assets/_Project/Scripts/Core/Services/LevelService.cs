using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using WordPuzzle.Core.Architecture;
using WordPuzzle.Data.Models;

namespace WordPuzzle.Core.Services
{
    /// <summary>
    /// Реальная реализация сервиса загрузки уровней из JSON файлов
    /// Заменяет MockLevelService из этапа 1-2
    /// </summary>
    public class LevelService : ILevelService
    {
        public bool IsInitialized { get; private set; }
        
        private readonly Dictionary<int, LevelData> _cachedLevels;
        private readonly string _levelsResourcePath = "Levels";
        private int _totalLevelsCount = 0;
        
        /// <summary>
        /// Конструктор сервиса
        /// </summary>
        public LevelService()
        {
            _cachedLevels = new Dictionary<int, LevelData>();
        }
        
        /// <summary>
        /// Инициализация сервиса - сканирование доступных уровней
        /// </summary>
        public async UniTask InitializeAsync()
        {
            GameLogger.LogInfo("LevelService", "Initializing Level Service...");
            
            try
            {
                // Сканируем доступные уровни в Resources
                await ScanAvailableLevelsAsync();
                
                IsInitialized = true;
                GameLogger.LogInfo("LevelService", $"Level Service initialized successfully. Found {_totalLevelsCount} levels");
            }
            catch (Exception ex)
            {
                GameLogger.LogException("LevelService", ex);
                throw;
            }
        }
        
        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        public void Dispose()
        {
            if (!IsInitialized) return;
            
            _cachedLevels.Clear();
            _totalLevelsCount = 0;
            IsInitialized = false;
            
            GameLogger.LogInfo("LevelService", "Level Service disposed");
        }
        
        /// <summary>
        /// Загрузка данных уровня
        /// </summary>
        public async UniTask<LevelData> LoadLevelAsync(int levelId)
        {
            if (!IsInitialized)
            {
                GameLogger.LogError("LevelService", "Service is not initialized");
                return null;
            }
            
            // Проверяем кэш
            if (_cachedLevels.TryGetValue(levelId, out var cachedLevel))
            {
                GameLogger.LogInfo("LevelService", $"Returning cached level {levelId}");
                return cachedLevel;
            }
            
            try
            {
                GameLogger.LogInfo("LevelService", $"Loading level {levelId} from JSON...");
                
                // Формируем путь к файлу
                string fileName = $"level_{levelId:D3}";
                string resourcePath = $"{_levelsResourcePath}/{fileName}";
                
                // Загружаем JSON из Resources
                var textAsset = Resources.Load<TextAsset>(resourcePath);
                
                if (textAsset == null)
                {
                    GameLogger.LogError("LevelService", $"Level file not found: {resourcePath}");
                    return null;
                }
                
                // Парсим JSON
                var levelData = await ParseLevelDataAsync(textAsset.text, levelId);
                
                if (levelData != null && levelData.IsValid())
                {
                    // Кэшируем успешно загруженный уровень
                    _cachedLevels[levelId] = levelData;
                    GameLogger.LogInfo("LevelService", $"Level {levelId} loaded and cached successfully");
                    
                    return levelData;
                }
                else
                {
                    GameLogger.LogError("LevelService", $"Level {levelId} data is invalid");
                    return null;
                }
            }
            catch (Exception ex)
            {
                GameLogger.LogException("LevelService", ex);
                return null;
            }
        }
        
        /// <summary>
        /// Получение общего количества уровней
        /// </summary>
        public int GetTotalLevelsCount()
        {
            return _totalLevelsCount;
        }
        
        /// <summary>
        /// Проверка существования уровня
        /// </summary>
        public bool IsLevelExists(int levelId)
        {
            if (levelId <= 0) return false;
            
            // Если уровень уже кэширован, он точно существует
            if (_cachedLevels.ContainsKey(levelId)) return true;
            
            // Проверяем существование файла в Resources
            string fileName = $"level_{levelId:D3}";
            string resourcePath = $"{_levelsResourcePath}/{fileName}";
            
            var textAsset = Resources.Load<TextAsset>(resourcePath);
            return textAsset != null;
        }
        
        /// <summary>
        /// Сканирование доступных уровней в Resources
        /// </summary>
        private async UniTask ScanAvailableLevelsAsync()
        {
            GameLogger.LogInfo("LevelService", $"Scanning levels in Resources/{_levelsResourcePath}...");
            
            _totalLevelsCount = 0;
            
            // Проверяем существование уровней начиная с 1
            for (int levelId = 1; levelId <= 999; levelId++) // Максимум 999 уровней
            {
                string fileName = $"level_{levelId:D3}";
                string resourcePath = $"{_levelsResourcePath}/{fileName}";
                
                var textAsset = Resources.Load<TextAsset>(resourcePath);
                
                if (textAsset != null)
                {
                    _totalLevelsCount = levelId;
                    GameLogger.LogInfo("LevelService", $"Found level file: {fileName}");
                    
                    // Освобождаем ресурс сразу после проверки
                    Resources.UnloadAsset(textAsset);
                }
                else
                {
                    // Если уровень не найден и это не первый пропущенный, останавливаемся
                    if (levelId > _totalLevelsCount + 5) // Допускаем пропуск максимум 5 файлов подряд
                    {
                        break;
                    }
                }
                
                // Даем возможность Unity обработать кадр
                if (levelId % 10 == 0)
                {
                    await UniTask.Yield();
                }
            }
            
            GameLogger.LogInfo("LevelService", $"Scan completed. Total levels found: {_totalLevelsCount}");
        }
        
        /// <summary>
        /// Парсинг JSON данных уровня
        /// </summary>
        private async UniTask<LevelData> ParseLevelDataAsync(string jsonText, int expectedLevelId)
        {
            await UniTask.Yield(); // Имитация асинхронной работы
            
            try
            {
                var levelData = JsonConvert.DeserializeObject<LevelData>(jsonText);
                
                if (levelData == null)
                {
                    GameLogger.LogError("LevelService", "Failed to deserialize level data - result is null");
                    return null;
                }
                
                // Проверяем соответствие ID
                if (levelData.LevelId != expectedLevelId)
                {
                    GameLogger.LogWarning("LevelService", $"Level ID mismatch: expected {expectedLevelId}, got {levelData.LevelId}");
                    levelData.LevelId = expectedLevelId; // Исправляем ID
                }
                
                // Валидация загруженных данных
                if (!levelData.IsValid())
                {
                    GameLogger.LogError("LevelService", $"Level {expectedLevelId} validation failed");
                    return null;
                }
                
                GameLogger.LogInfo("LevelService", 
                    $"Level {expectedLevelId} parsed successfully: {levelData.TargetWords.Length} words, {levelData.AvailableClusters.Length} clusters");
                
                return levelData;
            }
            catch (JsonException jsonEx)
            {
                GameLogger.LogError("LevelService", $"JSON parsing error for level {expectedLevelId}: {jsonEx.Message}");
                return null;
            }
            catch (Exception ex)
            {
                GameLogger.LogException("LevelService", ex);
                return null;
            }
        }
    }
}