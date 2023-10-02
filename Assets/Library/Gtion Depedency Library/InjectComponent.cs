using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Gtion.Plugin.DI
{
    [Flags]
    public enum InjectMode
    { 
        None = 0,
        This = 1 << 0,
        Childs = 1 << 1,
        Parent  = 1 << 2,
        Nested = 1 << 3,
    }

    public class InjectComponent : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField]
        InjectMode injectMode = InjectMode.This;
        [SerializeField]
        LiveMode liveMode = LiveMode.Scene;

        [SerializeField]
        bool isRegister = true;
        [SerializeField]
        bool isRequest = true;

        [Header("Debug")]
        [SerializeField]
        bool enableLog;

        private void Start()
        {
            if (injectMode == InjectMode.None)
            {
                enabled = false;
                return;
            }

            var original = GDi.EnableLog;
            GDi.EnableLog = enableLog;

            if ((injectMode & InjectMode.This) == InjectMode.This)
            {
                GetAllComponentAndExecute(transform);
            }

            if ((injectMode & InjectMode.Childs) == InjectMode.Childs)
            {
                if ((injectMode & InjectMode.Nested) != InjectMode.Nested)
                {
                    int count = transform.childCount;
                    for (int i = 0; i < count; i++)
                    {
                        GetAllComponentAndExecute(transform.GetChild(i));
                    }
                }
                else
                {
                    List<Transform> list = new List<Transform>();
                    GetChildrenRecursively(transform, list);

                    foreach (var child in list)
                    {
                        GetAllComponentAndExecute(child);
                    }
                }
            }

            

            if ((injectMode & InjectMode.Parent) == InjectMode.Parent)
            {
                if ((injectMode & InjectMode.Nested) != InjectMode.Nested)
                {
                    while (transform.parent != null)
                    {
                        GetAllComponentAndExecute(transform.parent);
                    }
                }
                else
                {
                    GetAllComponentAndExecute(transform.parent);
                }
            }

            GDi.EnableLog = original;
        }

        void GetChildrenRecursively(Transform parent, List<Transform> result)
        {
            int count = parent.childCount;
            for (int i = 0; i < count; i++)
            {
                var child = parent.GetChild(i);
                result.Add(child);
                GetChildrenRecursively(child, result);
            }
        }

        private void GetAllComponentAndExecute(Transform transform)
        {
            if (transform == null) return;

            var allComponent = transform.GetComponents<MonoBehaviour>();
            foreach (var component in allComponent)
            {
                if (component.GetType() == typeof(InjectComponent)) continue;

                ExecuteInjection(component);
            }
        }

        private void ExecuteInjection(MonoBehaviour mono)
        {
            if (isRegister)
            {
                GDi.Register(mono.GetType(), mono, liveMode);
            }

            if (isRequest)
            {
                if (mono is IInjectCallback icallback)
                {
                    GDi.Request(mono.GetType(), mono, icallback.OnDependencyReady);
                }
                else
                {
                    GDi.Request(mono.GetType(), mono, null);
                }
            }
        }
    }
}
