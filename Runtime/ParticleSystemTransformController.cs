using UnityEngine;

namespace Lumpn.Particles
{
    public struct ParticleSystemTransformController
    {
        private readonly Transform root;

        public ParticleSystemTransformController(Transform root)
        {
            this.root = root;
        }

        public void Apply(Vector3 position, Quaternion rotation)
        {
            root.SetLocalPositionAndRotation(position, rotation);
        }
    }
}
