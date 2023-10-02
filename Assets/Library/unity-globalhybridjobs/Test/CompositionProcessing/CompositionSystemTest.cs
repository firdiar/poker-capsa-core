using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HybridJobs;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

public class CompositionSystemTest : HybridSystem<RandomPositionText.ComponentPart>
{
    public override JobExecutionType ExecutionType => JobExecutionType.LateUpdate;

    public Vector2 boundMin = new Vector2(0 , -3.5f);
    public Vector2 boundMax = new Vector2(4, 3.5f);

    int latestIndex = 0;
    NativeArray<RandomPositionText.MovementData> movementData;
    NativeArray<Unity.Mathematics.Random> randomNumberGenerators;


    public override void OnCreated()
    {
        base.OnCreated();
        movementData = new NativeArray<RandomPositionText.MovementData>(64 , Allocator.Persistent);
        randomNumberGenerators = new NativeArray<Unity.Mathematics.Random>(Unity.Jobs.LowLevel.Unsafe.JobsUtility.MaxJobThreadCount, Allocator.Persistent);

        for (int i = 0; i < randomNumberGenerators.Length; i++)
        {
            randomNumberGenerators[i] = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, int.MaxValue));
        }
    }

    protected override void OnRegistered(RandomPositionText.ComponentPart component)
    {
        base.OnRegistered(component);
        component.JobExecutionIdentifier = latestIndex;        
        movementData[component.JobExecutionIdentifier] = component.GetMovementData();
        latestIndex++;
    }

    protected override void OnRemoved(RandomPositionText.ComponentPart component)
    {
        base.OnRemoved(component);
        int destroyedIndex = component.JobExecutionIdentifier;
        movementData[destroyedIndex] = movementData[latestIndex - 1];
        foreach (var item in HybridObjects)
        {
            if (item.JobExecutionIdentifier == latestIndex - 1)
            {
                item.JobExecutionIdentifier = destroyedIndex;
                break;
            }    
        }
        latestIndex--;
    }

    public override void OnCompleted()
    {
        foreach (var item in HybridObjects)
        {
            item.TextTransform.position = movementData[item.JobExecutionIdentifier].position;
        }
    }

    public override void OnDestroyed()
    {
        movementData.Dispose();
        randomNumberGenerators.Dispose();
    }

    public override JobHandle OnUpdate()
    {
        RandomPositionJobs randomPositionJobs = new RandomPositionJobs()
        {
            boundMin = boundMin,
            boundMax = boundMax,
            randomNumberGenerators = randomNumberGenerators,
            deltaTime = Time.deltaTime,
            movement = movementData
        };

        return randomPositionJobs.Schedule(latestIndex, 8);
    }


    [BurstCompile]
    struct RandomPositionJobs : IJobParallelFor
    {
        [ReadOnly]
        public Vector2 boundMin;
        [ReadOnly]
        public Vector2 boundMax;

        [Unity.Collections.LowLevel.Unsafe.NativeSetThreadIndex] 
        int threadId;
        [Unity.Collections.LowLevel.Unsafe.NativeDisableContainerSafetyRestriction] 
        public NativeArray<Unity.Mathematics.Random> randomNumberGenerators;

        [ReadOnly]
        public float deltaTime;

        public NativeArray<RandomPositionText.MovementData> movement;

        public void Execute(int index)
        {
            var position = movement[index].position;
            var target = movement[index].target;
            var speed = movement[index].speed;

            Unity.Mathematics.Random random = randomNumberGenerators[threadId];

            if (Vector2.Distance(position, target) < 1)
            {
                target = new Vector2(random.NextFloat(boundMin.x , boundMax.x) , random.NextFloat(boundMin.y, boundMax.y));
            }

            position = Vector2.MoveTowards(position, target, speed * deltaTime);
            
            randomNumberGenerators[threadId] = random;
            movement[index] = new RandomPositionText.MovementData()
            {
                position = position,
                target = target,
                speed = speed,
            };
        }
    }
}
