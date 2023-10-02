using HybridJobs;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

public class SystemDATest : HybridSystem<CompATest>
{
    public override JobExecutionType ExecutionType => JobExecutionType.Async;

    //Make sure this system executed before SystemDBTest
    public override int ExecutionOrder => -1;

    public override void OnCompleted()
    {
        Debug.Log("A Complete!");
        foreach (var item in HybridObjects)
        {
            item.transform.position = Random.insideUnitCircle;
        }
    }

    public override void OnDestroyed()
    {
    }

    public override JobHandle OnUpdate()
    {
        Debug.Log("A Start Scheduled!");
        SuperLongJob job = new SuperLongJob();
        return job.Schedule();
    }

    struct SuperLongJob : IJob
    {
        public void Execute()
        {
            for (int i = 0; i < 10000; i++)
            {
                for (int j = 0; j < 100000; j++)
                { 
                    
                }
            }
        }
    }
}
