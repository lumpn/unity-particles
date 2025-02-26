using UnityEngine;
using MinMaxCurve = UnityEngine.ParticleSystem.MinMaxCurve;

namespace Lumpn.Particles
{
    public struct ParticleSystemRotationController
    {
        private readonly ParticleSystem particleSystem;
        private readonly MinMaxCurve rotationX; // [degree/s]
        private readonly MinMaxCurve rotationY; // [degree/s]
        private readonly MinMaxCurve rotationZ; // [degree/s]

        public ParticleSystemRotationController(ParticleSystem ps)
        {
            particleSystem = ps;

            var rotation = particleSystem.rotationOverLifetime;
            rotationX = rotation.x;
            rotationY = rotation.y;
            rotationZ = rotation.z;
        }

        public void Apply(float rotationMultiplier)
        {
            var rotation = particleSystem.rotationOverLifetime;
            rotation.x = ParticleSystemUtils.Scale(rotationX, rotationMultiplier);
            rotation.y = ParticleSystemUtils.Scale(rotationY, rotationMultiplier);
            rotation.z = ParticleSystemUtils.Scale(rotationZ, rotationMultiplier);
        }
    }
}
