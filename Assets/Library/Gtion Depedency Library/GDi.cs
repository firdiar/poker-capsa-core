using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gtion.Plugin.DI
{
    public enum LiveMode
    { 
        Scene,
        Persistance
    }
    
    /// <summary>
    /// Gtion Depedency Injection
    /// </summary>
    public class GDi
    {
        private static GDi _instance;
        public static GDi Instance
        {
            get
            {
                if (_instance == null) _instance = new GDi();
                return _instance;
            }
        }

        public static bool EnableLog;

        [RuntimeInitializeOnLoadMethod]
        public static void Initialize() => Initialize(false);

        public static void Initialize(bool force)
        {
            if(force || _instance == null)
                _instance = new GDi();
        }

        /// <summary>
        /// Registering Object to Depedency Injection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="isPersistance"></param>
        public static void RegisterAndRequest<T>(T obj, LiveMode isPersistance = LiveMode.Scene, Action callbackOnReady = null)
        {
            Register(obj, isPersistance);
            Request(obj, callbackOnReady);
        }

        /// <summary>
        /// Registering Object to Depedency Injection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="isPersistance"></param>
        public static void Register<T>(T obj, LiveMode isPersistance = LiveMode.Scene) => Instance.RegisterObject(typeof(T) , obj, isPersistance == LiveMode.Persistance);

        /// <summary>
        /// Registering Object to Depedency Injection
        /// </summary>
        /// <param name="type"></param>
        /// <param name="obj"></param>
        /// <param name="isPersistance"></param>
        public static void Register(Type type , object obj, LiveMode isPersistance = LiveMode.Scene) => Instance.RegisterObject(type , obj, isPersistance == LiveMode.Persistance);

        /// <summary>
        /// Load Object directly from Depedency Injection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool Load<T>(out T obj) => Instance.LoadObject(out obj);

        /// <summary>
        /// Load Object directly from Depedency Injection
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static object Load(Type t) => Instance.LoadObject(t);

        /// <summary>
        /// Requesting Depedency Injection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="main"></param>
        /// <param name="callbackOnReady"></param>
        public static void Request<T>(T main, Action callbackOnReady = null) => Instance.RequestDepdency(typeof(T), main, callbackOnReady);

        /// <summary>
        /// Requesting Depedency Injection
        /// </summary>
        /// <param name="t"></param>
        /// <param name="main"></param>
        /// <param name="callbackOnReady"></param>
        public static void Request(Type t, object main, Action callbackOnReady = null) => Instance.RequestDepdency(t, main, callbackOnReady);

        public static void Clear(bool clearAll = false)
        {
            Instance.GetReferenceLib(false).Clear();
            if(clearAll) Instance.GetReferenceLib(true).Clear();
        }

        public static void Log(object message)
        {
            if(EnableLog)
                Debug.Log(message);
        }

        protected Dictionary<Type, object> ReferenceLibPersistance = new Dictionary<Type, object>();
        protected Dictionary<Type, object> ReferenceLibScene = new Dictionary<Type, object>();
        protected Dictionary<Type, object> GetReferenceLib(bool isPersistance) => isPersistance ? ReferenceLibPersistance : ReferenceLibScene;

        protected List<GRequest> DepedencyRequest = new List<GRequest>();

        public GDi() 
        {
            SceneManager.sceneUnloaded += Clear;
        }

        public void RegisterObject(Type type ,  object obj, bool isPersistance)
        {
            Log($"DI: Register {obj} as {type}");
            var ReferenceLib = GetReferenceLib(isPersistance);
            if (ReferenceLib.ContainsKey(type))
            {
                ReferenceLib[type] = obj;
            }
            else
            {
                ReferenceLib.Add(type, obj);
            }

            if (DepedencyRequest.Count > 0)
            {
                List<int> removeList = new List<int>();
                for (int i = 0; i < DepedencyRequest.Count; i++)
                {
                    if (DepedencyRequest[i].IsMainObjectDestroyed)
                    {
                        removeList.Add(i);
                    }
                    else if (DepedencyRequest[i].HandleObjectRegistered(type , obj))
                    {
                        removeList.Add(i);
                    }
                }

                for (int i = removeList.Count-1; i >= 0 ; i--)
                {
                    var req = DepedencyRequest[removeList[i]];
                    DepedencyRequest.RemoveAt(removeList[i]);
                    req.TriggerCallback();                    
                }

                Log($"DI: Requesting Count ({DepedencyRequest.Count})");
            }
        }

        public bool LoadObject<T>(out T obj)
        {
            object fieldValue = LoadObject(typeof(T));
            bool isObjDestroyed = (Equals(fieldValue, null) || fieldValue.Equals(null));

            if (!isObjDestroyed)
            {
                obj = (T)fieldValue;
                return true;
            }
            else
            {
                Log($"Can't Find '{typeof(T)} in Reference Library");
                obj = default;
                return false;
            }
        }

        public object LoadObject(Type t)
        {
            object temp;
            if (GetReferenceLib(false).TryGetValue(t, out temp))
            {
                return temp;
            }
            else if (GetReferenceLib(true).TryGetValue(t, out temp))
            {
                return temp;
            }
            else
            {
                return temp;
            }
        }

        public void RequestDepdency(Type t, IInjectCallback main)
        {
            RequestDepdency(t , main, null);
        }
        public void RequestDepdency(Type t , object main, Action callbackOnReady = null)
        {
            Dictionary<Type, FieldInfo> requestDepedency = new Dictionary<Type, FieldInfo>();
            foreach (var field in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var att = field.GetCustomAttributes(typeof(GInject), true).FirstOrDefault() as GInject;
                object fieldValue = field.GetValue(main);
                
                //Checking attribute set as depdency injection and field is empty
                if (att != null && IsObjectDestroyed(fieldValue))
                {

                    //checking current library
                    object registeredObj = LoadObject(field.FieldType);
                    if (IsObjectDestroyed(registeredObj))
                    {
                        requestDepedency.Add(field.FieldType, field);
                    }
                    else
                    {
                        //using existing value to fill depedency
                        field.SetValue(main, registeredObj);
                    }
                    
                }
            }

            if (requestDepedency.Count > 0)
            {
                int index = DepedencyRequest.FindIndex(item => item.IsEqual(main));
                GRequest request = new GRequest(t, main, requestDepedency, callbackOnReady);

                if (index == -1)
                {
                    DepedencyRequest.Add(request);
                }
                else
                {
                    DepedencyRequest[index] = request;
                }
            }
            else
            {
                Log($"DI: {main} Requesting Depedency (Ready!)");
                IInjectCallback injectCallback = null;
                if (main is IInjectCallback inject)
                {
                    inject.IsDependencyReady = true;
                    injectCallback = inject;
                }

                if (callbackOnReady != null)
                {
                    callbackOnReady.Invoke();
                }
                else if (injectCallback != null)
                {
                    injectCallback.OnDependencyReady();
                }
            }
        }

        public void Clear(Scene scene) 
        {
            if(scene == null || !scene.isSubScene) Instance.GetReferenceLib(false).Clear();
        }


        public static bool IsObjectDestroyed(object obj) => Equals(obj, null) || obj.Equals(null);
    }

    public class GRequest
    {
        protected Type MainType;
        protected object MainObject;
        protected Dictionary<Type, FieldInfo> RequestDepedency;
        
        protected Action OnDepedencyReady;

        public bool IsEqual(object obj) => MainObject == obj;
        public bool IsMainObjectDestroyed => GDi.IsObjectDestroyed(MainObject);
        public Type RequestType => MainType;

        public GRequest(Type type, object obj, Dictionary<Type, FieldInfo> requestDepedency, Action callbackOnReady)
        {

            GDi.Log($"DI: {obj} Requesting Depedency ({requestDepedency.Count})");
            foreach (var pair in requestDepedency)
            {
                //GDi.Log($"DI: > {pair.Key}");
            }

            MainType = type;
            MainObject = obj;
            RequestDepedency = requestDepedency;
            OnDepedencyReady = callbackOnReady;
        }

        public bool HandleObjectRegistered<T>(T obj) => HandleObjectRegistered(typeof(T), obj);

        public bool HandleObjectRegistered(Type type , object obj)
        {
            bool isComplete = false;

            if (RequestDepedency.ContainsKey(type))
            {
                RequestDepedency[type].SetValue(MainObject, obj);

                isComplete = IsDepdencyComplete();
                GDi.Log($"DI: Update {MainObject} Request Complete : {isComplete}");
            }
            
            return isComplete;
        }

        public bool IsDepdencyComplete() 
        {
            foreach (var pair in RequestDepedency)
            {
                if (GDi.IsObjectDestroyed(pair.Value.GetValue(MainObject)))
                {
                    return false;
                }
            }
            return true;
        }

        public void TriggerCallback() 
        {
            GDi.Log($"DI: {MainObject} Depedency Ready!");
            IInjectCallback injectCallback = null;
            if (MainObject is IInjectCallback inject)
            {
                inject.IsDependencyReady = true;
                injectCallback = inject;
            }

            if (OnDepedencyReady != null)
            {
                OnDepedencyReady.Invoke();                
            }
            else if(injectCallback != null)
            {
                injectCallback.OnDependencyReady();
            }
        }
    }
}
