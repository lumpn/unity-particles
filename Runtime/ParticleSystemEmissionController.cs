using UnityEngine;
using MinMaxCurve = UnityEngine.ParticleSystem.MinMaxCurve;

namespace Lumpn.Particles
{
    public struct ParticleSystemEmissionController
    {
        private readonly ParticleSystem particleSystem;

        // main module
        private readonly float duration; // [s]
        private readonly float startDelay; // [s]

        // emission module
        private readonly MinMaxCurve emissionRateOverTime;
        private readonly float emissionDuration; // derived [s]

        public ParticleSystemEmissionController(ParticleSystem ps)
        {
            particleSystem = ps;

            var main = particleSystem.main;
            duration = main.duration;
            startDelay = main.startDelay.constant;

            var emission = particleSystem.emission;
            emissionRateOverTime = emission.rateOverTime;
            emissionDuration = CalculateEmissionDuration(emission, duration);
        }

        public float GetStartDelay(float durationMultiplier)
        {
            return startDelay;
        }

        public float GetEmissionDuration(float durationMultiplier)
        {
            return (emissionDuration * durationMultiplier);
        }

        private void Emit(Vector3 emitterVelocity, int count)
        {
            var main = particleSystem.main;
            main.emitterVelocity = emitterVelocity;

            particleSystem.Emit(count);
        }

        public float Emit(Vector3 emitterVelocity, float durationMultiplier, float amountMultiplier, float time, float deltaTime, float fraction)
        {
            var emission = particleSystem.emission;

            // accumulate bursts
            var prevTime = time - deltaTime;
            var burstAmount = 0;
            for (int i = emission.burstCount - 1; i >= 0; i--)
            {
                var burst = emission.GetBurst(i);
                if (burst.time > prevTime && burst.time <= time)
                {
                    // NOTE Jonas: we only support constant count, one cycle, 100% probability
                    burstAmount += (int)burst.count.constant;
                }
            }

            // accumulate continuous
            var normalizedTime = time / (duration * durationMultiplier);
            var rate = Mathf.Max(emissionRateOverTime.Evaluate(normalizedTime), 0f);
            var continuousAmount = rate * deltaTime;

            // compute total
            var total = (burstAmount + continuousAmount) * amountMultiplier + fraction;
            var totalAmount = Mathf.FloorToInt(total);
            var remainingFraction = total - totalAmount;

            // emit
            Emit(emitterVelocity, totalAmount);

            return remainingFraction;
        }

        private static float CalculateEmissionDuration(ParticleSystem.EmissionModule emission, float fullDuration)
        {
            var emissionRateOverTime = emission.rateOverTime;
            if (IsFullDuration(emissionRateOverTime))
            {
                return fullDuration;
            }

            var emissionDuration = 0f;
            for (int i = emission.burstCount - 1; i >= 0; i--)
            {
                var burst = emission.GetBurst(i);
                {
                    emissionDuration = Mathf.Max(emissionDuration, burst.time);
                }
            }
            return emissionDuration;
        }

        private static bool IsFullDuration(MinMaxCurve emissionRateOverTime)
        {
            switch (emissionRateOverTime.mode)
            {
                case ParticleSystemCurveMode.Constant:
                    return (emissionRateOverTime.constant > 0f);
                case ParticleSystemCurveMode.Curve:
                    return true;
            }
            return false;
        }
    }
}
