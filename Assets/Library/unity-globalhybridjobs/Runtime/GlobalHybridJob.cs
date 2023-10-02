using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using HybridJobs.Core;
using UnityEngine.Profiling;
using Unity.Collections;

namespace HybridJobs
{
    [System.Serializable]
    struct DebugSystemData
    {
        public string Name;
        public Type Type;
        public int Order;
        public bool Enabled;
        public JobExecutionType Execution;//should be readonly
    }

    //this have to executed before any logic
    [DefaultExecutionOrder(-100)]
    public class GlobalHybridJob : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InstantiateSelf()
        {
            instance = new GameObject(nameof(GlobalHybridJob)).AddComponent<GlobalHybridJob>();
            DontDestroyOnLoad(instance.gameObject);
            SceneManager.sceneUnloaded += Reset;
        }
        //Called when scene Changed, by SceneManager
        private static void Reset(Scene oldScene)
        {
            if (ResetJobOnSceneChanged)
            {
                Instance.ResetSystem();
            }
        }

        private static GlobalHybridJob instance;

        /// <summary>
        /// This is a toggle to remove all existing jobs, when scene is changed
        /// Disabling it will make Jobs Persistent, but please becareful of memory leak!
        /// </summary>
        public static bool ResetJobOnSceneChanged { get; set; } = true;

        /// <summary>
        /// Singleton Object of Global Hybrid Job
        /// </summary>
        public static GlobalHybridJob Instance => instance;

        private Dictionary<Type,HybridSystemBase> systemDict = new Dictionary<Type, HybridSystemBase>(8);

        [SerializeField]
        internal int InFrameJobs;
        private NativeArray<JobHandle> jobHandles;

        [Header("Development")]
        [SerializeField]
        private List<HybridSystemBase> existingSystem = new List<HybridSystemBase>(8);
        [SerializeField]
        private List<DebugSystemData> debugSystem = new List<DebugSystemData>(8);

        public void Editor_PushChanges()
        {
            foreach (var item in debugSystem)
            {
                systemDict[item.Type].Enabled = item.Enabled;
                Debug.Log($"System `{item.Type}` Enabled : {item.Enabled}");
            }
        }

        public void Editor_LogSystem()
        {
            foreach (var item in existingSystem)
            {
                string output = string.Empty;
                output += $"System `{item.GetType()}`\n";
                output += $"    Enabled         : {(item.Enabled)}\n";
                output += $"    Order           : {item.ExecutionOrder}`\n";
                output += $"    Jobs Complete   : {(item.ActiveHandle.IsCompleted)}\n";
                Debug.Log(output);
            }
        }

        private void OnDestroy()
        {
            HybridSystemBase[] allSystem = existingSystem.ToArray();
            for (int i = 0; i < allSystem.Length; i++)
            {
                if (allSystem[i].ExecutionType == JobExecutionType.InFrame)
                {
                    InFrameJobs--;
                }
                allSystem[i].OnDestroyed();
            }

            existingSystem.Clear();
            systemDict.Clear();
            debugSystem.Clear();
        }


        private void ResetSystem()
        {
            Debug.Log("[HybridJobs] Hybrid System is being Reset!");
            HybridSystemBase[] allSystem = existingSystem.ToArray();
            for (int i = 0; i < allSystem.Length; i++)
            {
                allSystem[i].OnReset();
            }

            existingSystem.RemoveAll(item =>
            {
                if (item.RequestToDestroy)
                {
                    item.OnDestroyed();
                    systemDict.Remove(item.GetType());
                    return true;
                }
                return false;
            });

#if UNITY_EDITOR
            debugSystem.RemoveAll(item => !systemDict.ContainsKey(item.Type));
#endif
        }

        public static T GetSystem<T>(bool allowIdleJob = true) where T : HybridSystemBase => Instance.GetHybridSystem<T>(allowIdleJob);
        public static T GetOrCreateSystem<T>() where T : HybridSystemBase => Instance.GetOrCreateSystem(typeof(T)) as T;

        public T GetHybridSystem<T>(bool allowIdleJob = true) where T : HybridSystemBase
        {
            Type type = typeof(T);
            if (systemDict.TryGetValue(type, out HybridSystemBase val))
            {
                if (val.Enabled || allowIdleJob) return (T)val;//only and if only system is active
            }
            return null;
        }
        private HybridSystemBase GetOrCreateSystem(Type type)// where T : HybridSystemBase, new()
        {
            if (systemDict.TryGetValue(type, out HybridSystemBase val))
            {
                if (val.RequestToDestroy)
                {
                    val.Reactivate();
                }
                return val;
            }

            //Using reflection is bad :( , but i have no choice, anyway it's just called once, so doesn't really matters
            HybridSystemBase newSystem = (HybridSystemBase)Activator.CreateInstance(type); ;
            newSystem.OnCreated();

            if (newSystem.ExecutionType == JobExecutionType.InFrame)
            {
                InFrameJobs++;
            }

            //register to dict, for easier 
            systemDict.Add(type, newSystem);

            //sorted insert
            var index = existingSystem.FindIndex(item => item.ExecutionOrder >= newSystem.ExecutionOrder);
            if (index == -1)
            {
                existingSystem.Add(newSystem);
            }
            else
            {
                existingSystem.Insert(index, newSystem);
            }

#if UNITY_EDITOR
            //editor only debug data
            var systemData = new DebugSystemData() 
            { 
                Name = newSystem.GetType().Name, 
                Type = type,
                Order = newSystem.ExecutionOrder,
                Enabled = true,
                Execution = newSystem.ExecutionType
            };
            if (index == -1)
            {
                debugSystem.Add(systemData);
            }
            else
            {
                debugSystem.Insert(index, systemData);
            }
#endif
            return newSystem;
        }

