using System;
using Cysharp.Threading.Tasks;
using WordPuzzle.Core.Services;

namespace WordPuzzle.Core.Architecture
{
    // ПРИМЕЧАНИЕ: MockLevelService удален, так как заменен на настоящий LevelService
    // Остальные моки сохраняются для следующих этапов
    
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
