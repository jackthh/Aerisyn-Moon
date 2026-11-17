#nullable disable
using System;
using System.Security.Cryptography;
using System.Text;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>Content hash used for Baked Asset rows in the Config Manifest.</summary>
    public static class ManifestContentHash
    {


        /// <summary>Lowercase hex SHA-256 of the bytes (64 characters).</summary>
        public static string Sha256Hex(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] digest = sha256.ComputeHash(bytes);
                StringBuilder builder = new StringBuilder(digest.Length * 2);
                for (int i = 0; i < digest.Length; i++)
                    builder.Append(digest[i].ToString("x2"));

                return builder.ToString();
            }
        }


    }
}
