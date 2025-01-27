using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monster_Trading_Cards_Game;
using Monster_Trading_Cards_Game.Database;  
using Monster_Trading_Cards_Game.Repositories;
using Monster_Trading_Cards_Game.Models;
using Monster_Trading_Cards_Game.Network;
using Monster_Trading_Cards_Game.Interfaces;
using System;
using Npgsql;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Unit_Tests_MTCG
{
    [TestClass]
    public class TestsCards
    {
        [TestMethod]
        public void TestCalculateDamage()
        {
            // Arrange: Konkrete Karten erstellen
            var card1 = new MonsterCard(1, "Goblins", 50, ElementType.Normal);
            var card2 = new MonsterCard(2, "Dragons", 60, ElementType.Fire);

            // Act: Die Methode ausführen
            var damage = card1.CalculateDamage(card2);

            // Assert: Das Ergebnis überprüfen
            Assert.AreEqual(0, damage, "Goblins should deal 0 damage to Dragons due to the special rule.");
        }
    }
}