namespace Football.Domain
{
    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Country { get; set; } = "";
        public int Number { get; set; }
        public string Position { get; set; } = "";
        public int Goals { get; set; }
        public int TeamId { get; set; }
        public Team? Team { get; set; }
    }
}