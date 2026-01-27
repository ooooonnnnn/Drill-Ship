using UnityEngine;

namespace Helper
{
    public static class Rigidbody2DExt
    {
        /// <summary>
        /// Makes two rigidbodies interact with a force. Each of them gets the same force in opposite dircetions
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
