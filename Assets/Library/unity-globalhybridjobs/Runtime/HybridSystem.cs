using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HybridJobs.Core;

namespace HybridJobs
{
    public abstract class HybridSystem<T> : HybridSystemBase where T : IHybridComponent
    {
        /// <summary>
        /// All Object that registered to this system
        /// </summary>
        protected HashSet<T> HybridObjects = new HashSet<T>();
        private List<T> RegisterReqest = new List<T>();
        private List<T> RemoveReqest = new List<T>();

        protected override int HybridObjectCount => HybridObjects.Count + RegisterReqest.Count - RemoveReqest.Count;

        public override void OnReset()
        {
            HybridObjects.Clear();
        }
        internal void Register(T component)
        {
            HybridObjects.Add(component);
            if (!IsJobRunning)
            {
                OnRegistered(component);
            }
            else
            {
                RegisterReqest.Add(component);
            }
        }

        /// <summary>
        /// Trigger on New Object Registered, Guaranteed when this method called the Jobs is not running
        /// </summary>
        /// <param name="component"></param>
        protected virtual void OnRegistered(T component) { }

        internal void Remove(T component)
        {
            HybridObjects.Remove(component);
            if (!IsJobRunning)
            {
                OnRemoved(component);
            }
            else
            {
                RemoveReqest.Add(component);
            }
        }

        /// <summary>
        /// Trigger on Object Removed, Guaranteed when this method called the Jobs is not running
        /// </summary>
        /// <param name="component"></param>
        protected virtual void OnRemoved(T component) { }

        /// <summary>
        /// Called when Job that being Scheduled on `OnUpdate()` Method is Finished
        /// </summary>
        public override sealed void OnComplete()
        {
            if (RemoveReqest.Count > 0)
            {
                foreach (var component in RemoveReqest)
                {
                    OnRemoved(component);
                }
                RemoveReqest.Clear();
            }

            if (RegisterReqest.Count > 0)
            {
                foreach (var component in RegisterReqest)
                {
                    OnRegistered(component);
                }
                RegisterReqest.Clear();
            }

            OnCompleted();
        }

        public abstract void OnCompleted();
    }

}
