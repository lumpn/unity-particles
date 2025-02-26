using UnityEngine;
using MinMaxCurve = UnityEngine.ParticleSystem.MinMaxCurve;

namespace Lumpn.Particles
{
    public struct ParticleSystemSpeedController
    {
        private readonly ParticleSystem particleSystem;
        private readonly MinMaxCurve startSpeed; // [m/s]

        public ParticleSystemSpeedController(ParticleSystem ps)
        {
            particleSystem = ps;

            var main = particleSystem.main;
            startSpeed = main.startSpeed;
        }

        public void Apply(float speedMultiplier)
        {
            var main = particleSystem.main;
            main.startSpeed = ParticleSystemUtils.Scale(startSpeed, speedMultiplier);
        }
    }
}
