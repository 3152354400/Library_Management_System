using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using backend.DTOs.Book;

public class BookCategoryTreeOperation
{
    private readonly string _connectionString;

    public BookCategoryTreeOperation(string connectionString)
    {
        _connectionString = connectionString;
    }

    // 获取所有分类
    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        var sql = @"
            SELECT CategoryID, CategoryName, ParentCategoryID
            FROM Category
            ORDER BY CategoryID";

        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();
        return await connection.QueryAsync<Category>(sql);
    }

    // 根据ID获取分类
    public async Task<Category?> GetCategoryByIdAsync(string categoryId)
    {
        var sql = @"
            SELECT CategoryID, CategoryName, ParentCategoryID
            FROM Category
            WHERE CategoryID = :categoryId";

        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();
        return await connection.QueryFirstOrDefaultAsync<Category>(sql, new { categoryId });
    }

    // 添加分类
    public async Task<int> AddCategoryAsync(Category category)
    {
        // 现在支持手动输入CategoryID
        var sql = @"
            INSERT INTO Category (CategoryID, CategoryName, ParentCategoryID)
            VALUES (:CategoryID, :CategoryName, :ParentCategoryID)";

        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();
        
        return await connection.ExecuteAsync(sql, category);
    }

    // 更新分类
    public async Task<int> UpdateCategoryAsync(Category category)
    {
        var sql = @"
            UPDATE Category 
            SET CategoryName = :CategoryName, ParentCategoryID = :ParentCategoryID
            WHERE CategoryID = :CategoryID";

        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();
        return await connection.ExecuteAsync(sql, category);
    }

    // 删除分类
    public async Task<int> DeleteCategoryAsync(string categoryId)
    {
        var sql = @"
            DELETE FROM Category 
            WHERE CategoryID = :categoryId";

        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();
        return await connection.ExecuteAsync(sql, new { categoryId });
    }

    // 检查分类下是否有关联图书
    public async Task<bool> HasBookInCategoryAsync(string categoryId)
    {
        var sql = @"
            SELECT COUNT(*)
            FROM Book_Classify
            WHERE CategoryID = :categoryId";

        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { categoryId });
        return count > 0;
    }

    // 获取分类的子分类数量
    public async Task<int> GetChildCategoryCountAsync(string categoryId)
    {
        var sql = @"
            SELECT COUNT(*)
            FROM Category
            WHERE ParentCategoryID = :categoryId";

        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();
        return await connection.ExecuteScalarAsync<int>(sql, new { categoryId });
    }

    // 获取分类的完整路径（从根到当前节点）
    public async Task<List<string>> GetCategoryPathAsync(string categoryId)
    {
        var path = new List<string>();
        var currentId = categoryId;

        while (!string.IsNullOrEmpty(currentId))
        {
            var category = await GetCategoryByIdAsync(currentId);
            if (category == null) break;

            path.Insert(0, category.CategoryName);
            currentId = category.ParentCategoryID ?? string.Empty;
        }

        return path;
    }

    // 检查分类名称在同级中是否重复
    public async Task<bool> IsCategoryNameDuplicateAsync(string categoryName, string? parentCategoryId, string? excludeCategoryId = null)
    {
        var sql = @"
            SELECT COUNT(*)
            FROM Category
            WHERE CategoryName = :categoryName 
            AND (ParentCategoryID = :parentCategoryId OR (ParentCategoryID IS NULL AND :parentCategoryId IS NULL))";

        object parameters;
        
        if (!string.IsNullOrEmpty(excludeCategoryId))
        {
            sql += " AND CategoryID != :excludeCategoryId";
            parameters = new { categoryName, parentCategoryId, excludeCategoryId };
        }
        else
        {
            parameters = new { categoryName, parentCategoryId };
        }

        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();
        var count = await connection.ExecuteScalarAsync<int>(sql, parameters);
        return count > 0;
    }
} 