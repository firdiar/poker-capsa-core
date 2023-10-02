using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HybridJobs;
using System;

public class HeavyParallelJobComponentTest : MonoBehaviour, IHybridComponent
{
    public int value;

    public int JobExecutionIdentifier { get; set; }

    public Type JobSystem => typeof(HeavyParallelJobSystemTest);

    private void Start()
    {
        GlobalHybridJob.Register(this);
    }

    private void OnDestroy()
    {
        GlobalHybridJob.Remove(this);
    }
}
