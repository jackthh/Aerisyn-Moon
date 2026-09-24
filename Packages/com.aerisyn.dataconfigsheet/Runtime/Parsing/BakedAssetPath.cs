#nullable disable
using System;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Baked Asset file naming: one `{ConfigTypeName}.asset` under the Pull Config output folder.
    /// </summary>
    public static class BakedAssetPath
    {


        /// <summary>
        /// Project-relative asset path for a Config Type under the shared output folder.
        /// </summary>
        public static string ForConfigType(string outputFolder, Type configType)
        {
            if (configType == null)
                throw new ArgumentNullException(nameof(configType));
            if (string.IsNullOrWhiteSpace(outputFolder))
                throw new ArgumentException("Output folder is empty.", nameof(outputFolder));

            string folder = outputFolder.Replace('\\', '/').Trim().TrimEnd('/');
            return folder + "/" + configType.Name + ".asset";
        }


    }
}
