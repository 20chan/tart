using System.Collections.Generic;

namespace tart.Simulations.Models.Port {
    public class PortInternal {
        private const float truckSpeed = 1f;
        private const float truckLoadAt = 100f;
        private const float truckUnloadAt = 240f;
        private const float truckDistance = 40f;

        private List<float> loadTrucks, unloadTrucks;

        public PortInternal() {
            loadTrucks = new List<float>();
            unloadTrucks = new List<float>();
        }

        public void Update(float time, float deltaTime) {

        }
    }
}