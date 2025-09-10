using UnityEngine;
using WordPuzzle.Core.Services;
using WordPuzzle.Core.Architecture;

namespace WordPuzzle.Core.Utils
{
    /// <summary>
    /// Статический локатор сервисов для доступа к основным сервисам игры
    /// Используется как fallback когда DI injection не работает
    /// </summary>
    public static class ServiceLocator
    {
        private static IUIService _uiService;
        private static IProgressService _progressService;
        private static ILevelService _levelService;
        private static ISceneService _sceneService;
        
        /// <summary>
        /// Инициализация сервис-локатора с основными сервисами
        /// Вызывается из GameBootstrap после инициализации всех сервисов
        /// </summary>
        public static void Initialize(
            IUIService uiService,
            IProgressService progressService,
            ILevelService levelService,
            ISceneService sceneService)
        {
            _uiService = uiService;
            _progressService = progressService;
            _levelService = levelService;
            _sceneService = sceneService;
            
            GameLogger.LogInfo("ServiceLocator", "ServiceLocator initialized with all services");
        }
        
        /// <summary>
        /// Получить UIService
        /// </summary>
        public static IUIService GetUIService()
        {
            if (_uiService != null) return _uiService;
            
            // Fallback: поиск в сцене
            var uiServiceComponent = Object.FindFirstObjectByType<UIService>();
            if (uiServiceComponent != null)
            {
                GameLogger.LogWarning("ServiceLocator", "Using fallback UIService from scene");
                return uiServiceComponent;
            }
            
            GameLogger.LogError("ServiceLocator", "UIService not found in ServiceLocator or scene");
            return null;
        }
        
        /// <summary>
        /// Получить ProgressService
        /// </summary>
        public static IProgressService GetProgressService()
        {
            if (_progressService != null) return _progressService;
            
            GameLogger.LogError("ServiceLocator", "ProgressService not found in ServiceLocator");
            return null;
        }
        
        /// <summary>
        /// Получить LevelService  
        /// </summary>
        public static ILevelService GetLevelService()
        {
            if (_levelService != null) return _levelService;
            
            GameLogger.LogError("ServiceLocator", "LevelService not found in ServiceLocator");
            return null;
        }
        
        /// <summary>
        /// Получить SceneService
        /// </summary>
        public static ISceneService GetSceneService()
        {
            if (_sceneService != null) return _sceneService;
            
            GameLogger.LogError("ServiceLocator", "SceneService not found in ServiceLocator");
            return null;
        }
        
        /// <summary>
        /// Очистка всех ссылок на сервисы
        /// </summary>
        public static void Clear()
        {
            _uiService = null;
            _progressService = null;
            _levelService = null;
            _sceneService = null;
            
            GameLogger.LogInfo("ServiceLocator", "ServiceLocator cleared");
        }
        
        /// <summary>
        /// Проверка доступности всех сервисов
        /// </summary>
        public static bool AreServicesReady()
        {
            return _uiService != null && 
                   _progressService != null && 
                   _levelService != null && 
                   _sceneService != null;
        }
        
        /// <summary>
        /// Получение информации о состоянии сервисов
        /// </summary>
        public static string GetServicesInfo()
        {
            return $"ServiceLocator Status:\n" +
                   $"- UIService: {(_uiService != null ? "Available" : "NULL")}\n" +
                   $"- ProgressService: {(_progressService != null ? "Available" : "NULL")}\n" +
                   $"- LevelService: {(_levelService != null ? "Available" : "NULL")}\n" +
                   $"- SceneService: {(_sceneService != null ? "Available" : "NULL")}";
        }
    }
}
