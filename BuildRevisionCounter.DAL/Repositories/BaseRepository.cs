using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BuildRevisionCounter.DAL.Repositories.Interfaces;
using MongoDB.Driver;

namespace BuildRevisionCounter.DAL.Repositories
{
    public abstract class BaseRepository<T> : Context, IRepository<T> where T : class, new()
    {
        protected BaseRepository(string name)
        {
            DbContext = Database.GetCollection<T>(name);
        }

        protected IMongoCollection<T> DbContext { get; private set; }

        /// <summary>
        /// Получает постраничный список сущностей
        /// </summary>
        /// <param name="predicate">Предикат поиска</param>
        /// <param name="pageSize">Количество элементов на странице</param>
        /// <param name="pageNumber">Номер страницы</param>
        /// <returns></returns>
        public async Task<List<T>> GetAllByPage(Expression<Func<T, bool>> predicate, int pageSize, int pageNumber)
        {
            return await DbContext
                .Find(r => true)
                .Skip(pageSize * (pageNumber - 1))
                .Limit(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Получает сущность по условию
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public async Task<T> Get(Expression<Func<T, bool>> predicate)
        {
            return await DbContext.Find(predicate).SingleOrDefaultAsync();
        }

        /// <summary>
        /// Получает сущность по условию
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public async Task<T> Get(FilterDefinition<T> predicate)
        {
            return await DbContext.Find(predicate).SingleOrDefaultAsync();
        }

        /// <summary>
        /// Добавляет новую сущность в репозиторий
        /// </summary>
        /// <param name="entity">Новая сущность</param>
        /// <returns></returns>
        public async Task Add(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            await DbContext.InsertOneAsync(entity);
        }

        /// <summary>
        /// Обновляет сущность в репозитории
        /// </summary>
        /// <param name="filter">Условия поиска обновляемого объекта</param>
        /// <param name="update">Поля для обновления</param>
        public async Task Update(FilterDefinition<T> filter, UpdateDefinition<T> update)
        {
            if (filter == null)
            {
                throw new ArgumentNullException("filter");
            }
            if (update == null)
            {
                throw new ArgumentNullException("update");
            }
            await DbContext.UpdateOneAsync(filter, update);
        }

        /// <summary>
        /// Удаляет сущность из репозитория
        /// </summary>
        /// <param name="filter">Условия поиска удаляемого объекта</param>
        public async Task Delete(FilterDefinition<T> filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException("filter");
            }
            await DbContext.DeleteOneAsync(filter);
        }
        /// <summary>
        /// Количество элементов сущности
        /// </summary>
        /// <returns></returns>
        public async Task<long> Count()
        {
            return await DbContext.CountAsync(x => true);
        }
        /// <summary>
        /// Количество элементов сущности по условию
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public async Task<long> Count(Expression<Func<T, bool>> predicate)
        {
            return await DbContext.CountAsync(predicate);
        }
    }
}
