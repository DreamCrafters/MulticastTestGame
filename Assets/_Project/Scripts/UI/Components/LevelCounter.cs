using UnityEngine;
using TMPro;
using DG.Tweening;
using VContainer;
using WordPuzzle.Core.Architecture;
using WordPuzzle.Core.Services;

namespace WordPuzzle.UI.Components
{
    /// <summary>
    /// Компонент для отображения счетчика пройденных уровней
    /// Обеспечивает автоматическое обновление с анимациями
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LevelCounter : MonoBehaviour
    {
        [Header("Display Settings")]
        [SerializeField] private string _defaultFormat = "Levels Completed: {0}";
        [SerializeField] private string _progressFormat = "Completed: {0}/{1}";
        [SerializeField] private string _gameCompletedFormat = "Game Completed! ({0}/{1})";
        [SerializeField] private string _loadingText = "Loading...";
        [SerializeField] private string _errorText = "Error loading progress";
        
        [Header("Animation Settings")]
        [SerializeField] private bool _enableCountUpAnimation = true;
        [SerializeField] private float _countUpDuration = 1.0f;
        [SerializeField] private Ease _countUpEase = Ease.OutQuad;
        [SerializeField] private bool _enableColorAnimation = true;
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _completedColor = Color.green;
        [SerializeField] private float _colorTransitionDuration = 0.5f;
        
        // Инжекция зависимостей через VContainer
        [Inject] private IProgressService _progressService;
        [Inject] private ILevelService _levelService;
        
        private TextMeshProUGUI _text;
        private int _lastDisplayedCount = -1;
        private bool _isInitialized = false;
        
        /// <summary>
        /// Текущее количество пройденных уровней
        /// </summary>
        public int CurrentCompletedCount { get; private set; }
        
        /// <summary>
        /// Общее количество доступных уровней
        /// </summary>
        public int TotalLevelsCount { get; private set; }
        
        /// <summary>
        /// Инициализация компонента
        /// </summary>
        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
            
            if (_text == null)
            {
                GameLogger.LogError("LevelCounter", "TextMeshProUGUI component not found!");
                return;
            }
            
            // Устанавливаем начальный цвет
            _text.color = _normalColor;
            
            GameLogger.LogInfo("LevelCounter", "Level Counter component initialized");
        }
        
        /// <summary>
        /// Автоматический запуск обновления при активации
        /// </summary>
        private void OnEnable()
        {
            if (_isInitialized == false)
            {
                _isInitialized = true;
                UpdateDisplay();
            }
            else
            {
                // При повторной активации обновляем с задержкой
                Invoke(nameof(UpdateDisplay), 0.1f);
            }
        }
        
        /// <summary>
        /// Публичный метод для принудительного обновления счетчика
        /// Вызывается из MainMenuScreen при необходимости
        /// </summary>
        public void UpdateDisplay()
        {
            if (_text == null)
            {
                GameLogger.LogWarning("LevelCounter", "Cannot update display - text component is null");
                return;
            }
            
            // Проверяем готовность сервисов
            if (!AreServicesReady())
            {
                ShowLoadingState();
                return;
            }
            
            try
            {
                // Получаем актуальные данные
                RefreshProgressData();
                
                // Определяем текст для отображения
                string displayText = GetDisplayText();
                
                // Обновляем с анимацией или без
                if (ShouldAnimateCountUp())
                {
                    AnimateCountUp(displayText);
                }
                else
                {
                    SetTextImmediate(displayText);
                }
                
                // Обновляем цвет если необходимо
                if (_enableColorAnimation)
                {
                    UpdateTextColor();
                }
                
                GameLogger.LogInfo("LevelCounter", $"Display updated: {CurrentCompletedCount}/{TotalLevelsCount}");
            }
            catch (System.Exception ex)
            {
                GameLogger.LogException("LevelCounter", ex);
                ShowErrorState();
            }
        }
        
        /// <summary>
        /// Проверка готовности сервисов
        /// </summary>
        private bool AreServicesReady()
        {
            return _progressService?.IsInitialized == true && 
                   _levelService?.IsInitialized == true;
        }
        
        /// <summary>
        /// Обновление данных прогресса
        /// </summary>
        private void RefreshProgressData()
        {
            CurrentCompletedCount = _progressService.GetCompletedLevelsCount();
            TotalLevelsCount = _levelService.GetTotalLevelsCount();
            
            GameLogger.LogInfo("LevelCounter", $"Progress data refreshed: {CurrentCompletedCount}/{TotalLevelsCount}");
        }
        
