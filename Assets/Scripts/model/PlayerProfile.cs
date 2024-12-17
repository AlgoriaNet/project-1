using System.Collections.Generic;

namespace model
{
    public class PlayerProfile : ApplicationModel
    {
        public Player Player { get; private set; }
        public List<Sidekick> Sidekick { get; private set; } = new();

        private static PlayerProfile _data;

        public static PlayerProfile Data
        {
            get { return _data ??= new PlayerProfile(); }
        }

        private PlayerProfile(){}
        
        public void SetPlayer(Player player)
        {
            Player = player;
            NotifyListeners("Player");
        }
    }
}