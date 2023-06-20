using UnityEngine;

namespace PlayerControls
{
    public class VacuumPart : MonoBehaviour
    {
        private TerraformController _terraformController;

        public void Start()
        {
            _terraformController = transform.parent.GetComponent<TerraformController>();
        }

        public void OnTriggerEnter(Collider other)
        {
            _terraformController.OnPartTriggerEnter(gameObject, other.gameObject);
        }

        public void OnTriggerExit(Collider other)
        {
            _terraformController.OnPartTriggerExit(gameObject, other.gameObject);
        }
    }
}