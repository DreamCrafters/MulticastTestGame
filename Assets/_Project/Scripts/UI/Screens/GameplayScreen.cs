using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WordPuzzle.Core.Architecture;
using WordPuzzle.Core.Services;
using WordPuzzle.Gameplay.Word;
using WordPuzzle.Gameplay.Cluster;
using WordPuzzle.Data.Models;

namespace WordPuzzle.UI.Screens
{
    /// <summary>
    /// Экран игрового процесса
    /// </summary>
    public class GameplayScreen : BaseScreen
    {
        [Header("Navigation UI")]
        [SerializeField] private Button _backToMenuButton;
        [SerializeField] private Button _mockWinButton;
        [SerializeField] private TextMeshProUGUI _levelInfoText;
        
        [Header("Gameplay Components")]
        [SerializeField] private GameField _gameField;
        [SerializeField] private ClusterPanel _clusterPanel;
        [SerializeField] private ClusterManager _clusterManager;
        
        [Header("Layout Components")]
        [SerializeField] private RectTransform _gameFieldContainer;
        [SerializeField] private RectTransform _clusterPanelContainer;
        
        [Header("Settings")]
        [SerializeField] private bool _autoLoadGameField = true;
        [SerializeField] private bool _enableClusterManagerEvents = true;

        private int _currentLevelId;
        private LevelData _currentLevelData;

        protected override string ScreenName => "Gameplay";

        protected override void OnInitialize()
        {
            GameLogger.LogInfo(ScreenName, "Setting up Gameplay screen with enhanced cluster system...");

            LoadLevelParameters();
            SetupNavigation();
            SetupGameplayComponents();
            
            enableBackButton = true;

            if (_autoLoadGameField)
            {
                LoadLevelDataAsync();
            }

            GameLogger.LogInfo(ScreenName, "Gameplay screen setup completed with ClusterManager");
        }

        protected override void SubscribeToUIEvents()
        {
            // Navigation buttons
            if (_backToMenuButton != null)
            {
                _backToMenuButton.onClick.AddListener(OnBackToMenuClicked);
            }

            if (_mockWinButton != null)
            {
                _mockWinButton.onClick.AddListener(OnMockWinClicked);
            }

            if (_enableClusterManagerEvents && _clusterManager != null)
            {
                SubscribeToClusterManagerEvents();
            }
        }

        protected override void UnsubscribeFromUIEvents()
        {
            // Navigation buttons
            if (_backToMenuButton != null)
            {
                _backToMenuButton.onClick.RemoveListener(OnBackToMenuClicked);
            }

            if (_mockWinButton != null)
            {
                _mockWinButton.onClick.RemoveListener(OnMockWinClicked);
            }

            // НОВОЕ: Отписка от событий ClusterManager
            if (_clusterManager != null)
            {
                UnsubscribeFromClusterManagerEvents();
            }
        }

        /// <summary>
        /// Загрузка параметров уровня
        /// </summary>
        private void LoadLevelParameters()
        {
            try
            {
                GameLogger.LogInfo(ScreenName, "Attempting to load level parameters...");
                
                var parameters = SceneService.GetSceneParameters<MainMenuScreen.GameplayParameters>();

                if (parameters != null)
                {
                    _currentLevelId = parameters.LevelId;
                    GameLogger.LogInfo(ScreenName, $"Successfully loaded level parameters: Level {_currentLevelId}");
                }
                else
                {
                    _currentLevelId = 1;
                    GameLogger.LogWarning(ScreenName, "No level parameters found, using default level 1");
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogException(ScreenName, ex);
                _currentLevelId = 1;
                GameLogger.LogWarning(ScreenName, "Exception occurred while loading parameters, using default level 1");
            }
        }

        /// <summary>
        /// Настройка навигационных элементов
        /// </summary>
        private void SetupNavigation()
        {
            // Level info
            if (_levelInfoText != null)
            {
                _levelInfoText.text = $"Level {_currentLevelId}";
            }

            // Navigation buttons
            if (_backToMenuButton != null)
            {
                var buttonText = _backToMenuButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = "Back to Menu";
                }
            }

            if (_mockWinButton != null)
            {
                var buttonText = _mockWinButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = "Mock Win (Test Victory)";
                }
            }
        }
        
