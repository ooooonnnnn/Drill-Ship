using UnityEditor;
using UnityEngine;

namespace Helper
{
    public static class AnimationCurveExt
    {
        public static float CentralDifDeltaTime = 0.01f;
        /// <summary>
        /// Evaluate the derivative of the curve at time.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static float EvaluateDerivative(this AnimationCurve curve, float time)
        {
            //TODO: Use direct derivative
            return (curve.Evaluate(time + CentralDifDeltaTime) - curve.Evaluate(time - CentralDifDeltaTime)) /
                   (2 * CentralDifDeltaTime);
        }
    }
}
