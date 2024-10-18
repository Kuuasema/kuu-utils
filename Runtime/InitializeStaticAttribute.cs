using System.Reflection;

namespace Kuuasema.Utils {

    public class InitializeStaticAttribute : System.Attribute {
        public int Order;
        public MethodInfo Method;
        public InitializeStaticAttribute() {
            this.Order = int.MaxValue;
        }
        public InitializeStaticAttribute(int order) { 
            this.Order = order; 
        }
    }
}