using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using WordPuzzle.Core.Architecture;
using WordPuzzle.Core.Services;
using WordPuzzle.UI.Components;
using System.Threading.Tasks;

namespace WordPuzzle.UI.Screens
{
    /// <summary>
    /// Экран главного меню игры (ОБНОВЛЕН для Этапа 5)
    /// Использует компоненты LevelCounter и PlayButton для модульности
    /// </summary>
    public class MainMenuScreen : BaseScreen
    {
        [Header("UI Components")]
        [SerializeField] private LevelCounter _levelCounter;
        [SerializeField] private PlayButton _playButton;
        [SerializeField] private TextMeshProUGUI _titleText;
        
        [Header("Additional UI")]
        [SerializeField] private Button _resetProgressButton; // Для отладки
        [SerializeField] private RectTransform _mainPanel;
        
        [Header("Settings")]
        [SerializeField] private string _titleTextContent = "Word Puzzle Game";
        [SerializeField] private bool _showDebugResetButton = true;
        
        [Header("Animations")]
        [SerializeField] private bool _enableEntranceAnimation = true;
        [SerializeField] private float _entranceAnimationDuration = 0.8f;
        [SerializeField] private Ease _entranceEase = Ease.OutBack;
        [SerializeField] private float _titleAnimationDelay = 0.2f;
        [SerializeField] private float _componentsAnimationDelay = 0.4f;
        
        protected override string ScreenName => "MainMenu";
        
        /// <summary>
        /// Инициализация главного меню
        /// </summary>
        protected override void OnInitialize()
        {
            GameLogger.LogInfo(ScreenName, "Setting up Main Menu with modular components...");
            
            SetupTitle();
            SetupComponents();
            SetupDebugButton();
            
            if (_enableEntranceAnimation)
            {
                PlayEntranceAnimation();
            }
            
            GameLogger.LogInfo(ScreenName, "Main Menu setup completed");
        }
        
        /// <summary>
        /// Подписка на события UI
        /// </summary>
        protected override void SubscribeToUIEvents()
        {
            // Подписываемся на событие Play button
            if (_playButton != null)
            {
                _playButton.OnPlayButtonClicked += OnPlayButtonClicked;
                _playButton.OnStateChanged += OnPlayButtonStateChanged;
            }
            else
            {
                GameLogger.LogWarning(ScreenName, "PlayButton component is not assigned!");
            }
            
            // Подписываемся на кнопку сброса прогресса если она есть
            if (_resetProgressButton != null)
            {
                _resetProgressButton.onClick.AddListener(OnResetProgressClicked);
            }
        }
        
        /// <summary>
        /// Отписка от событий UI
        /// </summary>
        protected override void UnsubscribeFromUIEvents()
        {
            if (_playButton != null)
            {
                _playButton.OnPlayButtonClicked -= OnPlayButtonClicked;
                _playButton.OnStateChanged -= OnPlayButtonStateChanged;
            }
            
            if (_resetProgressButton != null)
            {
                _resetProgressButton.onClick.RemoveListener(OnResetProgressClicked);
            }
        }
        
        /// <summary>
        /// Настройка заголовка
        /// </summary>
        private void SetupTitle()
        {
            if (_titleText != null)
            {
                _titleText.text = _titleTextContent;
                GameLogger.LogInfo(ScreenName, $"Title set to: {_titleTextContent}");
                
                if (_enableEntranceAnimation)
                {
                    // Подготовка к анимации - делаем невидимым
                    _titleText.alpha = 0f;
                }
            }
            else
            {
                GameLogger.LogWarning(ScreenName, "Title text component is not assigned!");
            }
        }
        
        /// <summary>
        /// Настройка компонентов
        /// </summary>
        private void SetupComponents()
        {
            // Проверяем наличие компонентов
            if (_levelCounter == null)
            {
                GameLogger.LogWarning(ScreenName, "LevelCounter component is not assigned! Looking in children...");
                _levelCounter = GetComponentInChildren<LevelCounter>();
            }
            
            if (_playButton == null)
            {
                GameLogger.LogWarning(ScreenName, "PlayButton component is not assigned! Looking in children...");
                _playButton = GetComponentInChildren<PlayButton>();
            }
            
            // Подготовка к анимации
            if (_enableEntranceAnimation)
            {
                PrepareComponentsForAnimation();
            }
            
            GameLogger.LogInfo(ScreenName, $"Components setup: LevelCounter={_levelCounter != null}, PlayButton={_playButton != null}");
        }
        
