using System;
using System.Threading.Tasks;
using Dapper;
using Oracle.ManagedDataAccess.Client;

public class LogService
{
    private readonly string _connectionString;

    public LogService(string connectionString)
    {
        _connectionString = connectionString;
    }

    // 记录操作日志
    public async Task<int> LogOperationAsync(string operationContent, string operatorType, string operatorId, string operationStatus, string? errorMessage = null)
    {
        var sql = @"
            INSERT INTO Log (LogID, OperationTime, OperationContent, OperatorType, OperatorID, OperationStatus, ErrorMessage)
            VALUES (LOG_SEQ.NEXTVAL, CURRENT_TIMESTAMP, :operationContent, :operatorType, :operatorId, :operationStatus, :errorMessage)";

        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();
        return await connection.ExecuteAsync(sql, new 
        { 
            operationContent, 
            operatorType, 
            operatorId, 
            operationStatus, 
            errorMessage 
        });
    }

    // 记录分类添加日志
    public async Task LogCategoryAddAsync(string categoryName, string categoryPath, string operatorId)
    {
        var operationContent = $"添加分类：{categoryPath}";
        await LogOperationAsync(operationContent, "Librarian", operatorId, "成功");
    }

    // 记录分类修改日志
    public async Task LogCategoryUpdateAsync(string oldCategoryName, string newCategoryName, string categoryPath, string operatorId)
    {
        var operationContent = $"修改分类：{categoryPath}（{oldCategoryName} → {newCategoryName}）";
        await LogOperationAsync(operationContent, "Librarian", operatorId, "成功");
    }

    // 记录分类删除日志
    public async Task LogCategoryDeleteAsync(string categoryName, string categoryPath, string operatorId)
    {
        var operationContent = $"删除分类：{categoryPath}";
        await LogOperationAsync(operationContent, "Librarian", operatorId, "成功");
    }

    // 记录操作成功日志
    public async Task LogOperationSuccessAsync(string operationContent, string operatorType, string operatorId)
    {
        await LogOperationAsync(operationContent, operatorType, operatorId, "成功");
    }

    // 记录操作失败日志
    public async Task LogOperationFailureAsync(string operationContent, string operatorType, string operatorId, string errorMessage)
    {
        await LogOperationAsync(operationContent, operatorType, operatorId, "失败", errorMessage);
    }
} 