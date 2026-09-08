using Dapper;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.DTOs.Reader;
using System;

namespace backend.Repositories.Space
{
    public class SpaceRepository
    {
        private readonly string _connectionString;
        public SpaceRepository(string connectionString) { _connectionString = connectionString; }

        public async Task<IEnumerable<SeatDto>> GetSeatLayoutAsync(int buildingId, int floor)
        {
            var sql = "SELECT * FROM V_Seat_Status WHERE BuildingID = :BuildingID AND Floor = :Floor ORDER BY SeatNumber";
            using var connection = new OracleConnection(_connectionString);
            return await connection.QueryAsync<SeatDto>(sql, new { BuildingID = buildingId, Floor = floor });
        }

        public async Task CreateSeatReservationInTransactionAsync(int seatId, long readerId, DateTime startTime, DateTime endTime)
        {
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                // 步骤 1: 在事务内检查读者是否已有未完成的预约 (移除 FOR UPDATE)
                var activeReservationsSql = "SELECT COUNT(*) FROM Reserve_Seat WHERE ReaderID = :ReaderID AND Status = '未完成'";
                var activeCount = await connection.ExecuteScalarAsync<int>(activeReservationsSql, new { ReaderID = readerId }, transaction);
                if (activeCount > 0)
                {
                    throw new InvalidOperationException("您已有有效的未完成预约，无法再次预约。");
                }

                // 步骤 2: 在事务内检查座位在该时间段是否已被占用 (移除 FOR UPDATE)
                var seatAvailableSql = "SELECT COUNT(*) FROM Reserve_Seat WHERE SeatID = :SeatID AND Status = '未完成' AND StartTime < :EndTime AND EndTime > :StartTime";
                var conflictingCount = await connection.ExecuteScalarAsync<int>(seatAvailableSql, new { SeatID = seatId, StartTime = startTime, EndTime = endTime }, transaction);
                if (conflictingCount > 0)
                {
                    throw new InvalidOperationException("该座位在此时间段已被预约，请选择其他时间。");
                }

                // 步骤 3: 在事务内插入新的预约记录
                var insertSql = @"
                    INSERT INTO Reserve_Seat (ReaderID, SeatID, StartTime, EndTime, Status) 
                    VALUES (:ReaderID, :SeatID, :StartTime, :EndTime, '未完成')";
                await connection.ExecuteAsync(insertSql, new { ReaderID = readerId, SeatID = seatId, StartTime = startTime, EndTime = endTime }, transaction);
                
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
            
        public async Task CreateSeatReservationAsync(int seatId, long readerId, DateTime startTime, DateTime endTime)
        {
            // 存入的 startTime 和 endTime 已经是 UTC 时间了
            var sql = @"
                INSERT INTO Reserve_Seat (ReaderID, SeatID, StartTime, EndTime, Status) 
                VALUES (:ReaderID, :SeatID, :StartTime, :EndTime, '未完成')";
            using var connection = new OracleConnection(_connectionString);
            await connection.ExecuteAsync(sql, new { ReaderID = readerId, SeatID = seatId, StartTime = startTime, EndTime = endTime });
        }

        public async Task<IEnumerable<MyReservationDto>> GetMyReservationsAsync(long readerId)
        {
            var sql = @"
                SELECT ReservationID, ReaderID, SeatID, BuildingName, Floor, SeatNumber, StartTime, EndTime, Status
                FROM V_MyReservations
                WHERE ReaderID = :ReaderID
                ORDER BY StartTime DESC";
                
            using var connection = new OracleConnection(_connectionString);
            return await connection.QueryAsync<MyReservationDto>(sql, new { ReaderID = readerId });
        }

        public async Task<bool> CancelReservationAsync(int reservationId, long readerId)
        {
            var sql = "UPDATE Reserve_Seat SET Status = '取消' WHERE ReservationID = :ReservationID AND ReaderID = :ReaderID AND Status = '未完成'";
            using var connection = new OracleConnection(_connectionString);
            var affectedRows = await connection.ExecuteAsync(sql, new { ReservationID = reservationId, ReaderID = readerId });
            return affectedRows > 0;
        }
    }
}