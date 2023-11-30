using Akka.Event;
using Samurai.Integration.EntityFramework.Database;
using System;
using System.Linq;

namespace Samurai.Integration.EntityFramework.Repositories
{
    public class ShopifyMaxVariantsQueryGraphQL
    {
        protected readonly DatabaseContext _db;
        readonly ILoggingAdapter _logger;
        public ShopifyMaxVariantsQueryGraphQL(DatabaseContext db, ILoggingAdapter logger)
        {
            _db = db;
            _logger = logger;
        }

        public string HasMaxVariants(long? id)
        {
            var standard = "25";
            try
            {                
                var query = _db.Tenants.Where(x => x.Id == id).Select(s => s.ShopifyData.MaxVariantsQueryGraphQL).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(query))
                    return standard;

                return query;
            }
            catch (Exception ex)
            {
                _logger.Warning("Erro ao buscar número máximo de variantes para o tenantId: {1} | ErrorMessage: {0}", ex.Message, id);
                return standard;
            }
        }
    }
}
