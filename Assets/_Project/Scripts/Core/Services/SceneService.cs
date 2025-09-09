using System;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using WordPuzzle.Core.Architecture;

namespace WordPuzzle.Core.Services
{
    /// <summary>
    /// Реализация сервиса управления сценами
    /// Обеспечивает асинхронную загрузку сцен Unity с отслеживанием прогресса
    /// </summary>
    public class SceneService : ISceneService
    {
        public bool IsInitialized { get; private set; }
        
        public event Action<string> OnSceneLoadStarted;
        public event Action<string> OnSceneLoadCompleted;
        
        private readonly IUIService _uiService;
        private string _currentSceneName;
        private object _currentSceneParameters;
        
        /// <summary>
        /// Конструктор с инжекцией UI сервиса для экрана загрузки
        /// </summary>
        public SceneService(IUIService uiService)
        {
            _uiService = uiService;
        }
        
        /// <summary>
        /// Инициализация сервиса сцен
        /// </summary>
        public async UniTask InitializeAsync()
        {
            GameLogger.LogInfo("SceneService", "Initializing Scene Service...");
            
            // Получаем текущую активную сцену
            _currentSceneName = SceneManager.GetActiveScene().name;
            _currentSceneParameters = null;
            
            // Подписываемся на события Unity SceneManager
            SceneManager.sceneLoaded += OnUnitySceneLoaded;
            SceneManager.sceneUnloaded += OnUnitySceneUnloaded;
            
            await UniTask.Yield(); // Имитация асинхронной инициализации
            
            IsInitialized = true;
            GameLogger.LogInfo("SceneService", $"Scene Service initialized. Current scene: {_currentSceneName}");
        }
        
        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        public void Dispose()
        {
            if (IsInitialized == false) return;
            
            // Отписываемся от событий
            SceneManager.sceneLoaded -= OnUnitySceneLoaded;
            SceneManager.sceneUnloaded -= OnUnitySceneUnloaded;
            
            OnSceneLoadStarted = null;
            OnSceneLoadCompleted = null;
            
            IsInitialized = false;
            GameLogger.LogInfo("SceneService", "Scene Service disposed");
        }
        
        /// <summary>
        /// Асинхронная загрузка сцены
        /// </summary>
        public async UniTask LoadSceneAsync(string sceneName, bool showLoadingScreen = true)
        {
            if (IsInitialized == false)
            {
                GameLogger.LogError("SceneService", "SceneService is not initialized");
                return;
            }
            
            if (string.IsNullOrEmpty(sceneName))
            {
                GameLogger.LogError("SceneService", "Scene name cannot be null or empty");
                return;
            }
            
            GameLogger.LogInfo("SceneService", $"Loading scene: {sceneName}");
            OnSceneLoadStarted?.Invoke(sceneName);
            
            try
            {
                // Показываем экран загрузки если необходимо
                if (showLoadingScreen)
                {
                    _uiService?.ShowLoadingScreen($"Loading {sceneName}...");
                }
                
                // Асинхронная загрузка сцены
                var asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
                
                if (asyncOperation == null)
                {
                    throw new InvalidOperationException($"Failed to start loading scene: {sceneName}");
                }
                
                // Не активируем сцену сразу, чтобы контролировать процесс
                asyncOperation.allowSceneActivation = false;
                
                // Ждем загрузки до 90%
                while (asyncOperation.progress < 0.9f)
                {
                    GameLogger.LogInfo("SceneService", $"Loading progress: {asyncOperation.progress:P0}");
                    await UniTask.Yield();
                }
                
                // Активируем сцену
                asyncOperation.allowSceneActivation = true;
                
                // Ждем полного завершения
                await asyncOperation.ToUniTask();
                
                // Скрываем экран загрузки
                if (showLoadingScreen)
                {
                    _uiService?.HideLoadingScreen();
                }
                
                _currentSceneName = sceneName;
                // ИСПРАВЛЕНО: НЕ сбрасываем параметры если они уже установлены через LoadSceneAsync(sceneName, parameters)
                // _currentSceneParameters = null; // УБРАНО!
                
                GameLogger.LogInfo("SceneService", $"Scene {sceneName} loaded successfully");
                OnSceneLoadCompleted?.Invoke(sceneName);
            }
            catch (Exception ex)
            {
                GameLogger.LogException("SceneService", ex);
                
                if (showLoadingScreen)
                {
                    _uiService?.HideLoadingScreen();
                }
                
                _uiService?.ShowMessage($"Failed to load scene: {sceneName}", 5f);
                throw;
            }
        }
        
        /// <summary>
        /// Загрузка сцены с параметрами
        /// </summary>
        public async UniTask LoadSceneAsync(string sceneName, object sceneParameters)
        {
            GameLogger.LogInfo("SceneService", $"Loading scene {sceneName} with parameters: {sceneParameters?.GetType().Name ?? "null"}");
            
            // ИСПРАВЛЕНО: Сохраняем параметры перед загрузкой
            _currentSceneParameters = sceneParameters;
            
            // ИСПРАВЛЕНО: После установки параметров загружаем сцену
            await LoadSceneAsync(sceneName, showLoadingScreen: true);
            
            // Логируем для отладки
            GameLogger.LogInfo("SceneService", $"Scene {sceneName} loaded with parameters. Current parameters: {_currentSceneParameters?.GetType().Name ?? "null"}");
        }
        
        /// <summary>
        /// Получение имени текущей сцены
        /// </summary>
        public string GetCurrentSceneName()
        {
            return _currentSceneName ?? SceneManager.GetActiveScene().name;
        }
        
        /// <summary>
        /// Получение параметров текущей сцены
        /// </summary>
        public T GetSceneParameters<T>() where T : class
        {
            var result = _currentSceneParameters as T;
            
            // Логируем для отладки
            if (result != null)
            {
                GameLogger.LogInfo("SceneService", $"Retrieved scene parameters: {typeof(T).Name}");
            }
            else
            {
                GameLogger.LogWarning("SceneService", $"Failed to retrieve scene parameters of type {typeof(T).Name}. Current parameters: {_currentSceneParameters?.GetType().Name ?? "null"}");
            }
            
            return result;
        }
        
        /// <summary>
        /// Очистка параметров сцены (вызывается при переходе без параметров)
        /// </summary>
        public void ClearSceneParameters()
        {
            GameLogger.LogInfo("SceneService", "Clearing scene parameters");
            _currentSceneParameters = null;
        }
        
        /// <summary>
        /// Обработчик события загрузки сцены от Unity
        /// </summary>
        private void OnUnitySceneLoaded(Scene scene, LoadSceneMode mode)
        {
            GameLogger.LogInfo("SceneService", $"Unity scene loaded: {scene.name} (mode: {mode})");
        }
        
        /// <summary>
        /// Обработчик события выгрузки сцены от Unity
        /// </summary>
        private void OnUnitySceneUnloaded(Scene scene)
        {
            GameLogger.LogInfo("SceneService", $"Unity scene unloaded: {scene.name}");
        }
    }
}