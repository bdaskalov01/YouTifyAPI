namespace WebAPIProgram.Models.Kafka;

public class ProfileViewEvent
{
    public string ViewedUserId { get; set; }
    public string ViewerId { get; set; }
    public DateTime Timestamp { get; set; }
}