using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace WordPuzzle.UI.Components
{
    /// <summary>
    /// Компонент диалога подтверждения
    /// ПОЛНОСТЬЮ ИСПРАВЛЕНО: правильное создание UI элементов без ошибок
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
        /// ИСПРАВЛЕНО: Безопасная настройка всех компонентов
        /// </summary>
        private void SetupComponents()
        {
            try
            {
                Debug.Log("[ConfirmDialog] Starting SetupComponents...");

                // ШАГ 1: Убеждаемся что у объекта есть RectTransform
                if (GetComponent<RectTransform>() == null)
                {
                    gameObject.AddComponent<RectTransform>();
                }

                // ШАГ 2: Создаем/проверяем CanvasGroup ПЕРВЫМ
                SetupCanvasGroup();

                // ШАГ 3: Создаем базовые элементы
                CreateBasicElements();

                // ШАГ 4: Создаем кнопки
                SetupButtons();

                // ШАГ 5: Устанавливаем начальное состояние
                _canvasGroup.alpha = 0f;
                transform.localScale = Vector3.zero;
                gameObject.SetActive(false);

                Debug.Log("[ConfirmDialog] ConfirmDialog setup completed successfully");
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
        /// НОВОЕ: Безопасная настройка CanvasGroup
        /// </summary>
        private void SetupCanvasGroup()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                {
                    _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                    Debug.Log("[ConfirmDialog] CanvasGroup component added");
                }
            }
        }

        /// <summary>
        /// ИСПРАВЛЕНО: Безопасное создание базовых элементов
        /// </summary>
        private void CreateBasicElements()
        {
            Debug.Log("[ConfirmDialog] Creating basic elements...");

            // Background - используем существующий Image или создаем новый
            if (_backgroundImage == null)
            {
                _backgroundImage = GetComponent<Image>();
                if (_backgroundImage == null)
                {
                    _backgroundImage = gameObject.AddComponent<Image>();
                }
                _backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);

                // Настройка размера
                var rect = GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(500, 300);

                Debug.Log("[ConfirmDialog] Background image created");
            }

            // Title Text
            if (_titleText == null)
            {
                _titleText = CreateTextElement("Title", new Vector2(0f, 0.7f), new Vector2(1f, 0.9f), 28, FontStyles.Bold);
                Debug.Log("[ConfirmDialog] Title text created");
            }

            // Message Text  
            if (_messageText == null)
            {
                _messageText = CreateTextElement("Message", new Vector2(0f, 0.3f), new Vector2(1f, 0.7f), 20, FontStyles.Normal);
                _messageText.textWrappingMode = TextWrappingModes.Normal;
                Debug.Log("[ConfirmDialog] Message text created");
            }

            Debug.Log("[ConfirmDialog] Basic elements creation completed");
        }

        /// <summary>
        /// НОВОЕ: Безопасное создание текстовых элементов
        /// </summary>
        private TextMeshProUGUI CreateTextElement(string name, Vector2 anchorMin, Vector2 anchorMax, float fontSize, FontStyles fontStyle)
        {
            // Создаем GameObject
            var textObject = new GameObject(name);
            textObject.transform.SetParent(transform, false);

            // Добавляем RectTransform
            var rectTransform = textObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = new Vector2(20, 0);
            rectTransform.offsetMax = new Vector2(-20, 0);

            // Добавляем TextMeshProUGUI
            var textComponent = textObject.AddComponent<TextMeshProUGUI>();
            textComponent.text = name;
            textComponent.fontSize = fontSize;
            textComponent.color = Color.white;
            textComponent.alignment = TextAlignmentOptions.Center;
            textComponent.fontStyle = fontStyle;

            return textComponent;
        }

        /// <summary>
        /// ИСПРАВЛЕНО: Безопасное создание кнопок
        /// </summary>
        private void SetupButtons()
        {
            Debug.Log("[ConfirmDialog] Setting up buttons...");

            try
            {
                // Confirm Button
                if (_confirmButton == null)
                {
                    _confirmButton = CreateButton("ConfirmButton", new Vector2(0.6f, 0.1f), new Vector2(0.9f, 0.25f),
                                                  new Color(0.2f, 0.7f, 0.3f, 1f), "OK");
                    Debug.Log("[ConfirmDialog] Confirm button created");
                }

                // Cancel Button
                if (_cancelButton == null)
                {
                    _cancelButton = CreateButton("CancelButton", new Vector2(0.1f, 0.1f), new Vector2(0.4f, 0.25f),
                                                 new Color(0.6f, 0.3f, 0.3f, 1f), "Cancel");
                    Debug.Log("[ConfirmDialog] Cancel button created");
                }

                // Подписываемся на события
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
        /// НОВОЕ: Безопасное создание кнопок
        /// </summary>
        private Button CreateButton(string name, Vector2 anchorMin, Vector2 anchorMax, Color color, string text)
        {
            // Создаем GameObject для кнопки
            var buttonObject = new GameObject(name);
            buttonObject.transform.SetParent(transform, false);

            // Добавляем RectTransform
            var rectTransform = buttonObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            // Добавляем Image и Button
            var image = buttonObject.AddComponent<Image>();
            image.color = color;
            var button = buttonObject.AddComponent<Button>();

            // Создаем текст кнопки
            var textObject = new GameObject("Text");
            textObject.transform.SetParent(buttonObject.transform, false);

            var textRect = textObject.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var buttonText = textObject.AddComponent<TextMeshProUGUI>();
            buttonText.text = text;
            buttonText.fontSize = 18;
            buttonText.color = Color.white;
            buttonText.alignment = TextAlignmentOptions.Center;

            return button;
        }

        /// <summary>
        /// НОВОЕ: Создание минимального диалога в случае критической ошибки
        /// </summary>
        private void CreateFallbackDialog()
        {
            Debug.Log("[ConfirmDialog] Creating fallback dialog...");

            // Минимальный CanvasGroup
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            // Минимальный фон
            if (_backgroundImage == null)
            {
                _backgroundImage = gameObject.AddComponent<Image>();
                _backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);
            }

            // Минимальные кнопки
            if (_confirmButton == null)
            {
                var confirmObj = new GameObject("FallbackConfirm");
                confirmObj.transform.SetParent(transform, false);
                confirmObj.AddComponent<RectTransform>();
                _confirmButton = confirmObj.AddComponent<Button>();
                confirmObj.AddComponent<Image>().color = Color.green;
                _confirmButton.onClick.AddListener(() => { Hide(); _onConfirm?.Invoke(); });
            }

            if (_cancelButton == null)
            {
                var cancelObj = new GameObject("FallbackCancel");
                cancelObj.transform.SetParent(transform, false);
                cancelObj.AddComponent<RectTransform>();
                _cancelButton = cancelObj.AddComponent<Button>();
                cancelObj.AddComponent<Image>().color = Color.red;
                _cancelButton.onClick.AddListener(() => { Hide(); _onCancel?.Invoke(); });
            }

            Debug.Log("[ConfirmDialog] Fallback dialog created");
        }

        /// <summary>
        /// ИСПРАВЛЕНО: Показать диалог с полными проверками
        /// </summary>
        public void Show(string title, string message, Action confirmed = null, Action cancelled = null, Action onClose = null)
        {
            Debug.Log($"[ConfirmDialog] Show called with title: '{title}', message: '{message}'");

            if (_canvasGroup == null)
            {
                Debug.LogError("[ConfirmDialog] Critical error - CanvasGroup is null even after setup!");
                confirmed?.Invoke(); // Вызываем подтверждение по умолчанию
                return;
            }

            _onConfirm = confirmed;
            _onCancel = cancelled;
            _onClose = onClose;

            // Безопасная установка текста
            if (_titleText != null)
            {
                _titleText.text = title ?? "Confirm";
            }

            if (_messageText != null)
            {
                _messageText.text = message ?? "";
            }

            gameObject.SetActive(true);

            // Анимация появления
            var sequence = DOTween.Sequence();
            sequence.Append(_canvasGroup.DOFade(1f, _showDuration).SetEase(_showEase));
            sequence.Join(transform.DOScale(Vector3.one, _showDuration).SetEase(_showEase));

            Debug.Log($"[ConfirmDialog] Dialog shown successfully");
        }

        /// <summary>
        /// Скрыть диалог
        /// </summary>
        public void Hide()
        {
            if (_canvasGroup == null)
            {
                Debug.LogError("[ConfirmDialog] Cannot hide - CanvasGroup is null!");
                gameObject.SetActive(false);
                _onClose?.Invoke();
                Destroy(gameObject);
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
    }
}