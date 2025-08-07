namespace WebAPIProgram.Models.Database.Tables;

public class Songs
{ 
    public string Id { get; set; }
    public string Title { get; set; }
    public string Thumbnail { get; set; }
    public string Artist_Id { get; set; }
    public DateTime Release_Date { get; set; }
    public int Likes { get; set; }
}