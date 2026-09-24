using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Aerisyn.DataConfigSheet.Editor
{
    /// <summary>
    /// Runs one-way bake: Google tabs → CSV under Luban dataDir → Luban CLI → generated code/JSON.
    /// </summary>
    public static class DataConfigBakeRunner
    {
        #region Public API

        /// <summary>
        /// Validates config, exports CSV, runs Luban, then refreshes the AssetDatabase.
        /// </summary>
        public static async Task BakeAsync(BakeConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            Validate(config);

            string lubanProjectDir = DataConfigPathUtility.ResolveProjectPath(config.LubanProjectPath);
            string lubanConf = Path.Combine(lubanProjectDir, "luban.conf");
            if (!File.Exists(lubanConf))
                throw new FileNotFoundException(
                    $"luban.conf not found at '{lubanConf}'. Point BakeConfig Luban Project Path at a Luban project folder.",
                    lubanConf);

            string dataDir = ResolveLubanDataDir(lubanProjectDir, lubanConf);
            string lubanDll = DataConfigPathUtility.ResolveProjectPath(config.LubanDllPath);
            if (!File.Exists(lubanDll))
                throw new FileNotFoundException(
                    $"Luban.dll not found at '{lubanDll}'. Download Luban from focus-creative-games/luban releases " +
                    "into Tools/Luban (see package README).",
                    lubanDll);

            string outputCodeDir = DataConfigPathUtility.ResolveProjectPath(config.OutputCodeDir);
            string outputDataDir = DataConfigPathUtility.ResolveProjectPath(config.OutputDataDir);
            Directory.CreateDirectory(outputCodeDir);
            Directory.CreateDirectory(outputDataDir);

            EditorUtility.DisplayProgressBar("Data Config Sheet", "Exporting Google Sheets to CSV…", 0.25f);
            await GoogleSheetsCsvExporter.ExportAsync(config, dataDir);

            EditorUtility.DisplayProgressBar("Data Config Sheet", "Running Luban…", 0.65f);
            await RunLubanAsync(
                lubanDll,
                lubanConf,
                config.LubanTarget,
                outputCodeDir,
                outputDataDir);

            AssetDatabase.Refresh();
            Debug.Log(
                $"[DataConfigSheet] Bake complete. Code → '{config.OutputCodeDir}', Data → '{config.OutputDataDir}'",
                config);
        }

        #endregion


        #region Validation

        static void Validate(BakeConfig config)
        {
            if (string.IsNullOrWhiteSpace(config.SpreadsheetId))
                throw new InvalidOperationException($"BakeConfig '{config.name}' has no Spreadsheet Id.");

            if (config.SheetExports == null || config.SheetExports.Length == 0)
                throw new InvalidOperationException($"BakeConfig '{config.name}' has no Tab export entries.");

            if (string.IsNullOrWhiteSpace(config.LubanProjectPath))
                throw new InvalidOperationException($"BakeConfig '{config.name}' has an empty Luban Project Path.");

            if (string.IsNullOrWhiteSpace(config.LubanDllPath))
                throw new InvalidOperationException($"BakeConfig '{config.name}' has an empty Luban Dll Path.");

            if (string.IsNullOrWhiteSpace(config.OutputCodeDir) || string.IsNullOrWhiteSpace(config.OutputDataDir))
                throw new InvalidOperationException($"BakeConfig '{config.name}' needs Output Code Dir and Output Data Dir.");

            if (config.AuthMode == GoogleAuthMode.OAuthUser)
            {
                if (string.IsNullOrWhiteSpace(config.OAuthUserTokenPath))
                    throw new InvalidOperationException(
                        $"BakeConfig '{config.name}' has an empty OAuth user token path.");
            }
            else if (string.IsNullOrWhiteSpace(config.ServiceAccountCredentialPath))
            {
                throw new InvalidOperationException(
                    $"BakeConfig '{config.name}' has an empty service-account credential path.");
            }
        }

        #endregion


        #region Luban process

        /// <summary>
        /// Reads dataDir from luban.conf (JSON) so CSV lands where Luban expects input files.
        /// </summary>
        static string ResolveLubanDataDir(string lubanProjectDir, string lubanConfPath)
        {
            // Minimal parse: "dataDir": "Data" relative to luban project
            string json = File.ReadAllText(lubanConfPath);
            const string key = "\"dataDir\"";
            int keyIndex = json.IndexOf(key, StringComparison.Ordinal);
            if (keyIndex < 0)
                return Path.Combine(lubanProjectDir, "Data");

            int colon = json.IndexOf(':', keyIndex + key.Length);
            int quote1 = json.IndexOf('"', colon + 1);
            int quote2 = json.IndexOf('"', quote1 + 1);
            if (colon < 0 || quote1 < 0 || quote2 < 0)
                return Path.Combine(lubanProjectDir, "Data");

            string relative = json.Substring(quote1 + 1, quote2 - quote1 - 1).Trim();
            if (string.IsNullOrEmpty(relative))
                relative = "Data";

            return Path.GetFullPath(Path.Combine(lubanProjectDir, relative));
        }


        /// <summary>
        /// Invokes: dotnet Luban.dll --conf … -t client -c cs-simple-json -d json -x output*Dir=…
        /// </summary>
        static Task RunLubanAsync(
            string lubanDll,
            string lubanConf,
            string target,
            string outputCodeDir,
            string outputDataDir)
        {
            var tcs = new TaskCompletionSource<bool>();

            string args =
                $"\"{lubanDll}\" --conf \"{lubanConf}\" -t {target} -c cs-simple-json -d json " +
                $"--strict " +
                $"-x outputCodeDir=\"{outputCodeDir}\" -x outputDataDir=\"{outputDataDir}\"";

            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = args,
                WorkingDirectory = Path.GetDirectoryName(lubanDll) ?? Environment.CurrentDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };

            // Luban releases target net8; allow net9+ hosts without installing the 8.0 runtime
            startInfo.Environment["DOTNET_ROLL_FORWARD"] = "Major";

            var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
            var log = new StringBuilder();

            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null)
                    log.AppendLine(e.Data);
            };
            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null)
                    log.AppendLine(e.Data);
            };
            process.Exited += (_, __) =>
            {
                string text = log.ToString();
                if (process.ExitCode != 0)
                {
                    tcs.TrySetException(new InvalidOperationException(
                        $"Luban failed (exit {process.ExitCode}).\n{text}"));
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(text))
                        Debug.Log($"[DataConfigSheet] Luban:\n{text}");
                    tcs.TrySetResult(true);
                }

                process.Dispose();
            };

            if (!process.Start())
            {
                tcs.TrySetException(new InvalidOperationException(
                    "Failed to start 'dotnet'. Install .NET SDK 8+ and ensure it is on PATH."));
                return tcs.Task;
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            return tcs.Task;
        }

        #endregion
    }
}
