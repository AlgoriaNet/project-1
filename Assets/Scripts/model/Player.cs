using System.Collections.Generic;
using System.Numerics;

namespace model
{
    public class Player : ApplicationModel
    {
        public BigInteger Id;
        public string Name;
        public int Level;
        public int Exp;
        public int GoldCoin;
        public int Diamond;
        public int Stamina;
        public List<Equipment> Equipments;
        public List<Gemstone> Gemstones; 
    }
}