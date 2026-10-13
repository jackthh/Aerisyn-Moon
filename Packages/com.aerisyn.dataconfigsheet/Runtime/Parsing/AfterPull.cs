#nullable disable
using System;
using System.Collections.Generic;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Pre-write After Pull phase shared by live Google and inject PullFromGrids.
    /// Invokes each Config Type scratch's OnAfterPull after all included tabs parse
    /// and before any Baked Asset create/save.
    /// </summary>
    public static class AfterPull
    {


        /// <summary>
        /// Runs After Pull hooks in order. Returns true when all succeed.
        /// On throw, returns false with an error so the runner writes nothing;
        /// later hooks are not invoked.
        /// </summary>
        public static bool TryInvokeAll(
            IReadOnlyList<Action> hooks,
            out VerticalNestParseError error)
        {
            if (hooks == null)
                throw new ArgumentNullException(nameof(hooks));

            for (int i = 0; i < hooks.Count; i++)
            {
                Action hook = hooks[i];
                if (hook == null)
                    throw new ArgumentException($"After Pull hooks[{i}] is null.", nameof(hooks));

                try
                {
                    hook.Invoke();
                }
                catch (Exception ex)
                {
                    error = new VerticalNestParseError(
                        -1,
                        -1,
                        "After Pull failed: " + ex.Message);
                    return false;
                }
            }

            error = null;
            return true;
        }


    }
}
