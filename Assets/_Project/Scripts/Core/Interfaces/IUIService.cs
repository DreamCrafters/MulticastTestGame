using System;
using WordPuzzle.Core.Architecture;

namespace WordPuzzle.Core.Services
{
    /// <summary>
    /// Сервис для управления пользовательским интерфейсом
    /// Отвечает за показ/скрытие экранов, всплывающих окон и анимаций
    /// </summary>
    public interface IUIService : IGameService
    {
        /// <summary>
        /// Событие открытия экрана
        /// </summary>
        event Action<string> OnScreenOpened;
        
        /// <summary>
        /// Событие закрытия экрана
        /// </summary>
        event Action<string> OnScreenClosed;
        
        /// <summary>
        /// Показывает экран загрузки
        /// </summary>
        /// <param name="message">Сообщение для отображения</param>
        void ShowLoadingScreen(string message = "Loading...");
        
        /// <summary>
        /// Скрывает экран загрузки
        /// </summary>
        void HideLoadingScreen();
        
        /// <summary>
        /// Показывает всплывающее сообщение
        /// </summary>
        /// <param name="message">Текст сообщения</param>
        /// <param name="duration">Длительность показа в секундах</param>
        void ShowMessage(string message, float duration = 3f);
        
        /// <summary>
        /// Показывает диалог подтверждения
        /// </summary>
        /// <param name="title">Заголовок диалога</param>
        /// <param name="message">Текст сообщения</param>
        /// <param name="onConfirm">Callback при подтверждении</param>
        /// <param name="onCancel">Callback при отмене</param>
        void ShowConfirmDialog(string title, string message, Action onConfirm = null, Action onCancel = null);
        
        /// <summary>
        /// Устанавливает активность кнопки "Назад" (Android)
        /// </summary>
        /// <param name="isActive">Активна ли кнопка</param>
        /// <param name="onBackPressed">Callback при нажатии</param>
        void SetBackButtonActive(bool isActive, Action onBackPressed = null);
        
        /// <summary>
        /// Воспроизводит звук UI
        /// </summary>
        /// <param name="soundType">Тип звука</param>
        void PlayUISound(UISoundType soundType);
    }
    
    /// <summary>
    /// Типы звуков пользовательского интерфейса
    /// </summary>
    public enum UISoundType
    {
        ButtonClick,
        Success,
        Error,
        Notification
    }
}