using System;
using System.Collections.Generic;
using Aerisyn.DataConfigSheet;

namespace Aerisyn.DataConfigSheet.Samples.WeaponsPull
{
    /// <summary>
    /// Template Config Type: weapons → upgrade levels → bonus stats (Vertical Nest).
    /// Google tab title defaults to type name <c>WeaponsConfig</c> (or set [SheetTab]).
    /// </summary>
    public sealed class WeaponsConfig : ConfigTypeAsset
    {


        /// <summary>Root list Pull fills; name must stay <c>items</c>.</summary>
        public List<Weapon> items = new List<Weapon>();


    }


    /// <summary>One weapon row; blank parent cells continue nested upgrades.</summary>
    [Serializable]
    public sealed class Weapon
    {


        public string id = "";


        [ColumnAlias("Weapon Name")]
        public string name = "";


        public List<WeaponUpgrade> upgrades = new List<WeaponUpgrade>();


    }


    /// <summary>Upgrade level under a weapon; bonus_stats is a vertical primitive list.</summary>
    [Serializable]
    public sealed class WeaponUpgrade
    {


        public int upgrade_level;


        public List<int> bonus_stats = new List<int>();


    }
}
