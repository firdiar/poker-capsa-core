using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HybridJobs;
using System;

public class CompATest : MonoBehaviour, IHybridComponent
{
    public int JobExecutionIdentifier { get; set; }

    public Type JobSystem => typeof(SystemDATest);

    // Start is called before the first frame update
    void Start()
    {
        GlobalHybridJob.Register(this);
    }

    // Update is called once per frame
    private void OnDestroy()
    {
        GlobalHybridJob.Remove(this);
    }
}
