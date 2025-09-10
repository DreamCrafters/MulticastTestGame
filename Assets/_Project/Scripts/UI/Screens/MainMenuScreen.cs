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
        
        [Header("Settings")]
        [SerializeField] private string _titleTextContent = "Word Puzzle";
        [SerializeField] private string _levelCounterFormat = "Levels Completed: {0}";
        
        protected override string ScreenName => "MainMenu";
        
        protected override void OnInitialize()
        {
            GameLogger.LogInfo(ScreenName, "Setting up Main Menu UI elements...");
            
            SetupTitle();
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
        }
        
        protected override void UnsubscribeFromUIEvents()
        {
            if (_playButton != null)
            {
                _playButton.onClick.RemoveListener(OnPlayButtonClicked);
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
        /// ИСПРАВЛЕНО: Обновление счетчика с учетом завершения всех уровней
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
                // Проверяем доступность и инициализацию сервисов
                if (ProgressService?.IsInitialized == true && LevelService?.IsInitialized == true)
                {
                    int completedLevels = ProgressService.GetCompletedLevelsCount();
                    int totalLevels = LevelService.GetTotalLevelsCount();
                    int currentLevel = ProgressService.GetCurrentLevelNumber();
                    
                    string counterText;
                    
                    // Проверяем, завершены ли все уровни
                    if (ProgressService.AreAllLevelsCompleted(totalLevels) && totalLevels > 0)
                    {
                        counterText = $"Игра завершена! ({completedLevels}/{totalLevels})";
                    }
                    else if (totalLevels > 0)
                    {
                        counterText = $"Пройдено уровней: {completedLevels}/{totalLevels}";
                    }
                    else
                    {
                        counterText = string.Format(_levelCounterFormat, completedLevels);
                    }
                    
                    _levelCounterText.text = counterText;
                    
                    GameLogger.LogInfo(ScreenName, $"Level counter updated: {completedLevels} completed of {totalLevels}, current: {currentLevel}");
                }
                else
                {
                    _levelCounterText.text = "Levels Completed: --";
                    GameLogger.LogWarning(ScreenName, "Services not available or not initialized");
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogException(ScreenName, ex);
                _levelCounterText.text = "Levels Completed: Error";
            }
        }
        
        /// <summary>
        /// ИСПРАВЛЕНО: Улучшенная настройка кнопки Play с учетом завершения всех уровней
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
                    UpdatePlayButtonText("Loading...");
                    GameLogger.LogWarning(ScreenName, "Services not ready - disabling Play button");
                    return;
                }
                
                // Проверяем доступность уровней
                int totalLevels = LevelService.GetTotalLevelsCount();
                bool hasLevels = totalLevels > 0;
                
                if (!hasLevels)
                {
                    _playButton.interactable = false;
                    UpdatePlayButtonText("No Levels");
                    GameLogger.LogWarning(ScreenName, "No levels available - Play button disabled");
                    return;
                }
                
                _playButton.interactable = true;
                
                // Проверяем, завершены ли все уровни
                if (ProgressService.AreAllLevelsCompleted(totalLevels))
                {
                    UpdatePlayButtonText("Play Again");
                    GameLogger.LogInfo(ScreenName, $"All {totalLevels} levels completed - showing restart option");
                }
                else
                {
                    int currentLevel = ProgressService.GetCurrentLevelNumber();
                    UpdatePlayButtonText("Play");
                    GameLogger.LogInfo(ScreenName, $"Play button ready. Current level: {currentLevel} of {totalLevels}");
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogException(ScreenName, ex);
                _playButton.interactable = false;
                UpdatePlayButtonText("Error");
            }
        }
        
        /// <summary>
        /// НОВОЕ: Обновление текста кнопки Play
        /// </summary>
        private void UpdatePlayButtonText(string text)
        {
            if (_playButton != null)
            {
                var buttonText = _playButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = text;
                }
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
                
                // Проверяем, завершены ли все уровни
                if (ProgressService.AreAllLevelsCompleted(totalLevels))
                {
                    GameLogger.LogInfo(ScreenName, "All levels completed - showing restart confirmation");
                    ShowGameCompletionDialog(totalLevels);
                    return;
                }
                
                // Проверяем существование уровня
                if (!LevelService.IsLevelExists(currentLevel))
                {
                    GameLogger.LogError(ScreenName, $"Level {currentLevel} does not exist!");
                    UIService.ShowMessage("Level not found. Please check game configuration.", 3f);
                    return;
                }
                
                GameLogger.LogInfo(ScreenName, $"Starting level {currentLevel}");
                
                // Запускаем найденный уровень
                StartLevel(currentLevel);
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
        
        /// <summary>
        /// НОВОЕ: Показать диалог завершения всех уровней
        /// </summary>
        private void ShowGameCompletionDialog(int totalLevels)
        {
            string title = "Поздравляем!";
            string message = $"Вы прошли все {totalLevels} уровней!\n\nХотите начать игру заново?";
            
            UIService.ShowConfirmDialog(
                title,
                message,
                onConfirm: async () =>
                {
                    GameLogger.LogInfo(ScreenName, "Player confirmed game restart - resetting progress");
                    await ResetProgressAndStartNewGame();
                },
                onCancel: () =>
                {
                    GameLogger.LogInfo(ScreenName, "Player cancelled game restart");
                }
            );
        }
        
        /// <summary>
        /// НОВОЕ: Сброс прогресса и начало новой игры
        /// </summary>
        private async UniTask ResetProgressAndStartNewGame()
        {
            try
            {
                // Сбрасываем прогресс
                ProgressService.ResetProgress();
                GameLogger.LogInfo(ScreenName, "Progress reset completed");
                
                // Обновляем UI
                await RefreshProgressAndUpdateUI();
                
                // Показываем уведомление
                UIService.ShowMessage("Прогресс сброшен! Начинаем заново.", 2f);
                
                // Небольшая задержка для показа сообщения
                await UniTask.Delay(1000);
                
                // Запускаем первый уровень
                StartLevel(1);
            }
            catch (System.Exception ex)
            {
                GameLogger.LogException(ScreenName, ex);
                UIService.ShowMessage("Ошибка при сбросе прогресса. Попробуйте еще раз.", 3f);
            }
        }
        
        /// <summary>
        /// НОВОЕ: Запуск конкретного уровня
        /// </summary>
        private void StartLevel(int levelId)
        {
            try
            {
                // Создаем параметры для передачи в игровую сцену
                var gameplayParameters = new GameplayParameters
                {
                    LevelId = levelId
                };
                
                GameLogger.LogInfo(ScreenName, $"Starting level {levelId}");
                
                // Загружаем игровую сцену с параметрами
                LoadSceneWithParametersSafe(SceneNames.Gameplay, gameplayParameters);
            }
            catch (System.Exception ex)
            {
                GameLogger.LogException(ScreenName, ex);
                UIService.ShowMessage("Ошибка запуска уровня. Попробуйте еще раз.", 3f);
            }
        }
        
        [System.Serializable]
        public class GameplayParameters
        {
            public int LevelId;
        }
    }
}