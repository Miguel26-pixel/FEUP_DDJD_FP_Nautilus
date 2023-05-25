using System;
using UnityEngine;

namespace Player
{
    public class TerraformController : MonoBehaviour
    {
        public LayerMask layerMask;
        public float radius;
        public float power;
        public float maxDistance;
        public bool canTerraform = true;
        public GameObject terraformCursorPrefab;

        private Transform _camera;
        private MeshGenerator _meshGenerator;
        private GameObject terraformCursor;

        private void Start()
        {
            if (Camera.main != null)
            {
                _meshGenerator = GameObject.Find("GenerationManager").GetComponent<MeshGenerator>();
                _camera = Camera.main.transform;
            }
            
            terraformCursor = Instantiate(terraformCursorPrefab, Vector3.zero, Quaternion.identity);
            terraformCursor.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            if (canTerraform)
            {
                RaycastHit hit;

                if (Physics.Raycast(_camera.position, _camera.forward, out hit, maxDistance, layerMask))
                {
                    terraformCursor.SetActive(true);
                    terraformCursor.transform.position = hit.point;
                    terraformCursor.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                }
                else
                {
                    terraformCursor.SetActive(false);
                }
            }
        }
        
        public void ActivateTerraform()
        {
            Debug.Log("Activate");
            canTerraform = true;
        }
        
        public void DeactivateTerraform()
        {
            Debug.Log("Deactivate");
            canTerraform = false;
            terraformCursor.SetActive(false);
        }
    }
}