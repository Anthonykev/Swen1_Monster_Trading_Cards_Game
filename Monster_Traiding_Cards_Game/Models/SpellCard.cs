using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monster_Trading_Cards_Game.Models
{
     public class SpellCard : Card
    {

        public SpellCard(string name) : base(name)
        {
        }

        public override void PlayCard()
        {
            Console.WriteLine($"Spell card '{Name}' is cast! It has {CardElementType} element.");
        }
    }
}
