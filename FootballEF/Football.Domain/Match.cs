using System;

namespace Football.Domain
{
    public class Match
    {
        public int Id { get; set; }
        public int HomeTeamId { get; set; }
        public Team? HomeTeam { get; set; }
        public int AwayTeamId { get; set; }
        public Team? AwayTeam { get; set; }
        public int HomeGoals { get; set; }
        public int AwayGoals { get; set; }
        public DateTime MatchDate { get; set; }
    }
}