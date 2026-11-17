using Aerisyn.DataConfigSheet;
using NUnit.Framework;

namespace Aerisyn.DataConfigSheet.Tests
{
    /// <summary>
    /// Environment Profile resolve seam (SpreadsheetIdResolver): Source Sheet spreadsheet id from
    /// active Environment Key + Feature Key. Hit returns the id; every miss fails clearly.
    /// </summary>
    public sealed class SpreadsheetIdResolverTests
    {


        #region Fixtures

        static readonly string[] EnvironmentKeys = { "Dev", "Test", "Production" };
        static readonly string[] FeatureKeys = { "ai_ship", "cannon" };


        static EnvironmentFeatureBinding[] CreateBindings()
        {
            return new[]
            {
                new EnvironmentFeatureBinding("Dev", "ai_ship", "dev-ai-ship-id"),
                new EnvironmentFeatureBinding("Dev", "cannon", "dev-cannon-id"),
                new EnvironmentFeatureBinding("Production", "ai_ship", "prod-ai-ship-id"),
            };
        }

        #endregion


        #region Hit

        [Test]
        public void Resolve_ActiveEnvironmentAndFeatureBound_ReturnsSpreadsheetId()
        {
            string spreadsheetId = SpreadsheetIdResolver.Resolve(
                EnvironmentKeys, FeatureKeys, CreateBindings(), "Dev", "cannon");

            Assert.That(spreadsheetId, Is.EqualTo("dev-cannon-id"));
        }


        [Test]
        public void Resolve_SameFeatureInOtherActiveEnvironment_ReturnsThatEnvironmentsId()
        {
            string spreadsheetId = SpreadsheetIdResolver.Resolve(
                EnvironmentKeys, FeatureKeys, CreateBindings(), "Production", "ai_ship");

            Assert.That(spreadsheetId, Is.EqualTo("prod-ai-ship-id"));
        }

        #endregion


        #region Miss

        [Test]
        public void Resolve_FeatureNotBoundInActiveEnvironment_ThrowsNamingEnvironmentAndFeature()
        {
            // cannon is bound in Dev only; Production has no row for it.
            InvalidOperationException error = Assert.Throws<InvalidOperationException>(
                () => SpreadsheetIdResolver.Resolve(
                    EnvironmentKeys, FeatureKeys, CreateBindings(), "Production", "cannon"));

            Assert.That(error!.Message, Does.Contain("Production"));
            Assert.That(error.Message, Does.Contain("cannon"));
            Assert.That(error.Message, Does.Contain("spreadsheet id"));
        }


        [Test]
        public void Resolve_BoundIdIsBlank_Throws()
        {
            EnvironmentFeatureBinding[] bindings =
            {
                new EnvironmentFeatureBinding("Dev", "cannon", "  "),
            };

            InvalidOperationException error = Assert.Throws<InvalidOperationException>(
                () => SpreadsheetIdResolver.Resolve(EnvironmentKeys, FeatureKeys, bindings, "Dev", "cannon"));

            Assert.That(error!.Message, Does.Contain("Dev"));
            Assert.That(error.Message, Does.Contain("cannon"));
        }


        [Test]
        public void Resolve_FeatureKeyNotInFeatureList_Throws()
        {
            InvalidOperationException error = Assert.Throws<InvalidOperationException>(
                () => SpreadsheetIdResolver.Resolve(
                    EnvironmentKeys, FeatureKeys, CreateBindings(), "Dev", "chestgacha"));

            Assert.That(error!.Message, Does.Contain("Feature Key"));
            Assert.That(error.Message, Does.Contain("chestgacha"));
        }


        [TestCase("Staging")]
        [TestCase("")]
        [TestCase(null)]
        public void Resolve_ActiveEnvironmentNotInEnvironmentList_Throws(string activeEnvironmentKey)
        {
            InvalidOperationException error = Assert.Throws<InvalidOperationException>(
                () => SpreadsheetIdResolver.Resolve(
                    EnvironmentKeys, FeatureKeys, CreateBindings(), activeEnvironmentKey, "cannon"));

            Assert.That(error!.Message, Does.Contain("Environment Key"));
        }


        [Test]
        public void Resolve_BindingForKeyNoLongerInList_IsNotUsed()
        {
            // Stale row: "Staging" was removed from the Environment Key list after binding.
            string[] environmentKeys = { "Dev" };
            EnvironmentFeatureBinding[] bindings =
            {
                new EnvironmentFeatureBinding("Staging", "cannon", "stale-id"),
            };

            Assert.Throws<InvalidOperationException>(
                () => SpreadsheetIdResolver.Resolve(environmentKeys, FeatureKeys, bindings, "Staging", "cannon"));
        }

        #endregion


    }
}
