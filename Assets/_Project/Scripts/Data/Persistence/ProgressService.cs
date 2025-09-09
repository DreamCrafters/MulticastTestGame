using System;
using Cysharp.Threading.Tasks;
using WordPuzzle.Core.Architecture;
using WordPuzzle.Core.Services;
using WordPuzzle.Data.Models;

namespace WordPuzzle.Data.Persistence
{
    /// <summary>
    /// Реализация сервиса прогресса игрока
    /// Заменяет MockProgressService из предыдущих этапов
    /// </summary>
    public class ProgressService : IProgressService
    {
        public bool IsInitialized { get; private set; }
        
        private PlayerProgress _currentProgress;
        private readonly object _saveLock = new object();
        
        /// <summary>
        /// Инициализация сервиса прогресса
        /// </summary>
        public async UniTask InitializeAsync()
        {
            GameLogger.LogInfo("ProgressService", "Initializing Progress Service...");
            
            try
            {
                // Загружаем существующий прогресс или создаем новый
                await LoadProgressAsync();
                
                IsInitialized = true;
                GameLogger.LogInfo("ProgressService", $"Progress Service initialized. Completed levels: {_currentProgress.CompletedLevelsCount}");
            }
            catch (Exception ex)
            {
                GameLogger.LogException("ProgressService", ex);
                
                // В случае ошибки создаем новый прогресс
                _currentProgress = new PlayerProgress();
                IsInitialized = true;
                
                GameLogger.LogWarning("ProgressService", "Initialized with new progress due to error");
            }
        }
        
        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        public void Dispose()
        {
            if (!IsInitialized) return;
            
            // Сохраняем прогресс при завершении
            try
            {
                SaveProgressSync();
            }
            catch (Exception ex)
            {
                GameLogger.LogException("ProgressService", ex);
            }
            
            _currentProgress = null;
            IsInitialized = false;
            
            GameLogger.LogInfo("ProgressService", "Progress Service disposed");
        }
        
        /// <summary>
        /// Получение количества пройденных уровней
        /// </summary>
        public int GetCompletedLevelsCount()
        {
            if (!IsInitialized || _currentProgress == null)
            {
                GameLogger.LogWarning("ProgressService", "Service not initialized, returning 0 completed levels");
                return 0;
            }
            
            return _currentProgress.CompletedLevelsCount;
        }
        
        /// <summary>
        /// Получение номера текущего уровня
        /// </summary>
        public int GetCurrentLevelNumber()
        {
            if (!IsInitialized || _currentProgress == null)
            {
                GameLogger.LogWarning("ProgressService", "Service not initialized, returning level 1");
                return 1;
            }
            
            return _currentProgress.GetNextLevelNumber();
        }
        
        /// <summary>
        /// Отметка уровня как пройденного
        /// </summary>
        public async UniTask MarkLevelCompletedAsync(int levelId, string[] completedWords)
        {
            if (!IsInitialized || _currentProgress == null)
            {
                GameLogger.LogError("ProgressService", "Cannot mark level completed - service not initialized");
                return;
            }
            
            if (levelId <= 0)
            {
                GameLogger.LogError("ProgressService", $"Invalid level ID: {levelId}");
                return;
            }
            
            if (completedWords == null || completedWords.Length == 0)
            {
                GameLogger.LogError("ProgressService", $"Cannot mark level {levelId} completed - no words provided");
                return;
            }
            
            try
            {
                GameLogger.LogInfo("ProgressService", $"Marking level {levelId} as completed with {completedWords.Length} words");
                
                // Отмечаем уровень как пройденный
                _currentProgress.MarkLevelCompleted(levelId, completedWords);
                
                // Асинхронно сохраняем прогресс
                await SaveProgressAsync();
                
                GameLogger.LogInfo("ProgressService", $"Level {levelId} marked as completed. Total completed: {_currentProgress.CompletedLevelsCount}");
            }
            catch (Exception ex)
            {
                GameLogger.LogException("ProgressService", ex);
                throw;
            }
        }
        
        /// <summary>
        /// Проверка, пройден ли уровень
        /// </summary>
        public bool IsLevelCompleted(int levelId)
        {
            if (!IsInitialized || _currentProgress == null)
            {
                return false;
            }
            
            return _currentProgress.IsLevelCompleted(levelId);
        }
        
        /// <summary>
        /// Получение порядка разгаданных слов для уровня
        /// </summary>
        public string[] GetLevelCompletionWords(int levelId)
        {
            if (!IsInitialized || _currentProgress == null)
            {
                return null;
            }
            
            var completionData = _currentProgress.GetLevelCompletion(levelId);
            return completionData?.CompletedWords;
        }
        
