using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace WordPuzzle.UI.Components
{
    /// <summary>
    /// Компонент диалога подтверждения
    /// ИСПРАВЛЕНО: правильный порядок инициализации CanvasGroup
    /// </summary>
    public class ConfirmDialog : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _fullscreenBlocker; // Полноэкранный блокер для предотвращения взаимодействия с фоном
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
        /// ИСПРАВЛЕНО: Настройка компонентов с правильным порядком
        /// </summary>
        private void SetupComponents()
        {
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

        /// <summary>
        /// Создание базовых элементов
        /// </summary>
        private void CreateBasicElements()
        {
            // Fullscreen Blocker - создается первым и занимает весь экран
            if (_fullscreenBlocker == null)
            {
                var blockerObject = new GameObject("FullscreenBlocker");
                blockerObject.transform.SetParent(transform, false);

                _fullscreenBlocker = blockerObject.AddComponent<Image>();
                _fullscreenBlocker.color = new Color(0f, 0f, 0f, 0.5f); // Полупрозрачный черный фон
                _fullscreenBlocker.raycastTarget = true; // Важно: блокирует клики

                var blockerRect = blockerObject.GetComponent<RectTransform>();
                // Растягиваем на весь экран
                blockerRect.anchorMin = Vector2.zero;
                blockerRect.anchorMax = Vector2.one;
                blockerRect.offsetMin = Vector2.zero;
                blockerRect.offsetMax = Vector2.zero;
                
                // Перемещаем в самый низ иерархии, чтобы он был позади диалога
                blockerObject.transform.SetAsFirstSibling();
                
                Debug.Log("[ConfirmDialog] Fullscreen blocker created");
            }

            // Dialog Panel - контейнер для самого диалога
            GameObject dialogPanel = null;
            if (_backgroundImage == null)
            {
                dialogPanel = new GameObject("DialogPanel");
                dialogPanel.transform.SetParent(transform, false);

                _backgroundImage = dialogPanel.AddComponent<Image>();
                _backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);

                var dialogRect = dialogPanel.GetComponent<RectTransform>();
                dialogRect.sizeDelta = new Vector2(500, 300);
                // Центрируем диалог
                dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
                dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
                dialogRect.anchoredPosition = Vector2.zero;
                
                Debug.Log("[ConfirmDialog] Dialog panel created");
            }
            else
            {
                dialogPanel = _backgroundImage.gameObject;
            }

            // Title - привязываем к диалоговой панели
            if (_titleText == null)
            {
                var titleObject = new GameObject("Title");
                titleObject.transform.SetParent(dialogPanel.transform, false);

                _titleText = titleObject.AddComponent<TextMeshProUGUI>();
                _titleText.fontSize = 28;
                _titleText.color = Color.white;
                _titleText.alignment = TextAlignmentOptions.Center;
                _titleText.fontStyle = FontStyles.Bold;

                var titleRect = titleObject.GetComponent<RectTransform>();
                titleRect.anchorMin = new Vector2(0f, 0.7f);
                titleRect.anchorMax = new Vector2(1f, 0.9f);
                titleRect.offsetMin = new Vector2(20, 0);
                titleRect.offsetMax = new Vector2(-20, 0);
                
                Debug.Log("[ConfirmDialog] Title text created");
            }

            // Message - привязываем к диалоговой панели
            if (_messageText == null)
            {
                var messageObject = new GameObject("Message");
                messageObject.transform.SetParent(dialogPanel.transform, false);

                _messageText = messageObject.AddComponent<TextMeshProUGUI>();
                _messageText.fontSize = 20;
                _messageText.color = Color.white;
                _messageText.alignment = TextAlignmentOptions.Center;
                _messageText.textWrappingMode = TextWrappingModes.Normal;

                var messageRect = messageObject.GetComponent<RectTransform>();
                messageRect.anchorMin = new Vector2(0f, 0.3f);
                messageRect.anchorMax = new Vector2(1f, 0.7f);
                messageRect.offsetMin = new Vector2(30, 0);
                messageRect.offsetMax = new Vector2(-30, 0);
                
                Debug.Log("[ConfirmDialog] Message text created");
            }
        }

        /// <summary>
        /// Настройка кнопок
        /// </summary>
        private void SetupButtons()
        {
            // Получаем ссылку на диалоговую панель
            var dialogPanel = _backgroundImage.gameObject;

            // Confirm Button - привязываем к диалоговой панели
            if (_confirmButton == null)
            {
                var confirmObject = new GameObject("ConfirmButton");
                confirmObject.transform.SetParent(dialogPanel.transform, false);

                _confirmButton = confirmObject.AddComponent<Button>();
                var confirmImage = confirmObject.AddComponent<Image>();
                confirmImage.color = new Color(0.2f, 0.7f, 0.3f, 1f);

                var confirmRect = confirmObject.GetComponent<RectTransform>();
                confirmRect.anchorMin = new Vector2(0.6f, 0.1f);
                confirmRect.anchorMax = new Vector2(0.9f, 0.25f);
                confirmRect.offsetMin = Vector2.zero;
                confirmRect.offsetMax = Vector2.zero;

                // Текст кнопки
                var confirmTextObj = new GameObject("Text");
                confirmTextObj.transform.SetParent(confirmObject.transform, false);
                var confirmText = confirmTextObj.AddComponent<TextMeshProUGUI>();
                confirmText.text = "OK";
                confirmText.fontSize = 18;
                confirmText.color = Color.white;
                confirmText.alignment = TextAlignmentOptions.Center;

                var confirmTextRect = confirmTextObj.GetComponent<RectTransform>();
                confirmTextRect.anchorMin = Vector2.zero;
                confirmTextRect.anchorMax = Vector2.one;
                confirmTextRect.offsetMin = Vector2.zero;
                confirmTextRect.offsetMax = Vector2.zero;
                
                Debug.Log("[ConfirmDialog] Confirm button created");
            }

            // Cancel Button - привязываем к диалоговой панели
            if (_cancelButton == null)
            {
                var cancelObject = new GameObject("CancelButton");
                cancelObject.transform.SetParent(dialogPanel.transform, false);

                _cancelButton = cancelObject.AddComponent<Button>();
                var cancelImage = cancelObject.AddComponent<Image>();
                cancelImage.color = new Color(0.6f, 0.3f, 0.3f, 1f);

                var cancelRect = cancelObject.GetComponent<RectTransform>();
                cancelRect.anchorMin = new Vector2(0.1f, 0.1f);
                cancelRect.anchorMax = new Vector2(0.4f, 0.25f);
                cancelRect.offsetMin = Vector2.zero;
                cancelRect.offsetMax = Vector2.zero;

                // Текст кнопки
                var cancelTextObj = new GameObject("Text");
                cancelTextObj.transform.SetParent(cancelObject.transform, false);
                var cancelText = cancelTextObj.AddComponent<TextMeshProUGUI>();
                cancelText.text = "Cancel";
                cancelText.fontSize = 18;
                cancelText.color = Color.white;
                cancelText.alignment = TextAlignmentOptions.Center;

                var cancelTextRect = cancelTextObj.GetComponent<RectTransform>();
                cancelTextRect.anchorMin = Vector2.zero;
                cancelTextRect.anchorMax = Vector2.one;
                cancelTextRect.offsetMin = Vector2.zero;
                cancelTextRect.offsetMax = Vector2.zero;
                
                Debug.Log("[ConfirmDialog] Cancel button created");
            }

            // События кнопок
            _confirmButton.onClick.AddListener(OnConfirmClicked);
            _cancelButton.onClick.AddListener(OnCancelClicked);

            // Добавляем кнопку к полноэкранному блокеру для закрытия диалога при клике на фон
            if (_fullscreenBlocker != null)
            {
                var blockerButton = _fullscreenBlocker.gameObject.GetComponent<Button>();
                if (blockerButton == null)
                {
                    blockerButton = _fullscreenBlocker.gameObject.AddComponent<Button>();
                }
                blockerButton.onClick.AddListener(OnBackgroundClicked);
                
                Debug.Log("[ConfirmDialog] Background click handler added");
            }
        }

        /// <summary>
        /// Показать диалог
        /// </summary>
        public void Show(string title, string message, Action confirmed = null, Action cancelled = null, Action onClose = null)
        {
            if (_canvasGroup == null)
            {
                Debug.LogError("[ConfirmDialog] Cannot show dialog - CanvasGroup is null!");
                return;
            }
            
            _onConfirm = confirmed;
            _onCancel = cancelled;
            _onClose = onClose;

            _titleText.text = title ?? "Confirm";
            _messageText.text = message ?? "";

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
        /// Обработка клика по фону (закрытие диалога)
        /// </summary>
        private void OnBackgroundClicked()
        {
            Debug.Log("[ConfirmDialog] Background clicked - closing dialog");
            _onCancel?.Invoke(); // Трактуем как отмену
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

            if (_fullscreenBlocker != null)
            {
                var blockerButton = _fullscreenBlocker.gameObject.GetComponent<Button>();
                if (blockerButton != null)
                    blockerButton.onClick.RemoveListener(OnBackgroundClicked);
            }
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