using Aerisyn.DataConfigSheet;
using NUnit.Framework;

namespace Aerisyn.DataConfigSheet.Tests
{
    /// <summary>
    /// Pull runner inclusion / ValidateTargets seam (PullTargetRules), shared by live fetch and
    /// inject PullFromGrids. All-unticked Validate throw is the no-write gate before bake.
    /// </summary>
    public sealed class PullTargetRulesTests
    {


        #region Types

        public sealed class AlphaConfig
        {
        }


        public sealed class BetaConfig
        {
        }

        #endregion


        #region ResolveIncluded

        [Test]
        public void ResolveIncluded_MixedFlags_ReturnsOnlyIncludedInListOrder()
        {
            PullTypeCandidate[] candidates =
            {
                new PullTypeCandidate(typeof(AlphaConfig), includeInPull: true),
                new PullTypeCandidate(typeof(BetaConfig), includeInPull: false),
                new PullTypeCandidate(typeof(AlphaConfig), includeInPull: true),
            };

            Type[] included = PullTargetRules.ResolveIncluded(candidates);

            Assert.That(included, Is.EqualTo(new[] { typeof(AlphaConfig), typeof(AlphaConfig) }));
        }

        #endregion


        #region Validate

        [Test]
        public void Validate_AllUnticked_ThrowsClearError()
        {
            PullTypeCandidate[] candidates =
            {
                new PullTypeCandidate(typeof(AlphaConfig), includeInPull: false),
                new PullTypeCandidate(typeof(BetaConfig), includeInPull: false),
            };

            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
                PullTargetRules.Validate("DemoPull", "Assets/Baked", candidates));

            Assert.That(ex.Message, Does.Contain("DemoPull"));
            Assert.That(ex.Message, Does.Contain("Include In Pull"));
        }


        [Test]
        public void Validate_EmptyOutputFolder_ThrowsClearError()
        {
            PullTypeCandidate[] candidates =
            {
                new PullTypeCandidate(typeof(AlphaConfig), includeInPull: true),
            };

            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
                PullTargetRules.Validate("DemoPull", "  ", candidates));

            Assert.That(ex.Message, Does.Contain("DemoPull"));
            Assert.That(ex.Message, Does.Contain("Output Folder"));
        }

        #endregion


    }
}
