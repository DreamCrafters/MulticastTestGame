using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using WordPuzzle.Core.Architecture;
using WordPuzzle.UI.Components;
using UnityEngine.UI;

namespace WordPuzzle.Core.Services
{
    /// <summary>
    /// Реализация сервиса управления пользовательским интерфейсом
    /// Заменяет MockUIService, обеспечивает полную функциональность UI
    /// </summary>
    public class UIService : MonoBehaviour, IUIService
    {
        [Header("UI References")]
        [SerializeField] private Canvas _uiCanvas;
        [SerializeField] private Transform _popupContainer;

        [Header("Loading Screen")]
        [SerializeField] private GameObject _loadingScreenPrefab;

        [Header("Message System")]
        [SerializeField] private GameObject _messagePrefab;
        [SerializeField] private int _maxSimultaneousMessages = 3;

        [Header("Confirm Dialog")]
        [SerializeField] private GameObject _confirmDialogPrefab;

        [Header("Audio")]
        [SerializeField] private AudioSource _uiAudioSource;
        [SerializeField] private UISounds _uiSounds;
        [SerializeField] private bool _enableUISounds = true;

        [Header("Back Button (Mobile)")]
        [SerializeField] private bool _enableBackButton = true;

        public bool IsInitialized { get; private set; }

        public event Action<string> OnScreenOpened;
        public event Action<string> OnScreenClosed;

        // UI элементы
        private LoadingScreen _currentLoadingScreen;
        private readonly System.Collections.Generic.List<MessagePopup> _activeMessages = new();
        private ConfirmDialog _currentDialog;

        // Back button handling
        private bool _backButtonActive = false;
        private Action _backButtonCallback;

        /// <summary>
        /// Инициализация UI сервиса
        /// </summary>
        public async UniTask InitializeAsync()
        {
            GameLogger.LogInfo("UIService", "Initializing UI Service...");

            try
            {
                // Настройка Canvas
                await SetupUICanvas();

                // Настройка Audio
                SetupAudioSource();

                // Настройка Back Button для мобильных
                SetupBackButtonHandling();

                // Предварительная загрузка ресурсов если нужно
                await PreloadUIResources();

                IsInitialized = true;
                GameLogger.LogInfo("UIService", "UI Service initialized successfully");
            }
            catch (Exception ex)
            {
                GameLogger.LogException("UIService", ex);
                throw;
            }
        }

        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        public void Dispose()
        {
            if (!IsInitialized) return;

            // Закрываем все активные UI элементы
            HideLoadingScreen();
            CloseAllMessages();
            CloseConfirmDialog();

            // Очистка событий
            OnScreenOpened = null;
            OnScreenClosed = null;
            _backButtonCallback = null;

            // Остановка анимаций
            DOTween.Kill(this);

            IsInitialized = false;
            GameLogger.LogInfo("UIService", "UI Service disposed");
        }

        #region Loading Screen

        /// <summary>
        /// Показать экран загрузки
        /// </summary>
        public void ShowLoadingScreen(string message = "Loading...")
        {
            if (!IsInitialized)
            {
                GameLogger.LogWarning("UIService", "Service not initialized");
                return;
            }

            // Если уже показан, обновляем сообщение
            if (_currentLoadingScreen != null)
            {
                _currentLoadingScreen.UpdateMessage(message);
                return;
            }

            try
            {
                _currentLoadingScreen = CreateLoadingScreen();
                if (_currentLoadingScreen != null)
                {
                    _currentLoadingScreen.Show(message);
                    GameLogger.LogInfo("UIService", $"Loading screen shown: {message}");
                }
            }
            catch (Exception ex)
            {
                GameLogger.LogException("UIService", ex);
            }
        }

        /// <summary>
        /// Скрыть экран загрузки
        /// </summary>
        public void HideLoadingScreen()
        {
            if (_currentLoadingScreen == null) return;

            try
            {
                _currentLoadingScreen.Hide(() =>
                {
                    if (_currentLoadingScreen != null)
                    {
                        Destroy(_currentLoadingScreen.gameObject);
                        _currentLoadingScreen = null;
                    }
                });

                GameLogger.LogInfo("UIService", "Loading screen hidden");
            }
            catch (Exception ex)
            {
                GameLogger.LogException("UIService", ex);
            }
        }

        #endregion

        #region Messages

        /// <summary>
        /// Показать всплывающее сообщение
        /// </summary>
        public void ShowMessage(string message, float duration = 3f)
        {
            if (!IsInitialized || string.IsNullOrEmpty(message))
            {
                GameLogger.LogWarning("UIService", "Cannot show message - service not initialized or message empty");
                return;
            }

            // Ограничиваем количество одновременных сообщений
            if (_activeMessages.Count >= _maxSimultaneousMessages)
            {
                var oldestMessage = _activeMessages[0];
                oldestMessage?.Hide();
                _activeMessages.RemoveAt(0);
            }

            try
            {
                var messagePopup = CreateMessage();
                if (messagePopup != null)
                {
                    _activeMessages.Add(messagePopup);
                    messagePopup.Show(message, duration, () => _activeMessages.Remove(messagePopup));

                    GameLogger.LogInfo("UIService", $"Message shown: {message} (duration: {duration}s)");
                }
            }
            catch (Exception ex)
            {
                GameLogger.LogException("UIService", ex);
            }
        }

