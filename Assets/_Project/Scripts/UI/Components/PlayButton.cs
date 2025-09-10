using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using VContainer;
using Cysharp.Threading.Tasks;
using WordPuzzle.Core.Architecture;
using WordPuzzle.Core.Services;
using WordPuzzle.UI.Screens;

namespace WordPuzzle.UI.Components
{
    /// <summary>
    /// Компонент для управления кнопкой Play в главном меню
    /// Обеспечивает правильную логику запуска уровней и обновления состояния
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class PlayButton : MonoBehaviour
    {
        [Header("Button States")]
        [SerializeField] private string _playText = "Play";
        [SerializeField] private string _playAgainText = "Play Again";
        [SerializeField] private string _continueText = "Continue";
        [SerializeField] private string _loadingText = "Loading...";
        [SerializeField] private string _noLevelsText = "No Levels";
        [SerializeField] private string _errorText = "Error";
        
        [Header("Visual Settings")]
        [SerializeField] private bool _enableHoverAnimation = true;
        [SerializeField] private bool _enablePressAnimation = true;
        [SerializeField] private float _pressScale = 0.95f;
        [SerializeField] private float _pressDuration = 0.1f;
        
        [Header("State Colors")]
        [SerializeField] private ColorBlock _normalColors = ColorBlock.defaultColorBlock;
        [Space]
        [SerializeField] private bool _autoSetupDisabledColors = true;
        [SerializeField] private ColorBlock _disabledColors;
        [Space]
        [SerializeField] private bool _autoSetupGameCompletedColors = true;
        [SerializeField] private ColorBlock _gameCompletedColors;

        // Инжекция зависимостей
        [Inject] private IProgressService _progressService;
        [Inject] private ILevelService _levelService;
        [Inject] private ISceneService _sceneService;
        [Inject] private IUIService _uiService;
        
        private Button _button;
        private TextMeshProUGUI _buttonText;
        private RectTransform _rectTransform;
        
        private int _currentLevelNumber;
        private int _totalLevelsCount;
        private bool _isGameCompleted;
        private bool _isInitialized = false;
        
        /// <summary>
        /// Событие нажатия на кнопку Play
        /// </summary>
        public event Action<int> OnPlayButtonClicked;
        
        /// <summary>
        /// Событие изменения состояния кнопки
        /// </summary>
        public event Action<PlayButtonState> OnStateChanged;
        
        /// <summary>
        /// Текущее состояние кнопки
        /// </summary>
        public PlayButtonState CurrentState { get; private set; }
        
        /// <summary>
        /// Инициализация компонента
        /// </summary>
        private void Awake()
        {
            InitializeComponents();
            SetupDefaultDisabledColors();
        }
        
        /// <summary>
        /// Настройка компонентов
        /// </summary>
        private void InitializeComponents()
        {
            _button = GetComponent<Button>();
            _buttonText = GetComponentInChildren<TextMeshProUGUI>();
            _rectTransform = GetComponent<RectTransform>();
            
            if (_button == null)
            {
                GameLogger.LogError("PlayButton", "Button component not found!");
                return;
            }
            
            if (_buttonText == null)
            {
                GameLogger.LogWarning("PlayButton", "TextMeshProUGUI component not found in children!");
            }
            
            // Подписываемся на событие нажатия
            _button.onClick.AddListener(OnButtonClicked);
            
            GameLogger.LogInfo("PlayButton", "Play Button component initialized");
        }
        
        /// <summary>
        /// Настройка цветов для отключенного состояния по умолчанию
        /// </summary>
        private void SetupDefaultDisabledColors()
        {
            if (_autoSetupDisabledColors)
            {
                _disabledColors = _normalColors;
                _disabledColors.normalColor = Color.gray;
                _disabledColors.highlightedColor = Color.gray;
                _disabledColors.selectedColor = Color.gray;
            }
            
            if (_autoSetupGameCompletedColors)
            {
                _gameCompletedColors = _normalColors;
                _gameCompletedColors.normalColor = Color.green;
                _gameCompletedColors.highlightedColor = Color.green * 1.2f;
                _gameCompletedColors.selectedColor = Color.green * 0.8f;
            }
        }
        
        /// <summary>
        /// Автоматическое обновление при активации
        /// </summary>
        private void OnEnable()
        {
            if (_isInitialized == false)
            {
                _isInitialized = true;
                UpdateButtonState();
            }
            else
            {
                // При повторной активации обновляем с небольшой задержкой
                Invoke(nameof(UpdateButtonState), 0.1f);
            }
            
            SetupHoverAnimations();
        }
        
        /// <summary>
        /// Очистка при деактивации
        /// </summary>
        private void OnDisable()
        {
            DOTween.Kill(_rectTransform);
        }
        
        /// <summary>
        /// Публичный метод для обновления состояния кнопки
        /// Вызывается из MainMenuScreen при необходимости
        /// </summary>
        public void UpdateButtonState()
        {
            if (_button == null)
            {
                GameLogger.LogWarning("PlayButton", "Cannot update state - button component is null");
                return;
            }
            
            try
            {
                // Проверяем готовность сервисов
                if (!AreServicesReady())
                {
                    SetState(PlayButtonState.Loading);
                    return;
                }
                
                // Обновляем данные о прогрессе
                RefreshGameData();
                
                // Определяем состояние кнопки
                PlayButtonState newState = DetermineButtonState();
                
                // Обновляем состояние если изменилось
                if (CurrentState != newState)
                {
                    SetState(newState);
                }
                
                GameLogger.LogInfo("PlayButton", $"State updated: {CurrentState}, Level: {_currentLevelNumber}/{_totalLevelsCount}");
            }
            catch (System.Exception ex)
            {
                GameLogger.LogException("PlayButton", ex);
                SetState(PlayButtonState.Error);
            }
        }
        
        /// <summary>
        /// Проверка готовности сервисов
        /// </summary>
        private bool AreServicesReady()
        {
            return _progressService?.IsInitialized == true && 
                   _levelService?.IsInitialized == true &&
                   _sceneService?.IsInitialized == true;
        }
        
        /// <summary>
        /// Обновление данных игры
        /// </summary>
        private void RefreshGameData()
        {
            _currentLevelNumber = _progressService.GetCurrentLevelNumber();
            _totalLevelsCount = _levelService.GetTotalLevelsCount();
            _isGameCompleted = _progressService.AreAllLevelsCompleted(_totalLevelsCount);
            
            GameLogger.LogInfo("PlayButton", $"Game data refreshed: Level {_currentLevelNumber}, Total {_totalLevelsCount}, Completed: {_isGameCompleted}");
        }
        
        /// <summary>
        /// Определение состояния кнопки на основе данных игры
        /// </summary>
        private PlayButtonState DetermineButtonState()
        {
            // Нет доступных уровней
            if (_totalLevelsCount <= 0)
            {
                return PlayButtonState.NoLevels;
            }
            
            // Все уровни пройдены
            if (_isGameCompleted)
            {
                return PlayButtonState.GameCompleted;
            }
            
            // Первый уровень
            if (_currentLevelNumber == 1)
            {
                return PlayButtonState.NewGame;
            }
            
            // Продолжить игру
            return PlayButtonState.Continue;
        }
        
        /// <summary>
        /// Установка состояния кнопки
        /// </summary>
        private void SetState(PlayButtonState state)
        {
            CurrentState = state;
            
            switch (state)
            {
                case PlayButtonState.Loading:
                    SetupLoadingState();
                    break;
                    
                case PlayButtonState.NewGame:
                    SetupNewGameState();
                    break;
                    
                case PlayButtonState.Continue:
                    SetupContinueState();
                    break;
                    
                case PlayButtonState.GameCompleted:
                    SetupGameCompletedState();
                    break;
                    
                case PlayButtonState.NoLevels:
                    SetupNoLevelsState();
                    break;
                    
                case PlayButtonState.Error:
                    SetupErrorState();
                    break;
            }
            
            OnStateChanged?.Invoke(state);
            GameLogger.LogInfo("PlayButton", $"Button state set to: {state}");
        }
        
        /// <summary>
        /// Настройка состояния загрузки
        /// </summary>
        private void SetupLoadingState()
        {
            _button.interactable = false;
            SetButtonText(_loadingText);
            SetButtonColors(_disabledColors);
            
            // Повторная попытка через задержку
            Invoke(nameof(UpdateButtonState), 0.5f);
        }
        
        /// <summary>
        /// Настройка состояния новой игры
        /// </summary>
        private void SetupNewGameState()
        {
            _button.interactable = true;
            SetButtonText(_playText);
            SetButtonColors(_normalColors);
        }
        
        /// <summary>
        /// Настройка состояния продолжения игры
        /// </summary>
        private void SetupContinueState()
        {
            _button.interactable = true;
            SetButtonText($"{_continueText} (Level {_currentLevelNumber})");
            SetButtonColors(_normalColors);
        }
        
        /// <summary>
        /// Настройка состояния завершенной игры
        /// </summary>
        private void SetupGameCompletedState()
        {
            _button.interactable = true;
            SetButtonText(_playAgainText);
            SetButtonColors(_gameCompletedColors);
        }
        
        /// <summary>
        /// Настройка состояния отсутствия уровней
        /// </summary>
        private void SetupNoLevelsState()
        {
            _button.interactable = false;
            SetButtonText(_noLevelsText);
            SetButtonColors(_disabledColors);
        }
        
        /// <summary>
        /// Настройка состояния ошибки
        /// </summary>
        private void SetupErrorState()
        {
            _button.interactable = false;
            SetButtonText(_errorText);
            SetButtonColors(_disabledColors);
        }
        
        /// <summary>
        /// Установка текста кнопки
        /// </summary>
        private void SetButtonText(string text)
        {
            if (_buttonText != null)
            {
                _buttonText.text = text;
            }
        }
        
        /// <summary>
        /// Установка цветовой схемы кнопки
        /// </summary>
        private void SetButtonColors(ColorBlock colors)
        {
            _button.colors = colors;
        }
        
        /// <summary>
        /// Настройка анимаций наведения
        /// </summary>
        private void SetupHoverAnimations()
        {
            if (!_enableHoverAnimation || _rectTransform == null) return;
            
            // Сброс масштаба
            _rectTransform.localScale = Vector3.one;
        }
        
        /// <summary>
        /// Обработка нажатия на кнопку
        /// </summary>
        private async void OnButtonClicked()
        {
            if (!_button.interactable)
            {
                GameLogger.LogWarning("PlayButton", "Button clicked but not interactable");
                return;
            }
            
            GameLogger.LogInfo("PlayButton", $"Play button clicked, state: {CurrentState}");
            
            // Анимация нажатия
            if (_enablePressAnimation)
            {
                await PlayPressAnimation();
            }
            
            // Воспроизводим звук
            _uiService?.PlayUISound(UISoundType.ButtonClick);
            
            try
            {
                await HandleButtonClick();
            }
            catch (System.Exception ex)
            {
                GameLogger.LogException("PlayButton", ex);
                _uiService?.ShowMessage("Failed to start game. Please try again.", 3f);
            }
        }
        
        /// <summary>
        /// Анимация нажатия кнопки
        /// </summary>
        private async UniTask PlayPressAnimation()
        {
            if (_rectTransform == null) return;
            
            await _rectTransform.DOScale(_pressScale, _pressDuration).AsyncWaitForCompletion();
            await _rectTransform.DOScale(1f, _pressDuration).AsyncWaitForCompletion();
        }
        
        /// <summary>
        /// Обработка логики нажатия кнопки
        /// </summary>
        private async UniTask HandleButtonClick()
        {
            switch (CurrentState)
            {
                case PlayButtonState.NewGame:
                case PlayButtonState.Continue:
                    await StartLevel(_currentLevelNumber);
                    break;
                    
                case PlayButtonState.GameCompleted:
                    await HandleGameCompletedClick();
                    break;
                    
                default:
                    GameLogger.LogWarning("PlayButton", $"Unhandled button state: {CurrentState}");
                    break;
            }
        }
        
        /// <summary>
        /// Запуск уровня
        /// </summary>
        private async UniTask StartLevel(int levelId)
        {
            // Проверяем существование уровня
            if (!_levelService.IsLevelExists(levelId))
            {
                GameLogger.LogError("PlayButton", $"Level {levelId} does not exist!");
                _uiService?.ShowMessage("Level not found. Please check game configuration.", 3f);
                return;
            }
            
            // Создаем параметры для игровой сцены
            var gameplayParameters = new MainMenuScreen.GameplayParameters
            {
                LevelId = levelId
            };
            
            GameLogger.LogInfo("PlayButton", $"Starting level {levelId}");
            OnPlayButtonClicked?.Invoke(levelId);
            
            // Загружаем игровую сцену
            _sceneService.ClearSceneParameters();
            await _sceneService.LoadSceneAsync(SceneNames.Gameplay, gameplayParameters);
        }
        
        /// <summary>
        /// Обработка нажатия при завершенной игре
        /// </summary>
        private async UniTask HandleGameCompletedClick()
        {
            string title = "Поздравляем!";
            string message = $"Вы прошли все {_totalLevelsCount} уровней!\n\nХотите начать игру заново?";
            
            bool confirmed = await ShowConfirmDialog(title, message);
            
            if (confirmed)
            {
                GameLogger.LogInfo("PlayButton", "Player confirmed game restart");
                await ResetGameAndStart();
            }
            else
            {
                GameLogger.LogInfo("PlayButton", "Player cancelled game restart");
            }
        }
        
        /// <summary>
        /// Показ диалога подтверждения
        /// </summary>
        private async UniTask<bool> ShowConfirmDialog(string title, string message)
        {
            bool result = false;
            bool dialogCompleted = false;
            
            _uiService?.ShowConfirmDialog(
                title,
                message,
                onConfirm: () => { result = true; dialogCompleted = true; },
                onCancel: () => { result = false; dialogCompleted = true; }
            );
            
            // Ждем завершения диалога
            while (!dialogCompleted)
            {
                await UniTask.Delay(50);
            }
            
            return result;
        }
        
        /// <summary>
        /// Сброс игры и запуск заново
        /// </summary>
        private async UniTask ResetGameAndStart()
        {
            try
            {
                // Сбрасываем прогресс
                _progressService.ResetProgress();
                GameLogger.LogInfo("PlayButton", "Progress reset completed");
                
                // Обновляем состояние кнопки
                UpdateButtonState();
                
                // Показываем сообщение
                _uiService?.ShowMessage("Прогресс сброшен! Начинаем заново.", 2f);
                
                // Небольшая задержка для показа сообщения
                await UniTask.Delay(1000);
                
                // Запускаем первый уровень
                await StartLevel(1);
            }
            catch (System.Exception ex)
            {
                GameLogger.LogException("PlayButton", ex);
                _uiService?.ShowMessage("Ошибка при сбросе прогресса. Попробуйте еще раз.", 3f);
            }
        }
        
        /// <summary>
        /// Очистка ресурсов
        /// </summary>
        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnButtonClicked);
            }
            
            DOTween.Kill(_rectTransform);
            
            OnPlayButtonClicked = null;
            OnStateChanged = null;
        }
        
        /// <summary>
        /// Получение отладочной информации
        /// </summary>
        public string GetDebugInfo()
        {
            return $"PlayButton Debug:\n" +
                   $"- State: {CurrentState}\n" +
                   $"- Current Level: {_currentLevelNumber}\n" +
                   $"- Total Levels: {_totalLevelsCount}\n" +
                   $"- Game Completed: {_isGameCompleted}\n" +
                   $"- Interactable: {_button?.interactable}\n" +
                   $"- Services Ready: {AreServicesReady()}";
        }
        
        #if UNITY_EDITOR
        /// <summary>
        /// Тестовые методы для редактора
        /// </summary>
        [ContextMenu("Test Update State")]
        private void TestUpdateState()
        {
            UpdateButtonState();
        }
        
        [ContextMenu("Show Debug Info")]
        private void ShowDebugInfo()
        {
            Debug.Log(GetDebugInfo());
        }
        #endif
    }
    
    /// <summary>
    /// Состояния кнопки Play
    /// </summary>
    public enum PlayButtonState
    {
        Loading,        // Загрузка данных
        NewGame,        // Начать новую игру (первый уровень)
        Continue,       // Продолжить игру
        GameCompleted,  // Игра завершена, предложить начать заново
        NoLevels,       // Нет доступных уровней
        Error           // Ошибка
    }
}