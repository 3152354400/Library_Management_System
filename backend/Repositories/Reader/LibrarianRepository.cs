using Dapper;
using Oracle.ManagedDataAccess.Client;
using backend.Common.Utils;
using backend.Models;


namespace backend.Repositories.LibrarianRepository;

public class LibrarianRepository
{
    /**
     * 数据库连接字符串
     */
    private readonly string _connectionString;


    /**
     * 构造函数
     * @param connectionString 数据库连接字符串
     * @return 无
     */
    public LibrarianRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    /**
     * 根据 LibrarianID 获取 Librarian 信息
     * @param librarianID 读者 ID
     * @return 返回 Librarian 对象
     */
    public async Task<Librarian> GetByLibrarianIDAsync(long librarianID)
    {
        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();
        var sql = "SELECT * FROM Librarian WHERE LibrarianID = :LibrarianID";
        return await connection.QueryFirstOrDefaultAsync<Librarian>(sql, new { LibrarianID = librarianID });
    }

    /**
     * 根据 StaffNo 获取 Librarian 信息
     * @param staffNo 用户名
     * @return 返回 Librarian 对象
     */
    public async Task<Librarian> GetByStaffNoAsync(string staffNo)
    {
        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();
        var sql = "SELECT * FROM Librarian WHERE StaffNo = :StaffNo";
        return await connection.QueryFirstOrDefaultAsync<Librarian>(sql, new { StaffNo = staffNo});
    }

    /**
     * 获取所有 Librarian 信息
     * @return 返回 Librarian 对象列表
     */
    public async Task<IEnumerable<Librarian>> GetAllLibrariansAsync()
    {
        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();
        var sql = "SELECT * FROM Librarian";
        return await connection.QueryAsync<Librarian>(sql);
    }

    /**
     * 新增一个 Librarian
     * @param librarian Librarian 对象
     * @return 新增成功返回 1，否则返回 0
     */
    public async Task<int> InsertLibrarianAsync(Librarian librarian)
    {
        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();

        librarian.Password = PasswordUtils.HashPassword(librarian.Password);

        var sql = @"
            INSERT INTO Librarian (StaffNo, Password, Name, Permission)
            VALUES (:StaffNo, :Password, :Name, :Permission)";

        return await connection.ExecuteAsync(sql, librarian);
    }

    /**
     * ����һ�� Librarian ����
     * @param librarian Librarian ����
     * @return ������Ӱ�������
     */
    public async Task<int> UpdateLibrarianAsync(Librarian librarian)
    {
        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();

        librarian.Password = PasswordUtils.HashPassword(librarian.Password); // ȷ�����뱻��ϣ����

        var sql = @"
            UPDATE Librarian
            SET StaffNo = :StaffNo,
                Password = :Password,
                Name = :Name,
                Permission = :Permission
            WHERE LibrarianID = :LibrarianID";

        return await connection.ExecuteAsync(sql, librarian);
    }


    /**
     * ���� LibrarianID ɾ��һ�� Librarian ����
     * @param librarianID LibrarianID
     * @return ������Ӱ�������
     */
    public async Task<int> DeleteLibrarianAsync(long librarianID)
    {
        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();
        var sql = "DELETE FROM Librarian WHERE LibrarianID = :LibrarianID";
        return await connection.ExecuteAsync(sql, new { LibrarianID = librarianID });
    }

    /**
     * ����û����Ƿ����
     * @param staffNo �û���
     * @return ���� true ������ڣ����򷵻� false
     */
    public async Task<bool> IsStaffNoExistsAsync(string staffNo)
    {
        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();
        var sql = "SELECT COUNT(*) FROM Librarian WHERE StaffNo = :StaffNo";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { StaffNo = staffNo });
        return count > 0;
    }


    /**
        * ��������
        */
    public async Task<int> ResetPasswordAsync(string staffNo, string newPassword)
    {
        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"
            UPDATE Librarian
            SET Password = :newPassword
            WHERE StaffNo = :staffNo";

        return await connection.ExecuteAsync(sql, new { newPassword, staffNo });
    }


}
