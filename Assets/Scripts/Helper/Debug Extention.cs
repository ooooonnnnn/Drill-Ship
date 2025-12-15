using UnityEngine;

namespace Helper
{
    public static class MyDebug
    {
        public static void DrawX(Vector3 position, float size)
        {
            DrawX( position, size, Color.white);
        }
    
        public static void DrawX(Vector3 position, float size, Color color, float duration = 0.0f, bool depthTest = true)
        {
            Debug.DrawLine(position + size * Vector3.left, position + size * Vector3.right, color, duration, depthTest);
            Debug.DrawLine(position + size * Vector3.down, position + size * Vector3.up, color, duration, depthTest);
            Debug.DrawLine(position + size * Vector3.back, position + size * Vector3.forward, color, duration, depthTest);
        }
    }
}
