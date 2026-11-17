#nullable disable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>One named Pull Config run for Pull All (the Editor wraps PullAsync in this).</summary>
    public sealed class PullAllJob
    {


        public PullAllJob(string name, Func<Task<PullJobResult>> run)
        {
            Name = name ?? "";
            Run = run ?? throw new ArgumentNullException(nameof(run));
        }


        /// <summary>Pull Config name; used when the job throws before producing a result.</summary>
        public string Name { get; }


        /// <summary>Starts the Pull for this config.</summary>
        public Func<Task<PullJobResult>> Run { get; }


    }


    /// <summary>
    /// Pull All orchestration: runs every job in the given order and never stops at the first
    /// failure, so the combined summary shows the full set of passed and failed Pull Configs.
    /// </summary>
    public static class PullAllRunner
    {


        /// <summary>
        /// Runs jobs sequentially (one OAuth/Sheets session at a time, stable log order). A job that
        /// throws, synchronously or asynchronously, becomes a failed result carrying the exception
        /// message; the remaining jobs still run.
        /// </summary>
        public static async Task<IReadOnlyList<PullJobResult>> RunAllAsync(IReadOnlyList<PullAllJob> jobs)
        {
            if (jobs == null)
                throw new ArgumentNullException(nameof(jobs));

            List<PullJobResult> results = new List<PullJobResult>(jobs.Count);
            for (int i = 0; i < jobs.Count; i++)
            {
                PullAllJob job = jobs[i];
                try
                {
                    results.Add(await job.Run());
                }
                catch (Exception exception)
                {
                    results.Add(PullJobResult.Failed(job.Name, exception.Message));
                }
            }

            return results;
        }


    }
}
