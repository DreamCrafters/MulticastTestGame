using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WordPuzzle.Core.Architecture;
using WordPuzzle.Core.Services;

namespace WordPuzzle.UI.Screens
{
    /// <summary>
    /// Экран победы при завершении уровня
    /// Отображает разгаданные слова и кнопки навигации
    /// </summary>
    public class VictoryScreen : BaseScreen
    {
        [Header("UI Elements")]
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private TextMeshProUGUI _levelCompletedText;
        [SerializeField] private TextMeshProUGUI _completionTimeText;
        [SerializeField] private Transform _wordsListContainer;
        [SerializeField] private GameObject _wordItemPrefab;
        
        [Header("Settings")]
        [SerializeField] private string _levelCompletedFormat = "Level {0} Completed!";
        [SerializeField] private string _completionTimeFormat = "Time: {0:F1}s";
        
        private VictoryParameters _victoryParameters;
        
        protected override string ScreenName => "Victory";
        
        /// <summary>
        /// Инициализация экрана победы
        /// </summary>
        protected override void OnInitialize()
        {
            GameLogger.LogInfo(ScreenName, "Setting up Victory screen...");
            
            // Получаем параметры победы
            LoadVictoryParameters();
            
            // Настройка UI элементов
            SetupVictoryUI();
            
            // Проигрываем звук победы
            UIService?.PlayUISound(UISoundType.Success);
            
            GameLogger.LogInfo(ScreenName, "Victory screen setup completed");
        }
        
        /// <summary>
        /// Подписка на события UI
        /// </summary>
        protected override void SubscribeToUIEvents()
        {
            if (_mainMenuButton != null)
            {
                _mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }
            else
            {
                GameLogger.LogWarning(ScreenName, "Main Menu button is not assigned!");
            }
            
            if (_nextLevelButton != null)
            {
                _nextLevelButton.onClick.AddListener(OnNextLevelClicked);
            }
            else
            {
                GameLogger.LogWarning(ScreenName, "Next Level button is not assigned!");
            }
        }
        
        /// <summary>
        /// Отписка от событий UI
        /// </summary>
        protected override void UnsubscribeFromUIEvents()
        {
            if (_mainMenuButton != null)
            {
                _mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
            }
            
            if (_nextLevelButton != null)
            {
                _nextLevelButton.onClick.RemoveListener(OnNextLevelClicked);
            }
        }
        
