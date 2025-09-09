using Cysharp.Threading.Tasks;

namespace WordPuzzle.Core.Architecture
{
    /// <summary>
    /// Базовый интерфейс для всех игровых сервисов
    /// Обеспечивает единообразную инициализацию и завершение работы
    /// </summary>
    public interface IGameService
    {
        /// <summary>
        /// Инициализация сервиса
        /// Вызывается при старте приложения через DI контейнер
        /// </summary>
        UniTask InitializeAsync();
        
        /// <summary>
        /// Завершение работы сервиса
        /// Вызывается при закрытии приложения для корректной очистки ресурсов
        /// </summary>
        void Dispose();
        
        /// <summary>
        /// Состояние готовности сервиса
        /// true - сервис инициализирован и готов к работе
        /// </summary>
        bool IsInitialized { get; }
    }
}