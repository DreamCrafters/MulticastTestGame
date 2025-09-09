using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WordPuzzle.Core.Architecture;
using WordPuzzle.Core.Services;

namespace WordPuzzle.UI.Screens
{
    /// <summary>
    /// Экран игрового процесса
    /// На данном этапе содержит заглушки для тестирования навигации
    /// </summary>
    public class GameplayScreen : BaseScreen
    {
        [Header("UI Elements")]
        [SerializeField] private Button _backToMenuButton;
        [SerializeField] private Button _mockWinButton; // Временная кнопка для тестирования
        [SerializeField] private TextMeshProUGUI _levelInfoText;
        [SerializeField] private TextMeshProUGUI _placeholderText;
        
        [Header("Settings")]
        [SerializeField] private string _placeholderMessage = "Gameplay will be implemented in later stages.\nFor now, use buttons to test navigation.";
        
        private int _currentLevelId;
        
        protected override string ScreenName => "Gameplay";
        
        /// <summary>
        /// Инициализация игрового экрана
        /// </summary>
        protected override void OnInitialize()
        {
            GameLogger.LogInfo(ScreenName, "Setting up Gameplay screen...");
            
            // Получаем параметры уровня из SceneService
            LoadLevelParameters();
            
            // Настройка UI элементов
            SetupUI();
            
            // Настройка кнопки возврата в Android
            enableBackButton = true;
            
            GameLogger.LogInfo(ScreenName, "Gameplay screen setup completed");
        }
        
        /// <summary>
        /// Подписка на события UI
        /// </summary>
        protected override void SubscribeToUIEvents()
        {
            if (_backToMenuButton != null)
            {
                _backToMenuButton.onClick.AddListener(OnBackToMenuClicked);
            }
            else
            {
                GameLogger.LogWarning(ScreenName, "Back to Menu button is not assigned!");
            }
            
            if (_mockWinButton != null)
            {
                _mockWinButton.onClick.AddListener(OnMockWinClicked);
            }
            else
            {
                GameLogger.LogWarning(ScreenName, "Mock Win button is not assigned!");
            }
        }
        
        /// <summary>
        /// Отписка от событий UI
        /// </summary>
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
        
        /// <summary>
        /// Загрузка параметров уровня
        /// </summary>
        private void LoadLevelParameters()
        {
            try
            {
                var parameters = SceneService.GetSceneParameters<MainMenuScreen.GameplayParameters>();
                
                if (parameters != null)
                {
                    _currentLevelId = parameters.LevelId;
                    GameLogger.LogInfo(ScreenName, $"Loaded level parameters: Level {_currentLevelId}");
                }
                else
                {
                    // Значения по умолчанию если параметры не переданы
                    _currentLevelId = 1;
                    GameLogger.LogWarning(ScreenName, "No level parameters found, using default level 1");
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogException(ScreenName, ex);
                _currentLevelId = 1;
            }
        }
        
        /// <summary>
        /// Настройка UI элементов
        /// </summary>
        private void SetupUI()
        {
            // Настройка информации об уровне
            if (_levelInfoText != null)
            {
                _levelInfoText.text = $"Level {_currentLevelId}";
            }
            
            // Настройка заглушки
            if (_placeholderText != null)
            {
                _placeholderText.text = _placeholderMessage;
            }
            
            // Настройка кнопок
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
        /// Обработка кнопки возврата в меню
        /// </summary>
        private void OnBackToMenuClicked()
        {
            GameLogger.LogInfo(ScreenName, "Back to Menu button clicked");
            
            // Показываем диалог подтверждения
            UIService.ShowConfirmDialog(
                "Exit Level", 
                "Are you sure you want to exit the level? Progress will be lost.",
                onConfirm: () => {
                    GameLogger.LogInfo(ScreenName, "Confirmed exit to main menu");
                    LoadSceneSafe(SceneNames.MainMenu);
                },
                onCancel: () => {
                    GameLogger.LogInfo(ScreenName, "Cancelled exit to main menu");
                }
            );
        }
        
        /// <summary>
        /// Обработка кнопки "Назад" Android
        /// </summary>
        protected override void OnBackButtonPressed()
        {
            GameLogger.LogInfo(ScreenName, "Android back button pressed");
            OnBackToMenuClicked(); // Используем ту же логику что и кнопка меню
        }
        
        /// <summary>
        /// Временная кнопка для тестирования победы
        /// </summary>
        private void OnMockWinClicked()
        {
            GameLogger.LogInfo(ScreenName, "Mock Win button clicked - simulating level completion");
            
            try
            {
                // Имитируем завершение уровня
                var mockCompletedWords = new string[] { "MOCK", "TEST", "WORD", "DONE" };
                
                // Создаем параметры для экрана победы
                var victoryParameters = new VictoryScreen.VictoryParameters
                {
                    LevelId = _currentLevelId,
                    CompletedWords = mockCompletedWords,
                    CompletionTime = 120.5f // 2 минуты для примера
                };
                
                // Отмечаем уровень как пройденный (для тестирования)
                _ = ProgressService.MarkLevelCompletedAsync(_currentLevelId, mockCompletedWords);
                
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
        /// Асинхронная загрузка данных уровня для будущих этапов
        /// </summary>
        private async void LoadLevelDataAsync()
        {
            try
            {
                GameLogger.LogInfo(ScreenName, $"Loading level {_currentLevelId} data...");
                
                var levelData = await LevelService.LoadLevelAsync(_currentLevelId);
                
                if (levelData != null)
                {
                    GameLogger.LogInfo(ScreenName, $"Level data loaded: {levelData.TargetWords.Length} words, {levelData.AvailableClusters.Length} clusters");
                    
                    // Здесь в будущих этапах будет инициализация игрового поля
                    // InitializeGameField(levelData);
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