using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HybridJobs;
using Unity.Jobs;

public class SystemDBTest : HybridSystem<CompBTest>
{
    public override JobExecutionType ExecutionType => JobExecutionType.Async;

    public override void OnCompleted()
    {
        Debug.Log("B Complete!");
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
        Debug.Log("B Start Scheduled!");
        SuperFastJob job = new SuperFastJob();
        SystemDATest dependencySystem = GetSystem<SystemDATest>();

        if (dependencySystem != null)
        {
            return job.Schedule(dependencySystem.ActiveHandle);
        }
        else
        {
            return job.Schedule();
        }
    }

    struct SuperFastJob : IJob
    {
        public void Execute()
        {
            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < 100; j++)
                {

                }
            }
        }
    }
}
