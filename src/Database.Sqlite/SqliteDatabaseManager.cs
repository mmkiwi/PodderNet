using System.Security.Cryptography;

using Dapper;

using Microsoft.Data.Sqlite;

using MMKiwi.PodderNet.Model.Database;
using MMKiwi.PodderNet.Model.GPodderApi;

namespace MMKiwi.PodderNet.Database.Sqlite;

public class SqliteDatabaseManager : IDatabaseManager
{
    private readonly SqliteConnection _connection;

    public SqliteDatabaseManager(SqliteConnection connection)
    {
        _connection = connection;
    }

    private const int SaltSize = 32;
    private const int KeySize = 128; // 1024 bits
    private const int Iterations = 50000;
    private static HashAlgorithmName HashAlgorithm => HashAlgorithmName.SHA512;

    public async Task<bool> ValidatePassword(string username, byte[] password)
    {
        const string query = "SELECT * FROM Users WHERE Username=@Param1";
        var parameter = Parameter.Create(username);

        User userToTest = await _connection.QueryFirstOrDefaultAsync<User>(query, parameter) ?? new User // ok
        {
            Username = username, Salt = new byte[SaltSize], PasswordHash = new byte[KeySize],
        };
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, userToTest.Salt, Iterations, HashAlgorithm,
            KeySize);

        return CryptographicOperations.FixedTimeEquals(userToTest.PasswordHash, hash);
    }

    public Task<bool> CreateUser(string username, ReadOnlySpan<byte> password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithm,
            KeySize
        );

        const string query =
            """
            INSERT INTO Users (Username, PasswordHash, Salt)
            VALUES (@Username, @PasswordHash, @Salt)
            """;

        User userToAdd = new() { Username = username, PasswordHash = hash, Salt = salt };

        return ImplAsync(_connection, userToAdd);

        static async Task<bool> ImplAsync(SqliteConnection connection, User userToAdd)
        {
            var res = await connection.ExecuteAsync(query, userToAdd); // ok

            return res > 0;
        }
    }

    public async Task UpdateOrInsertDevice(int userId, string publicId,
        DeviceRequest deviceInfo)
    {
        ArgumentNullException.ThrowIfNull(publicId);

        const string currentDeviceQuery =
            """
            SELECT Id FROM Devices 
            WHERE UserId = @Param1 AND Devices.PublicId = @Param2
            LIMIT 1
            """;

        const string insertQuery =
            """
            INSERT INTO Devices (UserId, PublicId, Caption, DeviceType, Subscriptions)
            VALUES (@UserId, @PublicId, @DeviceType, @Caption, @Subscriptions)
            """;

        var parameters = Parameter.Create(userId, publicId);

        var current = await _connection.ExecuteScalarAsync<int>(currentDeviceQuery, parameters); // ok

        if (current > 0)
        {
            await _connection.ExecuteAsync(insertQuery, // ok
                new Device()
                {
                    UserId = userId,
                    PublicId = publicId,
                    Caption = deviceInfo.Caption,
                    Type = deviceInfo.Type,
                    Subscriptions = 0
                }
            );
        }
    }

    public IAsyncEnumerable<Device> GetDevices(string username)
    {
        const string query =
            """
            SELECT Devices.*, Users.Id, Users.Username FROM Devices
            LEFT JOIN Users on Users.Id = Devices.UserId
            WHERE Username=@Param1
            """;
        var parameter = Parameter.Create(username);
        return _connection.QueryUnbufferedAsync<Device>(query, parameter); // OK
    }

    public async Task<Device> GetDevice(string publicId, string username)
    {
        const string query =
            """
            SELECT Devices.* From Devices
                INNER JOIN Users ON Users.Id = Devices.UserId
                WHERE PublicID = @Param1 AND Username = @Param2
            """;

        var parameters = Parameter.Create(publicId, username);

        return await _connection.QuerySingleAsync<Device>(query, parameters); // ok
    }

    public async Task EnsureCreated()
    {
        const string CreateTables =
            """
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username VARCHAR(50) NOT NULL,
                Salt CHAR(32) NOT NULL,
                PasswordHash CHAR(128) NOT NULL);

            CREATE UNIQUE INDEX IF NOT EXISTS Users_Username
                ON Users(Username);

                CREATE TABLE IF NOT EXISTS Devices (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserId INTEGER NOT NULL,
                    PublicId VARCHAR(50) NOT NULL,
                    Caption VARCHAR(50),
                    DeviceType INTEGER,
                    Subscriptions INTEGER NOT NULL,

                    FOREIGN KEY (UserId) REFERENCES Users(Id)
                );

            CREATE UNIQUE INDEX IF NOT EXISTS Users_Username
                ON Users(Username);
            CREATE UNIQUE INDEX IF NOT EXISTS Users_Username_PublicId
                ON Users(Username);
            """;
        await _connection.ExecuteAsync(CreateTables); // ok
    }

    public ValueTask DisposeAsync() => _connection.DisposeAsync();
    public void Dispose() => _connection.Dispose();
}