        /// <summary>
        /// Настройка кнопки сброса прогресса для отладки
        /// </summary>
        private void SetupDebugButton()
        {
            if (_resetProgressButton != null)
            {
                bool shouldShow = _showDebugResetButton;
                
                #if !UNITY_EDITOR && !DEVELOPMENT_BUILD
                shouldShow = false; // Скрываем в релизной сборке
                #endif
                
                _resetProgressButton.gameObject.SetActive(shouldShow);
                
                if (shouldShow)
                {
                    var buttonText = _resetProgressButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.text = "Reset Progress (Debug)";
                    }
                }
                
                GameLogger.LogInfo(ScreenName, $"Debug reset button: {(shouldShow ? "shown" : "hidden")}");
            }
        }
        
        /// <summary>
        /// Подготовка компонентов к анимации входа
        /// </summary>
        private void PrepareComponentsForAnimation()
        {
            if (_mainPanel != null)
            {
                _mainPanel.localScale = Vector3.zero;
            }
            else
            {
                // Подготавливаем отдельные компоненты
                if (_levelCounter != null)
                {
                    var counterRect = _levelCounter.GetComponent<RectTransform>();
                    if (counterRect != null) counterRect.localScale = Vector3.zero;
                }
                
                if (_playButton != null)
                {
                    var buttonRect = _playButton.GetComponent<RectTransform>();
                    if (buttonRect != null) buttonRect.localScale = Vector3.zero;
                }
            }
        }
        
        /// <summary>
        /// Анимация входа в главное меню
        /// </summary>
        private async void PlayEntranceAnimation()
        {
            GameLogger.LogInfo(ScreenName, "Playing entrance animation...");
            
            var sequence = DOTween.Sequence();
            
            // Анимация заголовка
            if (_titleText != null)
            {
                sequence.Insert(_titleAnimationDelay, 
                    _titleText.DOFade(1f, _entranceAnimationDuration * 0.6f).SetEase(Ease.OutQuad));
            }
            
            // Анимация основной панели или отдельных компонентов
            if (_mainPanel != null)
            {
                sequence.Insert(_componentsAnimationDelay,
                    _mainPanel.DOScale(Vector3.one, _entranceAnimationDuration).SetEase(_entranceEase));
            }
            else
            {
                // Анимируем компоненты отдельно
                if (_levelCounter != null)
                {
                    var counterRect = _levelCounter.GetComponent<RectTransform>();
                    if (counterRect != null)
                    {
                        sequence.Insert(_componentsAnimationDelay,
                            counterRect.DOScale(Vector3.one, _entranceAnimationDuration * 0.8f).SetEase(_entranceEase));
                    }
                }
                
                if (_playButton != null)
                {
                    var buttonRect = _playButton.GetComponent<RectTransform>();
                    if (buttonRect != null)
                    {
                        sequence.Insert(_componentsAnimationDelay + 0.1f,
                            buttonRect.DOScale(Vector3.one, _entranceAnimationDuration * 0.8f).SetEase(_entranceEase));
                    }
                }
            }
            
            // Ждем завершения анимации
            await sequence.AsyncWaitForCompletion();
            
            GameLogger.LogInfo(ScreenName, "Entrance animation completed");
        }
        
        /// <summary>
        /// Обновление всех компонентов при возврате в меню
        /// </summary>
        private async void OnEnable()
        {
            // Обновляем компоненты только если экран уже инициализирован
            if (IsInitialized)
            {
                GameLogger.LogInfo(ScreenName, "Main menu became active - refreshing all components");
                
                // Принудительно обновляем прогресс
                await RefreshProgressAndUpdateComponents();
            }
        }
        
        /// <summary>
        /// Обновление прогресса и всех компонентов
        /// </summary>
        private async UniTask RefreshProgressAndUpdateComponents()
        {
            try
            {
                // Обновляем прогресс из сохранения
                if (ProgressService?.IsInitialized == true)
                {
                    var progressServiceImpl = ProgressService as WordPuzzle.Data.Persistence.ProgressService;
                    if (progressServiceImpl != null)
                    {
                        await progressServiceImpl.RefreshProgressAsync();
                        GameLogger.LogInfo(ScreenName, "Progress refreshed from disk");
                    }
                }
                
                // Обновляем счетчик уровней
                if (_levelCounter != null)
                {
                    _levelCounter.UpdateDisplay();
                }
                
                // Обновляем кнопку Play
                if (_playButton != null)
                {
                    _playButton.UpdateButtonState();
                }
                
                GameLogger.LogInfo(ScreenName, "All components updated successfully");
            }
            catch (System.Exception ex)
            {
                GameLogger.LogException(ScreenName, ex);
                
                // В случае ошибки просто обновляем компоненты с текущими данными
                _levelCounter?.UpdateDisplay();
                _playButton?.UpdateButtonState();
            }
        }
        
