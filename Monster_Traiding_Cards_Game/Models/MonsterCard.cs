using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monster_Trading_Cards_Game.Models
{
    public class MonsterCard: Card
    {
        public MonsterCard(string name) : base(name)
        {
        }

        public override void PlayCard()
        {
            Console.WriteLine($"Monster card '{Name}' is cast! It has {CardElementType} element.");
        }
    }
}
