using UnityEngine;
using VContainer;
using WordPuzzle.Core.Architecture;
using WordPuzzle.Core.Services;
using WordPuzzle.Core.Utils;

namespace WordPuzzle.UI.Screens
{
    /// <summary>
    /// Базовый класс для всех экранов игры
    /// Обеспечивает единообразную инициализацию и доступ к сервисам
    /// </summary>
    public abstract class BaseScreen : MonoBehaviour
    {
        [Header("Screen Settings")]
        [SerializeField] private bool _initializeOnStart = true;
        [SerializeField] protected bool enableBackButton = false;
        
        // Инжекция сервисов через VContainer
        [Inject] protected ISceneService SceneService { get; private set; }
        [Inject] protected IUIService UIService { get; private set; }
        [Inject] protected IProgressService ProgressService { get; private set; }
        [Inject] protected ILevelService LevelService { get; private set; }
        
        // Резервный доступ к сервисам для случаев, когда инжекция не сработала
        protected IUIService GetUIService()
        {
            // Сначала пробуем инжектированный сервис
            if (UIService != null) return UIService;
            
            // Затем пробуем ServiceLocator
            var serviceLocatorUI = ServiceLocator.GetUIService();
            if (serviceLocatorUI != null) 
            {
                GameLogger.LogWarning(ScreenName, "Using UIService from ServiceLocator");
                return serviceLocatorUI;
            }
            
            GameLogger.LogError(ScreenName, "UIService not available through DI or ServiceLocator");
            return null;
        }
        
        protected IProgressService GetProgressService()
        {
            // Сначала пробуем инжектированный сервис
            if (ProgressService != null) return ProgressService;
            
            // Затем пробуем ServiceLocator
            var serviceLocatorProgress = ServiceLocator.GetProgressService();
            if (serviceLocatorProgress != null)
            {
                GameLogger.LogWarning(ScreenName, "Using ProgressService from ServiceLocator");
                return serviceLocatorProgress;
            }
            
            GameLogger.LogError(ScreenName, "ProgressService not available through DI or ServiceLocator");
            return null;
        }
        
        protected ILevelService GetLevelService()
        {
            // Сначала пробуем инжектированный сервис
            if (LevelService != null) return LevelService;
            
            // Затем пробуем ServiceLocator
            var serviceLocatorLevel = ServiceLocator.GetLevelService();
            if (serviceLocatorLevel != null)
            {
                GameLogger.LogWarning(ScreenName, "Using LevelService from ServiceLocator");
                return serviceLocatorLevel;
            }
            
            GameLogger.LogError(ScreenName, "LevelService not available through DI or ServiceLocator");
            return null;
        }
        
        protected ISceneService GetSceneService()
        {
            // Сначала пробуем инжектированный сервис
            if (SceneService != null) return SceneService;
            
            // Затем пробуем ServiceLocator
            var serviceLocatorScene = ServiceLocator.GetSceneService();
            if (serviceLocatorScene != null)
            {
                GameLogger.LogWarning(ScreenName, "Using SceneService from ServiceLocator");
                return serviceLocatorScene;
            }
            
            GameLogger.LogError(ScreenName, "SceneService not available through DI or ServiceLocator");
            return null;
        }
        
        /// <summary>
        /// Название экрана для логирования
        /// </summary>
        protected abstract string ScreenName { get; }
        
        /// <summary>
        /// Инициализирован ли экран
        /// </summary>
        public bool IsInitialized { get; private set; }
        
        /// <summary>
        /// Unity Start - автоматическая инициализация
        /// </summary>
        private void Start()
        {
            if (_initializeOnStart)
            {
                InitializeScreen();
            }
        }
        