        /// <summary>
        /// ОБНОВЛЕНО: Настройка компонентов игрового поля с ClusterManager
        /// </summary>
        private void SetupGameplayComponents()
        {
            // Проверяем и создаем GameField
            if (_gameField == null)
            {
                GameLogger.LogInfo(ScreenName, "GameField not assigned, looking for existing component...");
                _gameField = FindFirstObjectByType<GameField>();
                
                if (_gameField == null && _gameFieldContainer != null)
                {
                    GameLogger.LogInfo(ScreenName, "Creating GameField dynamically...");
                    var gameFieldObject = new GameObject("GameField");
                    gameFieldObject.transform.SetParent(_gameFieldContainer, false);
                    _gameField = gameFieldObject.AddComponent<GameField>();
                }
            }
            
            // Проверяем и создаем ClusterPanel
            if (_clusterPanel == null)
            {
                GameLogger.LogInfo(ScreenName, "ClusterPanel not assigned, looking for existing component...");
                _clusterPanel = FindFirstObjectByType<ClusterPanel>();
                
                if (_clusterPanel == null && _clusterPanelContainer != null)
                {
                    GameLogger.LogInfo(ScreenName, "Creating ClusterPanel dynamically...");
                    var clusterPanelObject = new GameObject("ClusterPanel");
                    clusterPanelObject.transform.SetParent(_clusterPanelContainer, false);
                    
                    // Настраиваем как полноразмерный элемент
                    var clusterPanelRect = clusterPanelObject.AddComponent<RectTransform>();
                    clusterPanelRect.anchorMin = Vector2.zero;
                    clusterPanelRect.anchorMax = Vector2.one;
                    clusterPanelRect.offsetMin = Vector2.zero;
                    clusterPanelRect.offsetMax = Vector2.zero;
                    
                    _clusterPanel = clusterPanelObject.AddComponent<ClusterPanel>();
                }
            }

            // НОВОЕ: Проверяем и создаем ClusterManager
            if (_clusterManager == null)
            {
                GameLogger.LogInfo(ScreenName, "ClusterManager not assigned, looking for existing component...");
                _clusterManager = FindFirstObjectByType<ClusterManager>();
                
                if (_clusterManager == null)
                {
                    GameLogger.LogInfo(ScreenName, "Creating ClusterManager dynamically...");
                    var clusterManagerObject = new GameObject("ClusterManager");
                    clusterManagerObject.transform.SetParent(transform, false);
                    _clusterManager = clusterManagerObject.AddComponent<ClusterManager>();
                    
                    // Связываем с ClusterPanel
                    if (_clusterPanel != null)
                    {
                        // ClusterManager автоматически найдет ClusterPanel при инициализации
                        GameLogger.LogInfo(ScreenName, "ClusterManager will auto-link with ClusterPanel");
                    }
                }
            }
            
            // Логирование состояния компонентов
            GameLogger.LogInfo(ScreenName, $"Gameplay components status: GameField={_gameField != null}, ClusterPanel={_clusterPanel != null}, ClusterManager={_clusterManager != null}");
            
            if (_gameField != null)
            {
                GameLogger.LogInfo(ScreenName, $"GameField initialized: {_gameField.IsInitialized}");
            }
            
            if (_clusterPanel != null)
            {
                GameLogger.LogInfo(ScreenName, $"ClusterPanel initialized: {_clusterPanel.IsInitialized}");
            }

            if (_clusterManager != null)
            {
                GameLogger.LogInfo(ScreenName, $"ClusterManager initialized: {_clusterManager.IsInitialized}");
            }
        }

        /// <summary>
        /// НОВОЕ: Подписка на события ClusterManager
        /// </summary>
        private void SubscribeToClusterManagerEvents()
        {
            _clusterManager.OnClusterCreated += OnClusterCreated;
            _clusterManager.OnClusterPlaced += OnClusterPlaced;
            _clusterManager.OnClusterRemoved += OnClusterRemoved;
            _clusterManager.OnClusterSelected += OnClusterSelected;
            _clusterManager.OnAllClustersValidated += OnAllClustersValidated;

            GameLogger.LogInfo(ScreenName, "Subscribed to ClusterManager events");
        }

        /// <summary>
        /// НОВОЕ: Отписка от событий ClusterManager
        /// </summary>
        private void UnsubscribeFromClusterManagerEvents()
        {
            _clusterManager.OnClusterCreated -= OnClusterCreated;
            _clusterManager.OnClusterPlaced -= OnClusterPlaced;
            _clusterManager.OnClusterRemoved -= OnClusterRemoved;
            _clusterManager.OnClusterSelected -= OnClusterSelected;
            _clusterManager.OnAllClustersValidated -= OnAllClustersValidated;

            GameLogger.LogInfo(ScreenName, "Unsubscribed from ClusterManager events");
        }
        
