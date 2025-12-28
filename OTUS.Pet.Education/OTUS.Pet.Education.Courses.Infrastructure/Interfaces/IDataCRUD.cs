using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OTUS.Pet.Education.Courses.Infrastructure.Interfaces
{
    public interface IDataCRUD<T>
    {
        /// <summary>
        /// Добавление одной записи
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        Task AddSingle(T arg);
        /// <summary>
        /// Добавление множества записей
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        Task AddMany(List<T> arg);

        /// <summary>
        /// Получение одной записи
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<T?> GetSingle(Guid id);
        /// <summary>
        /// Получение множества записей
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task<Dictionary<Guid, T?>> GetMany(List<Guid> ids);
        /// <summary>
        /// Получение записей
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        Task<List<T>> Get(int limit);
        /// <summary>
        /// Изменение одной записи
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        Task ChangeSingle(T arg);
        /// <summary>
        /// Изменение множества записей
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        Task ChangeMany(List<T> arg);

        /// <summary>
        /// Удаление одной записи
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteSingle(Guid id);
        /// <summary>
        /// Удаление множества записей
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task DeleteMany(List<Guid> ids);
    }
}