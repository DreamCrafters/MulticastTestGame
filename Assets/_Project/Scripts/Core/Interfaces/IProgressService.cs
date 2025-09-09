using Cysharp.Threading.Tasks;
using WordPuzzle.Core.Architecture;

namespace WordPuzzle.Core.Services
{
    /// <summary>
    /// Сервис для работы с прогрессом игрока
    /// Отвечает за сохранение и загрузку состояния прохождения игры
    /// </summary>
    public interface IProgressService : IGameService
    {
        /// <summary>
        /// Получает количество пройденных уровней
        /// </summary>
        /// <returns>Количество завершенных уровней</returns>
        int GetCompletedLevelsCount();

        /// <summary>
        /// Получает номер текущего (следующего не пройденного) уровня
        /// </summary>
        /// <returns>Номер уровня для прохождения</returns>
        int GetCurrentLevelNumber();

        /// <summary>
        /// Отмечает уровень как пройденный
        /// </summary>
        /// <param name="levelId">Идентификатор завершенного уровня</param>
        /// <param name="completedWords">Порядок разгаданных слов</param>
        UniTask MarkLevelCompletedAsync(int levelId, string[] completedWords);

        /// <summary>
        /// Проверяет, пройден ли указанный уровень
        /// </summary>
        /// <param name="levelId">Идентификатор уровня</param>
        /// <returns>true если уровень завершен</returns>
        bool IsLevelCompleted(int levelId);

        /// <summary>
        /// Получает порядок разгаданных слов для пройденного уровня
        /// </summary>
        /// <param name="levelId">Идентификатор уровня</param>
        /// <returns>Массив слов в порядке разгадывания или null</returns>
        string[] GetLevelCompletionWords(int levelId);

        /// <summary>
        /// Сброс всего прогресса (для отладки)
        /// </summary>
        void ResetProgress();

        /// <summary>
        /// Проверка, завершены ли все доступные уровни
        /// </summary>
        /// <param name="totalLevels">Общее количество доступных уровней</param>
        /// <returns>true если все уровни пройдены</returns>
        public bool AreAllLevelsCompleted(int totalLevels);
    }
}