using UnityEngine;

namespace Utils
{
    public static class MathUtils
    {
        public static int Modulo(int x, int m)
        {
            return (x % m + m) % m;
        }

        public static bool SphereIntersectsBox(Vector3 sphereCentre, float sphereRadius, Vector3 boxCentre,
            Vector3 boxSize)
        {
            float closestX = Mathf.Clamp(sphereCentre.x, boxCentre.x - boxSize.x / 2, boxCentre.x + boxSize.x / 2);
            float closestY = Mathf.Clamp(sphereCentre.y, boxCentre.y - boxSize.y / 2, boxCentre.y + boxSize.y / 2);
            float closestZ = Mathf.Clamp(sphereCentre.z, boxCentre.z - boxSize.z / 2, boxCentre.z + boxSize.z / 2);
            
            float dx = closestX - sphereCentre.x;
            float dy = closestY - sphereCentre.y;
            float dz = closestZ - sphereCentre.z;
            
            float sqrDistance = dx * dx + dy * dy + dz * dz;
            return sqrDistance < sphereRadius * sphereRadius;
        }
    }
}