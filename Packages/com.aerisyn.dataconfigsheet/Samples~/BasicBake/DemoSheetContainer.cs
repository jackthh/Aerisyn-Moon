using Cathei.BakingSheet;

namespace Aerisyn.DataConfigSheet.Samples.BasicBake
{
    /// <summary>
    /// Demo SheetContainer. Property name "Items" must match the Google Sheet tab name.
    /// </summary>
    public sealed class DemoSheetContainer : SheetContainerBase
    {
        public DemoSheetContainer(global::Microsoft.Extensions.Logging.ILogger logger) : base(logger)
        {
        }


        public DemoItemsSheet Items { get; private set; }
    }
}
