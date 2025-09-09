using System;
using Cysharp.Threading.Tasks;
using WordPuzzle.Core.Services;
using WordPuzzle.Data.Models; // Новый импорт

namespace WordPuzzle.Core.Architecture
{
    // ПРИМЕЧАНИЕ: MockLevelService удален, так как заменен на настоящий LevelService
    // Остальные моки сохраняются для следующих этапов
    
    /// <summary>
    /// Мок сервиса прогресса для этапа 1-3
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
    /// Мок сервиса UI для этапа 1-3
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