        #endregion

        #region Confirm Dialog

        /// <summary>
        /// Показать диалог подтверждения
        /// </summary>
        public void ShowConfirmDialog(string title, string message, Action onConfirm = null, Action onCancel = null)
        {
            if (!IsInitialized)
            {
                GameLogger.LogWarning("UIService", "Service not initialized");
                return;
            }

            // Закрываем предыдущий диалог если есть
            CloseConfirmDialog();

            try
            {
                _currentDialog = CreateConfirmDialog();
                if (_currentDialog != null)
                {
                    _currentDialog.Show(title, message,
                        confirmed: onConfirm,
                        cancelled: onCancel,
                        onClose: () => _currentDialog = null);

                    GameLogger.LogInfo("UIService", $"Confirm dialog shown: {title} - {message}");
                }
            }
            catch (Exception ex)
            {
                GameLogger.LogException("UIService", ex);
                // В случае ошибки вызываем подтверждение по умолчанию
                onConfirm?.Invoke();
            }
        }

        #endregion

        #region Back Button

        /// <summary>
        /// Управление кнопкой "Назад" (Android)
        /// </summary>
        public void SetBackButtonActive(bool isActive, Action onBackPressed = null)
        {
            _backButtonActive = isActive && _enableBackButton;
            _backButtonCallback = onBackPressed;

            GameLogger.LogInfo("UIService", $"Back button active: {_backButtonActive}");
        }

        #endregion

        #region Audio

        /// <summary>
        /// Воспроизвести звук UI
        /// </summary>
        public void PlayUISound(UISoundType soundType)
        {
            if (!_enableUISounds || _uiAudioSource == null || _uiSounds == null)
            {
                return;
            }

            AudioClip clip = _uiSounds.GetClip(soundType);
            if (clip != null)
            {
                _uiAudioSource.PlayOneShot(clip);
                GameLogger.LogInfo("UIService", $"Playing UI sound: {soundType}");
            }
        }

        #endregion

        #region Screen Notifications

        /// <summary>
        /// Уведомить об открытии экрана
        /// </summary>
        public void NotifyScreenOpened(string screenName)
        {
            GameLogger.LogInfo("UIService", $"Screen opened: {screenName}");
            OnScreenOpened?.Invoke(screenName);
        }

        /// <summary>
        /// Уведомить о закрытии экрана
        /// </summary>
        public void NotifyScreenClosed(string screenName)
        {
            GameLogger.LogInfo("UIService", $"Screen closed: {screenName}");
            OnScreenClosed?.Invoke(screenName);
        }

        #endregion

        #region Private Setup Methods

        /// <summary>
        /// Настройка UI Canvas
        /// </summary>
        private async UniTask SetupUICanvas()
        {
            if (_uiCanvas == null)
            {
                // Создаем Canvas динамически
                var canvasObject = new GameObject("UIServiceCanvas");
                DontDestroyOnLoad(canvasObject);

                _uiCanvas = canvasObject.AddComponent<Canvas>();
                _uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _uiCanvas.sortingOrder = 1000; // Поверх всего

                var canvasScaler = canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(1920, 1080);
                canvasScaler.matchWidthOrHeight = 0.5f;

                canvasObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();

                GameLogger.LogInfo("UIService", "UI Canvas created dynamically");
            }

            // Создаем контейнер для popup элементов
            if (_popupContainer == null)
            {
                var containerObject = new GameObject("PopupContainer");
                containerObject.transform.SetParent(_uiCanvas.transform, false);

                var rectTransform = containerObject.AddComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;

                _popupContainer = containerObject.transform;
            }

            await UniTask.Yield();
        }

        /// <summary>
        /// Настройка AudioSource
        /// </summary>
        private void SetupAudioSource()
        {
            if (_uiAudioSource == null && _enableUISounds)
            {
                var audioObject = new GameObject("UIAudioSource");
                audioObject.transform.SetParent(transform);

                _uiAudioSource = audioObject.AddComponent<AudioSource>();
                _uiAudioSource.playOnAwake = false;
                _uiAudioSource.volume = 0.7f;

                GameLogger.LogInfo("UIService", "UI AudioSource created");
            }
        }

        /// <summary>
        /// Настройка обработки кнопки "Назад"
        /// </summary>
        private void SetupBackButtonHandling()
        {
            if (_enableBackButton)
            {
                // Обработка будет в Update
                GameLogger.LogInfo("UIService", "Back button handling enabled");
            }
        }

        /// <summary>
        /// Предварительная загрузка UI ресурсов
        /// </summary>
        private async UniTask PreloadUIResources()
        {
            // Можно предварительно загрузить звуки или другие ресурсы
            await UniTask.Yield();
            GameLogger.LogInfo("UIService", "UI resources preloaded");
        }

