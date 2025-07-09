using battle;

namespace model
{
    public class Sidekick : Living
    {
        public string id { get; set; }
        public string base_id { get; set; }
        public int skill_level { get; set; } = 1;
        public int star { get; set; } = 0;
        public bool is_deployed { get; set; }
        public string player_id { get; set; }
        public System.DateTime created_at { get; set; }
        public System.DateTime updated_at { get; set; }
    }
}