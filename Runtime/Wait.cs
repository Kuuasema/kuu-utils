using UnityEngine;

namespace Kuuasema.Utils {
    /**
    * Static class to hold common yield objects.
    */
    public static class Wait {
        public static readonly WaitForSeconds ONE_MILLISECOND = new WaitForSeconds(0.001f); 
        public static readonly WaitForSeconds ONE_SECOND = new WaitForSeconds(1); 
        public static readonly WaitForSeconds TEN_SECONDS = new WaitForSeconds(1); 
        public static readonly WaitForFixedUpdate FOR_FIXED_UPDATE = new WaitForFixedUpdate(); 
    }
}
