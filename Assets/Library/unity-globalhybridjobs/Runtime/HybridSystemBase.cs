using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HybridJobs.Core;
using Unity.Jobs;
using System;

namespace HybridJobs.Core
{
    public abstract class HybridSystemBase
    {
        private bool enabled = true;

        /// <summary>
        /// Is This System Enabled/Running?
        /// </summary>
        public bool Enabled
        {
            get 
            {
                return enabled && !RequestToDestroy && HybridObjectCount > 0;
            }
            set 
            {
                enabled = value;
            }
        }

        /// <summary>
        /// Is This System is going to be destroyed on Reset?
        /// </summary>
        public bool RequestToDestroy => requestToDestroy;
        public bool requestToDestroy = false;

        /// <summary>
        /// Ordering Update Execution, Smaller the number the faster it's executed
        /// </summary>
        public virtual int ExecutionOrder => 0;

        /// <summary>
        /// Decide when the execution will complete
        /// </summary>
        public abstract JobExecutionType ExecutionType { get; }
        /// <summary>
        /// Handle of Current Job
        /// </summary>
        public JobHandle ActiveHandle = new JobHandle();
        /// <summary>
        /// Is Job Running right now?
        /// </summary>
        public virtual bool IsJobRunning { get; set; }

        /// <summary>
        /// Accurately Count Object include in System
        /// </summary>
        protected abstract int HybridObjectCount { get; }
        /// <summary>
        /// On System Created
        /// </summary>
        public virtual void OnCreated() { }
        /// <summary>
        /// On System Reset, Called on Scene Changed
        /// </summary>
        public abstract void OnReset();

        /// <summary>
        /// Called to check whether we are going to call OnUpdate() method or skip it
        /// </summary>
        /// <returns></returns>
        public virtual bool IsAllowedToUpdate()
        {
            return true;
        }

        /// <summary>
        /// Called Every Frame, asking JobHandle to be scheduled.
        /// NOTE: you can pass `default` if you want to just skip it
        /// </summary>
        /// <returns></returns>
        public abstract JobHandle OnUpdate();
        /// <summary>
        /// Called when Job that being Scheduled on `OnUpdate()` Method is Finished
        /// </summary>
        public abstract void OnComplete();

        /// <summary>
        /// Called on Reset Event, if the `RequestToDestroy` is true, this will be called instead of `OnReset()`
        /// </summary>
        public abstract void OnDestroyed();

        /// <summary>
        /// Cancel to destroy this object when `Reset Event` happen.
        /// </summary>
        internal void Reactivate()
        {
            requestToDestroy = false;
        }

        /// <summary>
        /// Mark this system to be destroyed when the `Reset Event` happen later.
        /// </summary>
        protected void DestroySelf()
        {
            //we are not destroying system while running in scene, and scene can be reactivated
            //this way we can reuse the system without have to create new system again and again.
            requestToDestroy = true;
        }

        public T GetSystem<T>() where T : HybridSystemBase => GlobalHybridJob.GetSystem<T>();
    }
    
}
