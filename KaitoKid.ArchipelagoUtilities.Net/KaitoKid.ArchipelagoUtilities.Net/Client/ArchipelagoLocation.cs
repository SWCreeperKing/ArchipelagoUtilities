namespace KaitoKid.ArchipelagoUtilities.Net.Client
{
    public class ArchipelagoLocation
    {
        public string Name { get; set; }
        public long Id { get; set; }

        public ArchipelagoLocation(string name, long id)
        {
            Name = name;
            Id = id;
        }
    }
}