        /// <summary>
        /// Получение текста для отображения
        /// </summary>
        private string GetDisplayText()
        {
            // Если нет доступных уровней
            if (TotalLevelsCount <= 0)
            {
                return string.Format(_defaultFormat, CurrentCompletedCount);
            }
            
            // Если все уровни пройдены
            if (IsGameCompleted())
            {
                return string.Format(_gameCompletedFormat, CurrentCompletedCount, TotalLevelsCount);
            }
            
            // Обычное отображение прогресса
            return string.Format(_progressFormat, CurrentCompletedCount, TotalLevelsCount);
        }
        
        /// <summary>
        /// Проверка завершения всех уровней
        /// </summary>
        private bool IsGameCompleted()
        {
            return TotalLevelsCount > 0 && 
                   _progressService.AreAllLevelsCompleted(TotalLevelsCount);
        }
        
        /// <summary>
        /// Нужно ли анимировать увеличение счетчика
        /// </summary>
        private bool ShouldAnimateCountUp()
        {
            return _enableCountUpAnimation && 
                   _lastDisplayedCount >= 0 && 
                   CurrentCompletedCount > _lastDisplayedCount;
        }
        
        /// <summary>
        /// Анимация увеличения счетчика
        /// </summary>
        private void AnimateCountUp(string finalText)
        {
            if (_lastDisplayedCount < 0)
            {
                SetTextImmediate(finalText);
                return;
            }
            
            GameLogger.LogInfo("LevelCounter", $"Animating count up from {_lastDisplayedCount} to {CurrentCompletedCount}");
            
            // Анимируем промежуточные значения
            DOTween.To(
                getter: () => _lastDisplayedCount,
                setter: value => {
                    string intermediateText;
                    if (TotalLevelsCount > 0)
                    {
                        intermediateText = string.Format(_progressFormat, value, TotalLevelsCount);
                    }
                    else
                    {
                        intermediateText = string.Format(_defaultFormat, value);
                    }
                    _text.text = intermediateText;
                },
                endValue: CurrentCompletedCount,
                duration: _countUpDuration
            )
            .SetEase(_countUpEase)
            .OnComplete(() => {
                _text.text = finalText;
                _lastDisplayedCount = CurrentCompletedCount;
            });
        }
        
        /// <summary>
        /// Мгновенная установка текста
        /// </summary>
        private void SetTextImmediate(string text)
        {
            _text.text = text;
            _lastDisplayedCount = CurrentCompletedCount;
        }
        
        /// <summary>
        /// Обновление цвета текста
        /// </summary>
        private void UpdateTextColor()
        {
            Color targetColor = IsGameCompleted() ? _completedColor : _normalColor;
            
            if (_text.color != targetColor)
            {
                _text.DOColor(targetColor, _colorTransitionDuration);
                GameLogger.LogInfo("LevelCounter", $"Animating color to {(IsGameCompleted() ? "completed" : "normal")}");
            }
        }
        
        /// <summary>
        /// Отображение состояния загрузки
        /// </summary>
        private void ShowLoadingState()
        {
            _text.text = _loadingText;
            _text.color = _normalColor;
            
            // Повторная попытка через небольшую задержку
            Invoke(nameof(UpdateDisplay), 0.5f);
        }
        
        /// <summary>
        /// Отображение состояния ошибки
        /// </summary>
        private void ShowErrorState()
        {
            _text.text = _errorText;
            _text.color = Color.red;
        }
        
        /// <summary>
        /// Публичный метод для сброса анимации
        /// Полезен при сбросе прогресса
        /// </summary>
        public void ResetAnimation()
        {
            _lastDisplayedCount = -1;
            DOTween.Kill(_text);
            
            GameLogger.LogInfo("LevelCounter", "Animation reset");
        }
        
        /// <summary>
        /// Очистка ресурсов при уничтожении
        /// </summary>
        private void OnDestroy()
        {
            // Останавливаем все анимации DOTween связанные с этим компонентом
            DOTween.Kill(_text);
        }
        
        /// <summary>
        /// Получение отладочной информации
        /// </summary>
        public string GetDebugInfo()
        {
            return $"LevelCounter Debug:\n" +
                   $"- Current: {CurrentCompletedCount}\n" +
                   $"- Total: {TotalLevelsCount}\n" +
                   $"- Last Displayed: {_lastDisplayedCount}\n" +
                   $"- Game Completed: {IsGameCompleted()}\n" +
                   $"- Services Ready: {AreServicesReady()}";
        }
        
        #if UNITY_EDITOR
        /// <summary>
        /// Тестовый метод для отладки в редакторе
        /// </summary>
        [ContextMenu("Test Update Display")]
        private void TestUpdateDisplay()
        {
            UpdateDisplay();
        }
        
        [ContextMenu("Test Reset Animation")]
        private void TestResetAnimation()
        {
            ResetAnimation();
        }
        
        [ContextMenu("Show Debug Info")]
        private void ShowDebugInfo()
        {
            Debug.Log(GetDebugInfo());
        }
        #endif
    }
}