        /// <summary>
        /// Register Component to It's System
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        public static void Register<T>(T component) where T : IHybridComponent => Instance.RegisterComponent(component);
        public void RegisterComponent<T>(T component) where T : IHybridComponent
        {
            HybridSystem<T> system;
            if (systemDict.TryGetValue(component.JobSystem, out HybridSystemBase baseSystem))
            {
                system = (HybridSystem<T>)baseSystem;
            }
            else
            {
                baseSystem = GetOrCreateSystem(component.JobSystem);
                system = (HybridSystem<T>)baseSystem;
            }

            
            system.Register(component);
        }

        /// <summary>
        /// Remove Component from it's System
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        public static void Remove<T>(T component) where T : IHybridComponent => Instance.RemoveComponent(component);
        public void RemoveComponent<T>(T component) where T : IHybridComponent
        {
            if (systemDict.TryGetValue(component.JobSystem, out HybridSystemBase baseSystem))
            {
                HybridSystem<T> system = (HybridSystem<T>)baseSystem;
                system.Remove(component);
            }
            else
            {
                Debug.LogWarning("Unable to Remove component, System no longger exist");
            }            
        }

        private void Update()
        {
            bool isOnFinishFrameJobExist = InFrameJobs > 0;
            if (isOnFinishFrameJobExist)
            {
                jobHandles = new NativeArray<JobHandle>(InFrameJobs, Allocator.Temp);
            }

            Profiler.BeginSample("Running Job");
            RunJobs();
            Profiler.EndSample();

            if (isOnFinishFrameJobExist)
            {
                JobHandle.CompleteAll(jobHandles);
            }

            Profiler.BeginSample("Complete Job");
            CompleteJob(false);//trigger OnComplete for finished jobs
            Profiler.EndSample();

            if (isOnFinishFrameJobExist)
            {
                jobHandles.Dispose();
            }
        }

        private void LateUpdate()
        {
            Profiler.BeginSample("Complete Late Update Job");
            //Force Complete on Execution Type is Late Update
            for (int i = 0; i < existingSystem.Count; i++)
            {
                var system = existingSystem[i];
                if (system.Enabled && (system.ExecutionType == JobExecutionType.LateUpdate || system.ExecutionType == JobExecutionType.InFrame) && !system.ActiveHandle.IsCompleted)
                {
                    system.ActiveHandle.Complete();//force complete
                }
            }
            Profiler.EndSample();

            Profiler.BeginSample("Complete Job");
            CompleteJob();
            Profiler.EndSample();
        }

        /// <summary>
        /// Run Job if Allowed
        /// </summary>
        private void RunJobs() 
        {
            int onFinishJobCounter = 0;
            for (int i = 0; i < existingSystem.Count; i++)
            {
                //execute new jobs, if previous job already done
                var system = existingSystem[i];
                if (system.Enabled && system.ActiveHandle.IsCompleted && !system.IsJobRunning && system.IsAllowedToUpdate())
                {
#if UNITY_EDITOR
                    Profiler.BeginSample(debugSystem[i].Name);
#endif
                    system.IsJobRunning = true;
                    system.ActiveHandle = existingSystem[i].OnUpdate();
                    if (system.ExecutionType == JobExecutionType.InFrame && system.IsJobRunning)
                    {
                        jobHandles[onFinishJobCounter] = system.ActiveHandle;
                        onFinishJobCounter++;
                    }
#if UNITY_EDITOR
                    Profiler.EndSample();
#endif
                }
            }
        }

        /// <summary>
        /// Trigger OnComplete on Finished Job
        /// </summary>
        private void CompleteJob(bool force = true)
        {
            for (int i = 0; i < existingSystem.Count; i++)
            {
                var system = existingSystem[i];
                if (system.Enabled && system.IsJobRunning && system.ActiveHandle.IsCompleted )
                {
#if UNITY_EDITOR
                    Profiler.BeginSample(debugSystem[i].Name);
#endif
                    if (force || system.ActiveHandle.IsCompleted)
                    {
                        system.ActiveHandle.Complete();//ensure job has complete
                        system.OnComplete();
                        system.IsJobRunning = false;
                    }
#if UNITY_EDITOR
                    Profiler.EndSample();
#endif
                }
            }
        }
    }
}
