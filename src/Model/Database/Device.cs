using System.ComponentModel.DataAnnotations;

namespace MMKiwi.PodderNet.Model.Database;

public class Device
{
    [Key] public int Id { get; set; }

    public required int UserId { get; set; }

    public required string PublicId { get; set; }

    public string? Caption { get; set; }

    public DeviceType? Type { get; set; }

    public required int Subscriptions { get; set; }
}