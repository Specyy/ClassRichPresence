using System;
using System.Collections.Generic;
using System.Text;

namespace ClassRichPresence.State
{
    public interface IAppState
    {
        /// <summary>
        /// Runs state and returns the next.
        /// </summary>
        /// <returns>The next state. Null to terminate app.</returns>
        IAppState Run();
    }
}
