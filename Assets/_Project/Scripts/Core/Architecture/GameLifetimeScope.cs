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
    /// </summary>
    public class GameLifetimeScope : LifetimeScope
    {
        [Header("Debug Settings")]
        [SerializeField] private bool _enableDebugLogging = true;

        /// <summary>
        /// Настройка контейнера DI
        /// Регистрация всех сервисов как Singleton для переиспользования между сценами
        /// </summary>
        /// <param name="builder">Билдер контейнера VContainer</param>
        protected override void Configure(IContainerBuilder builder)
        {
            if (_enableDebugLogging)
            {
                GameLogger.LogInfo("GameLifetimeScope", "Starting DI container configuration...");
            }

            // Регистрация сервисов как Singleton
            // Порядок регистрации важен для зависимостей
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
        /// </summary>
        /// <param name="builder">Билдер контейнера</param>
        private void RegisterCoreServices(IContainerBuilder builder)
        {
            if (_enableDebugLogging)
            {
                GameLogger.LogInfo("GameLifetimeScope", "Registering core services...");
            }

            // Регистрация реальных сервисов этапа 2
            builder.Register<ISceneService, SceneService>(Lifetime.Singleton);
            builder.Register<UINavigationService>(Lifetime.Singleton);
            builder.Register<ILevelService, LevelService>(Lifetime.Singleton);
            builder.Register<IProgressService, ProgressService>(Lifetime.Singleton);

            // Регистрация оставшихся моков для этапа 4
            builder.Register<IUIService, MockUIService>(Lifetime.Singleton);

            if (_enableDebugLogging)
            {
                GameLogger.LogInfo("GameLifetimeScope", "Core services registered");
            }
        }

        /// <summary>
        /// Вызывается при создании контейнера
        /// </summary>
        protected override void Awake()
        {
            // Предотвращение уничтожения при смене сцены
            DontDestroyOnLoad(gameObject);

            GameLogger.LogInfo("GameLifetimeScope", "GameLifetimeScope created and marked as DontDestroyOnLoad");

            base.Awake();
        }

        /// <summary>
        /// Вызывается при уничтожении объекта
        /// </summary>
        protected override void OnDestroy()
        {
            GameLogger.LogInfo("GameLifetimeScope", "GameLifetimeScope destroyed");
            base.OnDestroy();
        }
    }
}