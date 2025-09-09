using System;
using Cysharp.Threading.Tasks;
using WordPuzzle.Core.Architecture;

namespace WordPuzzle.Core.Services
{
    /// <summary>
    /// Сервис для управления загрузкой и переходами между сценами
    /// Обеспечивает асинхронную загрузку с возможностью отслеживания прогресса
    /// </summary>
    public interface ISceneService : IGameService
    {
        /// <summary>
        /// Событие начала загрузки сцены
        /// </summary>
        event Action<string> OnSceneLoadStarted;

        /// <summary>
        /// Событие завершения загрузки сцены
        /// </summary>
        event Action<string> OnSceneLoadCompleted;

        /// <summary>
        /// Загружает сцену асинхронно
        /// </summary>
        /// <param name="sceneName">Имя сцены для загрузки</param>
        /// <param name="showLoadingScreen">Показывать ли экран загрузки</param>
        /// <returns>Задача загрузки</returns>
        UniTask LoadSceneAsync(string sceneName, bool showLoadingScreen = true);

        /// <summary>
        /// Загружает сцену с передачей параметров
        /// </summary>
        /// <param name="sceneName">Имя сцены</param>
        /// <param name="sceneParameters">Параметры для передачи в сцену</param>
        /// <returns>Задача загрузки</returns>
        UniTask LoadSceneAsync(string sceneName, object sceneParameters);

        /// <summary>
        /// Получает имя текущей активной сцены
        /// </summary>
        /// <returns>Имя сцены</returns>
        string GetCurrentSceneName();

        /// <summary>
        /// Получает параметры, переданные в текущую сцену
        /// </summary>
        /// <typeparam name="T">Тип параметров</typeparam>
        /// <returns>Параметры или default(T)</returns>
        T GetSceneParameters<T>() where T : class;

        /// <summary>
        /// Очистка параметров сцены (вызывается при переходе без параметров)
        /// </summary>
        public void ClearSceneParameters();
    }

    /// <summary>
    /// Константы имен сцен для избежания магических строк
    /// </summary>
    public static class SceneNames
    {
        public const string Bootstrap = "Bootstrap";
        public const string MainMenu = "MainMenu";
        public const string Gameplay = "Gameplay";
        public const string Victory = "Victory";
    }
}