using UnityEngine;
namespace Kuuasema.Utils {

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class DisableInspectorAttribute : PropertyAttribute {
        public DisableInspectorAttribute() { }
    }
}