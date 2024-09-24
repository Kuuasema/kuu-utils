
namespace Kuuasema.Utils {
    /**
    * Class for handling scene references more neatly than strings.
    */
    //TODO custom inspector property drawer
    public class SceneReference {
        public string Name { get; set; }
        public SceneReference() {}
        public SceneReference(string name) {
            this.Name = name;
        }
    }
}