using UnityEngine;
using VContainer;
using VContainer.Unity;
using WordPuzzle.Core.Services;
using WordPuzzle.UI.Navigation;
using WordPuzzle.Data.Persistence;

namespace WordPuzzle.Core.Architecture
{
    /// <summary>
    /// Основной LifetimeScope приложения
    /// Регистрирует все глобальные сервисы и настраивает DI контейнер
    /// ИСПРАВЛЕНО: добавлен import для ProgressService
    /// </summary>
    public class GameLifetimeScope : LifetimeScope
    {
        [Header("Debug Settings")]
        [SerializeField] private bool _enableDebugLogging = true;

        /// <summary>
        /// Настройка контейнера DI
        /// Регистрация всех сервисов как Singleton для переиспользования между сценами
        /// </summary>
        protected override void Configure(IContainerBuilder builder)
        {
            if (_enableDebugLogging)
            {
                GameLogger.LogInfo("GameLifetimeScope", "Starting DI container configuration...");
            }

            // Регистрация сервисов как Singleton
            RegisterCoreServices(builder);

            // Регистрация Entry Point для автоматического запуска инициализации
            builder.RegisterEntryPoint<GameBootstrap>();

            if (_enableDebugLogging)
            {
                GameLogger.LogInfo("GameLifetimeScope", "DI container configuration completed");
            }
        }

        /// <summary>
        /// Регистрация основных сервисов приложения
        /// ИСПРАВЛЕНО: правильная регистрация ProgressService
        /// </summary>
        private void RegisterCoreServices(IContainerBuilder builder)
        {
            if (_enableDebugLogging)
            {
                GameLogger.LogInfo("GameLifetimeScope", "Registering core services...");
            }

            // Регистрация реальных сервисов
            builder.Register<ISceneService, SceneService>(Lifetime.Singleton);
            builder.Register<UINavigationService>(Lifetime.Singleton);
            builder.Register<ILevelService, LevelService>(Lifetime.Singleton);
            
            // ИСПРАВЛЕНО: правильная регистрация ProgressService из нужного namespace
            builder.Register<IProgressService, ProgressService>(Lifetime.Singleton);

            // Регистрация оставшихся моков
            builder.Register<IUIService, MockUIService>(Lifetime.Singleton);

            if (_enableDebugLogging)
            {
                GameLogger.LogInfo("GameLifetimeScope", "Core services registered");
            }
        }

        protected override void Awake()
        {
            DontDestroyOnLoad(gameObject);
            GameLogger.LogInfo("GameLifetimeScope", "GameLifetimeScope created and marked as DontDestroyOnLoad");
            
            // Добавляем обработчики событий приложения для автосохранения
            SetupApplicationEventHandlers();
            
            base.Awake();
        }
        
        /// <summary>
        /// Настройка обработчиков событий приложения для автосохранения
        /// </summary>
        private void SetupApplicationEventHandlers()
        {
            // Сохраняем при потере фокуса (пользователь свернул приложение или переключился на другое)
            Application.focusChanged += OnApplicationFocusChanged;
        }
        
        /// <summary>
        /// Обработка изменения фокуса приложения (через новый API)
        /// </summary>
        private void OnApplicationFocusChanged(bool hasFocus)
        {
            if (_enableDebugLogging)
            {
                GameLogger.LogInfo("GameLifetimeScope", $"Application focus changed: {hasFocus}");
            }
            
            if (!hasFocus)
            {
                // Приложение потеряло фокус - сохраняем прогресс
                TrySaveProgress("focus lost");
            }
        }
        
        /// <summary>
        /// Обработка фокуса приложения (старый Unity callback)
        /// </summary>
        private void OnApplicationFocus(bool hasFocus)
        {
            if (_enableDebugLogging)
            {
                GameLogger.LogInfo("GameLifetimeScope", $"OnApplicationFocus: {hasFocus}");
            }
            
            if (!hasFocus)
            {
                // Приложение потеряло фокус - сохраняем прогресс
                TrySaveProgress("application focus lost");
            }
        }
        
        /// <summary>
        /// Обработка паузы приложения (мобильные платформы)
        /// </summary>
        private void OnApplicationPause(bool pauseStatus)
        {
            if (_enableDebugLogging)
            {
                GameLogger.LogInfo("GameLifetimeScope", $"OnApplicationPause: {pauseStatus}");
            }
            
            if (pauseStatus)
            {
                // Приложение поставлено на паузу - сохраняем прогресс
                TrySaveProgress("application paused");
            }
        }
        
        
        /// <summary>
        /// Попытка сохранить прогресс с обработкой ошибок
        /// </summary>
        private void TrySaveProgress(string reason)
        {
            try
            {
                // Получаем ProgressService из контейнера, если он доступен
                if (Container != null)
                {
                    var progressService = Container.Resolve<IProgressService>();
                    if (progressService?.IsInitialized == true)
                    {
                        // Вызываем принудительное сохранение
                        var progressServiceImpl = progressService as ProgressService;
                        if (progressServiceImpl != null)
                        {
                            progressServiceImpl.ForceSaveProgress();
                            
                            if (_enableDebugLogging)
                            {
                                GameLogger.LogInfo("GameLifetimeScope", $"Progress saved due to: {reason}");
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                // Это нормально - может случиться если контейнер не инициализирован или сервис не зарегистрирован
                if (_enableDebugLogging)
                {
                    GameLogger.LogInfo("GameLifetimeScope", $"Could not save progress due to: {reason} - {ex.Message}");
                }
            }
        }

        protected override void OnDestroy()
        {
            GameLogger.LogInfo("GameLifetimeScope", "GameLifetimeScope destroyed");
            
            // Отписываемся от событий приложения
            Application.focusChanged -= OnApplicationFocusChanged;
            
            // Финальное сохранение при закрытии
            TrySaveProgress("application shutdown");
            
            base.OnDestroy();
        }
    }
}
