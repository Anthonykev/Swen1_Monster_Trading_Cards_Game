using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monster_Trading_Cards_Game.Models;

namespace Unit_Tests_MTCG
{
    [TestClass]
    public class CardTests
    {
        [TestMethod]
        public void TestCalculateDamage_GoblinsDoNotAttackDragons()
        {
            // Arrange: Konkrete Karten erstellen
            var card1 = new MonsterCard(1, "Goblins", 50, ElementType.Normal);
            var card2 = new MonsterCard(2, "Dragons", 60, ElementType.Fire);

            // Act: Die Methode ausführen
            var damage = card1.CalculateDamage(card2);

            // Assert: Das Ergebnis überprüfen
            Assert.AreEqual(0, damage, "Goblins should deal 0 damage to Dragons due to the special rule.");
        }

        [TestMethod]
        public void TestCalculateDamage_OrksDealNoDamageToWizzards()
        {
            var card1 = new MonsterCard(3, "Orks", 50, ElementType.Normal);
            var card2 = new MonsterCard(4, "Wizzard", 70, ElementType.Fire);

            var damage = card1.CalculateDamage(card2);

            Assert.AreEqual(0, damage, "Orks should deal no damage to Wizzards due to the special rule.");
        }

        [TestMethod]
        public void TestCalculateDamage_KnightsLoseToTsunami()
        {
            var card1 = new MonsterCard(5, "Knights", 50, ElementType.Normal);
            var card2 = new SpellCard(6, "Tsunami", 70, ElementType.Water);

            var damage = card2.CalculateDamage(card1);

            Assert.AreEqual(double.MaxValue, damage, "Knights should instantly lose to Tsunami due to the special rule.");
        }

        [TestMethod]
        public void TestCalculateDamage_KrakenImmuneToSpells()
        {
            var card1 = new MonsterCard(7, "Kraken", 60, ElementType.Water);
            var card2 = new SpellCard(8, "Fire Spell", 50, ElementType.Fire);

            var damage = card2.CalculateDamage(card1);

            Assert.AreEqual(0, damage, "Kraken should take no damage from Spells due to the special rule.");
        }

        [TestMethod]
        public void TestCalculateDamage_FireElvesEvadeDragons()
        {
            var card1 = new MonsterCard(9, "FireElves", 50, ElementType.Fire);
            var card2 = new MonsterCard(10, "Dragons", 70, ElementType.Fire);

            var damage = card2.CalculateDamage(card1);

            Assert.AreEqual(0, damage, "FireElves should evade attacks from Dragons due to the special rule.");
        }

        [TestMethod]
        public void TestCalculateDamage_WaterDealsDoubleDamageToFire()
        {
            var card1 = new SpellCard(11, "Water Spell", 40, ElementType.Water);
            var card2 = new MonsterCard(12, "Fire Monster", 50, ElementType.Fire);

            var damage = card1.CalculateDamage(card2);

            Assert.AreEqual(80, damage, "Water should deal double damage to Fire.");
        }

        [TestMethod]
        public void TestCalculateDamage_FireDealsDoubleDamageToNormal()
        {
            var card1 = new SpellCard(13, "Fire Spell", 40, ElementType.Fire);
            var card2 = new MonsterCard(14, "Normal Monster", 50, ElementType.Normal);

            var damage = card1.CalculateDamage(card2);

            Assert.AreEqual(80, damage, "Fire should deal double damage to Normal.");
        }

        [TestMethod]
        public void TestCalculateDamage_NormalDealsDoubleDamageToWater()
        {
            var card1 = new MonsterCard(15, "Normal Monster", 50, ElementType.Normal);
            var card2 = new MonsterCard(16, "Water Monster", 50, ElementType.Water);

            var damage = card1.CalculateDamage(card2);

            Assert.AreEqual(100, damage, "Normal should deal double damage to Water.");
        }

        [TestMethod]
        public void TestCalculateDamage_SameElementDealsNormalDamage()
        {
            var card1 = new MonsterCard(23, "Fire Monster", 50, ElementType.Fire);
            var card2 = new MonsterCard(24, "Fire Monster", 50, ElementType.Fire);

            var damage = card1.CalculateDamage(card2);

            Assert.AreEqual(50, damage, "Same elements should deal normal damage.");
        }

        [TestMethod]
        public void TestCalculateDamage_NormalVsNormalDealsNormalDamage()
        {
            var card1 = new MonsterCard(25, "Normal Monster", 50, ElementType.Normal);
            var card2 = new MonsterCard(26, "Normal Monster", 50, ElementType.Normal);

            var damage = card1.CalculateDamage(card2);

            Assert.AreEqual(50, damage, "Normal vs. Normal should deal normal damage.");
        }

        [TestMethod]
        public void TestCalculateDamage_FireVsWaterDealsHalfDamage()
        {
            var card1 = new MonsterCard(27, "Fire Monster", 50, ElementType.Fire);
            var card2 = new MonsterCard(28, "Water Monster", 50, ElementType.Water);

            var damage = card1.CalculateDamage(card2);

            Assert.AreEqual(25, damage, "Fire should deal half damage to Water.");
        }

        [TestMethod]
        public void TestCalculateDamage_WaterVsFireDealsDoubleDamage()
        {
            var card1 = new MonsterCard(29, "Water Monster", 50, ElementType.Water);
            var card2 = new MonsterCard(30, "Fire Monster", 50, ElementType.Fire);

            var damage = card1.CalculateDamage(card2);

            Assert.AreEqual(100, damage, "Water should deal double damage to Fire.");
        }
    }
}
