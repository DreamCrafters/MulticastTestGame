using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using WordPuzzle.Core.Architecture;
using WordPuzzle.Core.Services;
using WordPuzzle.UI.Screens;

namespace WordPuzzle.UI.Navigation
{
    /// <summary>
    /// Сервис для централизованного управления навигацией UI
    /// Обеспечивает единообразное управление переходами между экранами
    /// </summary>
    public class UINavigationService : IGameService
    {
        public bool IsInitialized { get; private set; }
        
        /// <summary>
        /// Событие изменения экрана
        /// </summary>
        public event Action<string, string> OnScreenChanged; // (from, to)
        
        /// <summary>
        /// Событие начала перехода между экранами
        /// </summary>
        public event Action<string> OnNavigationStarted;
        
        /// <summary>
        /// Событие завершения перехода между экранами
        /// </summary>
        public event Action<string> OnNavigationCompleted;
        
        private readonly ISceneService _sceneService;
        private readonly IUIService _uiService;
        
        private readonly Stack<string> _navigationHistory;
        private string _currentScreen;
        
        /// <summary>
        /// Конструктор с инжекцией зависимостей
        /// </summary>
        public UINavigationService(ISceneService sceneService, IUIService uiService)
        {
            _sceneService = sceneService;
            _uiService = uiService;
            _navigationHistory = new Stack<string>();
        }
        
        /// <summary>
        /// Инициализация сервиса навигации
        /// </summary>
        public async UniTask InitializeAsync()
        {
            GameLogger.LogInfo("UINavigationService", "Initializing UI Navigation Service...");
            
            // Подписываемся на события загрузки сцен
            _sceneService.OnSceneLoadStarted += OnSceneLoadStarted;
            _sceneService.OnSceneLoadCompleted += OnSceneLoadCompleted;
            
            // Получаем текущую сцену
            _currentScreen = _sceneService.GetCurrentSceneName();
            
            await UniTask.Yield();
            
            IsInitialized = true;
            GameLogger.LogInfo("UINavigationService", $"UI Navigation Service initialized. Current screen: {_currentScreen}");
        }
        
        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        public void Dispose()
        {
            if (IsInitialized == false) return;
            
            // Отписываемся от событий
            _sceneService.OnSceneLoadStarted -= OnSceneLoadStarted;
            _sceneService.OnSceneLoadCompleted -= OnSceneLoadCompleted;
            
            OnScreenChanged = null;
            OnNavigationStarted = null;
            OnNavigationCompleted = null;
            
            _navigationHistory.Clear();
            
            IsInitialized = false;
            GameLogger.LogInfo("UINavigationService", "UI Navigation Service disposed");
        }
        
        /// <summary>
        /// Навигация к экрану по имени
        /// </summary>
        /// <param name="screenName">Имя целевого экрана</param>
        /// <param name="addToHistory">Добавить ли в историю навигации</param>
        public async UniTask NavigateToAsync(string screenName, bool addToHistory = true)
        {
            if (IsInitialized == false)
            {
                GameLogger.LogError("UINavigationService", "Service is not initialized");
                return;
            }
            
            if (string.IsNullOrEmpty(screenName))
            {
                GameLogger.LogError("UINavigationService", "Screen name cannot be null or empty");
                return;
            }
            
            if (_currentScreen == screenName)
            {
                GameLogger.LogInfo("UINavigationService", $"Already on screen {screenName}");
                return;
            }
            
            GameLogger.LogInfo("UINavigationService", $"Navigating from {_currentScreen} to {screenName}");
            
            try
            {
                string previousScreen = _currentScreen;
                
                OnNavigationStarted?.Invoke(screenName);
                
                // Добавляем текущий экран в историю если необходимо
                if (addToHistory && !string.IsNullOrEmpty(_currentScreen))
                {
                    _navigationHistory.Push(_currentScreen);
                    GameLogger.LogInfo("UINavigationService", $"Added {_currentScreen} to navigation history");
                }
                
                // Загружаем новую сцену
                await _sceneService.LoadSceneAsync(screenName);
                
                _currentScreen = screenName;
                
                OnNavigationCompleted?.Invoke(screenName);
                OnScreenChanged?.Invoke(previousScreen, screenName);
                
                GameLogger.LogInfo("UINavigationService", $"Navigation completed: {previousScreen} -> {screenName}");
            }
            catch (Exception ex)
            {
                GameLogger.LogException("UINavigationService", ex);
                _uiService?.ShowMessage($"Failed to navigate to {screenName}", 3f);
                throw;
            }
        }
        
