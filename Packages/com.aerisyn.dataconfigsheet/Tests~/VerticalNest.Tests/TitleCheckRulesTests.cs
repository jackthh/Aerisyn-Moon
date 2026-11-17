using Aerisyn.DataConfigSheet;
using NUnit.Framework;

namespace Aerisyn.DataConfigSheet.Tests
{
    /// <summary>
    /// Title Check prefix seam (TitleCheckRules): default prefixes per Environment Key, per-key
    /// overrides, and case-sensitive prefix match against the Source Sheet file title.
    /// </summary>
    public sealed class TitleCheckRulesTests
    {


        #region Prefix match

        [TestCase("[Dev]_AI_SHIP", "[Dev]")]
        [TestCase("[Dev]", "[Dev]")]
        [TestCase("[Tester]_CONVENTIONS", "[Tester]")]
        [TestCase("[Production]_CANNON", "[Production]")]
        public void Matches_TitleStartsWithPrefix_ReturnsTrue(string title, string prefix)
        {
            Assert.That(TitleCheckRules.Matches(title, prefix), Is.True);
        }


        [TestCase("[Tester]_AI_SHIP", "[Dev]")]
        [TestCase("[dev]_AI_SHIP", "[Dev]")]
        [TestCase("[DEV]_AI_SHIP", "[Dev]")]
        [TestCase("Dev_AI_SHIP", "[Dev]")]
        [TestCase("AI_SHIP [Dev]", "[Dev]")]
        [TestCase(" [Dev]_AI_SHIP", "[Dev]")]
        [TestCase("[Prod]_AI_SHIP", "[Production]")]
        [TestCase("", "[Dev]")]
        [TestCase(null, "[Dev]")]
        public void Matches_TitleDoesNotStartWithPrefix_ReturnsFalse(string? title, string prefix)
        {
            Assert.That(TitleCheckRules.Matches(title, prefix), Is.False);
        }


        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void Matches_BlankPrefix_ReturnsFalse(string? prefix)
        {
            // A blank prefix must never accept every title.
            Assert.That(TitleCheckRules.Matches("[Dev]_AI_SHIP", prefix), Is.False);
        }

        #endregion


        #region Expected prefix

        [TestCase("Dev", "[Dev]")]
        [TestCase("Test", "[Tester]")]
        [TestCase("Production", "[Production]")]
        public void ResolveExpectedPrefix_SeededEnvironmentWithoutOverride_ReturnsDefault(
            string environmentKey, string expectedPrefix)
        {
            string prefix = TitleCheckRules.ResolveExpectedPrefix(environmentKey, null);

            Assert.That(prefix, Is.EqualTo(expectedPrefix));
        }


        [Test]
        public void ResolveExpectedPrefix_OverrideForActiveEnvironment_WinsOverDefault()
        {
            EnvironmentTitlePrefix[] overrides =
            {
                new EnvironmentTitlePrefix("Test", "[QA]"),
            };

            Assert.That(TitleCheckRules.ResolveExpectedPrefix("Test", overrides), Is.EqualTo("[QA]"));
        }


        [Test]
        public void ResolveExpectedPrefix_OverrideForOtherEnvironment_IsIgnored()
        {
            EnvironmentTitlePrefix[] overrides =
            {
                new EnvironmentTitlePrefix("Test", "[QA]"),
            };

            Assert.That(TitleCheckRules.ResolveExpectedPrefix("Dev", overrides), Is.EqualTo("[Dev]"));
        }


        [Test]
        public void ResolveExpectedPrefix_BlankOverride_FallsBackToDefault()
        {
            // An unfilled Inspector row must not disable the guard.
            EnvironmentTitlePrefix[] overrides =
            {
                new EnvironmentTitlePrefix("Dev", "  "),
            };

            Assert.That(TitleCheckRules.ResolveExpectedPrefix("Dev", overrides), Is.EqualTo("[Dev]"));
        }


        [Test]
        public void ResolveExpectedPrefix_CustomEnvironmentWithOverride_ReturnsOverride()
        {
            EnvironmentTitlePrefix[] overrides =
            {
                new EnvironmentTitlePrefix("Staging", "[Staging]"),
            };

            Assert.That(TitleCheckRules.ResolveExpectedPrefix("Staging", overrides), Is.EqualTo("[Staging]"));
        }


        [Test]
        public void ResolveExpectedPrefix_CustomEnvironmentWithoutOverride_ThrowsNamingEnvironment()
        {
            InvalidOperationException error = Assert.Throws<InvalidOperationException>(
                () => TitleCheckRules.ResolveExpectedPrefix("Staging", null));

            Assert.That(error!.Message, Does.Contain("Staging"));
            Assert.That(error.Message, Does.Contain("title prefix"));
        }


        [Test]
        public void ResolveExpectedPrefix_DefaultKeysAreCaseSensitive()
        {
            // "dev" is not the seeded "Dev" key, so it has no default.
            Assert.Throws<InvalidOperationException>(
                () => TitleCheckRules.ResolveExpectedPrefix("dev", null));
        }

        #endregion


    }
}
