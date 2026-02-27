namespace MMKiwi.PodderNet.Model.Database;

public class User
{
    public int Id { get; set; }

    public required string Username { get; set; }
    
    public required byte[] Salt { get; set; }

    public required byte[] PasswordHash { get; set; }
}