        /// <summary>
        /// ОБНОВЛЕНО: Загрузка данных уровня через ClusterManager
        /// </summary>
        private async void LoadLevelDataAsync()
        {
            try
            {
                GameLogger.LogInfo(ScreenName, $"Loading level {_currentLevelId} data for enhanced cluster system...");

                var levelData = await LevelService.LoadLevelAsync(_currentLevelId);

                if (levelData != null)
                {
                    _currentLevelData = levelData;
                    
                    GameLogger.LogInfo(ScreenName, $"Level data loaded successfully: {levelData.TargetWords.Length} words, {levelData.AvailableClusters.Length} clusters");

                    // Выводим информацию о загруженном уровне
                    GameLogger.LogInfo(ScreenName, "Target words:");
                    foreach (var wordData in levelData.TargetWords)
                    {
                        GameLogger.LogInfo(ScreenName, $"  - {wordData.Word} = [{string.Join(", ", wordData.Clusters)}]");
                    }

                    GameLogger.LogInfo(ScreenName, $"Available clusters: [{string.Join(", ", levelData.AvailableClusters)}]");

                    // ОБНОВЛЕНО: Используем ClusterManager для настройки
                    SetupGameFieldWithLevelData(levelData);
                    
                    // Обновляем UI с реальными данными
                    UpdateUIWithLevelData(levelData);
                }
                else
                {
                    GameLogger.LogError(ScreenName, $"Failed to load level {_currentLevelId} data");
                    UIService.ShowMessage("Failed to load level. Returning to menu.", 3f);
                    LoadSceneSafe(SceneNames.MainMenu);
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogException(ScreenName, ex);
                UIService.ShowMessage("Error loading level data.", 3f);
            }
        }
        
        /// <summary>
        /// ОБНОВЛЕНО: Настройка игрового поля через ClusterManager
        /// </summary>
        private void SetupGameFieldWithLevelData(LevelData levelData)
        {
            // Настройка игрового поля (очищаем и готовим к игре)
            if (_gameField != null)
            {
                _gameField.LoadLevelData(levelData);
                GameLogger.LogInfo(ScreenName, "GameField setup with level data");
            }
            else
            {
                GameLogger.LogWarning(ScreenName, "GameField is null, cannot setup with level data");
            }
            
            // ОБНОВЛЕНО: Используем ClusterManager для загрузки кластеров
            if (_clusterManager != null)
            {
                _clusterManager.LoadLevel(levelData);
                GameLogger.LogInfo(ScreenName, "ClusterManager loaded level data and created clusters");
            }
            else
            {
                GameLogger.LogWarning(ScreenName, "ClusterManager is null, falling back to ClusterPanel");
                
                // Fallback: используем ClusterPanel напрямую
                if (_clusterPanel != null)
                {
                    _clusterPanel.CreateClusters(levelData);
                    GameLogger.LogInfo(ScreenName, "ClusterPanel populated with level clusters (fallback)");
                }
            }
        }
        
        /// <summary>
        /// ОБНОВЛЕНО: Обновление UI с расширенной информацией
        /// </summary>
        private void UpdateUIWithLevelData(LevelData levelData)
        {
            if (_levelInfoText != null)
            {
                string clusterStats = "";
                if (_clusterManager != null && _clusterManager.IsInitialized)
                {
                    clusterStats = $"\nAvailable: {_clusterManager.AvailableClusters.Count}, Placed: {_clusterManager.PlacedClusters.Count}";
                }

                _levelInfoText.text = $"Level {levelData.LevelId}\n{levelData.TargetWords.Length} words, {levelData.AvailableClusters.Length} clusters{clusterStats}";
            }
        }

        // НОВЫЕ: Обработчики событий ClusterManager

        /// <summary>
        /// Обработка создания кластера
        /// </summary>
        private void OnClusterCreated(ClusterData cluster)
        {
            GameLogger.LogInfo(ScreenName, $"Cluster created: '{cluster.Text}' (ID: {cluster.ClusterId})");
            // Здесь можно добавить дополнительную логику, например, анимации
        }

        /// <summary>
        /// Обработка размещения кластера
        /// </summary>
        private void OnClusterPlaced(ClusterData cluster)
        {
            GameLogger.LogInfo(ScreenName, $"Cluster placed: '{cluster.Text}' at word {cluster.Position.WordIndex}, cell {cluster.Position.StartCellIndex}");
            
            // Обновляем UI
            if (_levelInfoText != null && _clusterManager != null)
            {
                UpdateUIWithLevelData(_currentLevelData);
            }
            
            // Проверяем завершение уровня
            if (_clusterManager != null && _clusterManager.IsLevelComplete())
            {
                GameLogger.LogInfo(ScreenName, "Level completed through cluster placement!");
                // Здесь можно добавить автоматическую проверку решения
            }
        }

        /// <summary>
        /// Обработка удаления кластера
        /// </summary>
        private void OnClusterRemoved(ClusterData cluster)
        {
            GameLogger.LogInfo(ScreenName, $"Cluster removed: '{cluster.Text}' (ID: {cluster.ClusterId})");
            
            // Обновляем UI
            if (_levelInfoText != null && _clusterManager != null)
            {
                UpdateUIWithLevelData(_currentLevelData);
            }
        }

        /// <summary>
        /// Обработка выбора кластера
        /// </summary>
        private void OnClusterSelected(ClusterData cluster)
        {
            GameLogger.LogInfo(ScreenName, $"Cluster selected: '{cluster.Text}' (ID: {cluster.ClusterId})");
            // Здесь можно добавить подсветку возможных позиций для размещения
        }

        /// <summary>
        /// Обработка завершения валидации кластеров
        /// </summary>
        private void OnAllClustersValidated()
        {
            GameLogger.LogInfo(ScreenName, "All clusters validated successfully");
            UIService.ShowMessage("All clusters are valid!", 2f);
        }

        // НОВЫЕ: Обработчики тестовых кнопок

        /// <summary>
        /// Тестовое создание кластеров
        /// </summary>
        private void OnTestCreateClustersClicked()
        {
            GameLogger.LogInfo(ScreenName, "Test Create Clusters button clicked");
            
            if (_clusterManager != null)
            {
                _clusterManager.CreateTestLevel();
                UIService.ShowMessage("Test clusters created via ClusterManager", 2f);
            }
            else if (_clusterPanel != null)
            {
                _clusterPanel.CreateTestClusters();
                UIService.ShowMessage("Test clusters created via ClusterPanel (fallback)", 2f);
            }
            else
            {
                GameLogger.LogWarning(ScreenName, "No cluster components available");
                UIService.ShowMessage("No cluster components available", 2f);
            }
        }

        /// <summary>
        /// Тестовая очистка всех кластеров
        /// </summary>
        private void OnTestClearAllClicked()
        {
            GameLogger.LogInfo(ScreenName, "Test Clear All button clicked");
            
            bool cleared = false;
            
            if (_gameField != null)
            {
                _gameField.ClearAllLetters();
                cleared = true;
            }
            
            if (_clusterManager != null)
            {
                _clusterManager.ClearAllClusters();
                cleared = true;
            }
            else if (_clusterPanel != null)
            {
                _clusterPanel.ClearAllClusters();
                cleared = true;
            }
            
            if (cleared)
            {
                UIService.ShowMessage("Game field and clusters cleared", 2f);
                // Обновляем UI
                if (_levelInfoText != null)
                {
                    _levelInfoText.text = $"Level {_currentLevelId} (Cleared)";
                }
            }
            else
            {
                GameLogger.LogWarning(ScreenName, "No components available to clear");
                UIService.ShowMessage("No components to clear", 2f);
            }
        }

        /// <summary>
        /// НОВОЕ: Тестовая валидация кластеров
        /// </summary>
        private void OnTestValidateClustersClicked()
        {
            GameLogger.LogInfo(ScreenName, "Test Validate Clusters button clicked");
            
            if (_clusterManager != null)
            {
                _clusterManager.ValidateAllClusters();
                
                string stats = _clusterManager.GetStatistics();
                Debug.Log($"Cluster Statistics:\n{stats}");
                
                UIService.ShowMessage("Cluster validation completed - check console", 3f);
            }
            else
            {
                GameLogger.LogWarning(ScreenName, "ClusterManager not available for validation");
                UIService.ShowMessage("ClusterManager not available", 2f);
            }
        }

        /// <summary>
        /// Обработка кнопки возврата в меню
        /// </summary>
        private void OnBackToMenuClicked()
        {
            GameLogger.LogInfo(ScreenName, "Back to Menu button clicked");

            UIService.ShowConfirmDialog(
                "Exit Level",
                "Are you sure you want to exit the level? Progress will be lost.",
                onConfirm: () =>
                {
                    GameLogger.LogInfo(ScreenName, "Confirmed exit to main menu");
                    LoadSceneSafe(SceneNames.MainMenu);
                },
                onCancel: () =>
                {
                    GameLogger.LogInfo(ScreenName, "Cancelled exit to main menu");
                }
            );
        }

        protected override void OnBackButtonPressed()
        {
            GameLogger.LogInfo(ScreenName, "Android back button pressed");
            OnBackToMenuClicked();
        }

        /// <summary>
        /// Обработка тестовой кнопки победы
        /// </summary>
        private async void OnMockWinClicked()
        {
            GameLogger.LogInfo(ScreenName, "Mock Win button clicked - simulating level completion");

            try
            {
                // Имитируем завершение уровня с реалистичными словами
                var mockCompletedWords = new string[] { "КЛАСТЕР", "ПРОЕКТ", "ЗАДАЧА", "ИГРОКА" };

                // Сохраняем прогресс
                GameLogger.LogInfo(ScreenName, $"Marking level {_currentLevelId} as completed...");
                
                await ProgressService.MarkLevelCompletedAsync(_currentLevelId, mockCompletedWords);
                
                GameLogger.LogInfo(ScreenName, $"Level {_currentLevelId} marked as completed successfully");

                // Проверяем что прогресс действительно сохранился
                bool isCompleted = ProgressService.IsLevelCompleted(_currentLevelId);
                int newCompletedCount = ProgressService.GetCompletedLevelsCount();
                int newCurrentLevel = ProgressService.GetCurrentLevelNumber();
                
                GameLogger.LogInfo(ScreenName, $"Progress verification: completed={isCompleted}, total={newCompletedCount}, next={newCurrentLevel}");

                // Создаем параметры для экрана победы
                var victoryParameters = new VictoryScreen.VictoryParameters
                {
                    LevelId = _currentLevelId,
                    CompletedWords = mockCompletedWords,
                    CompletionTime = 120.5f
                };

                GameLogger.LogInfo(ScreenName, $"Creating victory parameters for level {_currentLevelId}");

                // Переходим на экран победы
                LoadSceneWithParametersSafe(SceneNames.Victory, victoryParameters);
            }
            catch (System.Exception ex)
            {
                GameLogger.LogException(ScreenName, ex);
                UIService.ShowMessage("Failed to complete level test.", 3f);
            }
        }
        
        /// <summary>
        /// ОБНОВЛЕНО: Получение отладочной информации с ClusterManager
        /// </summary>
        public string GetGameFieldDebugInfo()
        {
            var info = "=== Gameplay Screen Debug Info (Enhanced) ===\n";
            info += $"Current Level: {_currentLevelId}\n";
            info += $"Level Data Loaded: {_currentLevelData != null}\n\n";
            
            if (_gameField != null)
            {
                info += $"GameField: {_gameField.GetDebugInfo()}\n\n";
            }
            else
            {
                info += "GameField: null\n\n";
            }
            
            if (_clusterManager != null)
            {
                info += $"ClusterManager:\n{_clusterManager.GetStatistics()}\n\n";
            }
            else
            {
                info += "ClusterManager: null\n\n";
            }
            
            if (_clusterPanel != null)
            {
                info += $"ClusterPanel: {_clusterPanel.GetDebugInfo()}\n";
            }
            else
            {
                info += "ClusterPanel: null\n";
            }
            
            return info;
        }
        
        #if UNITY_EDITOR
        [ContextMenu("Show Enhanced Debug Info")]
        private void ShowEnhancedDebugInfo()
        {
            Debug.Log(GetGameFieldDebugInfo());
        }
        
        [ContextMenu("Test ClusterManager Features")]
        private void TestClusterManagerFeatures()
        {
            if (_clusterManager != null)
            {
                Debug.Log("=== Testing ClusterManager Features ===");
                
                // Тест создания кластеров
                _clusterManager.CreateTestLevel();
                Debug.Log($"Test level created. Statistics:\n{_clusterManager.GetStatistics()}");
                
                // Тест валидации
                _clusterManager.ValidateAllClusters();
                
                // Тест поиска
                var foundClusters = _clusterManager.FindClustersByText("КЛ");
                Debug.Log($"Found {foundClusters.Count} clusters with text 'КЛ'");
            }
            else
            {
                Debug.LogWarning("ClusterManager is not available for testing");
            }
        }
        #endif
    }
}