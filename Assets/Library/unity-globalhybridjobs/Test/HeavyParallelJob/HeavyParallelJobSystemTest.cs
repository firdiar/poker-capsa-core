using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HybridJobs;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using System.Linq;

public class HeavyParallelJobSystemTest : HybridSystem<HeavyParallelJobComponentTest>
{
    const int executionForLoopExample = 64;
  
    //To improve Framerate While Executing heavy task, the Execution Type is Set to Async
    public override JobExecutionType ExecutionType => JobExecutionType.Async;


    int[] tempList = new int[500000];
    NativeArray<int> result;
    
    protected override void OnRegistered(HeavyParallelJobComponentTest component)
    {
        base.OnRegistered(component);
        var objList = HybridObjects.ToArray();
        for (int i = 0; i < tempList.Length; i++)
        {
            int index = i % objList.Length;
            tempList[i] = (objList[index].value);
        }
    }

    public override void OnCompleted()
    {
        //This is the way you Recive a Result
        Debug.Log(result[0]);
        result.Dispose();
    }

    public override JobHandle OnUpdate()
    {
        //Standard Allocation of Native Array
        result = new NativeArray<int>(executionForLoopExample, Allocator.TempJob);
        NativeArray<int> allValue = new NativeArray<int>(tempList, Allocator.TempJob);

        SumAllValueJob job = new SumAllValueJob()
        {
            result = result,
            AllValues = allValue
        };
        return job.Schedule(executionForLoopExample, 8);
    }

    public override void OnDestroyed()
    {
        //don't forget to destroy your allocation  on System when it's being destroyed
        Debug.Log("Clear Allocation of System");
        if (result != null && result.IsCreated)
        {
            result.Dispose();
        }
    }

    [BurstCompile]
    struct SumAllValueJob : IJobParallelFor
    {
        [ReadOnly]
        [DeallocateOnJobCompletion]
        public NativeArray<int> AllValues;
        [WriteOnly]
        public NativeArray<int> result;

        public void Execute(int index)
        {
            //We do Sum , as much 500.000 times multiplied by 64
            int tempInt = 0;
            foreach (var item in AllValues)
            {
                tempInt += item;
            }
            result[index] = tempInt;
        }
    }
}
