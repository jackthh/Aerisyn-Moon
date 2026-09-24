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

        #endregion


    }
}
