using Dapper;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.DTOs.Admin;
using System.Data;

namespace backend.Repositories.Admin
{
    public class AnnouncementRepository
    {
        private readonly string _connectionString;

        public AnnouncementRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<AnnouncementDto>> GetAllAnnouncementsAsync()
        {
            const string sql = "SELECT * FROM Announcement ORDER BY CreateTime DESC";
            using var connection = new OracleConnection(_connectionString);
            return await connection.QueryAsync<AnnouncementDto>(sql);
        }

        public async Task<IEnumerable<AnnouncementDto>> GetPublicAnnouncementsAsync()
        {
            const string sql = @"
                SELECT *
                FROM Announcement
                WHERE TRIM(Status) = TRIM(:Status)
                ORDER BY CreateTime DESC";

            using var connection = new OracleConnection(_connectionString);
            return await connection.QueryAsync<AnnouncementDto>(sql, new { Status = "发布中" });
        }

        public async Task<AnnouncementDto> CreateAnnouncementAsync(UpsertAnnouncementDto dto, int librarianId)
        {
            const string sql = @"
                INSERT INTO Announcement (LibrarianID, Title, Content, TargetGroup, Status)
                VALUES (:LibrarianID, :Title, :Content, :TargetGroup, '发布中')
                RETURNING AnnouncementID, CreateTime INTO :AnnouncementID, :CreateTime";

            using var connection = new OracleConnection(_connectionString);
            var parameters = new DynamicParameters(dto);
            parameters.Add("LibrarianID", librarianId);
            parameters.Add("AnnouncementID", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("CreateTime", dbType: DbType.DateTime, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(sql, parameters);

            return new AnnouncementDto
            {
                AnnouncementID = parameters.Get<int>("AnnouncementID"),
                LibrarianID = librarianId,
                Title = dto.Title,
                Content = dto.Content,
                TargetGroup = dto.TargetGroup,
                Status = "发布中",
                CreateTime = parameters.Get<DateTime>("CreateTime")
            };
        }

        public async Task<AnnouncementDto?> UpdateAnnouncementAsync(int id, UpsertAnnouncementDto dto)
        {
            const string sql = @"
                UPDATE Announcement
                SET Title = :Title,
                    Content = :Content,
                    TargetGroup = :TargetGroup
                WHERE AnnouncementID = :AnnouncementID";

            using var connection = new OracleConnection(_connectionString);
            await connection.ExecuteAsync(sql, new { dto.Title, dto.Content, dto.TargetGroup, AnnouncementID = id });

            const string updatedSql = "SELECT * FROM Announcement WHERE AnnouncementID = :AnnouncementID";
            return await connection.QuerySingleOrDefaultAsync<AnnouncementDto>(updatedSql, new { AnnouncementID = id });
        }

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            const string sql = "UPDATE Announcement SET Status = :Status WHERE AnnouncementID = :AnnouncementID";
            using var connection = new OracleConnection(_connectionString);
            var affectedRows = await connection.ExecuteAsync(sql, new { Status = status, AnnouncementID = id });
            return affectedRows > 0;
        }
    }
}
