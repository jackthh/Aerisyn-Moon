using System.Linq;
using Aerisyn.DataConfigSheet;
using NUnit.Framework;

namespace Aerisyn.DataConfigSheet.Tests
{
    /// <summary>
    /// Setup checklist seam (SetupChecklist): which prerequisites the Data Config Sheet window shows as
    /// done, advisory or blocking, and whether Pull All is ready to run. Pure; no Unity or Google.
    /// </summary>
    public sealed class SetupChecklistTests
    {


        #region Fixtures

        static SetupInputs Ready()
        {
            return new SetupInputs
            {
                EnvironmentKeys = new[] { "Dev", "Test", "Production" },
                ActiveEnvironmentKey = "Dev",
                FeatureKeys = new[] { "Weapons" },
                PullAllFolderPath = "Assets/Config",
                PullAllFolderExists = true,
                PullConfigCount = 2,
                UsesOAuth = true,
                CredentialPath = "client.json",
                CredentialExists = true,
                SignedIn = true,
                ManifestAssetPath = "Assets/Config/ConfigManifest.asset",
            };
        }


        static SetupItem Item(SetupInputs inputs, string id)
        {
            return SetupChecklist.Evaluate(inputs).Single(item => item.Id == id);
        }

        #endregion


        #region Readiness

        [Test]
        public void FullySetUp_IsReadyForPullAll()
        {
            Assert.That(SetupChecklist.IsReadyForPullAll(SetupChecklist.Evaluate(Ready())), Is.True);
        }


        [Test]
        public void Evaluate_ReturnsItemsInStableOrder()
        {
            string[] ids = SetupChecklist.Evaluate(Ready()).Select(item => item.Id).ToArray();

            Assert.That(ids, Is.EqualTo(new[]
            {
                SetupChecklist.EnvironmentId,
                SetupChecklist.FeatureKeysId,
                SetupChecklist.PullAllFolderId,
                SetupChecklist.AuthId,
                SetupChecklist.ManifestId,
            }));
        }

        #endregion


        #region Environment

        [Test]
        public void Environment_ActiveKeyNotPicked_Blocks()
        {
            SetupInputs inputs = Ready();
            inputs.ActiveEnvironmentKey = "";

            Assert.That(Item(inputs, SetupChecklist.EnvironmentId).Status, Is.EqualTo(SetupItemStatus.Blocking));
            Assert.That(SetupChecklist.IsReadyForPullAll(SetupChecklist.Evaluate(inputs)), Is.False);
        }


        [Test]
        public void Environment_ActiveKeyRemovedFromList_BlocksAndNamesTheKey()
        {
            SetupInputs inputs = Ready();
            inputs.ActiveEnvironmentKey = "Staging";

            SetupItem item = Item(inputs, SetupChecklist.EnvironmentId);

            Assert.That(item.Status, Is.EqualTo(SetupItemStatus.Blocking));
            Assert.That(item.Detail, Does.Contain("Staging"));
        }


        [Test]
        public void Environment_ValidKey_IsOkAndNamesTheKey()
        {
            SetupItem item = Item(Ready(), SetupChecklist.EnvironmentId);

            Assert.That(item.Status, Is.EqualTo(SetupItemStatus.Ok));
            Assert.That(item.Detail, Does.Contain("Dev"));
        }

        #endregion


        #region Feature Keys

        [Test]
        public void FeatureKeys_None_WarnsButDoesNotBlock()
        {
            SetupInputs inputs = Ready();
            inputs.FeatureKeys = new string[0];

            Assert.That(Item(inputs, SetupChecklist.FeatureKeysId).Status, Is.EqualTo(SetupItemStatus.Warning));
            Assert.That(SetupChecklist.IsReadyForPullAll(SetupChecklist.Evaluate(inputs)), Is.True);
        }

        #endregion