        /// <summary>
        /// Навигация к экрану с параметрами
        /// </summary>
        /// <param name="screenName">Имя экрана</param>
        /// <param name="parameters">Параметры для передачи</param>
        /// <param name="addToHistory">Добавить ли в историю навигации</param>
        public async UniTask NavigateToAsync(string screenName, object parameters, bool addToHistory = true)
        {
            if (IsInitialized == false)
            {
                GameLogger.LogError("UINavigationService", "Service is not initialized");
                return;
            }
            
            GameLogger.LogInfo("UINavigationService", $"Navigating to {screenName} with parameters: {parameters?.GetType().Name ?? "null"}");
            
            try
            {
                string previousScreen = _currentScreen;
                
                OnNavigationStarted?.Invoke(screenName);
                
                // Добавляем в историю
                if (addToHistory && !string.IsNullOrEmpty(_currentScreen))
                {
                    _navigationHistory.Push(_currentScreen);
                }
                
                // Загружаем сцену с параметрами
                await _sceneService.LoadSceneAsync(screenName, parameters);
                
                _currentScreen = screenName;
                
                OnNavigationCompleted?.Invoke(screenName);
                OnScreenChanged?.Invoke(previousScreen, screenName);
                
                GameLogger.LogInfo("UINavigationService", $"Navigation with parameters completed: {previousScreen} -> {screenName}");
            }
            catch (Exception ex)
            {
                GameLogger.LogException("UINavigationService", ex);
                _uiService?.ShowMessage($"Failed to navigate to {screenName}", 3f);
                throw;
            }
        }
        
        /// <summary>
        /// Возврат к предыдущему экрану из истории навигации
        /// </summary>
        public async UniTask NavigateBackAsync()
        {
            if (_navigationHistory.Count == 0)
            {
                GameLogger.LogWarning("UINavigationService", "Navigation history is empty, cannot go back");
                return;
            }
            
            string previousScreen = _navigationHistory.Pop();
            GameLogger.LogInfo("UINavigationService", $"Navigating back to {previousScreen}");
            
            // Не добавляем в историю при возврате назад
            await NavigateToAsync(previousScreen, addToHistory: false);
        }
        
        /// <summary>
        /// Очистка истории навигации
        /// </summary>
        public void ClearNavigationHistory()
        {
            int historyCount = _navigationHistory.Count;
            _navigationHistory.Clear();
            
            GameLogger.LogInfo("UINavigationService", $"Navigation history cleared ({historyCount} entries removed)");
        }
        
        /// <summary>
        /// Получение текущего экрана
        /// </summary>
        public string GetCurrentScreen()
        {
            return _currentScreen;
        }
        
        /// <summary>
        /// Проверка наличия истории навигации
        /// </summary>
        public bool CanNavigateBack()
        {
            return _navigationHistory.Count > 0;
        }
        
        /// <summary>
        /// Получение размера истории навигации
        /// </summary>
        public int GetNavigationHistoryCount()
        {
            return _navigationHistory.Count;
        }
        
        /// <summary>
        /// Быстрые методы навигации к основным экранам
        /// </summary>
        public async UniTask NavigateToMainMenuAsync()
        {
            await NavigateToAsync(SceneNames.MainMenu);
        }
        
        public async UniTask NavigateToGameplayAsync(int levelId)
        {
            var parameters = new MainMenuScreen.GameplayParameters { LevelId = levelId };
            await NavigateToAsync(SceneNames.Gameplay, parameters);
        }
        
        public async UniTask NavigateToVictoryAsync(int levelId, string[] completedWords, float completionTime)
        {
            var parameters = new VictoryScreen.VictoryParameters
            {
                LevelId = levelId,
                CompletedWords = completedWords,
                CompletionTime = completionTime
            };
            await NavigateToAsync(SceneNames.Victory, parameters);
        }
        
        /// <summary>
        /// Обработка начала загрузки сцены
        /// </summary>
        private void OnSceneLoadStarted(string sceneName)
        {
            GameLogger.LogInfo("UINavigationService", $"Scene load started: {sceneName}");
        }
        
        /// <summary>
        /// Обработка завершения загрузки сцены
        /// </summary>
        private void OnSceneLoadCompleted(string sceneName)
        {
            GameLogger.LogInfo("UINavigationService", $"Scene load completed: {sceneName}");
        }
    }
}