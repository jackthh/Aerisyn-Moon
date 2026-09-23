using Cathei.BakingSheet;

namespace Aerisyn.DataConfigSheet.Samples.BasicBake
{
    /// <summary>
    /// Demo sheet: one row per item. Tab name on Google Sheet must be "Items".
    /// </summary>
    public sealed class DemoItemsSheet : Sheet<DemoItemsSheet.Row>
    {
        public sealed class Row : SheetRow
        {
            public string Name { get; private set; }
            public int Price { get; private set; }
        }
    }
}
