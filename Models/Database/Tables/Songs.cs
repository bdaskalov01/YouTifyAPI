namespace WebAPIProgram.Models.Database.Tables;

public class Songs
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Artist_Id { get; set; }
    public DateOnly Release_Date { get; set; }
    public int Likes { get; set; }
}