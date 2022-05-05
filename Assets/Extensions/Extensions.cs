using Unity.Mathematics;
using UnityEngine;

namespace Extensions
{
    public static class Vector3Extension
    {
        public static float2 ToFloat2(this Vector3 vector3)
            => new float2(vector3.x, vector3.y);

        public static float DistanceTo(this Vector3 vector1, Vector3 vector2)
            => Vector3.Distance(vector1, vector2);

        public static float Length(this Vector3 vector3)
            => Mathf.Sqrt(vector3.x * vector3.x + vector3.y * vector3.y + vector3.z * vector3.z);

        public static Vector2 ToVector2(this Vector3 vector3)
            => new Vector2(vector3.x, vector3.y);
    }

    public static class Vector2Extension
    {
        public static Vector3 ToVector3(this Vector2 vector2)
            => new Vector3(vector2.x, vector2.y);

    }

    public static class Float2Extension
    {
        public static Vector3 ToVector3(this float2 vector3)
            => new Vector3(vector3.x, vector3.y);

        public static Vector2 ToVector2(this float2 vector3)
            => new Vector2(vector3.x, vector3.y);
    }

    public static class Int2Extension
    {
        public static bool IsEqual(this int2 a, int2 b)
            => a.x == b.x && a.y == b.y;
    }
}

