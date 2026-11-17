using System;
using System.Collections.Generic;
using Aerisyn.DataConfigSheet;

namespace Aerisyn.DataConfigSheet.Samples.WeaponsPull
{
    /// <summary>
    /// Local Only + After Pull template: sheet owns turnSpeed; derivatives refill every Pull.
    /// Google tab title defaults to type name <c>MovementConfig</c> (or set [SheetTab]).
    /// </summary>
    public sealed class MovementConfig : ConfigTypeAsset
    {


        /// <summary>Root list Pull fills; name must stay <c>items</c>.</summary>
        public List<MovementRow> items = new List<MovementRow>();


        /// <summary>
        /// After Pull: all included tabs parsed; Baked Assets not written yet.
        /// Fill Local Only derivatives from sheet-owned fields. Throw fails the Pull with no writes.
        /// </summary>
        public override void OnAfterPull()
        {
            for (int i = 0; i < items.Count; i++)
            {
                MovementRow row = items[i];
                // Sheet owns turnSpeed; accel/decel stay off the Source Sheet contract
                row.accelerateTurnSpeed = row.turnSpeed * 2;
                row.decelerateTurnSpeed = row.turnSpeed;
            }
        }


    }


    /// <summary>One movement row; Local Only fields are game-owned derivatives.</summary>
    [Serializable]
    public sealed class MovementRow
    {


        public string id = "";


        public int turnSpeed;


        [LocalOnly]
        public int accelerateTurnSpeed;


        [LocalOnly]
        public int decelerateTurnSpeed;


    }
}
