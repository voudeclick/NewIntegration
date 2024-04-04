using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using VDC.Integration.Domain.Dtos;
using VDC.Integration.Domain.Entities.Database.Logs;
using VDC.Integration.EntityFramework.Database;

namespace VDC.Integration.EntityFramework.Repositories
{
    public static class ConstClass
    {
        public const string _blockedCommands = "select,update,create,delete";
    }

    public class LogsAbandonMessageRepository
    {
        public string query = @"SELECT * FROM [dbo].[LogsAbandonMessages] WHERE TenantId = @tenantId";

        protected readonly DatabaseContext _database;
        protected readonly IConfiguration _configuration;

        public LogsAbandonMessageRepository(DatabaseContext database, IConfiguration configuration)
        {
            _database = database;
            _configuration = configuration;
        }

        public async Task<List<LogsAbandonMessage>> GetByTenantIdAsync(long tenantId) => await _database.LogsAbandonMessages.Where(s => s.TenantId == tenantId).AsNoTracking().ToListAsync();

        public async Task<LogsAbandonMessage> GetByLogIdAsync(Guid LogId) => await _database.LogsAbandonMessages.Where(s => s.LogId == LogId).AsNoTracking().FirstOrDefaultAsync();

        public async Task<List<LogsAbandonMessageDto>> GetByFilterAsync(LogsDTO logs)
        {
            if (logs.GetType().GetProperties().Any(x => x.GetValue(logs) != null && ConstClass._blockedCommands.Contains(x.GetValue(logs).ToString().ToLower())))
                return new List<LogsAbandonMessageDto>();

            query = String.Concat(query, String.Format(@$" and(JSON_VALUE(Payload, '$.ProductInfo.ExternalId') = '{logs.Filter}'
                                                               OR JSON_VALUE(Payload, '$.ProductInfo.ExternalProductId') = '{logs.Filter}'
                                                               OR JSON_VALUE(Payload, '$.ProductInfo.ExternalCode') = '{logs.Filter}'
                                                               OR JSON_VALUE(Payload, '$.ProductInfo.SkuOriginal') = '{logs.Filter}'
                                                               OR JSON_VALUE(Payload,'$.ProductInfo.Title') LIKE '%{logs.Filter}%'  
                                                               OR JSON_VALUE(Payload,'$.IdProduto') = '{logs.Filter}'
                                                               OR JSON_VALUE(Payload,'$.ExternalId') = '{logs.Filter}'
                                                               OR JSON_VALUE(Payload,'$.ShopifyId') = '{logs.Filter}'
                                                               OR JSON_VALUE(Payload,'$.ID') = '{logs.Filter}')"));

            return await SearchAsync(logs.TenantId);
        }

        private async Task<List<LogsAbandonMessageDto>> SearchAsync(long tenantId)
        {
            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("Database")))
            {
                try
                {
                    var dataTable = new DataTable();
                    var cmd = new SqlCommand();

                    cmd = new SqlCommand(query, conn);
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.AddWithValue("@tenantId", tenantId);

                    cmd.CommandTimeout = 7200;

                    if (conn.State == ConnectionState.Open)
                        conn.Close();

                    conn.Open();

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    dataTable.Load(reader);

                    conn.Close();

                    return Newtonsoft.Json.JsonConvert.DeserializeObject<List<LogsAbandonMessageDto>>
                        (
                            Newtonsoft.Json.JsonConvert.SerializeObject(dataTable)
                        );
                }
                catch (SqlException sq)
                {
                    throw sq;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public Task AddAsync(LogsAbandonMessage logs)
        {
            _database.LogsAbandonMessages.Add(logs);

            return _database.SaveChangesAsync();
        }

        public Task UpdateAsync(LogsAbandonMessage logs)
        {
            _database.LogsAbandonMessages.Update(logs);
            return _database.SaveChangesAsync();
        }

        public Task DeleteAsync(LogsAbandonMessage logs)
        {
            _database.LogsAbandonMessages.Remove(logs);
            return _database.SaveChangesAsync();
        }

        public async Task<bool> ExistAsync(LogsAbandonMessage logs) => await _database.LogsAbandonMessages.AnyAsync(x => x.Type == logs.Type && x.WebJob == logs.WebJob && x.Method == logs.Method && x.Error == logs.Error && x.Payload == logs.Payload);

    }
}
