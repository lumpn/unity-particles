using System.Collections.Generic;
using UnityEngine;

namespace Lumpn.Particles
{
    public sealed class ParticleSystemController : MonoBehaviour
    {
        private struct ScheduleEntry
        {
            public int index; // into particleSystems
            public float startTime, endTime; // [s]
            public Vector3 position; // [m]
            public Quaternion rotation;
            public Vector3 velocity; // [m/s]
            public float durationScale; // [1]
            public float lifetimeScale; // [1]
            public float sizeScale;  // [1]
            public float fraction;  // fractional particle not emitted in previous frame [1]
        }

        [SerializeField] private Transform root;
        [SerializeField] private float defaultDuration = 1f;
        [SerializeField] private float defaultSize = 1f;
        [SerializeField] private float defaultLifetime = 1f;

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
                    var localTime = Mathf.Max(time - entry.startTime, 0f);
                    schedule[i] = Emit(entry, localTime, deltaTime);
                }
                if (time > entry.endTime)
                {
                    schedule.RemoveUnorderedAt(i);
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

        public void Emit(Vector3 position, Quaternion rotation, Vector3 velocity = default, float duration = 1f, float lifetime = 1f, float size = 1f, float delay = 0f)
        {
            var durationScale = duration / defaultDuration;
            var lifetimeScale = lifetime / defaultLifetime;
            var sizeScale = size / defaultSize;

            var time = Time.time;
            for (int i = 0; i < particleSystems.Length; i++)
            {
                var p = particleSystems[i];

                var startDelay = delay + p.GetStartDelay(durationScale);
                var emissionDuration = p.GetEmissionDuration(durationScale);
                var totalDuration = startDelay + emissionDuration;
                var entry = new ScheduleEntry
                {
                    index = i,
                    startTime = time + startDelay,
                    endTime = time + totalDuration,
                    position = position,
                    rotation = rotation,
                    velocity = velocity,
                    durationScale = durationScale,
                    lifetimeScale = lifetimeScale,
                    sizeScale = sizeScale,
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

        private ScheduleEntry Emit(ScheduleEntry entry, float time, float deltaTime)
        {
            root.SetLocalPositionAndRotation(entry.position, entry.rotation);

            var ps = particleSystems[entry.index];
            entry.fraction = ps.Emit(entry.velocity, entry.durationScale, entry.lifetimeScale, entry.sizeScale, time, deltaTime, entry.fraction);

            return entry;
        }
    }
}
