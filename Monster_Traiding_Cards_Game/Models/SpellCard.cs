using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Monster_Trading_Cards_Game.Models
{
    public class SpellCard : Card
    {
        public SpellCard(int id, string name, int damage, ElementType elementType)
            : base(id, name, damage, elementType)
        {
        }

        public override void PlayCard()
        {
            Console.WriteLine($"Spell card '{Name}' is cast! It has {CardElementType} element.");
        }
    }
}

