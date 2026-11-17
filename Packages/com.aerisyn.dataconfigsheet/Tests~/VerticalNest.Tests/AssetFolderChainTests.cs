using System;
using System.Collections.Generic;
using Aerisyn.DataConfigSheet;
using NUnit.Framework;

namespace Aerisyn.DataConfigSheet.Tests
{
    /// <summary>
    /// First-create folder rule (AssetFolderChain): which (parent, name) folders must be created so an
    /// asset path such as Assets/AerisynDataConfig/DataConfigSettings.asset can be saved.
    /// </summary>
    public sealed class AssetFolderChainTests
    {


        [Test]
        public void MissingFolders_AllPresent_ReturnsNothing()
        {
            var missing = AssetFolderChain.MissingFolders(
                "Assets/AerisynDataConfig/DataConfigSettings.asset",
                folder => true);

            Assert.That(missing, Is.Empty);
        }


        [Test]
        public void MissingFolders_OnlyLeafMissing_ReturnsLeafWithParent()
        {
            var existing = new HashSet<string> { "Assets" };

            var missing = AssetFolderChain.MissingFolders(
                "Assets/AerisynDataConfig/DataConfigSettings.asset",
                existing.Contains);

            Assert.That(missing, Is.EqualTo(new[] { ("Assets", "AerisynDataConfig") }));
        }


        [Test]
        public void MissingFolders_NestedMissing_ReturnsParentsBeforeChildren()
        {
            var existing = new HashSet<string> { "Assets" };

            var missing = AssetFolderChain.MissingFolders(
                "Assets/A/B/Settings.asset",
                existing.Contains);

            Assert.That(missing, Is.EqualTo(new[] { ("Assets", "A"), ("Assets/A", "B") }));
        }


        [Test]
        public void MissingFolders_BackslashesAreNormalized()
        {
            var existing = new HashSet<string> { "Assets" };

            var missing = AssetFolderChain.MissingFolders("Assets\\A\\Settings.asset", existing.Contains);

            Assert.That(missing, Is.EqualTo(new[] { ("Assets", "A") }));
        }


        [Test]
        public void MissingFolders_PathOutsideAssets_Throws()
        {
            Assert.Throws<InvalidOperationException>(
                () => AssetFolderChain.MissingFolders("Library/X/S.asset", folder => false));
        }


    }
}
