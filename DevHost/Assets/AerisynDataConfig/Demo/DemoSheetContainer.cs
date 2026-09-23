using Cathei.BakingSheet;

namespace Aerisyn.DataConfigSheet.DevHost
{
    /// <summary>
    /// DevHost SheetContainer. Property "Items" matches the Google Sheet tab name.
    /// </summary>
    public sealed class DemoSheetContainer : SheetContainerBase
    {
        public DemoSheetContainer(global::Microsoft.Extensions.Logging.ILogger logger) : base(logger)
        {
        }


        public DemoItemsSheet Items { get; private set; }
    }
}
