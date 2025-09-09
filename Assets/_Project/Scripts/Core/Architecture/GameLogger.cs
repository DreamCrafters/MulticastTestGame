using System;
using UnityEngine;

namespace WordPuzzle.Core.Architecture
{
    /// <summary>
    /// Централизованная система логирования для игры
    /// Обеспечивает единообразное логирование с категориями и уровнями важности
    /// </summary>
    public static class GameLogger
    {
        /// <summary>
        /// Включено ли логирование в сборке
        /// В релизной сборке автоматически отключается для производительности
        /// </summary>
        public static bool IsLoggingEnabled =>
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            true;
#else
            false;
#endif

        /// <summary>
        /// Логирование информационного сообщения
        /// </summary>
        /// <param name="category">Категория лога (например, "SceneService", "GameplayManager")</param>
        /// <param name="message">Сообщение для логирования</param>
        /// <param name="context">Контекст Unity Object (опционально)</param>
        public static void LogInfo(string category, string message, UnityEngine.Object context = null)
        {
            if (IsLoggingEnabled == false) return;
            
            string formattedMessage = $"[{category}] {message}";
            Debug.Log(formattedMessage, context);
        }
        
        /// <summary>
        /// Логирование предупреждения
        /// </summary>
        /// <param name="category">Категория лога</param>
        /// <param name="message">Сообщение предупреждения</param>
        /// <param name="context">Контекст Unity Object (опционально)</param>
        public static void LogWarning(string category, string message, UnityEngine.Object context = null)
        {
            if (IsLoggingEnabled == false) return;
            
            string formattedMessage = $"[{category}] WARNING: {message}";
            Debug.LogWarning(formattedMessage, context);
        }
        
        /// <summary>
        /// Логирование ошибки
        /// </summary>
        /// <param name="category">Категория лога</param>
        /// <param name="message">Сообщение об ошибке</param>
        /// <param name="context">Контекст Unity Object (опционально)</param>
        public static void LogError(string category, string message, UnityEngine.Object context = null)
        {
            string formattedMessage = $"[{category}] ERROR: {message}";
            Debug.LogError(formattedMessage, context);
        }
        
        /// <summary>
        /// Логирование исключения
        /// </summary>
        /// <param name="category">Категория лога</param>
        /// <param name="exception">Исключение для логирования</param>
        /// <param name="context">Контекст Unity Object (опционально)</param>
        public static void LogException(string category, Exception exception, UnityEngine.Object context = null)
        {
            string formattedMessage = $"[{category}] EXCEPTION: {exception.Message}";
            Debug.LogException(exception, context);
        }
        
        /// <summary>
        /// Логирование инициализации сервиса
        /// Специальный метод для отслеживания порядка инициализации
        /// </summary>
        /// <param name="serviceName">Имя сервиса</param>
        /// <param name="isSuccess">Успешна ли инициализация</param>
        public static void LogServiceInitialization(string serviceName, bool isSuccess)
        {
            if (IsLoggingEnabled == false) return;
            
            string status = isSuccess ? "INITIALIZED" : "FAILED TO INITIALIZE";
            string message = $"Service {serviceName}: {status}";
            
            if (isSuccess)
                LogInfo("ServiceManager", message);
            else
                LogError("ServiceManager", message);
        }
        
        /// <summary>
        /// Логирование DI резолва зависимостей
        /// Помогает отслеживать корректность настройки контейнера
        /// </summary>
        /// <param name="interfaceType">Тип интерфейса</param>
        /// <param name="implementationType">Тип реализации</param>
        public static void LogDependencyResolved(Type interfaceType, Type implementationType)
        {
            if (IsLoggingEnabled == false) return;
            
            LogInfo("DI", $"Resolved {interfaceType.Name} -> {implementationType.Name}");
        }
    }
}