        #region Pull All folder

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void PullAllFolder_Blank_Blocks(string folder)
        {
            SetupInputs inputs = Ready();
            inputs.PullAllFolderPath = folder;

            Assert.That(Item(inputs, SetupChecklist.PullAllFolderId).Status, Is.EqualTo(SetupItemStatus.Blocking));
        }


        [Test]
        public void PullAllFolder_MissingOnDisk_Blocks()
        {
            SetupInputs inputs = Ready();
            inputs.PullAllFolderExists = false;

            Assert.That(Item(inputs, SetupChecklist.PullAllFolderId).Status, Is.EqualTo(SetupItemStatus.Blocking));
        }


        [Test]
        public void PullAllFolder_NoPullConfigs_Blocks()
        {
            SetupInputs inputs = Ready();
            inputs.PullConfigCount = 0;

            Assert.That(Item(inputs, SetupChecklist.PullAllFolderId).Status, Is.EqualTo(SetupItemStatus.Blocking));
        }


        [Test]
        public void PullAllFolder_WithPullConfigs_IsOkAndShowsCount()
        {
            SetupItem item = Item(Ready(), SetupChecklist.PullAllFolderId);

            Assert.That(item.Status, Is.EqualTo(SetupItemStatus.Ok));
            Assert.That(item.Detail, Does.Contain("2"));
        }

        #endregion


        #region Auth

        [Test]
        public void Auth_OAuthNotSignedIn_Blocks()
        {
            SetupInputs inputs = Ready();
            inputs.SignedIn = false;

            Assert.That(Item(inputs, SetupChecklist.AuthId).Status, Is.EqualTo(SetupItemStatus.Blocking));
        }


        [Test]
        public void Auth_OAuthClientSecretsBlank_Blocks()
        {
            SetupInputs inputs = Ready();
            inputs.CredentialPath = "";
            inputs.CredentialExists = false;

            Assert.That(Item(inputs, SetupChecklist.AuthId).Status, Is.EqualTo(SetupItemStatus.Blocking));
        }


        [Test]
        public void Auth_OAuthClientSecretsMissingOnDisk_Blocks()
        {
            SetupInputs inputs = Ready();
            inputs.CredentialExists = false;

            Assert.That(Item(inputs, SetupChecklist.AuthId).Status, Is.EqualTo(SetupItemStatus.Blocking));
        }


        [Test]
        public void Auth_ServiceAccountWithFile_IsOkWithoutSignIn()
        {
            SetupInputs inputs = Ready();
            inputs.UsesOAuth = false;
            inputs.SignedIn = false;

            Assert.That(Item(inputs, SetupChecklist.AuthId).Status, Is.EqualTo(SetupItemStatus.Ok));
        }


        [Test]
        public void Auth_ServiceAccountFileMissing_Blocks()
        {
            SetupInputs inputs = Ready();
            inputs.UsesOAuth = false;
            inputs.CredentialExists = false;

            Assert.That(Item(inputs, SetupChecklist.AuthId).Status, Is.EqualTo(SetupItemStatus.Blocking));
        }

        #endregion


        #region Manifest

        [Test]
        public void Manifest_Blank_IsOkAndSaysSkipped()
        {
            SetupInputs inputs = Ready();
            inputs.ManifestAssetPath = "";

            SetupItem item = Item(inputs, SetupChecklist.ManifestId);

            Assert.That(item.Status, Is.EqualTo(SetupItemStatus.Ok));
            Assert.That(item.Detail, Does.Contain("skipped").IgnoreCase);
        }

        #endregion


        #region Blocking count

        [Test]
        public void CountBlocking_CountsOnlyBlockingItems()
        {
            SetupInputs inputs = Ready();
            inputs.ActiveEnvironmentKey = "";
            inputs.SignedIn = false;
            inputs.FeatureKeys = new string[0];

            Assert.That(SetupChecklist.CountBlocking(SetupChecklist.Evaluate(inputs)), Is.EqualTo(2));
        }

        #endregion


    }
}
