using System;

namespace GameStore.Domain
{
    public class Game
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Developer { get; set; } = "";
        public string Genre { get; set; } = "";
        public DateTime ReleaseDate { get; set; }
        public string GameMode { get; set; } = ""; // Однопользовательский / Многопользовательский
        public int CopiesSold { get; set; }
    }
}