using System.Collections.Generic;
using UnityEngine;

namespace Lumpn.Particles
{
    public sealed class ParticleSystemController : MonoBehaviour
    {
        private struct ParticleSystemEntry
        {
            private readonly ParticleSystem particleSystem;
            private readonly float startDelay;
            private readonly float emitDuration;

            private readonly ParticleSystem.MinMaxCurve startSpeed;
            private readonly ParticleSystem.MinMaxCurve startSizeX;
            private readonly ParticleSystem.MinMaxCurve startSizeY;
            private readonly ParticleSystem.MinMaxCurve startSizeZ;

            private readonly float duration;
            private readonly ParticleSystem.MinMaxCurve rateOverTime;

            private readonly Vector3 shapePosition;
            private readonly Vector3 shapeScale;

            public ParticleSystemEntry(ParticleSystem p)
            {
                particleSystem = p;

                var main = p.main;
                duration = main.duration;
                startDelay = main.startDelay.constant;
                startSpeed = main.startSpeed;
                startSizeX = main.startSizeX;
                startSizeY = main.startSizeY;
                startSizeZ = main.startSizeZ;

                var emission = p.emission;
                rateOverTime = emission.rateOverTime;

                var shape = p.shape;
                shapePosition = shape.position;
                shapeScale = shape.scale;

                emitDuration = 0f;
                bool fullDuration = false;
                switch (rateOverTime.mode)
                {
                    case ParticleSystemCurveMode.Constant:
                        fullDuration = (rateOverTime.constant > 0f);
                        break;
                    case ParticleSystemCurveMode.Curve:
                        fullDuration = true;
                        break;
                }

                if (fullDuration)
                {
                    emitDuration = duration;
                }
                else
                {
                    for (int i = emission.burstCount - 1; i >= 0; i--)
                    {
                        var burst = emission.GetBurst(i);
                        {
                            emitDuration = Mathf.Max(emitDuration, burst.time);
                        }
                    }
                }
            }

            public float GetStartDelay(float durationScale)
            {
                return startDelay;
            }

            public float GetEmitDuration(float durationScale)
            {
                return (emitDuration * durationScale);
            }

            private void Emit(float scale, int count)
            {
                var main = particleSystem.main;
                main.startSpeed = Scale(startSpeed, scale);
                main.startSizeX = Scale(startSizeX, scale);
                main.startSizeY = Scale(startSizeY, scale);
                main.startSizeZ = Scale(startSizeZ, scale);

                var shape = particleSystem.shape;
                shape.position = shapePosition * scale;
                shape.scale = shapeScale * scale;

                particleSystem.Emit(count);
            }

            public float Emit(float sizeScale, float durationScale, float time, float deltaTime, float fraction)
            {
                var emission = particleSystem.emission;

                // accumulate bursts
                var prevTime = Mathf.Max(time - deltaTime, 0f);
                var burstAmount = 0;
                for (int i = emission.burstCount - 1; i >= 0; i--)
                {
                    var burst = emission.GetBurst(i);
                    if (burst.time >= prevTime && burst.time <= time)
                    {
                        // NOTE Jonas: we only support constant count, one cycle, 100% probability
                        burstAmount += (int)burst.count.constant;
                    }
                }

                // accumulate continuous
                var normalizedTime = time / (duration * durationScale);
                var rate = Mathf.Max(rateOverTime.Evaluate(normalizedTime), 0f);
                var continuousTotal = fraction + rate * deltaTime;
                var continuousAmount = (int)continuousTotal;
                var remainingFraction = continuousTotal - continuousAmount;

                // TODO Jonas: spread continuous particles across deltaTime?
                Emit(sizeScale, burstAmount + continuousAmount);

                return remainingFraction;
            }

            private static ParticleSystem.MinMaxCurve Scale(ParticleSystem.MinMaxCurve curve, float scale)
            {
                curve.constantMin *= scale;
                curve.constantMax *= scale;
                curve.curveMultiplier = scale;
                return curve;
            }
        }

        private struct ScheduleEntry
        {
            public int index;
            public float startTime, endTime;
            public Vector3 position;
            public Quaternion rotation;
            public float sizeScale;
            public float durationScale;
            public float fraction;
        }

        [SerializeField] private Transform root;
        [SerializeField] private float defaultSize = 1f;
        [SerializeField] private float defaultDuration = 1f;

        private ParticleSystemEntry[] particleSystems;

        private readonly List<ScheduleEntry> schedule = new List<ScheduleEntry>();

        protected void Start()
        {
            Initialize();
        }

        protected void Update()
        {
            var time = Time.time;
            var deltaTime = Time.deltaTime;

            for (int i = schedule.Count - 1; i >= 0; i--)
            {
                var entry = schedule[i];
                if (time >= entry.startTime)
                {
                    var relativeTime = Mathf.Max(time - entry.startTime, 0f);
                    schedule[i] = Emit(entry, relativeTime, deltaTime);

                    if (time >= entry.endTime)
                    {
                        schedule.RemoveUnorderedAt(i);
                    }
                }
            }
        }

        public void Initialize()
        {
            if (particleSystems == null)
            {
                var ps = root.GetComponentsInChildren<ParticleSystem>(false);
                particleSystems = new ParticleSystemEntry[ps.Length];

                for (int i = 0; i < particleSystems.Length; i++)
                {
                    particleSystems[i] = new ParticleSystemEntry(ps[i]);
                }
            }
        }

        public void Emit(Vector3 position, Quaternion rotation, float size = 1f, float duration = 1f, float delay = 0f)
        {
            var sizeScale = size / defaultSize;
            var durationScale = duration / defaultDuration;

            var time = Time.time;
            for (int i = 0; i < particleSystems.Length; i++)
            {
                var p = particleSystems[i];

                var startDelay = delay + p.GetStartDelay(durationScale);
                var emitDuration = p.GetEmitDuration(durationScale);
                var totalDuration = startDelay + emitDuration;

                var entry = new ScheduleEntry
                {
                    index = i,
                    startTime = time + startDelay,
                    endTime = time + totalDuration,
                    position = position,
                    rotation = rotation,
                    sizeScale = sizeScale,
                    durationScale = durationScale,
                    fraction = 0f,
                };

                if (totalDuration > 0f)
                {
                    schedule.Add(entry);
                }

                if (startDelay <= 0f)
                {
                    Emit(entry, 0f, 0f);
                }
            }
        }

        private ScheduleEntry Emit(ScheduleEntry entry, float relativeTime, float deltaTime)
        {
            root.SetLocalPositionAndRotation(entry.position, entry.rotation);

            var ps = particleSystems[entry.index];
            entry.fraction = ps.Emit(entry.sizeScale, entry.durationScale, relativeTime, deltaTime, entry.fraction);

            return entry;
        }
    }
}
