using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Monster_Trading_Cards_Game.Models
{
    public class MonsterCard : Card
    {
        public MonsterCard(int id, string name, int damage, ElementType elementType)
            : base(id, name, damage, elementType)
        {
        }

        public override void PlayCard()
        {
            Console.WriteLine($"Monster card '{Name}' is cast! It has {CardElementType} element.");
        }
    }
}


