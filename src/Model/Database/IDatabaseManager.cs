using System.Data.Common;

using MMKiwi.PodderNet.Model.GPodderApi;

namespace MMKiwi.PodderNet.Model.Database;

public interface IDatabaseManager: IAsyncDisposable, IDisposable
{
    public Task<bool> ValidatePassword(string username, byte[] password);
    public Task<bool> CreateUser(string username, ReadOnlySpan<byte> password);
    public Task UpdateOrInsertDevice(int userId, string publicId, DeviceRequest deviceInfo);
    public IAsyncEnumerable<Device> GetDevices(string username);
    public Task<Device> GetDevice(string publicId, string username);
    public Task EnsureCreated();
}