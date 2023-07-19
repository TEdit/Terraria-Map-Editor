namespace TEdit.Configuration;

public class BestiaryNpcData
{
    public int Id { get; set; }
    public int BannerId { get; set; }
    public string FullName { get; set; }
    public string Name { get; set; }
    public string BestiaryId { get; set; }
    public bool CanTalk { get; set; }
    public bool IsCritter { get; set; }
    public bool IsTownNpc { get; set; }
    public bool IsKillCredit { get; set; }
    public int BestiaryDisplayIndex { get; set; }
}
