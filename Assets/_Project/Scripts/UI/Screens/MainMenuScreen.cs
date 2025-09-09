using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using WordPuzzle.Core.Architecture;
using WordPuzzle.Core.Services;

namespace WordPuzzle.UI.Screens
{
    /// <summary>
    /// Экран главного меню игры
    /// ИСПРАВЛЕНО: добавлено обновление UI при активации экрана
    /// </summary>
    public class MainMenuScreen : BaseScreen
    {
        [Header("UI Elements")]
        [SerializeField] private Button _playButton;
        [SerializeField] private TextMeshProUGUI _levelCounterText;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private Button _resetProgressButton; // НОВОЕ: для тестирования
        
        [Header("Settings")]
        [SerializeField] private string _titleTextContent = "Word Puzzle";
        [SerializeField] private string _levelCounterFormat = "Levels Completed: {0}";
        [SerializeField] private bool _showResetButton = false; // НОВОЕ: для отладки
        
        protected override string ScreenName => "MainMenu";
        
        protected override void OnInitialize()
        {
            GameLogger.LogInfo(ScreenName, "Setting up Main Menu UI elements...");
            
            SetupTitle();
            SetupResetButton(); // НОВОЕ
            UpdateLevelCounter();
            SetupPlayButton();
            
            GameLogger.LogInfo(ScreenName, "Main Menu UI setup completed");
        }
        
        protected override void SubscribeToUIEvents()
        {
            if (_playButton != null)
            {
                _playButton.onClick.AddListener(OnPlayButtonClicked);
            }
            else
            {
                GameLogger.LogWarning(ScreenName, "Play button is not assigned!");
            }

            // НОВОЕ: кнопка сброса прогресса для тестирования
            if (_resetProgressButton != null)
            {
                _resetProgressButton.onClick.AddListener(OnResetProgressClicked);
            }
        }
        
        protected override void UnsubscribeFromUIEvents()
        {
            if (_playButton != null)
            {
                _playButton.onClick.RemoveListener(OnPlayButtonClicked);
            }

            if (_resetProgressButton != null)
            {
                _resetProgressButton.onClick.RemoveListener(OnResetProgressClicked);
            }
        }
        
        private void SetupTitle()
        {
            if (_titleText != null)
            {
                _titleText.text = _titleTextContent;
                GameLogger.LogInfo(ScreenName, $"Title set to: {_titleTextContent}");
            }
            else
            {
                GameLogger.LogWarning(ScreenName, "Title text component is not assigned!");
            }
        }

        /// <summary>
        /// НОВОЕ: Настройка кнопки сброса прогресса для тестирования
        /// </summary>
        private void SetupResetButton()
        {
            if (_resetProgressButton != null)
            {
                _resetProgressButton.gameObject.SetActive(_showResetButton);
                
                var buttonText = _resetProgressButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = "Reset Progress";
                }
            }
        }
        
