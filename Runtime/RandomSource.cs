using System.Collections.Generic;
using UnityEngine;

namespace Kuuasema.Utils {
    public class RandomSource {

        private Random.State sourceState;
        private Random.State restoreState;

        private Stack<Random.State> stateStack = new Stack<Random.State>();
        

        public RandomSource(Random.State state) {
            this.sourceState = state;
        }

        public RandomSource(int seed) {
            this.Begin();
            Random.InitState(seed);
            this.sourceState = Random.state;
            this.End();
        }

        public void Push() {
            this.stateStack.Push(this.sourceState);
        }

        public void Pop() {
            if (this.stateStack.Count > 0) {
                this.sourceState = this.stateStack.Pop();
            }
        }

        public float Next() {
            float value;
            this.Begin();
            value = Random.value;
            this.End();
            return value;
        }

        public float Range(float min, float max) {
            float value;
            this.Begin();
            value = Random.Range(min, max);
            this.End();
            return value;
        }

        public bool Bool() {
            bool value;
            this.Begin();
            value = Random.value < 0.5f;
            this.End();
            return value;
        }

        private void Begin() {
            this.restoreState = Random.state;
            Random.state = this.sourceState;
        }

        private void End() {
            this.sourceState = Random.state;
            Random.state = this.restoreState;
        }
    }
}