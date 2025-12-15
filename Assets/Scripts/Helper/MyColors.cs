using UnityEngine;

namespace Helper
{
    public static class MyColors
    {
        public static Color RandomColor => Color.HSVToRGB(Random.value, 1, 1);
    }
}
