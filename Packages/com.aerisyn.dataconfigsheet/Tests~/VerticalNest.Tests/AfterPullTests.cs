using Aerisyn.DataConfigSheet;
using NUnit.Framework;

namespace Aerisyn.DataConfigSheet.Tests
{
    /// <summary>
    /// Pre-write After Pull seam (thin helper used by PullFromGrids): fill Local Only before
    /// Baked Asset copy/write; a throw fails the phase so the runner writes nothing.
    /// </summary>
    public sealed class AfterPullTests
    {


        #region Types

        public sealed class DerivativeConfig
        {
            public List<DerivativeRow> items = new List<DerivativeRow>();
        }


        public sealed class DerivativeRow
        {
            public string id = "";
            public int turnSpeed;

            [LocalOnly]
            public int accelerateTurnSpeed;
        }

        #endregion


        #region Success path

        [Test]
        public void TryInvokeAll_FillsLocalOnly_ThenCopyRootItemsCarriesDerivatives()
        {
            // Sheet omits Local Only; After Pull fills accelerateTurnSpeed from turnSpeed
            SheetGrid grid = SheetGrid.FromRows(new[]
            {
                new[] { "id", "turnSpeed" },
                new[] { "bike", "10" },
            });

            var scratch = new DerivativeConfig();
            Assert.That(VerticalNestParser.ParseInto(scratch, grid).Success, Is.True);
            Assert.That(scratch.items[0].accelerateTurnSpeed, Is.EqualTo(0));

            Action[] hooks =
            {
                () =>
                {
                    for (int i = 0; i < scratch.items.Count; i++)
                        scratch.items[i].accelerateTurnSpeed = scratch.items[i].turnSpeed * 2;
                },
            };

            VerticalNestParseError error;
            Assert.That(AfterPull.TryInvokeAll(hooks, out error), Is.True);
            Assert.That(error, Is.Null);
            Assert.That(scratch.items[0].accelerateTurnSpeed, Is.EqualTo(20));

            // Written Baked Asset path without AssetDatabase: in-place items copy
            var existing = new DerivativeConfig();
            existing.items.Add(new DerivativeRow { id = "stale", accelerateTurnSpeed = 999 });
            BakedAssetItemsCopy.CopyRootItems(scratch, existing);

            Assert.That(existing.items, Has.Count.EqualTo(1));
            Assert.That(existing.items[0].id, Is.EqualTo("bike"));
            Assert.That(existing.items[0].turnSpeed, Is.EqualTo(10));
            Assert.That(existing.items[0].accelerateTurnSpeed, Is.EqualTo(20));
        }

        #endregion


        #region Failure path

        [Test]
        public void TryInvokeAll_Throw_FailsPhaseAndSkipsLaterHooks()
        {
            bool secondRan = false;
            Action[] hooks =
            {
                () => throw new InvalidOperationException("derivative boom"),
                () => { secondRan = true; },
            };

            VerticalNestParseError error;
            Assert.That(AfterPull.TryInvokeAll(hooks, out error), Is.False);

            Assert.That(secondRan, Is.False);
            Assert.That(error, Is.Not.Null);
            Assert.That(error.Row, Is.EqualTo(-1));
            Assert.That(error.Column, Is.EqualTo(-1));
            Assert.That(error.Message, Does.Contain("After Pull"));
            Assert.That(error.Message, Does.Contain("derivative boom"));
        }


        [Test]
        public void TryInvokeAll_EmptyHooks_Succeeds()
        {
            VerticalNestParseError error;
            Assert.That(AfterPull.TryInvokeAll(Array.Empty<Action>(), out error), Is.True);
            Assert.That(error, Is.Null);
        }

        #endregion


    }
}
