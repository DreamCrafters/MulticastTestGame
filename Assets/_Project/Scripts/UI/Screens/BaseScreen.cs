using UnityEngine;
using VContainer;
using WordPuzzle.Core.Architecture;
using WordPuzzle.Core.Services;

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
        /// Переопределяемый метод инициализации для наследников
        /// </summary>
        protected virtual void OnInitialize()
        {
            // Переопределяется в наследниках
        }
        
        /// <summary>
        /// Переопределяемый метод очистки для наследников
        /// </summary>
        protected virtual void OnCleanup()
        {
            // Переопределяется в наследниках
        }
        
        /// <summary>
        /// Подписка на события UI
        /// Переопределяется в наследниках
        /// </summary>
        protected virtual void SubscribeToUIEvents()
        {
            // Переопределяется в наследниках
        }
        
        /// <summary>
        /// Отписка от событий UI
        /// Переопределяется в наследниках
        /// </summary>
        protected virtual void UnsubscribeFromUIEvents()
        {
            // Переопределяется в наследниках
        }
        
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