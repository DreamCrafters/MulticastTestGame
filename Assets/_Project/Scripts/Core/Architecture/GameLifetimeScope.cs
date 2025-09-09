using UnityEngine;
using VContainer;
using VContainer.Unity;
using WordPuzzle.Core.Services;

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
            RegisterMockServices(builder);

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
            // На данном этапе регистрируем только моки
            // Реальные реализации будут добавлены на следующих этапах
            
            if (_enableDebugLogging)
            {
                GameLogger.LogInfo("GameLifetimeScope", "Registering core services...");
            }
        }
        
        /// <summary>
        /// Регистрация временных моков сервисов для разработки
        /// </summary>
        /// <param name="builder">Билдер контейнера</param>
        private void RegisterMockServices(IContainerBuilder builder)
        {
            // Регистрация моков как Singleton
            builder.Register<ILevelService, MockLevelService>(Lifetime.Singleton);
            builder.Register<IProgressService, MockProgressService>(Lifetime.Singleton);
            builder.Register<ISceneService, MockSceneService>(Lifetime.Singleton);
            builder.Register<IUIService, MockUIService>(Lifetime.Singleton);

            if (_enableDebugLogging)
            {
                GameLogger.LogInfo("GameLifetimeScope", "Mock services registered");
            }
        }
    }
}