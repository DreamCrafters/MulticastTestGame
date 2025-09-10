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
    /// Экран игрового процесса (ОБНОВЛЕН для Этапа 6)
    /// Интегрирует GameField и ClusterPanel для базовой структуры игрового поля
    /// </summary>
    public class GameplayScreen : BaseScreen
    {
        [Header("Navigation UI")]
        [SerializeField] private Button _backToMenuButton;
        [SerializeField] private Button _mockWinButton;
        [SerializeField] private TextMeshProUGUI _levelInfoText;
        
        [Header("Gameplay Components (NEW)")]
        [SerializeField] private GameField _gameField;
        [SerializeField] private ClusterPanel _clusterPanel;
        
        [Header("Layout Components")]
        [SerializeField] private RectTransform _gameFieldContainer;
        [SerializeField] private RectTransform _clusterPanelContainer;
        
        [Header("Settings")]
        [SerializeField] private bool _autoLoadGameField = true;

        private int _currentLevelId;
        private LevelData _currentLevelData;

        protected override string ScreenName => "Gameplay";

        protected override void OnInitialize()
        {
            GameLogger.LogInfo(ScreenName, "Setting up Gameplay screen with GameField and ClusterPanel...");

            LoadLevelParameters();
            SetupNavigation();
            SetupGameplayComponents();
            
            enableBackButton = true;

            if (_autoLoadGameField)
            {
                LoadLevelDataAsync();
            }

            GameLogger.LogInfo(ScreenName, "Gameplay screen setup completed with new components");
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
        /// НОВОЕ: Настройка компонентов игрового поля
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
            
            // Логирование состояния компонентов
            GameLogger.LogInfo(ScreenName, $"Gameplay components status: GameField={_gameField != null}, ClusterPanel={_clusterPanel != null}");
            
            if (_gameField != null)
            {
                GameLogger.LogInfo(ScreenName, $"GameField initialized: {_gameField.IsInitialized}");
            }
            
            if (_clusterPanel != null)
            {
                GameLogger.LogInfo(ScreenName, $"ClusterPanel initialized: {_clusterPanel.IsInitialized}");
            }
        }
        
        /// <summary>
        /// ОБНОВЛЕНО: Загрузка данных уровня и настройка компонентов игрового поля
        /// </summary>
        private async void LoadLevelDataAsync()
        {
            try
            {
                GameLogger.LogInfo(ScreenName, $"Loading level {_currentLevelId} data for game field setup...");

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

                    // НОВОЕ: Настраиваем игровое поле с загруженными данными
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
        /// НОВОЕ: Настройка игрового поля с данными уровня
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
            
            // НОВОЕ: Создание кластеров в панели
            if (_clusterPanel != null)
            {
                _clusterPanel.CreateClusters(levelData);
                GameLogger.LogInfo(ScreenName, "ClusterPanel populated with level clusters");
            }
            else
            {
                GameLogger.LogWarning(ScreenName, "ClusterPanel is null, cannot create clusters");
            }
        }
        
        /// <summary>
        /// НОВОЕ: Обновление UI с данными уровня
        /// </summary>
        private void UpdateUIWithLevelData(LevelData levelData)
        {
            if (_levelInfoText != null)
            {
                _levelInfoText.text = $"Level {levelData.LevelId}\n{levelData.TargetWords.Length} words, {levelData.AvailableClusters.Length} clusters";
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
        /// НОВОЕ: Тестовое заполнение игрового поля
        /// </summary>
        private void OnTestFillFieldClicked()
        {
            GameLogger.LogInfo(ScreenName, "Test Fill Field button clicked");
            
            if (_gameField != null)
            {
                _gameField.FillWithTestData();
                UIService.ShowMessage("Game field filled with test data", 2f);
            }
            else
            {
                GameLogger.LogWarning(ScreenName, "GameField is null, cannot fill with test data");
                UIService.ShowMessage("GameField not available", 2f);
            }
        }
        
        /// <summary>
        /// НОВОЕ: Тестовое создание кластеров
        /// </summary>
        private void OnTestCreateClustersClicked()
        {
            GameLogger.LogInfo(ScreenName, "Test Create Clusters button clicked");
            
            if (_clusterPanel != null)
            {
                _clusterPanel.CreateTestClusters();
                UIService.ShowMessage("Test clusters created", 2f);
            }
            else
            {
                GameLogger.LogWarning(ScreenName, "ClusterPanel is null, cannot create test clusters");
                UIService.ShowMessage("ClusterPanel not available", 2f);
            }
        }
        
        /// <summary>
        /// НОВОЕ: Очистка всех элементов игрового поля
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
            
            if (_clusterPanel != null)
            {
                _clusterPanel.ClearAllClusters();
                cleared = true;
            }
            
            if (cleared)
            {
                UIService.ShowMessage("Game field and clusters cleared", 2f);
            }
            else
            {
                GameLogger.LogWarning(ScreenName, "No components available to clear");
                UIService.ShowMessage("No components to clear", 2f);
            }
        }

        /// <summary>
        /// НОВОЕ: Получение отладочной информации о состоянии игрового поля
        /// </summary>
        public string GetGameFieldDebugInfo()
        {
            var info = "=== Gameplay Screen Debug Info ===\n";
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
        [ContextMenu("Show GameField Debug Info")]
        private void ShowGameFieldDebugInfo()
        {
            Debug.Log(GetGameFieldDebugInfo());
        }
        
        [ContextMenu("Test Setup All Components")]
        private void TestSetupAllComponents()
        {
            SetupGameplayComponents();
            if (_currentLevelData != null)
            {
                SetupGameFieldWithLevelData(_currentLevelData);
            }
            else
            {
                GameLogger.LogWarning(ScreenName, "No level data loaded for component setup test");
            }
        }
        #endif
    }
}