        /// <summary>
        /// Обработка нажатия Play button через компонент
        /// </summary>
        private void OnPlayButtonClicked(int levelId)
        {
            GameLogger.LogInfo(ScreenName, $"Play button clicked from component, level: {levelId}");
            
            // Дополнительная логика при необходимости
            // Основная логика уже в PlayButton компоненте
        }
        
        /// <summary>
        /// Обработка изменения состояния Play button
        /// </summary>
        private void OnPlayButtonStateChanged(PlayButtonState newState)
        {
            GameLogger.LogInfo(ScreenName, $"Play button state changed to: {newState}");
            
            // Можно добавить дополнительные эффекты в зависимости от состояния
            switch (newState)
            {
                case PlayButtonState.GameCompleted:
                    // Например, можно добавить конфетти или другие эффекты
                    UIService?.PlayUISound(UISoundType.Success);
                    break;
                    
                case PlayButtonState.Error:
                    UIService?.PlayUISound(UISoundType.Error);
                    break;
            }
        }
        
        /// <summary>
        /// Обработка кнопки сброса прогресса (отладка)
        /// </summary>
        private void OnResetProgressClicked()
        {
            GameLogger.LogInfo(ScreenName, "Reset progress button clicked (debug)");
            
            UIService?.ShowConfirmDialog(
                "Reset Progress (Debug)",
                "Are you sure you want to reset all progress? This cannot be undone.\n\n(This is a debug feature)",
                onConfirm: async () =>
                {
                    try
                    {
                        // Сбрасываем прогресс
                        ProgressService.ResetProgress();
                        
                        // Сбрасываем анимации компонентов для демонстрации
                        _levelCounter?.ResetAnimation();
                        
                        // Обновляем компоненты
                        await RefreshProgressAndUpdateComponents();
                        
                        UIService?.ShowMessage("Progress reset successfully! (Debug)", 2f);
                        GameLogger.LogInfo(ScreenName, "Debug progress reset completed");
                    }
                    catch (System.Exception ex)
                    {
                        GameLogger.LogException(ScreenName, ex);
                        UIService?.ShowMessage("Failed to reset progress", 3f);
                    }
                },
                onCancel: () =>
                {
                    GameLogger.LogInfo(ScreenName, "Debug progress reset cancelled");
                }
            );
        }
        
        /// <summary>
        /// Очистка ресурсов
        /// </summary>
        protected override void OnCleanup()
        {
            // Останавливаем все анимации
            DOTween.Kill(this);
            
            if (_titleText != null)
            {
                DOTween.Kill(_titleText);
            }
            
            if (_mainPanel != null)
            {
                DOTween.Kill(_mainPanel);
            }
        }
        
        /// <summary>
        /// Получение отладочной информации
        /// </summary>
        public string GetDebugInfo()
        {
            var levelCounterInfo = _levelCounter?.GetDebugInfo() ?? "LevelCounter: null";
            var playButtonInfo = _playButton?.GetDebugInfo() ?? "PlayButton: null";
            
            return $"MainMenuScreen Debug:\n" +
                   $"- Screen Initialized: {IsInitialized}\n" +
                   $"- Components Found: LC={_levelCounter != null}, PB={_playButton != null}\n" +
                   $"- Services Ready: Progress={ProgressService?.IsInitialized}, Level={LevelService?.IsInitialized}\n\n" +
                   $"{levelCounterInfo}\n\n" +
                   $"{playButtonInfo}";
        }
        
        #if UNITY_EDITOR
        /// <summary>
        /// Тестовые методы для редактора
        /// </summary>
        [ContextMenu("Test Refresh Components")]
        private async Task TestRefreshComponents()
        {
            await RefreshProgressAndUpdateComponents();
        }
        
        [ContextMenu("Test Entrance Animation")]
        private void TestEntranceAnimation()
        {
            if (_enableEntranceAnimation)
            {
                PrepareComponentsForAnimation();
                PlayEntranceAnimation();
            }
        }
        
        [ContextMenu("Show Debug Info")]
        private void ShowDebugInfo()
        {
            Debug.Log(GetDebugInfo());
        }
        #endif
        
        /// <summary>
        /// Класс параметров для игрового экрана
        /// Сохранен для совместимости с существующим кодом
        /// </summary>
        [System.Serializable]
        public class GameplayParameters
        {
            public int LevelId;
        }
    }
}