        /// <summary>
        /// ИСПРАВЛЕНО: Обновление счетчика с лучшей обработкой ошибок
        /// </summary>
        private void UpdateLevelCounter()
        {
            if (_levelCounterText == null)
            {
                GameLogger.LogWarning(ScreenName, "Level counter text component is not assigned!");
                return;
            }
            
            try
            {
                // Проверяем доступность и инициализацию сервиса
                if (ProgressService?.IsInitialized == true)
                {
                    int completedLevels = ProgressService.GetCompletedLevelsCount();
                    int currentLevel = ProgressService.GetCurrentLevelNumber();
                    
                    string counterText = string.Format(_levelCounterFormat, completedLevels);
                    _levelCounterText.text = counterText;
                    
                    GameLogger.LogInfo(ScreenName, $"Level counter updated: {completedLevels} completed, current: {currentLevel}");
                }
                else
                {
                    _levelCounterText.text = "Levels Completed: --";
                    GameLogger.LogWarning(ScreenName, "ProgressService not available or not initialized");
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogException(ScreenName, ex);
                _levelCounterText.text = "Levels Completed: Error";
            }
        }
        
        /// <summary>
        /// ИСПРАВЛЕНО: Улучшенная настройка кнопки Play
        /// </summary>
        private void SetupPlayButton()
        {
            if (_playButton == null)
            {
                GameLogger.LogWarning(ScreenName, "Play button is not assigned!");
                return;
            }
            
            try
            {
                // Проверяем доступность сервисов
                bool servicesReady = LevelService?.IsInitialized == true && ProgressService?.IsInitialized == true;
                
                if (!servicesReady)
                {
                    _playButton.interactable = false;
                    GameLogger.LogWarning(ScreenName, "Services not ready - disabling Play button");
                    return;
                }
                
                // Проверяем доступность уровней
                int totalLevels = LevelService.GetTotalLevelsCount();
                bool hasLevels = totalLevels > 0;
                
                _playButton.interactable = hasLevels;
                
                if (hasLevels)
                {
                    int currentLevel = ProgressService.GetCurrentLevelNumber();
                    GameLogger.LogInfo(ScreenName, $"Play button ready. Current level: {currentLevel} of {totalLevels}");
                }
                else
                {
                    GameLogger.LogWarning(ScreenName, "No levels available - Play button disabled");
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogException(ScreenName, ex);
                _playButton.interactable = false;
            }
        }
        
        /// <summary>
        /// ИСПРАВЛЕНО: Улучшенная логика определения текущего уровня
        /// </summary>
        private void OnPlayButtonClicked()
        {
            GameLogger.LogInfo(ScreenName, "Play button clicked");
            
            try
            {
                // Получаем номер текущего уровня
                int currentLevel = ProgressService.GetCurrentLevelNumber();
                int totalLevels = LevelService.GetTotalLevelsCount();
                
                GameLogger.LogInfo(ScreenName, $"Current level from ProgressService: {currentLevel}");
                GameLogger.LogInfo(ScreenName, $"Total levels available: {totalLevels}");
                
                // Логика "если пройдены все уровни, то снова запускается первый"
                if (currentLevel > totalLevels)
                {
                    currentLevel = 1;
                    GameLogger.LogInfo(ScreenName, "All levels completed, restarting from level 1");
                }
                
                // Проверяем существование уровня
                if (!LevelService.IsLevelExists(currentLevel))
                {
                    GameLogger.LogError(ScreenName, $"Level {currentLevel} does not exist!");
                    UIService.ShowMessage("Level not found. Please check game configuration.", 3f);
                    return;
                }
                
                GameLogger.LogInfo(ScreenName, $"Starting level {currentLevel}");
                
                // Создаем параметры для передачи в игровую сцену
                var gameplayParameters = new GameplayParameters
                {
                    LevelId = currentLevel
                };
                
                // Загружаем игровую сцену с параметрами
                LoadSceneWithParametersSafe(SceneNames.Gameplay, gameplayParameters);
            }
            catch (System.Exception ex)
            {
                GameLogger.LogException(ScreenName, ex);
                UIService.ShowMessage("Failed to start game. Please try again.", 3f);
            }
        }

        /// <summary>
        /// НОВОЕ: Обработка кнопки сброса прогресса для тестирования
        /// </summary>
        private void OnResetProgressClicked()
        {
            GameLogger.LogInfo(ScreenName, "Reset progress button clicked");
            
            UIService.ShowConfirmDialog(
                "Reset Progress",
                "Are you sure you want to reset all progress? This cannot be undone.",
                onConfirm: () =>
                {
                    try
                    {
                        ProgressService.ResetProgress();
                        UpdateLevelCounter();
                        SetupPlayButton();
                        UIService.ShowMessage("Progress reset successfully!", 2f);
                        GameLogger.LogInfo(ScreenName, "Progress reset completed");
                    }
                    catch (System.Exception ex)
                    {
                        GameLogger.LogException(ScreenName, ex);
                        UIService.ShowMessage("Failed to reset progress", 3f);
                    }
                },
                onCancel: () =>
                {
                    GameLogger.LogInfo(ScreenName, "Progress reset cancelled");
                }
            );
        }
        
        /// <summary>
        /// ИСПРАВЛЕНО: Обновление UI при возврате в главное меню
        /// </summary>
        private async void OnEnable()
        {
            // Обновляем UI только если экран уже инициализирован
            if (IsInitialized)
            {
                GameLogger.LogInfo(ScreenName, "Main menu became active - refreshing progress and updating UI");
                
                // Принудительно перезагружаем прогресс из сохранения
                await RefreshProgressAndUpdateUI();
            }
        }
        
        /// <summary>
        /// НОВОЕ: Перезагрузка прогресса и обновление UI
        /// </summary>
        private async UniTask RefreshProgressAndUpdateUI()
        {
            try
            {
                // Принудительно обновляем прогресс из диска
                if (ProgressService?.IsInitialized == true)
                {
                    var progressServiceImpl = ProgressService as WordPuzzle.Data.Persistence.ProgressService;
                    if (progressServiceImpl != null)
                    {
                        await progressServiceImpl.RefreshProgressAsync();
                    }
                }
                
                // Обновляем UI после загрузки свежих данных
                UpdateLevelCounter();
                SetupPlayButton();
            }
            catch (System.Exception ex)
            {
                GameLogger.LogException(ScreenName, ex);
                // В случае ошибки просто обновляем UI с текущими данными
                UpdateLevelCounter();
                SetupPlayButton();
            }
        }
        
        [System.Serializable]
        public class GameplayParameters
        {
            public int LevelId;
        }
    }
}