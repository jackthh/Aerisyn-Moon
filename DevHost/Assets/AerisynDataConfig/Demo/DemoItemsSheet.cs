using Cathei.BakingSheet;

namespace Aerisyn.DataConfigSheet.DevHost
{
    /// <summary>
    /// DevHost demo sheet. Google Sheet tab name must be "Items".
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
