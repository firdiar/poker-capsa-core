# Global Hybrid Jobs
## Built In
![Build Status](https://badgen.net/badge/Build/Passing/green) ![Version](https://badgen.net/badge/Release/v1.1.0/blue) ![Languages](https://badgen.net/badge/Languages/C-Sharp/blue)
![Engine](https://badgen.net/badge/Engine/Unity%202020.3.4f1/gray)
![License](https://badgen.net/shards/license/clear)

Depedencies
- Unity.Jobs : [Docs](https://docs.unity3d.com/Packages/com.unity.jobs@0.1/manual/index.html)
- Unity.Burst : [Docs](https://docs.unity3d.com/Packages/com.unity.burst@1.8/manual/index.html)

## Installation
Please check documentation below
> https://docs.unity3d.com/2019.3/Documentation/Manual/upm-ui-giturl.html

## What is it?
Do you know Unity ECS? you can call this Hybrid ECS, since ECS no longer support Hybrid.
This allows you to doing multithreading in easier way.

*Sorry that i made it in a Rush within half day, so i couldn't do proper documentation, but all the code is Full Documented. and it's.... pretty simple though :")

## Usage
Straight to the point, how to use this is similar to how you use Jobs in Native way, or System based in ECS.
##### 1. Create System
System is Data Processer, This system class process all component data then apply it back to the component.
```
using HybridJobs;
using Unity.Jobs;
using Unity.Burst;
using UnityEngine;

public class SystemDATest : HybridSystem<CompATest>
{
    //Type When job will be finished on execution
    public override JobExecutionType ExecutionType => JobExecutionType.Async;
    public override int ExecutionOrder => -1;//priority execution

    public override JobHandle OnUpdate()
    {
        //This is How you schedule a Job, you don't need to finish it immidiately
        SuperLongJob job = new SuperLongJob();
        return job.Schedule();
    }
    
    public override void OnCompleted()
    {
        //This method will be called when Job is Finished
        //`HybridObjects` is HashSet<T> contains All object registered to this system
        foreach (var item in HybridObjects)
        {
            //Do Something
        }
    }

    [BurstCompile]//Don't Forget this Attribute, makes it 4x faster
    struct SuperLongJob : IJob
    {
        public void Execute()
        { /* Do Something */ }
    }
}
```
##### 2. Create Component
For component you need to implement `IHybridComponent` interface on your class. it doesn't have to be `MonoBehaviour` it can be plain class. as long as you sure to Register and Remove it.
```
using UnityEngine;
using HybridJobs; //Make sure you put this Library
using System;

public class CompATest : MonoBehaviour, IHybridComponent
{
    //int property to help you managing order in System later
    public int JobExecutionIdentifier { get; set; }
    //Specify System of Hybrid Component
    public Type JobSystem => typeof(SystemDATest);

    private void Start()
    {
        //you need to register IHybridComponent to Global
        GlobalHybridJob.Register(this);
    }

    private void OnDestroy()
    {
        //you need to remove IHybridComponent to Global, when no longger used
        GlobalHybridJob.Remove(this);
    }
}
```

##### JobExecutionType 
- **InFrame** : Force to Finish within `Update()` Method
- **LateUpdate** : Finish in `LateUpdate()` Method
- **Async** : No Limit When to Finish, Callback will be called when jobs finished

I think.. That's it, furthermore you've to learn by yourself about Jobs/ECS to be able using Jobs properly.

Oh, to make sure that it's working, you can start play the game, then check this GameObject in "Don't Destroy Onload` part
[![image](/uploads/2626d1f8ed26de972efae467ea2cc281/image.png)]
it will shows all the system that currently active, as well you can enable-disable the system there.


## It's not Understandable :(
yes, sorry so much for very short documentation, but i've prepared Sample Script to help you understand it more.
please check this scene example:  `Test\GlobalHybridSampleJobs.unity`
there's 3 game object, each giving you example to implement jobs using this pacakge
[![Firdiar](https://gitlab.com/gtion-production/unity-globalhybridjobs/uploads/3ee574fed70f31ff5193a2e646585fd7/image.png)]

## Contributor


| Profile | 
| ------ |
| [![Firdiar](https://gitlab.com/uploads/-/system/user/avatar/2307294/avatar.png?raw=true)](https://www.linkedin.com/in/firdiar) |
| [Firdiansyah Ramadhan](https://www.linkedin.com/in/firdiar) | 


## License

MIT

** I want to make a Love Letter :) **