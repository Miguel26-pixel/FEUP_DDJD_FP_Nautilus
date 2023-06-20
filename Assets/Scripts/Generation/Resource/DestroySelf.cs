using UnityEngine;

namespace Generation.Resource
{
    public class DestroySelf : MonoBehaviour
    {
        public void SetTimer(float time)
        {
            Destroy(gameObject, time);
        }
    }
}