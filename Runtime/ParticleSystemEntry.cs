using UnityEngine;
using MinMaxCurve = UnityEngine.ParticleSystem.MinMaxCurve;

namespace Lumpn.Particles
{
    internal struct ParticleSystemEntry
    {
        private readonly ParticleSystem particleSystem;

        // main module
        private readonly float duration; // [s]
        private readonly float startDelay; // [s]
        private readonly MinMaxCurve startLifetime; // [s]
        private readonly MinMaxCurve startSpeed; // [m/s]
        private readonly MinMaxCurve startSizeX; // [m]
        private readonly MinMaxCurve startSizeY; // [m]
        private readonly MinMaxCurve startSizeZ; // [m]

        // emission module
        private readonly MinMaxCurve emissionRateOverTime;
        private readonly float emissionDuration; // derived [s]

        // shape module
        private readonly Vector3 shapePosition; // [m]
        private readonly Vector3 shapeScale; // [1]

        public ParticleSystemEntry(ParticleSystem p)
        {
            particleSystem = p;

            var main = p.main;
            duration = main.duration;
            startDelay = main.startDelay.constant;
            startLifetime = main.startLifetime;
            startSpeed = main.startSpeed;
            startSizeX = main.startSizeX;
            startSizeY = main.startSizeY;
            startSizeZ = main.startSizeZ;

            var emission = p.emission;
            emissionRateOverTime = emission.rateOverTime;
            emissionDuration = CalculateEmissionDuration(emission, duration);

            var shape = p.shape;
            shapePosition = shape.position;
            shapeScale = shape.scale;
        }

        public float GetStartDelay(float durationScale)
        {
            return startDelay;
        }

        public float GetEmissionDuration(float durationScale)
        {
            return (emissionDuration * durationScale);
        }

        private void Emit(Vector3 emitterVelocity, int count, float lifetimeScale, float sizeScale)
        {
            var main = particleSystem.main;

            main.emitterVelocity = emitterVelocity;
            main.startSpeed = Scale(startSpeed, sizeScale);
            main.startSizeX = Scale(startSizeX, sizeScale);
            main.startSizeY = Scale(startSizeY, sizeScale);
            main.startSizeZ = Scale(startSizeZ, sizeScale);
            main.startLifetime = Scale(startLifetime, lifetimeScale);

            var shape = particleSystem.shape;
            shape.position = shapePosition * sizeScale;
            shape.scale = shapeScale * sizeScale;

            particleSystem.Emit(count);
        }

        public float Emit(Vector3 emitterVelocity, float durationScale, float lifetimeScale, float sizeScale, float time, float deltaTime, float fraction)
        {
            var emission = particleSystem.emission;

            // accumulate bursts
            var prevTime = Mathf.Max(time - deltaTime, 0f);
            var burstAmount = 0;
            for (int i = emission.burstCount - 1; i >= 0; i--)
            {
                var burst = emission.GetBurst(i);
                if (burst.time >= prevTime && burst.time < time)
                {
                    // NOTE Jonas: we only support constant count, one cycle, 100% probability
                    burstAmount += (int)burst.count.constant;
                }
            }

            // accumulate continuous
            var normalizedTime = time / (duration * durationScale);
            var rate = Mathf.Max(emissionRateOverTime.Evaluate(normalizedTime), 0f);
            var continuousTotal = fraction + rate * deltaTime;
            var continuousAmount = Mathf.FloorToInt(continuousTotal);
            var remainingFraction = continuousTotal - continuousAmount;

            // emit
            var totalAmount = burstAmount + continuousAmount;
            Emit(emitterVelocity, totalAmount, lifetimeScale, sizeScale);

            return remainingFraction;
        }

        private static MinMaxCurve Scale(MinMaxCurve curve, float scale)
        {
            curve.constantMin *= scale;
            curve.constantMax *= scale;
            curve.curveMultiplier = scale;
            return curve;
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
