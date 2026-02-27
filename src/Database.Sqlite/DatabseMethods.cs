using System.Security.Cryptography;

using Dapper;

using Microsoft.Data.Sqlite;

using MMKiwi.PodderNet.Model.Database;
using MMKiwi.PodderNet.Model.GPodderApi;

namespace MMKiwi.PodderNet.Database.Sqlite;

public class SqliteDatabaseManager: IDatabaseManager
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
        User userToTest =
            await await _connection.QuerySingleAsync("SELECT * FROM Users WHERE Username=@Username",
                new { Username = username }) ?? new User()
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

        User userToAdd = new() { Username = username, PasswordHash = hash, Salt = salt };

        return ImplAsync(_connection, userToAdd);

        static async Task<bool> ImplAsync(SqliteConnection _connection, User userToAdd)
        {
            var res = await _connection.ExecuteAsync(
                "INSERT INTO Users (Username, PasswordHash, Salt) VALUES (@Username, @PasswordHash, @Salt)", userToAdd);

            return res > 0;
        }
    }

    public async Task UpdateOrInsertDevice(int userId, string id,
        DeviceRequest deviceInfo)
    {
        ArgumentNullException.ThrowIfNull(id);

        var current = await _connection.QueryFirstOrDefaultAsync<int?>(
            "SELECT Id FROM Devices " +
            "WHERE UserId = @UserId AND Devices.PublicId = @Id"
            , new { UserId = userId, Id = id });

        if (current.HasValue)
        {
            await _connection.ExecuteAsync(
                "INSERT INTO Devices (UserId, PublicId, Caption, DeviceType, Subscriptions) " +
                "VALUES (@UserId, @PublicId, @DeviceType, @Caption, @Subscriptions)",
                new Device()
                {
                    UserId = userId,
                    PublicId = id,
                    Caption = deviceInfo.Caption,
                    Type = deviceInfo.Type,
                    Subscriptions = 0
                }
            );
        }
    }

    public IAsyncEnumerable<Device> GetDevices(string username)
    {
        return _connection.QueryUnbufferedAsync<Device>(
            "SELECT Devices.*, Users.Id, Users.Username FROM Devices LEFT JOIN Users on Users.Id = Devices.UserId WHERE Username=@Username",
            new { Username = username });
    }

    public Task<Device> GetDevice(string publicId, string username)
    {
        const string query =
            """
            SELECT Devices.* From Devices
                INNER JOIN Users ON Users.Id = Devices.UserId
                WHERE PublicID = @PublicID AND Username = @Username
            """;
        
        return _connection.QuerySingle(query,
            new { PublicId = publicId, Username = username });
    }

    public async Task EnsureCreated()
    {
        const string UsersCreate = 
        """
        CREATE TABLE IF NOT EXISTS Users (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Username VARCHAR(50) NOT NULL,
            Salt CHAR(32) NOT NULL,
            PasswordHash CHAR(128) NOT NULL);
        """;
        
        const string DevicesCreate =
            """
            CREATE TABLE IF NOT EXISTS Devices (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId INTEGER NOT NULL,
                PublicId VARCHAR(50) NOT NULL,
                Caption VARCHAR(50),
                DeviceType INTEGER,
                Subscriptions INTEGER NOT NULL,
                
                FOREIGN KEY (UserId)
                REFERENCES Users(Id) 
            );
            """;
        await _connection.ExecuteAsync(UsersCreate);
        await _connection.ExecuteAsync(DevicesCreate);
    }

    public ValueTask DisposeAsync() => _connection.DisposeAsync();
    public void Dispose() => _connection.Dispose();
}