        /// <summary>
        /// Сброс всего прогресса
        /// </summary>
        public void ResetProgress()
        {
            if (!IsInitialized)
            {
                GameLogger.LogWarning("ProgressService", "Cannot reset progress - service not initialized");
                return;
            }
            
            GameLogger.LogInfo("ProgressService", "Resetting all progress...");
            
            try
            {
                // Сбрасываем текущий прогресс
                _currentProgress.ResetProgress();
                
                // Удаляем все сохранения
                SaveData.DeleteAllSaveData();
                
                // Сохраняем новый пустой прогресс
                SaveProgressSync();
                
                GameLogger.LogInfo("ProgressService", "Progress reset completed");
            }
            catch (Exception ex)
            {
                GameLogger.LogException("ProgressService", ex);
            }
        }
        
        /// <summary>
        /// Асинхронная загрузка прогресса
        /// </summary>
        private async UniTask LoadProgressAsync()
        {
            GameLogger.LogInfo("ProgressService", "Loading player progress...");
            
            // Имитируем асинхронную загрузку
            await UniTask.Delay(10);
            
            _currentProgress = SaveData.LoadPlayerProgress();
            
            if (_currentProgress == null)
            {
                _currentProgress = new PlayerProgress();
                GameLogger.LogWarning("ProgressService", "Failed to load progress, using new instance");
            }
            
            GameLogger.LogInfo("ProgressService", "Progress loaded successfully");
        }
        
        /// <summary>
        /// Принудительное сохранение прогресса (синхронное, для событий приложения)
        /// </summary>
        public void ForceSaveProgress()
        {
            if (!IsInitialized)
            {
                GameLogger.LogWarning("ProgressService", "Cannot force save - service not initialized");
                return;
            }

            lock (_saveLock)
            {
                SaveProgressSync();
            }
        }
        
        /// <summary>
        /// Принудительная перезагрузка прогресса из сохранения (для обновления UI)
        /// </summary>
        public async UniTask RefreshProgressAsync()
        {
            if (!IsInitialized)
            {
                GameLogger.LogWarning("ProgressService", "Cannot refresh progress - service not initialized");
                return;
            }

            try
            {
                GameLogger.LogInfo("ProgressService", "Refreshing progress from disk...");
                
                // Перезагружаем прогресс из диска
                await LoadProgressAsync();
                
                GameLogger.LogInfo("ProgressService", $"Progress refreshed. Completed levels: {_currentProgress.CompletedLevelsCount}");
            }
            catch (Exception ex)
            {
                GameLogger.LogException("ProgressService", ex);
                throw;
            }
        }
        
        /// <summary>
        /// Асинхронное сохранение прогресса
        /// </summary>
        private async UniTask SaveProgressAsync()
        {
            // Switch to main thread for PlayerPrefs operations
            await UniTask.SwitchToMainThread();
            
            lock (_saveLock)
            {
                SaveProgressSync();
            }
        }
        
        /// <summary>
        /// Синхронное сохранение прогресса
        /// </summary>
        private void SaveProgressSync()
        {
            if (_currentProgress == null)
            {
                GameLogger.LogError("ProgressService", "Cannot save null progress");
                return;
            }
            
            bool success = SaveData.SavePlayerProgress(_currentProgress);
            
            if (success)
            {
                GameLogger.LogInfo("ProgressService", "Progress saved successfully");
            }
            else
            {
                GameLogger.LogError("ProgressService", "Failed to save progress");
            }
        }
        
        /// <summary>
        /// Получение отладочной информации о прогрессе
        /// </summary>
        public string GetProgressDebugInfo()
        {
            if (!IsInitialized || _currentProgress == null)
            {
                return "Progress Service not initialized";
            }
            
            var saveInfo = SaveData.GetSaveInfo();
            var currentInfo = $"Current Progress:\nCompleted Levels: {_currentProgress.CompletedLevelsCount}\nNext Level: {_currentProgress.GetNextLevelNumber()}\nLast Save: {_currentProgress.LastSaveTime:yyyy-MM-dd HH:mm:ss}";
            
            // Добавляем информацию о конкретных пройденных уровнях
            var levelsInfo = "Completed Levels Details:\n";
            foreach (var kvp in _currentProgress.CompletedLevels)
            {
                levelsInfo += $"  Level {kvp.Key}: {string.Join(", ", kvp.Value.CompletedWords)}\n";
            }
            
            return $"{currentInfo}\n\n{levelsInfo}\n{saveInfo}";
        }
    }
}