        /// <summary>
        /// Загрузка параметров победы
        /// </summary>
        private void LoadVictoryParameters()
        {
            try
            {
                GameLogger.LogInfo(ScreenName, "Attempting to load victory parameters...");
                
                _victoryParameters = SceneService.GetSceneParameters<VictoryParameters>();
                
                if (_victoryParameters != null)
                {
                    GameLogger.LogInfo(ScreenName, $"Successfully loaded victory parameters: Level {_victoryParameters.LevelId}, {_victoryParameters.CompletedWords.Length} words");
                    GameLogger.LogInfo(ScreenName, $"Completed words: [{string.Join(", ", _victoryParameters.CompletedWords)}]");
                    GameLogger.LogInfo(ScreenName, $"Completion time: {_victoryParameters.CompletionTime:F1}s");
                }
                else
                {
                    // Создаем параметры по умолчанию
                    _victoryParameters = new VictoryParameters
                    {
                        LevelId = 1,
                        CompletedWords = new string[] { "ТЕСТ", "ДАНН", "МОКД", "СЛОВ" },
                        CompletionTime = 60f
                    };
                    GameLogger.LogWarning(ScreenName, "No victory parameters found, using mock data");
                    
                    // Дополнительная отладочная информация
                    GameLogger.LogInfo(ScreenName, "Debug: Checking SceneService state...");
                    if (SceneService != null)
                    {
                        string currentScene = SceneService.GetCurrentSceneName();
                        GameLogger.LogInfo(ScreenName, $"Debug: Current scene name: {currentScene}");
                        
                        // Попробуем получить любые параметры
                        var anyParams = SceneService.GetSceneParameters<object>();
                        GameLogger.LogInfo(ScreenName, $"Debug: Any parameters found: {anyParams?.GetType().Name ?? "null"}");
                    }
                    else
                    {
                        GameLogger.LogError(ScreenName, "Debug: SceneService is null!");
                    }
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogException(ScreenName, ex);
                
                // Fallback данные
                _victoryParameters = new VictoryParameters
                {
                    LevelId = 1,
                    CompletedWords = new string[] { "ERROR" },
                    CompletionTime = 0f
                };
                GameLogger.LogWarning(ScreenName, "Exception occurred while loading parameters, using fallback data");
            }
        }
        
        /// <summary>
        /// Настройка UI элементов победы
        /// </summary>
        private void SetupVictoryUI()
        {
            // Настройка заголовка
            if (_levelCompletedText != null)
            {
                _levelCompletedText.text = string.Format(_levelCompletedFormat, _victoryParameters.LevelId);
            }
            
            // Настройка времени прохождения
            if (_completionTimeText != null)
            {
                _completionTimeText.text = string.Format(_completionTimeFormat, _victoryParameters.CompletionTime);
            }
            
            // Отображение разгаданных слов
            DisplayCompletedWords();
            
            // Настройка кнопок
            SetupButtons();
        }
        
        /// <summary>
        /// Отображение списка разгаданных слов
        /// </summary>
        private void DisplayCompletedWords()
        {
            if (_wordsListContainer == null)
            {
                GameLogger.LogWarning(ScreenName, "Words list container is not assigned!");
                return;
            }
            
            // Очищаем существующие элементы
            foreach (Transform child in _wordsListContainer)
            {
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
            
            // Создаем элементы для каждого слова
            for (int i = 0; i < _victoryParameters.CompletedWords.Length; i++)
            {
                CreateWordItem(_victoryParameters.CompletedWords[i], i + 1);
            }
            
            GameLogger.LogInfo(ScreenName, $"Displayed {_victoryParameters.CompletedWords.Length} completed words");
        }
        
        /// <summary>
        /// Создание UI элемента для отображения слова
        /// </summary>
        private void CreateWordItem(string word, int orderNumber)
        {
            GameObject wordItem;
            
            if (_wordItemPrefab != null)
            {
                wordItem = Instantiate(_wordItemPrefab, _wordsListContainer);
            }
            else
            {
                // Создаем простой текстовый элемент если префаб не назначен
                wordItem = new GameObject($"WordItem_{orderNumber}");
                wordItem.transform.SetParent(_wordsListContainer);
                
                var textComponent = wordItem.AddComponent<TextMeshProUGUI>();
                textComponent.fontSize = 24;
                textComponent.color = Color.white;
                textComponent.alignment = TextAlignmentOptions.Center;
            }
            
            // Настраиваем текст
            var textMesh = wordItem.GetComponent<TextMeshProUGUI>();
            if (textMesh != null)
            {
                textMesh.text = $"{orderNumber}. {word}";
            }
            
            // Сбрасываем масштаб и позицию
            wordItem.transform.localScale = Vector3.one;
            wordItem.transform.localPosition = Vector3.zero;
        }
        
        /// <summary>
        /// Настройка кнопок навигации
        /// </summary>
        private void SetupButtons()
        {
            // Проверяем доступность следующего уровня
            int nextLevelId = _victoryParameters.LevelId + 1;
            bool hasNextLevel = LevelService != null && LevelService.IsLevelExists(nextLevelId);
            
            GameLogger.LogInfo(ScreenName, $"Checking next level {nextLevelId}: exists = {hasNextLevel}");
            
            if (_nextLevelButton != null)
            {
                _nextLevelButton.interactable = hasNextLevel;
                
                var buttonText = _nextLevelButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = hasNextLevel ? "Next Level" : "No More Levels";
                }
            }
            
            // Настройка кнопки главного меню
            if (_mainMenuButton != null)
            {
                var buttonText = _mainMenuButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = "Main Menu";
                }
            }
            
            GameLogger.LogInfo(ScreenName, $"Buttons setup completed: Next level available = {hasNextLevel}");
        }
        
        /// <summary>
        /// Обработка нажатия кнопки "Main Menu"
        /// </summary>
        private void OnMainMenuClicked()
        {
            GameLogger.LogInfo(ScreenName, "Main Menu button clicked");
            LoadSceneSafe(SceneNames.MainMenu);
        }
        
        /// <summary>
        /// Обработка нажатия кнопки "Next Level"
        /// </summary>
        private void OnNextLevelClicked()
        {
            GameLogger.LogInfo(ScreenName, "Next Level button clicked");
            
            try
            {
                int nextLevelId = _victoryParameters.LevelId + 1;
                
                if (LevelService == null)
                {
                    GameLogger.LogError(ScreenName, "LevelService is null!");
                    UIService.ShowMessage("Service error. Returning to menu.", 3f);
                    LoadSceneSafe(SceneNames.MainMenu);
                    return;
                }
                
                if (LevelService.IsLevelExists(nextLevelId) == false)
                {
                    GameLogger.LogWarning(ScreenName, $"Next level {nextLevelId} does not exist");
                    UIService.ShowMessage("No more levels available!", 2f);
                    return;
                }
                
                // Создаем параметры для следующего уровня
                var gameplayParameters = new MainMenuScreen.GameplayParameters
                {
                    LevelId = nextLevelId
                };
                
                GameLogger.LogInfo(ScreenName, $"Loading next level: {nextLevelId}");
                LoadSceneWithParametersSafe(SceneNames.Gameplay, gameplayParameters);
            }
            catch (System.Exception ex)
            {
                GameLogger.LogException(ScreenName, ex);
                UIService.ShowMessage("Failed to load next level.", 3f);
            }
        }
        
        /// <summary>
        /// Параметры для экрана победы
        /// </summary>
        [System.Serializable]
        public class VictoryParameters
        {
            public int LevelId;
            public string[] CompletedWords;
            public float CompletionTime;
        }
    }
}