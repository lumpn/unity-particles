using UnityEngine;

namespace Lumpn.Particles
{
    public interface IVirtualParticleSystem
    {
        void Initialize();
        float GetStartDelay(float durationMultiplier);
        float GetEmissionDuration(float durationMultiplier);
        float Emit(Vector3 position, Quaternion rotation, Vector3 velocity, float durationMultiplier, float lifetimeMultiplier, float sizeMultiplier, float time, float deltaTime, float fraction);
    }
}
