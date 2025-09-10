using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace WordPuzzle.UI.Components
{
    /// <summary>
    /// Компонент диалога подтверждения
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
        /// Настройка компонентов
        /// </summary>
        private void SetupComponents()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

            CreateBasicElements();
            SetupButtons();

            // Начальное состояние
            _canvasGroup.alpha = 0f;
            transform.localScale = Vector3.zero;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Создание базовых элементов
        /// </summary>
        private void CreateBasicElements()
        {
            // Background
            if (_backgroundImage == null)
            {
                _backgroundImage = gameObject.AddComponent<Image>();
                _backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);

                // Добавляем закругленные углы если возможно
                var rect = GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(500, 300);
            }

            // Title
            if (_titleText == null)
            {
                var titleObject = new GameObject("Title");
                titleObject.transform.SetParent(transform, false);

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
            }

            // Message
            if (_messageText == null)
            {
                var messageObject = new GameObject("Message");
                messageObject.transform.SetParent(transform, false);

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
            }
        }

        /// <summary>
        /// Настройка кнопок
        /// </summary>
        private void SetupButtons()
        {
            // Confirm Button
            if (_confirmButton == null)
            {
                var confirmObject = new GameObject("ConfirmButton");
                confirmObject.transform.SetParent(transform, false);

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
            }

            // Cancel Button
            if (_cancelButton == null)
            {
                var cancelObject = new GameObject("CancelButton");
                cancelObject.transform.SetParent(transform, false);

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
            }

            // События кнопок
            _confirmButton.onClick.AddListener(OnConfirmClicked);
            _cancelButton.onClick.AddListener(OnCancelClicked);
        }

        /// <summary>
        /// Показать диалог
        /// </summary>
        public void Show(string title, string message, Action confirmed = null, Action cancelled = null, Action onClose = null)
        {
            _onConfirm = confirmed;
            _onCancel = cancelled;
            _onClose = onClose;

            _titleText.text = title ?? "Confirm";
            _messageText.text = message ?? "";

            gameObject.SetActive(true);

            var sequence = DOTween.Sequence();
            sequence.Append(_canvasGroup.DOFade(1f, _showDuration).SetEase(_showEase));
            sequence.Join(transform.DOScale(Vector3.one, _showDuration).SetEase(_showEase));
        }

        /// <summary>
        /// Скрыть диалог
        /// </summary>
        public void Hide()
        {
            var sequence = DOTween.Sequence();
            sequence.Append(_canvasGroup.DOFade(0f, _hideDuration).SetEase(_hideEase));
            sequence.Join(transform.DOScale(Vector3.zero, _hideDuration).SetEase(_hideEase));
            sequence.OnComplete(() =>
            {
                gameObject.SetActive(false);
                _onClose?.Invoke();
                Destroy(gameObject);
            });
        }

        /// <summary>
        /// Обработка нажатия кнопки подтверждения
        /// </summary>
        private void OnConfirmClicked()
        {
            _onConfirm?.Invoke();
            Hide();
        }

        /// <summary>
        /// Обработка нажатия кнопки отмены
        /// </summary>
        private void OnCancelClicked()
        {
            _onCancel?.Invoke();
            Hide();
        }

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