using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace WordPuzzle.UI.Components
{
    /// <summary>
    /// Компонент всплывающего сообщения
    /// ИСПРАВЛЕНО: мобильно-безопасное позиционирование с учетом Safe Area
    /// Позиционируется в правом верхнем углу с настраиваемыми отступами
    /// Автоматически учитывает notch и другие системные элементы мобильных устройств

    /// </summary>
    public class MessagePopup : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Animation Settings")]
        [SerializeField] private float _showDuration = 0.3f;
        [SerializeField] private float _hideDuration = 0.3f;
        [SerializeField] private Ease _showEase = Ease.OutBack;
        [SerializeField] private Ease _hideEase = Ease.InQuad;
        
        [Header("Positioning Settings")]
        [SerializeField] private float _rightMargin = 120f;  // Отступ справа
        [SerializeField] private float _topMargin = 80f;     // Отступ сверху
        [SerializeField] private Vector2 _messageSize = new Vector2(400f, 80f); // Размер сообщения


        private Action _onHideComplete;

        /// <summary>
        /// Инициализация компонента
        /// </summary>
        private void Awake()
        {
            SetupMobileSafePositioning();
            SetupComponents();
        }
        
        /// <summary>
        /// Настройка мобильно-безопасного позиционирования с учетом Safe Area
        /// </summary>
        private void SetupMobileSafePositioning()
        {
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }
            
            // Получаем Safe Area для мобильных устройств
            var safeArea = Screen.safeArea;
            var screenWidth = Screen.width;
            var screenHeight = Screen.height;
            
            // Дополнительные отступы для устройств с notch/safe area
            float additionalTopMargin = 0f;
            float additionalRightMargin = 0f;
            
            // Если Safe Area отличается от полного экрана, добавляем дополнительные отступы
            if (safeArea.y > 0) // Есть отступ сверху (notch или status bar)
            {
                additionalTopMargin = safeArea.y * 0.5f; // Дополнительный отступ
            }
            
            if (safeArea.x + safeArea.width < screenWidth) // Есть отступ справа
            {
                additionalRightMargin = (screenWidth - safeArea.x - safeArea.width) * 0.5f;
            }
            
            // Позиционирование в правом верхнем углу с безопасными отступами
            rectTransform.anchorMin = new Vector2(1, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.sizeDelta = _messageSize;
            
            var finalRightMargin = _rightMargin + additionalRightMargin;
            var finalTopMargin = _topMargin + additionalTopMargin;
            
            rectTransform.anchoredPosition = new Vector2(-finalRightMargin, -finalTopMargin);
            
            Debug.Log($"[MessagePopup] Mobile-safe positioning applied: " +
                     $"rightMargin={finalRightMargin} ({_rightMargin}+{additionalRightMargin}), " +
                     $"topMargin={finalTopMargin} ({_topMargin}+{additionalTopMargin}), " +
                     $"safeArea={safeArea}, screen={screenWidth}x{screenHeight}");
        }
        
        /// <summary>
        /// Настройка позиционирования с кастомными параметрами
        /// </summary>
        public void SetCustomPositioning(float rightMargin, float topMargin, Vector2 size)
        {
            _rightMargin = rightMargin;
            _topMargin = topMargin;
            _messageSize = size;
            
            SetupMobileSafePositioning();
        }

        /// <summary>
        /// Настройка компонентов с правильным порядком
        /// </summary>
        private void SetupComponents()
        {
            // Сначала создаем/проверяем CanvasGroup ПЕРЕД его использованием
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                {
                    _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                    Debug.Log("[MessagePopup] CanvasGroup component added dynamically");
                }
            }

            CreateBasicElements();

            // Теперь безопасно устанавливаем параметры
            _canvasGroup.alpha = 0f;
            transform.localScale = Vector3.zero;
            gameObject.SetActive(false);
            
            Debug.Log("[MessagePopup] MessagePopup components setup completed successfully");
        }

        /// <summary>
        /// Создание базовых элементов с правильным позиционированием для мобильных
        /// </summary>
        private void CreateBasicElements()
        {
            // Настройка основного RectTransform для центрированного позиционирования
            var mainRect = GetComponent<RectTransform>();
            if (mainRect == null)
            {
                mainRect = gameObject.AddComponent<RectTransform>();
            }
            
            // ИСПРАВЛЕНО: Позиционирование по центру вверху экрана, безопасно для мобильных
            mainRect.anchorMin = new Vector2(0.5f, 1f);  // Центр верха
            mainRect.anchorMax = new Vector2(0.5f, 1f);  // Центр верха
            mainRect.sizeDelta = _messageSize;           // Фиксированный размер
            mainRect.anchoredPosition = new Vector2(0, -_topMargin); // Отступ сверху
            
            // Background Image
            if (_backgroundImage == null)
            {
                _backgroundImage = gameObject.AddComponent<Image>();
                _backgroundImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f); // Чуть более непрозрачный
                
                // Добавляем закругленные углы если возможно
                _backgroundImage.type = Image.Type.Sliced;
                
                Debug.Log("[MessagePopup] Background image created with mobile-friendly positioning");
            }

            // Message Text
            if (_messageText == null)
            {
                var textObject = new GameObject("MessageText");
                textObject.transform.SetParent(transform, false);

                _messageText = textObject.AddComponent<TextMeshProUGUI>();
                _messageText.fontSize = 20; // Немного меньше для мобильных
                _messageText.color = Color.white;
                _messageText.alignment = TextAlignmentOptions.Center;
                _messageText.textWrappingMode = TextWrappingModes.Normal;
                _messageText.margin = new Vector4(15, 10, 15, 10); // Отступы для читаемости

                var textRect = textObject.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
                
                Debug.Log("[MessagePopup] Message text created with mobile-friendly settings");
            }
        }

        /// <summary>
        /// Показать сообщение
        /// </summary>
        public void Show(string message, float duration, Action onComplete = null)
        {
            if (_canvasGroup == null)
            {
                Debug.LogError("[MessagePopup] Cannot show message - CanvasGroup is null!");
                onComplete?.Invoke();
                return;
            }
            
            _onHideComplete = onComplete;

            gameObject.SetActive(true);
            _messageText.text = message;

            // Анимация появления с движением сверху вниз
            var startPosition = transform.localPosition;
            var targetPosition = startPosition;
            startPosition.y += 50f; // Начинаем чуть выше
            transform.localPosition = startPosition;

            var sequence = DOTween.Sequence();
            sequence.Append(_canvasGroup.DOFade(1f, _showDuration).SetEase(_showEase));
            sequence.Join(transform.DOScale(Vector3.one, _showDuration).SetEase(_showEase));
            sequence.Join(transform.DOLocalMove(targetPosition, _showDuration).SetEase(_showEase));
            sequence.AppendInterval(duration);
            sequence.AppendCallback(Hide);
            
            Debug.Log($"[MessagePopup] Message shown at mobile-safe position: {message} (duration: {duration}s)");
        }

        /// <summary>
        /// Скрыть сообщение
        /// </summary>
        public void Hide()
        {
            if (_canvasGroup == null)
            {
                Debug.LogError("[MessagePopup] Cannot hide message - CanvasGroup is null!");
                gameObject.SetActive(false);
                _onHideComplete?.Invoke();
                Destroy(gameObject);
                return;
            }
            
            var sequence = DOTween.Sequence();
            sequence.Append(_canvasGroup.DOFade(0f, _hideDuration).SetEase(_hideEase));
            sequence.Join(transform.DOScale(Vector3.zero, _hideDuration).SetEase(_hideEase));
            // Анимация движения вверх при исчезновении
            sequence.Join(transform.DOLocalMoveY(transform.localPosition.y + 30f, _hideDuration).SetEase(_hideEase));
            sequence.OnComplete(() =>
            {
                gameObject.SetActive(false);
                _onHideComplete?.Invoke();
                Destroy(gameObject);
            });
            
            Debug.Log("[MessagePopup] Message hidden");
        }

        /// <summary>
        /// Очистка при уничтожении
        /// </summary>
        private void OnDestroy()
        {
            DOTween.Kill(this);
        }
        
        /// <summary>
        /// Валидация компонента в редакторе
        /// </summary>
        private void OnValidate()
        {
            // Автоматически находим CanvasGroup если он не назначен
            if (_canvasGroup == null && gameObject != null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }
        }
    }
}