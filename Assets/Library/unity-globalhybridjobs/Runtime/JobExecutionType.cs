using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HybridJobs
{
    /// <summary>
    /// Type When job will be finished on execution
    /// </summary>
    public enum JobExecutionType
    {
        /// <summary>
        /// Force to Finish within `Update()` Method
        /// </summary>
        InFrame = 0,
        /// <summary>
        /// Finish in `LateUpdate()` Method
        /// </summary>
        LateUpdate = 1,
        /// <summary>
        /// No Limit When to Finish, Callback will be called when jobs finished
        /// </summary>
        Async = 2
    }
}