        #endregion

        #region UI Creation Methods

        /// <summary>
        /// Создание экрана загрузки
        /// </summary>
        private LoadingScreen CreateLoadingScreen()
        {
            GameObject loadingObject;

            if (_loadingScreenPrefab != null)
            {
                loadingObject = Instantiate(_loadingScreenPrefab, _popupContainer);
            }
            else
            {
                loadingObject = CreateSimpleLoadingScreen();
            }

            return loadingObject.GetComponent<LoadingScreen>() ?? loadingObject.AddComponent<LoadingScreen>();
        }

        /// <summary>
        /// Создание сообщения
        /// </summary>
        private MessagePopup CreateMessage()
        {
            GameObject messageObject;

            if (_messagePrefab != null)
            {
                messageObject = Instantiate(_messagePrefab, _popupContainer);
            }
            else
            {
                messageObject = CreateSimpleMessage();
            }

            return messageObject.GetComponent<MessagePopup>() ?? messageObject.AddComponent<MessagePopup>();
        }

        /// <summary>
        /// Создание диалога подтверждения
        /// </summary>
        private ConfirmDialog CreateConfirmDialog()
        {
            GameObject dialogObject;

            if (_confirmDialogPrefab != null)
            {
                dialogObject = Instantiate(_confirmDialogPrefab, _popupContainer);
            }
            else
            {
                dialogObject = CreateSimpleConfirmDialog();
            }

            return dialogObject.GetComponent<ConfirmDialog>() ?? dialogObject.AddComponent<ConfirmDialog>();
        }

        #endregion

        #region Simple UI Creation (Fallbacks)

        /// <summary>
        /// Создание простого экрана загрузки (если нет префаба)
        /// </summary>
        private GameObject CreateSimpleLoadingScreen()
        {
            var loadingObject = new GameObject("LoadingScreen");
            loadingObject.transform.SetParent(_popupContainer, false);

            // Полноэкранный фон
            var backgroundImage = loadingObject.AddComponent<UnityEngine.UI.Image>();
            backgroundImage.color = new Color(0, 0, 0, 0.8f);

            var rectTransform = loadingObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            return loadingObject;
        }

        /// <summary>
        /// Создание простого сообщения (если нет префаба)
        /// </summary>
        private GameObject CreateSimpleMessage()
        {
            var messageObject = new GameObject("Message");
            messageObject.transform.SetParent(_popupContainer, false);

            // Позиционирование теперь настраивается в MessagePopup.SetupMobileSafePositioning()
            // Базовая настройка для совместимости
            var rectTransform = messageObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(1, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.sizeDelta = new Vector2(400, 80);
            rectTransform.anchoredPosition = new Vector2(-120, -80);

            return messageObject;
        }

        /// <summary>
        /// Создание простого диалога (если нет префаба)
        /// ИСПРАВЛЕНО: с блокировкой интерактивности
        /// </summary>
        private GameObject CreateSimpleConfirmDialog()
        {
            // Создаем полноэкранный контейнер для диалога
            var dialogObject = new GameObject("ConfirmDialog");
            dialogObject.transform.SetParent(_popupContainer, false);
       
            // Делаем контейнер полноэкранным для правильной работы ConfirmDialog компонента

            var rectTransform = dialogObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;         

            return dialogObject;
        }

        #endregion

        #region Unity Update

        /// <summary>
        /// Обработка кнопки "Назад" в Update
        /// </summary>
        private void Update()
        {
            if (!IsInitialized || !_backButtonActive) return;

            // Android Back Button
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GameLogger.LogInfo("UIService", "Back button pressed");
                _backButtonCallback?.Invoke();
            }
        }

        #endregion

        #region Cleanup Methods

        /// <summary>
        /// Закрытие всех активных сообщений
        /// </summary>
        private void CloseAllMessages()
        {
            foreach (var message in _activeMessages)
            {
                if (message != null)
                {
                    message.Hide();
                }
            }
            _activeMessages.Clear();
        }

        /// <summary>
        /// Закрытие диалога подтверждения
        /// </summary>
        private void CloseConfirmDialog()
        {
            if (_currentDialog != null)
            {
                _currentDialog.Hide();
                _currentDialog = null;
            }
        }

        #endregion
    }

    /// <summary>
    /// Коллекция звуков UI
    /// </summary>
    [System.Serializable]
    public class UISounds
    {
        [SerializeField] private AudioClip _buttonClickSound;
        [SerializeField] private AudioClip _successSound;
        [SerializeField] private AudioClip _errorSound;
        [SerializeField] private AudioClip _notificationSound;

        public AudioClip GetClip(UISoundType soundType)
        {
            return soundType switch
            {
                UISoundType.ButtonClick => _buttonClickSound,
                UISoundType.Success => _successSound,
                UISoundType.Error => _errorSound,
                UISoundType.Notification => _notificationSound,
                _ => null
            };
        }
    }
}