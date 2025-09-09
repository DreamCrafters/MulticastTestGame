using Cysharp.Threading.Tasks;
using WordPuzzle.Core.Architecture;
using WordPuzzle.Data.Models; // Новый импорт для моделей данных

namespace WordPuzzle.Core.Services
{
    /// <summary>
    /// Сервис для работы с уровнями игры
    /// Отвечает за загрузку, валидацию и предоставление данных уровней
    /// </summary>
    public interface ILevelService : IGameService
    {
        /// <summary>
        /// Загружает данные уровня по его идентификатору
        /// </summary>
        /// <param name="levelId">Идентификатор уровня</param>
        /// <returns>Данные уровня или null если уровень не найден</returns>
        UniTask<LevelData> LoadLevelAsync(int levelId);
        
        /// <summary>
        /// Получает общее количество доступных уровней
        /// </summary>
        /// <returns>Количество уровней</returns>
        int GetTotalLevelsCount();
        
        /// <summary>
        /// Проверяет существование уровня с указанным идентификатором
        /// </summary>
        /// <param name="levelId">Идентификатор уровня</param>
        /// <returns>true если уровень существует</returns>
        bool IsLevelExists(int levelId);
    }
}