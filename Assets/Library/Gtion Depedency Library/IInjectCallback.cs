using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gtion.Plugin.DI
{
    public interface IInjectCallback
    {
        public bool IsDependencyReady { get; set; }
        public void OnDependencyReady();
    }
}
