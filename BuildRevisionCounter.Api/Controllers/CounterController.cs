using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using BuildRevisionCounter.Api.Security;
using BuildRevisionCounter.DAL.Repositories.Interfaces;
using BuildRevisionCounter.Model.BuildRevisionStorage;
using MongoDB.Driver;

namespace BuildRevisionCounter.Api.Controllers
{
    [RoutePrefix("api/counter")]
    [BasicAuthentication]
    public class CounterController : ApiController
    {
        private IRevisionRepository _revisionRepository;

        /// <summary>
        /// Конструктор контроллера номеров ревизий.
        /// </summary>
        /// <param name="revisionRepository"></param>
        public CounterController(IRevisionRepository revisionRepository)
        {
            _revisionRepository = revisionRepository;
        }

        [HttpGet]
        [Route("")]
        [Authorize(Roles = "admin, editor, anonymous")]
        public async Task<IReadOnlyCollection<RevisionModel>> GetAllRevision([FromUri] Int32 pageSize = 20, [FromUri] Int32 pageNumber = 1)
        {
            if (pageSize < 1 || pageNumber < 1)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            List<RevisionModel> revisions = await _revisionRepository.GetAllByPage(r => true, pageSize, pageNumber);

            if (revisions == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            return revisions;
        }

        [HttpGet]
        [Route("{revisionName}")]
        [Authorize(Roles = "admin, editor, anonymous")]
        public async Task<long> Current([FromUri] string revisionName)
        {
            RevisionModel revision = await _revisionRepository.Get(r => r.Id == revisionName);
            if (revision == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return revision.CurrentNumber;
        }

        [HttpPost]
        [Route("{revisionName}")]
        [Authorize(Roles = "buildserver")]
        public async Task<long> Bumping([FromUri] string revisionName)
        {
            // попробуем обновить документ
            var result = await FindOneAndUpdateRevisionModelAsync(revisionName);
            if (result != null)
                return result.CurrentNumber;

            // если не получилось, значит документ еще не был создан
            // создадим его с начальным значением 0
            try
            {
                await _revisionRepository.Add(new RevisionModel
                    {
                        Id = revisionName,
                        CurrentNumber = 0,
                        Created = DateTime.UtcNow
                    });
                return 0;
            }
            catch (MongoWriteException ex)
            {
                if (ex.WriteError.Category != ServerErrorCategory.DuplicateKey)
                    throw;
            }

            // если при вставке произошла ошибка значит мы не успели и запись там уже есть
            // и теперь попытка обновления должна пройти без ошибок
            result = await FindOneAndUpdateRevisionModelAsync(revisionName);

            return result.CurrentNumber;
        }

        /// <summary>
        ///		Инкриментит каунтер в БД
        /// </summary>
        /// <param name="revisionName"></param>
        /// <returns></returns>
        private async Task<RevisionModel> FindOneAndUpdateRevisionModelAsync(string revisionName)
        {
            return await _revisionRepository.FindOneAndUpdate(
                r => r.Id == revisionName, GetUpdateBuilder(), GetUpdateOptions());
        }

        private FindOneAndUpdateOptions<RevisionModel> GetUpdateOptions()
        {
            return new FindOneAndUpdateOptions<RevisionModel>
            {
                IsUpsert = false,
                ReturnDocument = ReturnDocument.After
            };
        }

        private UpdateDefinition<RevisionModel> GetUpdateBuilder()
        {
            return Builders<RevisionModel>.Update
                .Inc(r => r.CurrentNumber, 1)
                .SetOnInsert(r => r.Created, DateTime.UtcNow)
                .Set(r => r.Updated, DateTime.UtcNow);
        }
    }
}