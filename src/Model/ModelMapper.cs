using MMKiwi.PodderNet.Model.Database;
using MMKiwi.PodderNet.Model.GPodderApi;

using Riok.Mapperly.Abstractions;

namespace MMKiwi.PodderNet.Model;

[Mapper]
public static partial class ModelMapper
{
    [MapperIgnoreSource(nameof(Device.Id))]
    [MapperIgnoreSource(nameof(Device.UserId))]
    [MapProperty("PublicId", "Id")]
    public static partial DeviceResponse ToDevice(this Device device);
}