using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WordPuzzle.Core.Architecture;
using WordPuzzle.Core.Services;

namespace WordPuzzle.UI.Screens
{
    /// <summary>
    /// Экран игрового процесса
    /// ИСПРАВЛЕНО: правильное сохранение прогресса при завершении уровня
    /// </summary>
    public class GameplayScreen : BaseScreen
    {
        [Header("UI Elements")]
        [SerializeField] private Button _backToMenuButton;
        [SerializeField] private Button _mockWinButton;
        [SerializeField] private TextMeshProUGUI _levelInfoText;
        [SerializeField] private TextMeshProUGUI _placeholderText;
        [SerializeField] private TextMeshProUGUI _progressInfoText; // НОВОЕ

        [Header("Settings")]
        [SerializeField] private string _placeholderMessage = "Gameplay will be implemented in later stages.\nFor now, use buttons to test navigation.";

        private int _currentLevelId;

        protected override string ScreenName => "Gameplay";

        protected override void OnInitialize()
        {
            GameLogger.LogInfo(ScreenName, "Setting up Gameplay screen...");

            LoadLevelParameters();
            SetupUI();
            ShowProgressInfo(); // НОВОЕ
            enableBackButton = true;
            LoadLevelDataAsync();

            GameLogger.LogInfo(ScreenName, "Gameplay screen setup completed");
        }

        protected override void SubscribeToUIEvents()
        {
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
            if (_backToMenuButton != null)
            {
                _backToMenuButton.onClick.RemoveListener(OnBackToMenuClicked);
            }

            if (_mockWinButton != null)
            {
                _mockWinButton.onClick.RemoveListener(OnMockWinClicked);
            }
        }

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

        private void SetupUI()
        {
            if (_levelInfoText != null)
            {
                _levelInfoText.text = $"Level {_currentLevelId}";
            }

            if (_placeholderText != null)
            {
                _placeholderText.text = _placeholderMessage;
            }

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
        /// НОВОЕ: Отображение информации о прогрессе для отладки
        /// </summary>
        private void ShowProgressInfo()
        {
            if (_progressInfoText == null) return;

            try
            {
                if (ProgressService?.IsInitialized == true)
                {
                    int completedLevels = ProgressService.GetCompletedLevelsCount();
                    int currentLevel = ProgressService.GetCurrentLevelNumber();
                    bool isCurrentCompleted = ProgressService.IsLevelCompleted(_currentLevelId);

                    _progressInfoText.text = $"Progress Info:\nCompleted: {completedLevels}\nCurrent: {currentLevel}\nThis level completed: {isCurrentCompleted}";
                }
                else
                {
                    _progressInfoText.text = "Progress Service not available";
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogException(ScreenName, ex);
                _progressInfoText.text = "Error loading progress info";
            }
        }

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
        /// ИСПРАВЛЕНО: Правильное сохранение прогресса при завершении уровня
        /// </summary>
        private async void OnMockWinClicked()
        {
            GameLogger.LogInfo(ScreenName, "Mock Win button clicked - simulating level completion");

            try
            {
                // Имитируем завершение уровня с реалистичными словами
                var mockCompletedWords = new string[] { "КЛАСТЕР", "ПРОЕКТ", "ЗАДАЧА", "ИГРОКА" };

                // ИСПРАВЛЕНО: Сначала сохраняем прогресс, затем переходим на экран победы
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

        private async void LoadLevelDataAsync()
        {
            try
            {
                GameLogger.LogInfo(ScreenName, $"Loading level {_currentLevelId} data...");

                var levelData = await LevelService.LoadLevelAsync(_currentLevelId);

                if (levelData != null)
                {
                    GameLogger.LogInfo(ScreenName,
                        $"Level data loaded successfully: {levelData.TargetWords.Length} words, {levelData.AvailableClusters.Length} clusters");

                    // Выводим информацию о загруженном уровне
                    GameLogger.LogInfo(ScreenName, $"Target words:");
                    foreach (var wordData in levelData.TargetWords)
                    {
                        GameLogger.LogInfo(ScreenName, $"  - {wordData.Word} = [{string.Join(", ", wordData.Clusters)}]");
                    }

                    GameLogger.LogInfo(ScreenName, $"Available clusters: [{string.Join(", ", levelData.AvailableClusters)}]");

                    // Обновляем UI с реальными данными
                    if (_levelInfoText != null)
                    {
                        _levelInfoText.text = $"Level {levelData.LevelId}\n{levelData.TargetWords.Length} words, {levelData.AvailableClusters.Length} clusters";
                    }

                    if (_placeholderText != null)
                    {
                        _placeholderText.text = "Real level data loaded!\nCheck console for details.\n\nUse 'Mock Win' to test progress system.";
                    }

                    // Обновляем информацию о прогрессе
                    ShowProgressInfo();
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
    }
}