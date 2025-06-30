namespace WebAPIProgram.Models.Database.Tables;

public class Artists
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Subscribers { get; set; }
    public int Monthly_listeners { get; set; }
}