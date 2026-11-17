using Aerisyn.DataConfigSheet;
using NUnit.Framework;

namespace Aerisyn.DataConfigSheet.Tests
{
    /// <summary>
    /// Data Config Settings Environment Key / Feature Key list seam (KeyListRules): package seed defaults and
    /// pick membership. Inspectors may only pick keys that these lists define.
    /// </summary>
    public sealed class KeyListRulesTests
    {


        #region Seed defaults

        [Test]
        public void CreateSeedEnvironmentKeys_ReturnsDevTestProductionInOrder()
        {
            string[] seeded = KeyListRules.CreateSeedEnvironmentKeys();

            Assert.That(seeded, Is.EqualTo(new[] { "Dev", "Test", "Production" }));
        }


        [Test]
        public void CreateSeedEnvironmentKeys_ReturnsFreshArrayEachCall()
        {
            string[] first = KeyListRules.CreateSeedEnvironmentKeys();
            first[0] = "Mutated";

            Assert.That(KeyListRules.CreateSeedEnvironmentKeys()[0], Is.EqualTo("Dev"));
        }


        [Test]
        public void CreateSeedFeatureKeys_IsEmpty()
        {
            Assert.That(KeyListRules.CreateSeedFeatureKeys(), Is.Empty);
        }

        #endregion


        #region Pick membership

        [Test]
        public void IsMember_KeyInList_ReturnsTrue()
        {
            string[] environmentKeys = { "Dev", "Test", "Production" };

            Assert.That(KeyListRules.IsMember(environmentKeys, "Test"), Is.True);
        }


        [Test]
        public void IsMember_KeyNotInList_ReturnsFalse()
        {
            string[] environmentKeys = { "Dev", "Test", "Production" };

            Assert.That(KeyListRules.IsMember(environmentKeys, "Staging"), Is.False);
        }


        [Test]
        public void IsMember_DifferentCase_ReturnsFalse()
        {
            string[] environmentKeys = { "Dev", "Test", "Production" };

            Assert.That(KeyListRules.IsMember(environmentKeys, "dev"), Is.False);
        }


        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void IsMember_NullOrBlankKey_ReturnsFalse(string key)
        {
            string[] featureKeys = { "ai_ship", "" };

            Assert.That(KeyListRules.IsMember(featureKeys, key), Is.False);
        }


        [Test]
        public void IsMember_EmptyList_ReturnsFalse()
        {
            Assert.That(KeyListRules.IsMember(KeyListRules.CreateSeedFeatureKeys(), "ai_ship"), Is.False);
        }


        [Test]
        public void RequireMember_KeyNotInList_ThrowsNamingListAndKey()
        {
            string[] featureKeys = { "ai_ship", "cannon" };

            InvalidOperationException error = Assert.Throws<InvalidOperationException>(
                () => KeyListRules.RequireMember("Feature Key", featureKeys, "chestgacha"));

            Assert.That(error!.Message, Does.Contain("Feature Key"));
            Assert.That(error.Message, Does.Contain("chestgacha"));
            Assert.That(error.Message, Does.Contain("ai_ship"));
        }


        [Test]
        public void RequireMember_KeyInList_DoesNotThrow()
        {
            string[] featureKeys = { "ai_ship", "cannon" };

            Assert.DoesNotThrow(() => KeyListRules.RequireMember("Feature Key", featureKeys, "cannon"));
        }

        #endregion


    }
}
