namespace Rancher.Scrappers.Discourse
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Avatar_template { get; set; }
        public string Email { get; set; }
        public bool Active { get; set; }
        public bool Admin { get; set; }
        public bool Moderator { get; set; }
        public string LastSeenAt { get; set; }
        public string LastEmailedAt { get; set; }
        public string CreatedAt { get; set; }
        public string LastSeenAge { get; set; }
        public string LastEmailedAge { get; set; }
        public string CreatedAtAge { get; set; }
        public string UsernameLower { get; set; }
        public int TrustLevel { get; set; }
        public bool TrustLevelLocked { get; set; }
        public int FlagLevel { get; set; }
        public string Title { get; set; }
        public object SuspendedAt { get; set; }
        public object SuspendedTill { get; set; }
        public object Suspended { get; set; }
        public bool Blocked { get; set; }
        public string TimeRead { get; set; }
        public bool Staged { get; set; }
        public int DaysVisited { get; set; }
        public int PostsReadCount { get; set; }
        public int TopicsEntered { get; set; }
        public int PostCount { get; set; }
    }
}