using Aerisyn.DataConfigSheet;
using NUnit.Framework;

namespace Aerisyn.DataConfigSheet.Tests
{
    /// <summary>Pure helpers: tab title resolution and Header Emitter paste output.</summary>
    public sealed class HeaderEmitterAndTabTests
    {


        #region Types

        public sealed class DefaultTabConfig
        {
            public List<FlatEmitRow> items = new List<FlatEmitRow>();
        }


        [SheetTab("Weapons Table")]
        public sealed class OverrideTabConfig
        {
            public List<FlatEmitRow> items = new List<FlatEmitRow>();
        }


        public sealed class FlatEmitRow
        {
            [ColumnAlias("Weapons")]
            public string id = "";

            public int power;
        }


        public sealed class NestedEmitConfig
        {
            public List<NestedEmitWeapon> items = new List<NestedEmitWeapon>();
        }


        public sealed class NestedEmitWeapon
        {
            public string id = "";
            public List<NestedEmitUpgrade> upgrades = new List<NestedEmitUpgrade>();
        }


        public sealed class NestedEmitUpgrade
        {
            public int upgrade_level;
            public List<int> bonus_stats = new List<int>();
        }


        public sealed class LocalOnlyEmitConfig
        {
            public List<LocalOnlyEmitRow> items = new List<LocalOnlyEmitRow>();
        }


        public sealed class LocalOnlyEmitRow
        {
            public string id = "";
            public int turnSpeed;

            [LocalOnly]
            public int accelerateTurnSpeed;

            [LocalOnly]
            [ColumnAlias("Decel Turn")]
            public int decelerateTurnSpeed;
        }


        public sealed class NestLocalOnlyEmitConfig
        {
            public List<NestLocalOnlyEmitWeapon> items = new List<NestLocalOnlyEmitWeapon>();
        }


        public sealed class NestLocalOnlyEmitWeapon
        {
            public string id = "";
            public List<NestLocalOnlyEmitUpgrade> upgrades = new List<NestLocalOnlyEmitUpgrade>();
        }


        public sealed class NestLocalOnlyEmitUpgrade
        {
            public int upgrade_level;

            [LocalOnly]
            public int derivedBonus;

            public List<int> bonus_stats = new List<int>();
        }


        public sealed class LocalOnlyArrayEmitConfig
        {
            public List<LocalOnlyArrayEmitRow> items = new List<LocalOnlyArrayEmitRow>();
        }


        public sealed class LocalOnlyArrayEmitRow
        {
            public string id = "";

            [LocalOnly]
            public List<int> localBonuses = new List<int>();

            public int power;
        }

        #endregion


        #region Tab title

        [Test]
        public void ResolveTabTitle_DefaultsToTypeName()
        {
            Assert.That(ConfigTypeTabName.Resolve(typeof(DefaultTabConfig)), Is.EqualTo("DefaultTabConfig"));
        }


        [Test]
        public void ResolveTabTitle_UsesSheetTabAttributeWhenPresent()
        {
            Assert.That(ConfigTypeTabName.Resolve(typeof(OverrideTabConfig)), Is.EqualTo("Weapons Table"));
        }

        #endregion


        #region Header Emitter

        [Test]
        public void Emit_UsesAliasWhenPresentOtherwiseFieldName()
        {
            HeaderEmitResult result = HeaderEmitter.Emit(typeof(DefaultTabConfig));

            Assert.That(result.HeaderRow, Is.EqualTo(new[] { "Weapons", "power" }));
            Assert.That(result.IgnoreMarkerGuidance, Does.Contain("!!!"));
        }


        [Test]
        public void Emit_NestedShape_OrdersParentThenChildHeaders()
        {
            HeaderEmitResult result = HeaderEmitter.Emit(typeof(NestedEmitConfig));

            Assert.That(result.HeaderRow, Is.EqualTo(new[] { "id", "upgrade_level", "bonus_stats" }));
        }


        [Test]
        public void Emit_OmitsLocalOnlyFieldNamesAndAliases()
        {
            // Clipboard must match the sheet contract: Local Only names and aliases stay out
            HeaderEmitResult result = HeaderEmitter.Emit(typeof(LocalOnlyEmitConfig));

            Assert.That(result.HeaderRow, Is.EqualTo(new[] { "id", "turnSpeed" }));
            Assert.That(result.HeaderRow, Does.Not.Contain("accelerateTurnSpeed"));
            Assert.That(result.HeaderRow, Does.Not.Contain("decelerateTurnSpeed"));
            Assert.That(result.HeaderRow, Does.Not.Contain("Decel Turn"));
        }


        [Test]
        public void Emit_GuidanceMentionsOmittedLocalOnlyCount()
        {
            HeaderEmitResult result = HeaderEmitter.Emit(typeof(LocalOnlyEmitConfig));

            // Two Local Only fields on the row; guidance must say how many were left out
            Assert.That(result.LocalOnlyOmittedCount, Is.EqualTo(2));
            Assert.That(result.IgnoreMarkerGuidance, Does.Contain("2"));
            Assert.That(result.IgnoreMarkerGuidance, Does.Contain("Local Only").IgnoreCase);
            Assert.That(result.IgnoreMarkerGuidance, Does.Contain("!!!"));
        }


        [Test]
        public void Emit_NestLevelLocalOnly_OmitsAndCounts()
        {
            HeaderEmitResult result = HeaderEmitter.Emit(typeof(NestLocalOnlyEmitConfig));

            Assert.That(result.HeaderRow, Is.EqualTo(new[] { "id", "upgrade_level", "bonus_stats" }));
            Assert.That(result.LocalOnlyOmittedCount, Is.EqualTo(1));
            Assert.That(result.IgnoreMarkerGuidance, Does.Contain("1"));
            Assert.That(result.IgnoreMarkerGuidance, Does.Contain("Local Only").IgnoreCase);
        }


        [Test]
        public void Emit_NoLocalOnly_GuidanceHasZeroOmitCount()
        {
            HeaderEmitResult result = HeaderEmitter.Emit(typeof(DefaultTabConfig));

            Assert.That(result.LocalOnlyOmittedCount, Is.EqualTo(0));
            Assert.That(result.IgnoreMarkerGuidance, Does.Contain("!!!"));
            Assert.That(result.IgnoreMarkerGuidance, Does.Not.Contain("Local Only").IgnoreCase);
        }


        [Test]
        public void Emit_OmitsLocalOnlyPrimitiveArray()
        {
            HeaderEmitResult result = HeaderEmitter.Emit(typeof(LocalOnlyArrayEmitConfig));

            Assert.That(result.HeaderRow, Is.EqualTo(new[] { "id", "power" }));
            Assert.That(result.HeaderRow, Does.Not.Contain("localBonuses"));
            Assert.That(result.LocalOnlyOmittedCount, Is.EqualTo(1));
            Assert.That(result.IgnoreMarkerGuidance, Does.Contain("1"));
            Assert.That(result.IgnoreMarkerGuidance, Does.Contain("Local Only").IgnoreCase);
        }

        #endregion


    }
}
