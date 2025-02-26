using UnityEngine;

namespace Lumpn.Particles
{
    public sealed class EmitMoreParticleSystem : IVirtualParticleSystem
    {
        private readonly ParticleSystemTransformController transformController;
        private readonly ParticleSystemEmissionController emissionController;
        private readonly ParticleSystemLifetimeController lifetimeController;
        private readonly ParticleSystemSpeedController speedController;
        private readonly ParticleSystemShapeController shapeController;

        public EmitMoreParticleSystem(ParticleSystem ps, Transform root)
        {
            transformController = new ParticleSystemTransformController(root);
            emissionController = new ParticleSystemEmissionController(ps);
            lifetimeController = new ParticleSystemLifetimeController(ps);
            speedController = new ParticleSystemSpeedController(ps);
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
            var amountMultiplier = sizeMultiplier;

            transformController.Apply(position, rotation);
            lifetimeController.Apply(lifetimeMultiplier);
            speedController.Apply(sizeMultiplier);
            shapeController.Apply(sizeMultiplier);

            return emissionController.Emit(velocity, durationMultiplier, amountMultiplier, time, deltaTime, fraction);
        }
    }
}
