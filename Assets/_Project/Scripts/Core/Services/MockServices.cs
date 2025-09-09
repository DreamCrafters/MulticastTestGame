using System;
using Cysharp.Threading.Tasks;
using WordPuzzle.Core.Services;

namespace WordPuzzle.Core.Architecture
{
    /// <summary>
    /// Мок сервиса уровней для этапа 1
    /// Возвращает заглушечные данные для тестирования архитектуры
    /// </summary>
    public class MockLevelService : ILevelService
    {
        public bool IsInitialized { get; private set; }
        
        public async UniTask InitializeAsync()
        {
            GameLogger.LogInfo("MockLevelService", "Initializing mock level service...");
            
            // Имитация асинхронной загрузки
            await UniTask.Delay(100);
            
            IsInitialized = true;
            GameLogger.LogInfo("MockLevelService", "Mock level service initialized with 3 test levels");
        }
        
        public void Dispose()
        {
            IsInitialized = false;
            GameLogger.LogInfo("MockLevelService", "Mock level service disposed");
        }
        
        public async UniTask<LevelData> LoadLevelAsync(int levelId)
        {
            GameLogger.LogInfo("MockLevelService", $"Loading mock level {levelId}");
            
            // Имитация загрузки
            await UniTask.Delay(50);
            
            // Возвращаем тестовые данные
            return new LevelData
            {
                LevelId = levelId,
                TargetWords = new[] { "ТЕСТ", "МОКА", "ИГРА", "КОДУ" },
                AvailableClusters = new[] { "ТЕ", "СТ", "МО", "КА", "ИГ", "РА", "КО", "ДУ" }
            };
        }
        
        public int GetTotalLevelsCount()
        {
            return 3; // Тестовое количество уровней
        }
        
        public bool IsLevelExists(int levelId)
        {
            return levelId >= 1 && levelId <= 3;
        }
    }
    
    /// <summary>
    /// Мок сервиса прогресса для этапа 1
    /// </summary>
    public class MockProgressService : IProgressService
    {
        public bool IsInitialized { get; private set; }
        private int _completedLevels = 0;
        
        public async UniTask InitializeAsync()
        {
            GameLogger.LogInfo("MockProgressService", "Initializing mock progress service...");
            
            await UniTask.Delay(50);
            
            IsInitialized = true;
            GameLogger.LogInfo("MockProgressService", "Mock progress service initialized");
        }
        
        public void Dispose()
        {
            IsInitialized = false;
            GameLogger.LogInfo("MockProgressService", "Mock progress service disposed");
        }
        
        public int GetCompletedLevelsCount()
        {
            return _completedLevels;
        }
        
        public int GetCurrentLevelNumber()
        {
            return _completedLevels + 1;
        }
        
        public async UniTask MarkLevelCompletedAsync(int levelId, string[] completedWords)
        {
            GameLogger.LogInfo("MockProgressService", $"Marking level {levelId} as completed");
            
            await UniTask.Delay(10);
            
            if (levelId == _completedLevels + 1)
            {
                _completedLevels++;
                GameLogger.LogInfo("MockProgressService", $"Level {levelId} marked as completed. Total completed: {_completedLevels}");
            }
        }
        
        public bool IsLevelCompleted(int levelId)
        {
            return levelId <= _completedLevels;
        }
        
        public string[] GetLevelCompletionWords(int levelId)
        {
            if (IsLevelCompleted(levelId))
            {
                return new[] { "МОКА", "ТЕСТ", "ДАНН", "РАЗБ" };
            }
            return null;
        }
        
        public void ResetProgress()
        {
            _completedLevels = 0;
            GameLogger.LogInfo("MockProgressService", "Progress reset to 0");
        }
    }
    
    /// <summary>
    /// Мок сервиса UI для этапа 1
    /// </summary>
    public class MockUIService : IUIService
    {
        public bool IsInitialized { get; private set; }
        
        public event Action<string> OnScreenOpened;
        public event Action<string> OnScreenClosed;
        
        public async UniTask InitializeAsync()
        {
            GameLogger.LogInfo("MockUIService", "Initializing mock UI service...");
            
            await UniTask.Delay(20);
            
            IsInitialized = true;
            GameLogger.LogInfo("MockUIService", "Mock UI service initialized");
        }
        
        public void Dispose()
        {
            IsInitialized = false;
            OnScreenOpened = null;
            OnScreenClosed = null;
            GameLogger.LogInfo("MockUIService", "Mock UI service disposed");
        }
        
        public void ShowLoadingScreen(string message = "Loading...")
        {
            GameLogger.LogInfo("MockUIService", $"Showing loading screen: {message}");
        }
        
        public void HideLoadingScreen()
        {
            GameLogger.LogInfo("MockUIService", "Hiding loading screen");
        }
        
        public void ShowMessage(string message, float duration = 3f)
        {
            GameLogger.LogInfo("MockUIService", $"Showing message: {message} (duration: {duration}s)");
        }
        
        public void ShowConfirmDialog(string title, string message, Action onConfirm = null, Action onCancel = null)
        {
            GameLogger.LogInfo("MockUIService", $"Showing confirm dialog: {title} - {message}");
        }
        
        public void SetBackButtonActive(bool isActive, Action onBackPressed = null)
        {
            GameLogger.LogInfo("MockUIService", $"Back button active: {isActive}");
        }
        
        public void PlayUISound(UISoundType soundType)
        {
            GameLogger.LogInfo("MockUIService", $"Playing UI sound: {soundType}");
        }
        
        public void NotifyScreenOpened(string screenName)
        {
            GameLogger.LogInfo("MockUIService", $"Screen opened: {screenName}");
            OnScreenOpened?.Invoke(screenName);
        }
        
        public void NotifyScreenClosed(string screenName)
        {
            GameLogger.LogInfo("MockUIService", $"Screen closed: {screenName}");
            OnScreenClosed?.Invoke(screenName);
        }
    }
}