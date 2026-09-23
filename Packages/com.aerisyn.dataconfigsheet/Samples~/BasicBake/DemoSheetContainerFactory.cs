using Aerisyn.DataConfigSheet;
using Cathei.BakingSheet;
using UnityEngine;

namespace Aerisyn.DataConfigSheet.Samples.BasicBake
{
    /// <summary>
    /// Sample factory for BakeConfig. Create via Assets → Create → Aerisyn → Data Config Sheet → Demo Sheet Container Factory.
    /// </summary>
    [CreateAssetMenu(
        fileName = "DemoSheetContainerFactory",
        menuName = "Aerisyn/Data Config Sheet/Demo Sheet Container Factory",
        order = 10)]
    public sealed class DemoSheetContainerFactory : SheetContainerFactory
    {
        public override SheetContainerBase Create(global::Microsoft.Extensions.Logging.ILogger logger)
        {
            return new DemoSheetContainer(logger);
        }
    }
}
