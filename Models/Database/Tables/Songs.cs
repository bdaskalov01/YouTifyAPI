namespace WebAPIProgram.Models.Database.Tables;

public class Songs
{
    //TODO: Change Id type from int to string
    public string Id { get; set; }
    public string Title { get; set; }
    //Todo: Change Artist_id type from int to string 
    public string Thumbnail { get; set; }
    public string Artist_Id { get; set; }
    public DateTime Release_Date { get; set; }
    public int Likes { get; set; }
}