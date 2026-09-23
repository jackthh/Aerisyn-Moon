using Aerisyn.DataConfigSheet;
using Cathei.BakingSheet;
using UnityEngine;

namespace Aerisyn.DataConfigSheet.DevHost
{
    /// <summary>
    /// DevHost factory for BakeConfig. Create via Assets → Create → Aerisyn → Data Config Sheet → DevHost Demo Factory.
    /// </summary>
    [CreateAssetMenu(
        fileName = "DevHostDemoSheetContainerFactory",
        menuName = "Aerisyn/Data Config Sheet/DevHost Demo Factory",
        order = 11)]
    public sealed class DevHostDemoSheetContainerFactory : SheetContainerFactory
    {
        public override SheetContainerBase Create(global::Microsoft.Extensions.Logging.ILogger logger)
        {
            return new DemoSheetContainer(logger);
        }
    }
}