        /// <summary>
        /// Инициализация экрана
        /// </summary>
        public void InitializeScreen()
        {
            if (IsInitialized)
            {
                GameLogger.LogWarning(ScreenName, "Screen already initialized");
                return;
            }
            
            GameLogger.LogInfo(ScreenName, $"Initializing {ScreenName} screen...");
            
            try
            {
                // Настройка кнопки "Назад" для Android
                SetupBackButton();
                
                // Вызов переопределяемого метода инициализации
                OnInitialize();
                
                // Подписка на события UI
                SubscribeToUIEvents();
                
                IsInitialized = true;
                GameLogger.LogInfo(ScreenName, $"{ScreenName} screen initialized successfully");
                
                // Уведомляем UI Service об открытии экрана
                UIService?.NotifyScreenOpened(ScreenName);
            }
            catch (System.Exception ex)
            {
                GameLogger.LogException(ScreenName, ex);
                throw;
            }
        }
        
        /// <summary>
        /// Очистка ресурсов при уничтожении
        /// </summary>
        private void OnDestroy()
        {
            if (IsInitialized)
            {
                GameLogger.LogInfo(ScreenName, $"Destroying {ScreenName} screen...");
                
                // Отписка от событий
                UnsubscribeFromUIEvents();
                
                // Переопределяемая очистка
                OnCleanup();
                
                // Уведомляем UI Service о закрытии экрана
                UIService?.NotifyScreenClosed(ScreenName);
                
                IsInitialized = false;
            }
        }
        
        /// <summary>
        /// Настройка кнопки "Назад" для мобильных платформ
        /// </summary>
        private void SetupBackButton()
        {
            if (enableBackButton)
            {
                UIService?.SetBackButtonActive(true, OnBackButtonPressed);
            }
            else
            {
                UIService?.SetBackButtonActive(false);
            }
        }
        
        /// <summary>
        /// Обработка нажатия кнопки "Назад"
        /// Переопределяется в наследниках для кастомной логики
        /// </summary>
        protected virtual void OnBackButtonPressed()
        {
            GameLogger.LogInfo(ScreenName, "Back button pressed - default implementation");
            UIService?.PlayUISound(UISoundType.ButtonClick);
        }
        
        /// <summary>
        /// Переопределяемый метод очистки для наследников
        /// </summary>
        protected virtual void OnCleanup()
        {
            // Пустая реализация по умолчанию
        }

        /// <summary>
        /// Переопределяемый метод инициализации для наследников
        /// </summary>
        protected abstract void OnInitialize();

        /// <summary>
        /// Подписка на события UI
        /// Переопределяется в наследниках
        /// </summary>
        protected abstract void SubscribeToUIEvents();

        /// <summary>
        /// Отписка от событий UI
        /// Переопределяется в наследниках
        /// </summary>
        protected abstract void UnsubscribeFromUIEvents();
        
        /// <summary>
        /// Безопасная загрузка сцены с обработкой ошибок
        /// </summary>
        /// <param name="sceneName">Имя сцены</param>
        protected async void LoadSceneSafe(string sceneName)
        {
            try
            {
                UIService?.PlayUISound(UISoundType.ButtonClick);
                GameLogger.LogInfo(ScreenName, $"Loading scene: {sceneName}");
                SceneService.ClearSceneParameters();
                await SceneService.LoadSceneAsync(sceneName);
            }
            catch (System.Exception ex)
            {
                GameLogger.LogException(ScreenName, ex);
                UIService?.PlayUISound(UISoundType.Error);
                UIService?.ShowMessage($"Failed to load {sceneName}. Please try again.", 3f);
            }
        }
        
        /// <summary>
        /// Загрузка сцены с параметрами
        /// </summary>
        /// <param name="sceneName">Имя сцены</param>
        /// <param name="parameters">Параметры для передачи</param>
        protected async void LoadSceneWithParametersSafe(string sceneName, object parameters)
        {
            try
            {
                UIService?.PlayUISound(UISoundType.ButtonClick);
                GameLogger.LogInfo(ScreenName, $"Loading scene: {sceneName} with parameters");
                
                // ИСПРАВЛЕНО: Параметры будут установлены в SceneService.LoadSceneAsync(sceneName, parameters)
                await SceneService.LoadSceneAsync(sceneName, parameters);
            }
            catch (System.Exception ex)
            {
                GameLogger.LogException(ScreenName, ex);
                UIService?.PlayUISound(UISoundType.Error);
                UIService?.ShowMessage($"Failed to load {sceneName}. Please try again.", 3f);
            }
        }
    }
}