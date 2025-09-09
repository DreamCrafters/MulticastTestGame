using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WordPuzzle.Core.Architecture;
using WordPuzzle.Core.Services;

namespace WordPuzzle.UI.Screens
{
    /// <summary>
    /// Экран главного меню игры
    /// Содержит кнопку Play и счетчик пройденных уровней
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
        
        /// <summary>
        /// Инициализация главного меню
        /// </summary>
        protected override void OnInitialize()
        {
            GameLogger.LogInfo(ScreenName, "Setting up Main Menu UI elements...");
            
            // Настройка заголовка
            SetupTitle();
            
            // Настройка счетчика уровней
            UpdateLevelCounter();
            
            // Настройка кнопки Play
            SetupPlayButton();
            
            GameLogger.LogInfo(ScreenName, "Main Menu UI setup completed");
        }
        
        /// <summary>
        /// Подписка на события UI
        /// </summary>
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
        
        /// <summary>
        /// Отписка от событий UI
        /// </summary>
        protected override void UnsubscribeFromUIEvents()
        {
            if (_playButton != null)
            {
                _playButton.onClick.RemoveListener(OnPlayButtonClicked);
            }
        }
        
        /// <summary>
        /// Настройка заголовка игры
        /// </summary>
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
        /// Обновление счетчика пройденных уровней
        /// </summary>
        private void UpdateLevelCounter()
        {
            if (_levelCounterText == null)
            {
                GameLogger.LogWarning(ScreenName, "Level counter text component is not assigned!");
                return;
            }
            
            // Проверяем доступность сервиса прогресса
            if (ProgressService == null)
            {
                GameLogger.LogWarning(ScreenName, "ProgressService not available yet - showing placeholder text");
                _levelCounterText.text = "Levels Completed: --";
                return;
            }
            
            // Проверяем инициализацию сервиса
            if (!ProgressService.IsInitialized)
            {
                GameLogger.LogWarning(ScreenName, "ProgressService not initialized yet - showing placeholder text");
                _levelCounterText.text = "Levels Completed: --";
                return;
            }
            
            try
            {
                int completedLevels = ProgressService.GetCompletedLevelsCount();
                string counterText = string.Format(_levelCounterFormat, completedLevels);
                
                _levelCounterText.text = counterText;
                
                GameLogger.LogInfo(ScreenName, $"Level counter updated: {completedLevels} levels completed");
            }
            catch (System.Exception ex)
            {
                GameLogger.LogException(ScreenName, ex);
                _levelCounterText.text = "Levels Completed: --";
            }
        }
        
        /// <summary>
        /// Настройка кнопки Play
        /// </summary>
        private void SetupPlayButton()
        {
            if (_playButton == null)
            {
                GameLogger.LogWarning(ScreenName, "Play button is not assigned!");
                return;
            }
            
            // Проверяем доступность сервисов
            if (LevelService == null || ProgressService == null)
            {
                GameLogger.LogWarning(ScreenName, "Services not available yet - disabling Play button");
                _playButton.interactable = false;
                return;
            }
            
            // Проверяем инициализацию сервисов
            if (!LevelService.IsInitialized || !ProgressService.IsInitialized)
            {
                GameLogger.LogWarning(ScreenName, "Services not initialized yet - disabling Play button");
                _playButton.interactable = false;
                return;
            }
            
            // Проверяем доступность уровней
            int totalLevels = LevelService.GetTotalLevelsCount();
            bool hasLevels = totalLevels > 0;
            
            _playButton.interactable = hasLevels;
            
            if (hasLevels == false)
            {
                GameLogger.LogWarning(ScreenName, "No levels available - Play button disabled");
            }
            else
            {
                int currentLevel = ProgressService.GetCurrentLevelNumber();
                GameLogger.LogInfo(ScreenName, $"Play button ready. Current level: {currentLevel}");
            }
        }
        
        /// <summary>
        /// Обработчик нажатия кнопки Play
        /// </summary>
        private void OnPlayButtonClicked()
        {
            GameLogger.LogInfo(ScreenName, "Play button clicked");
            
            try
            {
                // Получаем номер текущего уровня
                int currentLevel = ProgressService.GetCurrentLevelNumber();
                int totalLevels = LevelService.GetTotalLevelsCount();
                
                // Логика "если пройдены все уровни, то снова запускается первый"
                if (currentLevel > totalLevels)
                {
                    currentLevel = 1;
                    GameLogger.LogInfo(ScreenName, "All levels completed, restarting from level 1");
                }
                
                // Проверяем существование уровня
                if (LevelService.IsLevelExists(currentLevel) == false)
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
        /// Обновление UI при возврате в главное меню
        /// Вызывается когда экран становится активным
        /// </summary>
        private void OnEnable()
        {
            if (IsInitialized)
            {
                UpdateLevelCounter();
            }
        }
        
        /// <summary>
        /// Параметры для передачи в игровую сцену
        /// </summary>
        [System.Serializable]
        public class GameplayParameters
        {
            public int LevelId;
        }
    }
}