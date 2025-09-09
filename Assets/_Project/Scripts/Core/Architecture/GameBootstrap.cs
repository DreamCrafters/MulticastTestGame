using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using VContainer.Unity;
using WordPuzzle.Core.Services;
using WordPuzzle.UI.Navigation;

namespace WordPuzzle.Core.Architecture
{
    /// <summary>
    /// Entry Point приложения для инициализации всех сервисов
    /// Автоматически запускается VContainer после создания контейнера
    /// </summary>
    public class GameBootstrap : IStartable, IDisposable
    {
        private readonly ILevelService _levelService;
        private readonly IProgressService _progressService;
        private readonly ISceneService _sceneService;
        private readonly IUIService _uiService;
        private readonly UINavigationService _navigationService;
        
        private readonly List<IGameService> _allServices;
        private bool _isInitialized = false;
        
        /// <summary>
        /// Конструктор с инжекцией зависимостей
        /// VContainer автоматически передает зарегистрированные сервисы
        /// </summary>
        public GameBootstrap(
            ILevelService levelService,
            IProgressService progressService,
            ISceneService sceneService,
            IUIService uiService,
            UINavigationService navigationService)
        {
            _levelService = levelService;
            _progressService = progressService;
            _sceneService = sceneService;
            _uiService = uiService;
            _navigationService = navigationService;
            
            // Сохраняем ссылки на все сервисы для групповых операций
            _allServices = new List<IGameService>
            {
                _levelService,
                _progressService,
                _sceneService,
                _uiService,
                _navigationService
            };
            
            GameLogger.LogInfo("GameBootstrap", "GameBootstrap constructor completed");
        }
        
        /// <summary>
        /// Запуск инициализации приложения
        /// Вызывается VContainer автоматически
        /// </summary>
        public async void Start()
        {
            GameLogger.LogInfo("GameBootstrap", "=== GAME INITIALIZATION STARTED ===");
            
            try
            {
                await InitializeAllServicesAsync();
                await LoadInitialSceneAsync();
                
                _isInitialized = true;
                GameLogger.LogInfo("GameBootstrap", "=== GAME INITIALIZATION COMPLETED SUCCESSFULLY ===");
            }
            catch (Exception exception)
            {
                GameLogger.LogException("GameBootstrap", exception);
                GameLogger.LogError("GameBootstrap", "=== GAME INITIALIZATION FAILED ===");
                
                // В продакшене здесь можно показать экран ошибки
                HandleInitializationError(exception);
            }
        }
        
        /// <summary>
        /// Последовательная инициализация всех сервисов
        /// </summary>
        private async UniTask InitializeAllServicesAsync()
        {
            GameLogger.LogInfo("GameBootstrap", $"Initializing {_allServices.Count} services...");
            
            foreach (var service in _allServices)
            {
                string serviceName = service.GetType().Name;
                
                try
                {
                    GameLogger.LogInfo("GameBootstrap", $"Initializing {serviceName}...");
                    await service.InitializeAsync();
                    
                    if (service.IsInitialized)
                    {
                        GameLogger.LogServiceInitialization(serviceName, true);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Service {serviceName} reports as not initialized");
                    }
                }
                catch (Exception ex)
                {
                    GameLogger.LogServiceInitialization(serviceName, false);
                    GameLogger.LogException("GameBootstrap", ex);
                    throw; // Прерываем инициализацию при ошибке любого сервиса
                }
            }
        }
        
        /// <summary>
        /// Загрузка начальной сцены после инициализации
        /// </summary>
        private async UniTask LoadInitialSceneAsync()
        {
            GameLogger.LogInfo("GameBootstrap", "Loading initial scene...");
            
            // Загружаем главное меню как стартовую сцену
            await _sceneService.LoadSceneAsync(SceneNames.MainMenu, showLoadingScreen: false);
            
            GameLogger.LogInfo("GameBootstrap", "Initial scene loaded successfully");
        }
        
        /// <summary>
        /// Обработка ошибки инициализации
        /// </summary>
        /// <param name="exception">Исключение, возникшее при инициализации</param>
        private void HandleInitializationError(Exception exception)
        {
            // В реальном проекте здесь может быть:
            // - Показ экрана ошибки
            // - Отправка ошибки в аналитику
            // - Попытка восстановления
            
            GameLogger.LogError("GameBootstrap", "Application will not function properly due to initialization failure");
        }
        
        /// <summary>
        /// Корректное завершение работы сервисов
        /// </summary>
        public void Dispose()
        {
            if (_isInitialized == false) return;
            
            GameLogger.LogInfo("GameBootstrap", "=== DISPOSING GAME SERVICES ===");
            
            // Освобождаем ресурсы всех сервисов в обратном порядке
            for (int i = _allServices.Count - 1; i >= 0; i--)
            {
                try
                {
                    var service = _allServices[i];
                    string serviceName = service.GetType().Name;
                    
                    GameLogger.LogInfo("GameBootstrap", $"Disposing {serviceName}...");
                    service.Dispose();
                }
                catch (Exception ex)
                {
                    GameLogger.LogException("GameBootstrap", ex);
                }
            }
            
            _allServices.Clear();
            _isInitialized = false;
            
            GameLogger.LogInfo("GameBootstrap", "=== GAME SERVICES DISPOSED ===");
        }
    }
}