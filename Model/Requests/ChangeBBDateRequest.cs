namespace Model.Requests;

public class ChangeBBDateRequest
{
    public List<Guid> ItemIds { get; set; } 
    public DateTime BBDate { get; set; }
}