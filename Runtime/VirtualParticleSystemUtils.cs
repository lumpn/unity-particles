using System.Collections.Generic;
using UnityEngine;

namespace Lumpn.Particles
{
    public static class VirtualParticleSystemUtils
    {
        private const string emitMoreTag = "EmitMore";

        public static IVirtualParticleSystem[] GenerateControllers(Transform root)
        {
            var ps = new List<ParticleSystem>();
            root.GetComponentsInChildren(false, ps);
            RemoveSubEmitters(ps);

            var particleSystems = new IVirtualParticleSystem[ps.Count];
            for (int i = 0; i < particleSystems.Length; i++)
            {
                var particleSystem = GetOrCreateVirtualParticleSystem(ps[i], root);
                particleSystem.Initialize();

                particleSystems[i] = particleSystem;
            }
            return particleSystems;
        }

        private static IVirtualParticleSystem GetOrCreateVirtualParticleSystem(ParticleSystem ps, Transform root)
        {
            if (ps.TryGetComponent<IVirtualParticleSystem>(out var vps))
            {
                return vps;
            }

            if (ps.CompareTag(emitMoreTag))
            {
                return new EmitMoreParticleSystem(ps, root);
            }

            return new VirtualParticleSystem(ps, root);
        }

        private static void RemoveSubEmitters(List<ParticleSystem> ps)
        {
            for (int i = ps.Count - 1; i >= 0; i--)
            {
                if (IsSubEmitter(ps[i], ps))
                {
                    ps.RemoveUnorderedAt(i);
                }
            }
        }

        private static bool IsSubEmitter(ParticleSystem ps, List<ParticleSystem> list)
        {
            foreach (var entry in list)
            {
                var subEmitters = entry.subEmitters;
                if (subEmitters.enabled)
                {
                    for (int i = 0; i < subEmitters.subEmittersCount; i++)
                    {
                        if (ps == subEmitters.GetSubEmitterSystem(i))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
