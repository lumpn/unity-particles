using UnityEngine;

namespace Lumpn.Particles
{
    public struct ParticleSystemShapeController
    {
        private readonly ParticleSystem particleSystem;
        private readonly Vector3 shapePosition; // [m]
        private readonly Vector3 shapeScale; // [m]

        public ParticleSystemShapeController(ParticleSystem ps)
        {
            particleSystem = ps;

            var shape = particleSystem.shape;
            shapePosition = shape.position;
            shapeScale = shape.scale;
        }

        public void Apply(float shapeMultiplier)
        {
            var shape = particleSystem.shape;
            shape.position = shapePosition * shapeMultiplier;
            shape.scale = shapeScale * shapeMultiplier;
        }
    }
}
