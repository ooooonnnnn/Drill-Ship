using UnityEngine;

namespace Helper
{
    public static class VectorUtils
    {
        public static Vector2Int RoundToInt(this Vector2 vector)
        {
            return new Vector2Int((int)vector.x, (int)vector.y);
        }
    }
}
