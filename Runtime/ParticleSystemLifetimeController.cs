using UnityEngine;
using MinMaxCurve = UnityEngine.ParticleSystem.MinMaxCurve;

namespace Lumpn.Particles
{
    public struct ParticleSystemLifetimeController
    {
        private readonly ParticleSystem particleSystem;
        private readonly MinMaxCurve startLifetime; // [s]

        public ParticleSystemLifetimeController(ParticleSystem ps)
        {
            particleSystem = ps;

            var main = particleSystem.main;
            startLifetime = main.startLifetime;
        }

        public void Apply(float lifetimeMultiplier)
        {
            var main = particleSystem.main;
            main.startLifetime = ParticleSystemUtils.Scale(startLifetime, lifetimeMultiplier);
        }
    }
}
