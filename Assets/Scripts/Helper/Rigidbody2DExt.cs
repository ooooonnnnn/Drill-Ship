using UnityEngine;

namespace Helper
{
    public static class Rigidbody2DExt
    {
        /// <summary>
        /// Add a force to the other rigidbody and an opposite force to this rigidbody.
        /// </summary>
        /// <param name="rb"></param>
        /// <param name="other"></param>
        /// <param name="force"></param>
        /// <param name="mode"></param>
        public static void AddForceEqualReaction(this Rigidbody2D rb, Rigidbody2D other, Vector2 force, ForceMode2D mode)
        {
            other.AddForce(force, mode);
            rb.AddForce(-force, mode);
        }
    }
}
