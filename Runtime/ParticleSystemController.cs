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
            public float durationMultiplier; // [1]
            public float lifetimeMultiplier; // [1]
            public float sizeMultiplier;  // [1]
            public float fraction;  // [1] fractional particle not emitted in previous frame
        }

        [SerializeField] private Transform root;
        [SerializeField] private float defaultDuration = 1f;
        [SerializeField] private float defaultSize = 1f;
        [SerializeField] private float defaultLifetime = 1f;

        private IVirtualParticleSystem[] particleSystems;

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
                if (time >= entry.endTime)
                {
                    schedule.RemoveUnorderedAt(i);
                }
            }
        }

        public void Initialize()
        {
            if (particleSystems == null)
            {
                particleSystems = VirtualParticleSystemUtils.GenerateControllers(root);
            }
        }

        public void Emit(Vector3 position, Quaternion rotation, Vector3 velocity, float duration, float lifetime, float size, float delay)
        {
            var durationMultiplier = duration / defaultDuration;
            var lifetimeMultiplier = lifetime / defaultLifetime;
            var sizeMultiplier = size / defaultSize;

            var time = Time.time;
            var deltaTime = Time.deltaTime;
            for (int i = 0; i < particleSystems.Length; i++)
            {
                var p = particleSystems[i];

                var startDelay = delay + p.GetStartDelay(durationMultiplier);
                var emissionDuration = p.GetEmissionDuration(durationMultiplier);
                var totalDuration = startDelay + emissionDuration;
                var entry = new ScheduleEntry
                {
                    index = i,
                    startTime = time + startDelay,
                    endTime = time + totalDuration,
                    position = position,
                    rotation = rotation,
                    velocity = velocity,
                    durationMultiplier = durationMultiplier,
                    lifetimeMultiplier = lifetimeMultiplier,
                    sizeMultiplier = sizeMultiplier,
                    fraction = 0f,
                };

                if (totalDuration > 0f)
                {
                    schedule.Add(entry);
                }

                if (startDelay <= 0f)
                {
                    Emit(entry, 0f, deltaTime);
                }
            }
        }

        private ScheduleEntry Emit(ScheduleEntry entry, float time, float deltaTime)
        {
            var ps = particleSystems[entry.index];
            entry.fraction = ps.Emit(entry.position, entry.rotation, entry.velocity, entry.durationMultiplier, entry.lifetimeMultiplier, entry.sizeMultiplier, time, deltaTime, entry.fraction);

            return entry;
        }
    }
}
