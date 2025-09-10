using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace WordPuzzle.UI.Components
{
    /// <summary>
    /// Компонент диалога подтверждения
    /// ПОЛНОСТЬЮ ИСПРАВЛЕНО: правильное создание UI объектов с RectTransform
    /// </summary>
    public class ConfirmDialog : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Animation Settings")]
        [SerializeField] private float _showDuration = 0.3f;
        [SerializeField] private float _hideDuration = 0.2f;
        [SerializeField] private Ease _showEase = Ease.OutBack;
        [SerializeField] private Ease _hideEase = Ease.InQuad;

        private Action _onConfirm;
        private Action _onCancel;
        private Action _onClose;

        /// <summary>
        /// Инициализация компонента
        /// </summary>
        private void Awake()
        {
            SetupComponents();
        }

        /// <summary>
        /// ИСПРАВЛЕНО: Правильная настройка всех компонентов
        /// </summary>
        private void SetupComponents()
        {
            try 
            {
                Debug.Log("[ConfirmDialog] Starting SetupComponents...");
                
                // ИСПРАВЛЕНИЕ: Сначала создаем/проверяем CanvasGroup ПЕРЕД его использованием
                if (_canvasGroup == null)
                {
                    _canvasGroup = GetComponent<CanvasGroup>();
                    if (_canvasGroup == null)
                    {
                        _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                        Debug.Log("[ConfirmDialog] CanvasGroup component added dynamically");
                    }
                }

                CreateBasicElements();
                SetupButtons();

                // ИСПРАВЛЕНИЕ: Теперь безопасно устанавливаем параметры, так как CanvasGroup точно существует
                _canvasGroup.alpha = 0f;
                transform.localScale = Vector3.zero;
                gameObject.SetActive(false);
                
                Debug.Log("[ConfirmDialog] ConfirmDialog components setup completed successfully");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ConfirmDialog] Error in SetupComponents: {ex.Message}");
                Debug.LogException(ex);
                
                // Создаем минимальные fallback компоненты
                CreateFallbackDialog();
            }
        }

        /// <summary>
        /// ПОЛНОСТЬЮ ПЕРЕПИСАНО: Правильное создание UI объектов
        /// </summary>
        private void CreateBasicElements()
        {
            Debug.Log("[ConfirmDialog] Creating basic elements...");
            
            // Background
            if (_backgroundImage == null)
            {
                _backgroundImage = gameObject.GetComponent<Image>();
                if (_backgroundImage == null)
                {
                    _backgroundImage = gameObject.AddComponent<Image>();
                }
                _backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);

                // Настройка размера
                var rect = GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.sizeDelta = new Vector2(500, 300);
                }
                
                Debug.Log("[ConfirmDialog] Background image created");
            }

            // ИСПРАВЛЕНО: Правильное создание UI объектов с RectTransform
            // Title
            if (_titleText == null)
            {
                Debug.Log("[ConfirmDialog] Creating title text...");
                _titleText = CreateUITextElement("Title", new Vector2(0f, 0.7f), new Vector2(1f, 0.9f), 28, FontStyles.Bold);
                Debug.Log("[ConfirmDialog] Title text created successfully");
            }

            // Message  
            if (_messageText == null)
            {
                Debug.Log("[ConfirmDialog] Creating message text...");
                _messageText = CreateUITextElement("Message", new Vector2(0f, 0.3f), new Vector2(1f, 0.7f), 20, FontStyles.Normal);
                _messageText.textWrappingMode = TextWrappingModes.Normal;
                Debug.Log("[ConfirmDialog] Message text created successfully");
            }
            
            Debug.Log("[ConfirmDialog] Basic elements creation completed");
        }

        /// <summary>
        /// НОВОЕ: Метод для правильного создания UI текстовых элементов
        /// </summary>
        private TextMeshProUGUI CreateUITextElement(string name, Vector2 anchorMin, Vector2 anchorMax, float fontSize, FontStyles fontStyle)
        {
            // Создаем GameObject с RectTransform для UI
            var textObject = new GameObject(name, typeof(RectTransform));
            textObject.transform.SetParent(transform, false);

            // Добавляем TextMeshProUGUI компонент
            var textComponent = textObject.AddComponent<TextMeshProUGUI>();
            textComponent.text = name;
            textComponent.fontSize = fontSize;
            textComponent.color = Color.white;
            textComponent.alignment = TextAlignmentOptions.Center;
            textComponent.fontStyle = fontStyle;

            // Настраиваем RectTransform
            var rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = new Vector2(20, 0);
            rectTransform.offsetMax = new Vector2(-20, 0);

            return textComponent;
        }

        /// <summary>
        /// ИСПРАВЛЕНО: Правильное создание кнопок
        /// </summary>
        private void SetupButtons()
        {
            Debug.Log("[ConfirmDialog] Setting up buttons...");
            
            try
            {
                // Confirm Button
                if (_confirmButton == null)
                {
                    _confirmButton = CreateUIButton("ConfirmButton", new Vector2(0.6f, 0.1f), new Vector2(0.9f, 0.25f), 
                                                  new Color(0.2f, 0.7f, 0.3f, 1f), "OK");
                    Debug.Log("[ConfirmDialog] Confirm button created");
                }

                // Cancel Button
                if (_cancelButton == null)
                {
                    _cancelButton = CreateUIButton("CancelButton", new Vector2(0.1f, 0.1f), new Vector2(0.4f, 0.25f), 
                                                 new Color(0.6f, 0.3f, 0.3f, 1f), "Cancel");
                    Debug.Log("[ConfirmDialog] Cancel button created");
                }

                // События кнопок
                _confirmButton.onClick.AddListener(OnConfirmClicked);
                _cancelButton.onClick.AddListener(OnCancelClicked);
                
                Debug.Log("[ConfirmDialog] Button setup completed");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ConfirmDialog] Error setting up buttons: {ex.Message}");
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// НОВОЕ: Метод для правильного создания кнопок UI
        /// </summary>
        private Button CreateUIButton(string name, Vector2 anchorMin, Vector2 anchorMax, Color color, string text)
        {
            // Создаем GameObject для кнопки с RectTransform
            var buttonObject = new GameObject(name, typeof(RectTransform));
            buttonObject.transform.SetParent(transform, false);

            // Добавляем компоненты кнопки
            var button = buttonObject.AddComponent<Button>();
            var image = buttonObject.AddComponent<Image>();
            image.color = color;

            // Настраиваем RectTransform
            var buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = anchorMin;
            buttonRect.anchorMax = anchorMax;
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;

            // Создаем текст кнопки
            var textObject = new GameObject("Text", typeof(RectTransform));
            textObject.transform.SetParent(buttonObject.transform, false);
            
            var buttonText = textObject.AddComponent<TextMeshProUGUI>();
            buttonText.text = text;
            buttonText.fontSize = 18;
            buttonText.color = Color.white;
            buttonText.alignment = TextAlignmentOptions.Center;

            var textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return button;
        }

        /// <summary>
        /// НОВОЕ: Создание простого fallback диалога в случае ошибки
        /// </summary>
        private void CreateFallbackDialog()
        {
            Debug.Log("[ConfirmDialog] Creating fallback dialog...");
            
            // Простой фон
            if (_backgroundImage == null)
            {
                _backgroundImage = gameObject.AddComponent<Image>();
                _backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);
            }
            
            // Простые кнопки без текста - просто чтобы диалог работал
            if (_confirmButton == null)
            {
                var confirmObj = new GameObject("FallbackConfirm", typeof(RectTransform));
                confirmObj.transform.SetParent(transform, false);
                _confirmButton = confirmObj.AddComponent<Button>();
                _confirmButton.onClick.AddListener(() => { Hide(); _onConfirm?.Invoke(); });
            }
            
            if (_cancelButton == null)
            {
                var cancelObj = new GameObject("FallbackCancel", typeof(RectTransform));
                cancelObj.transform.SetParent(transform, false);
                _cancelButton = cancelObj.AddComponent<Button>();
                _cancelButton.onClick.AddListener(() => { Hide(); _onCancel?.Invoke(); });
            }
        }

        /// <summary>
        /// ИСПРАВЛЕНО: Показать диалог с проверками на null
        /// </summary>
        public void Show(string title, string message, Action confirmed = null, Action cancelled = null, Action onClose = null)
        {
            Debug.Log($"[ConfirmDialog] Show called with title: '{title}', message: '{message}'");
            
            if (_canvasGroup == null)
            {
                Debug.LogError("[ConfirmDialog] Cannot show dialog - CanvasGroup is null!");
                return;
            }
            
            _onConfirm = confirmed;
            _onCancel = cancelled;
            _onClose = onClose;

            // ИСПРАВЛЕНО: Безопасная установка текста с проверкой на null
            if (_titleText != null)
            {
                _titleText.text = title ?? "Confirm";
            }
            else
            {
                Debug.LogWarning("[ConfirmDialog] Title text is null, cannot set title");
            }

            if (_messageText != null)
            {
                _messageText.text = message ?? "";
            }
            else
            {
                Debug.LogWarning("[ConfirmDialog] Message text is null, cannot set message");
            }

            gameObject.SetActive(true);

            var sequence = DOTween.Sequence();
            sequence.Append(_canvasGroup.DOFade(1f, _showDuration).SetEase(_showEase));
            sequence.Join(transform.DOScale(Vector3.one, _showDuration).SetEase(_showEase));
            
            Debug.Log($"[ConfirmDialog] Dialog shown: {title} - {message}");
        }

        /// <summary>
        /// Скрыть диалог
        /// </summary>
        public void Hide()
        {
            if (_canvasGroup == null)
            {
                Debug.LogError("[ConfirmDialog] Cannot hide dialog - CanvasGroup is null!");
                _onClose?.Invoke();
                return;
            }
            
            var sequence = DOTween.Sequence();
            sequence.Append(_canvasGroup.DOFade(0f, _hideDuration).SetEase(_hideEase));
            sequence.Join(transform.DOScale(Vector3.zero, _hideDuration).SetEase(_hideEase));
            sequence.OnComplete(() =>
            {
                gameObject.SetActive(false);
                _onClose?.Invoke();
                Destroy(gameObject);
            });
            
            Debug.Log("[ConfirmDialog] Dialog hidden");
        }

        /// <summary>
        /// Обработка нажатия кнопки подтверждения
        /// </summary>
        private void OnConfirmClicked()
        {
            Debug.Log("[ConfirmDialog] Confirm button clicked");
            _onConfirm?.Invoke();
            Hide();
        }

        /// <summary>
        /// Обработка нажатия кнопки отмены
        /// </summary>
        private void OnCancelClicked()
        {
            Debug.Log("[ConfirmDialog] Cancel button clicked");
            _onCancel?.Invoke();
            Hide();
        }

        /// <summary>
        /// Очистка при уничтожении
        /// </summary>
        private void OnDestroy()
        {
            DOTween.Kill(this);

            if (_confirmButton != null)
                _confirmButton.onClick.RemoveListener(OnConfirmClicked);

            if (_cancelButton != null)
                _cancelButton.onClick.RemoveListener(OnCancelClicked);
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