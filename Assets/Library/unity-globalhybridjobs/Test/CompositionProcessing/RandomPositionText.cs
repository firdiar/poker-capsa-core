using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HybridJobs;
using System;
using TMPro;

public class RandomPositionText : MonoBehaviour
{
    public struct MovementData
    {
        public Vector2 position;
        public Vector2 target;
        public float speed;
        public bool UpdateNext;
    }

    [System.Serializable]
    public class ComponentPart : IHybridComponent
    {
        public int JobExecutionIdentifier { get; set; }
        public Type JobSystem => typeof(CompositionSystemTest);

        public Transform TextTransform;
        public float Speed;

        //non-manual
        public Vector2 CurrentTarget;

        public ComponentPart(Transform t, float speed)
        {
            TextTransform = t;
            Speed = speed;
        }

        public MovementData GetMovementData() 
        {
            return new MovementData()
            {
                position = TextTransform.position,
                target = CurrentTarget,
                speed = Speed
            };
        }
    }

    [SerializeField]
    Transform[] transforms;
    [SerializeField]
    float speed;

    [SerializeField]
    List<ComponentPart> cache;
    [SerializeField]
    bool RandomRemoveComponent;

    private void OnValidate()
    {
        if (RandomRemoveComponent)
        {
            int index = UnityEngine.Random.Range(0, cache.Count);
            var c = cache[index];
            Debug.Log("Removing Item : "+ index);
            GlobalHybridJob.Remove(c);
            cache.RemoveAt(index);
            Destroy(c.TextTransform.gameObject);

            RandomRemoveComponent = false;
        }
    }

    private void Start()
    {
        foreach(var item in transforms)
        {
            ComponentPart c = new ComponentPart(item, speed);
            cache.Add(c);
            GlobalHybridJob.Register(c);
        }
        
    }

    private void OnDestroy()
    {
        foreach (var item in cache)
        {
            GlobalHybridJob.Remove(item);
        }
    }
}
