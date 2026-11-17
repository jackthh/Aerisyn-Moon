using Aerisyn.DataConfigSheet;
using NUnit.Framework;

namespace Aerisyn.DataConfigSheet.Tests
{
    /// <summary>
    /// Pull All folder-scope seam (PullConfigFolderScope): which Pull Config asset paths fall under the
    /// Data Config Settings folder path. Recursive, stable order; blank folder path fails clearly.
    /// </summary>
    public sealed class PullConfigFolderScopeTests
    {


        #region Scope

        [Test]
        public void Select_PathsDirectlyInFolder_AreIncluded()
        {
            string[] selected = PullConfigFolderScope.Select(
                "Assets/Config",
                new[] { "Assets/Config/A.asset", "Assets/Other/B.asset" });

            Assert.That(selected, Is.EqualTo(new[] { "Assets/Config/A.asset" }));
        }


        [Test]
        public void Select_NestedSubfolders_AreIncluded()
        {
            string[] selected = PullConfigFolderScope.Select(
                "Assets/Config",
                new[] { "Assets/Config/ai/ship/Ship.asset", "Assets/Config/Root.asset" });

            Assert.That(selected, Is.EquivalentTo(new[] { "Assets/Config/ai/ship/Ship.asset", "Assets/Config/Root.asset" }));
        }


        [Test]
        public void Select_SiblingFolderSharingNamePrefix_IsExcluded()
        {
            string[] selected = PullConfigFolderScope.Select(
                "Assets/Config",
                new[] { "Assets/ConfigOld/X.asset", "Assets/Config/Y.asset" });

            Assert.That(selected, Is.EqualTo(new[] { "Assets/Config/Y.asset" }));
        }


        [Test]
        public void Select_TrailingSlashAndBackslashes_AreNormalized()
        {
            string[] selected = PullConfigFolderScope.Select(
                "Assets\\Config\\",
                new[] { "Assets/Config/Y.asset" });

            Assert.That(selected, Is.EqualTo(new[] { "Assets/Config/Y.asset" }));
        }


        [Test]
        public void Select_FolderPathIsCaseSensitive()
        {
            string[] selected = PullConfigFolderScope.Select(
                "Assets/Config",
                new[] { "Assets/config/Y.asset" });

            Assert.That(selected, Is.Empty);
        }


        [Test]
        public void Select_ReturnsPathsInStableOrdinalOrder_RegardlessOfInputOrder()
        {
            string[] selected = PullConfigFolderScope.Select(
                "Assets/Config",
                new[] { "Assets/Config/b/Z.asset", "Assets/Config/A.asset", "Assets/Config/b/B.asset" });

            Assert.That(selected, Is.EqualTo(new[]
            {
                "Assets/Config/A.asset",
                "Assets/Config/b/B.asset",
                "Assets/Config/b/Z.asset",
            }));
        }


        [Test]
        public void Select_NoPathsUnderFolder_ReturnsEmpty()
        {
            Assert.That(
                PullConfigFolderScope.Select("Assets/Config", new[] { "Assets/Other/B.asset" }),
                Is.Empty);
        }

        #endregion


        #region Blank folder path

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Select_BlankFolderPath_ThrowsNamingPullAllFolder(string folderPath)
        {
            InvalidOperationException error = Assert.Throws<InvalidOperationException>(
                () => PullConfigFolderScope.Select(folderPath, new[] { "Assets/Config/A.asset" }));

            Assert.That(error!.Message, Does.Contain("Pull All folder"));
        }

        #endregion


    }
}
