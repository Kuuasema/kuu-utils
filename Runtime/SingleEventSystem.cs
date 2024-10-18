using UnityEngine.EventSystems;

namespace Kuuasema.Utils {
    // ensures only one event system is active on the scene if they are accompanied by this component
    public class SingleEventSystem : SingleBehaviour<EventSystem> {

    }
}