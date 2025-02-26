using UnityEngine;

namespace Lumpn.Particles
{
    internal sealed class VirtualParticleSystem : IVirtualParticleSystem
    {
        private readonly ParticleSystemTransformController transformController;
        private readonly ParticleSystemEmissionController emissionController;
        private readonly ParticleSystemLifetimeController lifetimeController;
        private readonly ParticleSystemSpeedController speedController;
        private readonly ParticleSystemSizeController sizeController;
        private readonly ParticleSystemShapeController shapeController;

        public VirtualParticleSystem(ParticleSystem ps, Transform root)
        {
            transformController = new ParticleSystemTransformController(root);
            emissionController = new ParticleSystemEmissionController(ps);
            lifetimeController = new ParticleSystemLifetimeController(ps);
            speedController = new ParticleSystemSpeedController(ps);
            sizeController = new ParticleSystemSizeController(ps);
            shapeController = new ParticleSystemShapeController(ps);
        }

        public void Initialize()
        {
        }

        public float GetStartDelay(float durationMultiplier)
        {
            return emissionController.GetStartDelay(durationMultiplier);
        }

        public float GetEmissionDuration(float durationMultiplier)
        {
            return emissionController.GetEmissionDuration(durationMultiplier);
        }

        public float Emit(Vector3 position, Quaternion rotation, Vector3 velocity, float durationMultiplier, float lifetimeMultiplier, float sizeMultiplier, float time, float deltaTime, float fraction)
        {
            var amountMultiplier = 1f;

            transformController.Apply(position, rotation);
            lifetimeController.Apply(lifetimeMultiplier);
            speedController.Apply(sizeMultiplier);
            sizeController.Apply(sizeMultiplier);
            shapeController.Apply(sizeMultiplier);

            return emissionController.Emit(velocity, durationMultiplier, amountMultiplier, time, deltaTime, fraction);
        }
    }
}
