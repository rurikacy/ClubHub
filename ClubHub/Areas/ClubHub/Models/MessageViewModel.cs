using Repository.DbEntity;

namespace ClubHub.Areas.ClubHub.Models;

public class MessageViewModel : Message
{
    public string ClubName { get; set; }
    public string AdminName { get; set; }
}