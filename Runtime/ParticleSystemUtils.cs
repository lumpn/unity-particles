using static UnityEngine.ParticleSystem;

namespace Lumpn.Particles
{
    public static class ParticleSystemUtils
    {
        public static MinMaxCurve Scale(MinMaxCurve curve, float scale)
        {
            curve.constantMin *= scale;
            curve.constantMax *= scale;
            curve.curveMultiplier = scale;
            return curve;
        }
    }
}
