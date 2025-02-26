using UnityEngine;
using MinMaxCurve = UnityEngine.ParticleSystem.MinMaxCurve;

namespace Lumpn.Particles
{
    public struct ParticleSystemSizeController
    {
        private readonly ParticleSystem particleSystem;
        private readonly MinMaxCurve startSizeX; // [m]
        private readonly MinMaxCurve startSizeY; // [m]
        private readonly MinMaxCurve startSizeZ; // [m]

        public ParticleSystemSizeController(ParticleSystem ps)
        {
            particleSystem = ps;

            var main = particleSystem.main;
            startSizeX = main.startSizeX;
            startSizeY = main.startSizeY;
            startSizeZ = main.startSizeZ;
        }

        public void Apply(float sizeMultiplier)
        {
            var main = particleSystem.main;
            main.startSizeX = ParticleSystemUtils.Scale(startSizeX, sizeMultiplier);
            main.startSizeY = ParticleSystemUtils.Scale(startSizeY, sizeMultiplier);
            main.startSizeZ = ParticleSystemUtils.Scale(startSizeZ, sizeMultiplier);
        }
    }
}
