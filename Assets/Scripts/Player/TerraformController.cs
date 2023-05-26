using System;
using System.Collections.Generic;
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
        private TerraformType _terraformType = TerraformType.Raise;

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
            if (!canTerraform)
            {
                return;
            }

            RaycastHit hit;
            HashSet<Vector3Int> alteredChunks = new HashSet<Vector3Int>();

            if (Physics.Raycast(_camera.position, _camera.forward, out hit, 200, layerMask))
            {
                terraformCursor.SetActive(true);
                terraformCursor.transform.position = hit.point;
                terraformCursor.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                
                float distance = Vector3.Distance(_camera.position, hit.point);
                
                if (distance > maxDistance)
                {
                    // TODO: Add a way to show that the player can't terraform here
                    return;
                }
                
                HashSet<Vector3Int> newChunks = Terraform(hit.point);
                
                foreach (var newChunk in newChunks)
                {
                    alteredChunks.Add(newChunk);
                }
            }
            else
            {
                terraformCursor.SetActive(false);
            }

            foreach (var alteredChunk in alteredChunks)
            {
                _meshGenerator.RegenerateChunk(alteredChunk);
            }
        }

        private HashSet<Vector3Int> Terraform(Vector3 position)
        {
            switch (_terraformType)
            {
                case TerraformType.Lower:
                    return _meshGenerator.Terraform(position, -power, radius);
                case TerraformType.Raise:
                    return _meshGenerator.Terraform(position, power, radius);
                case TerraformType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new HashSet<Vector3Int>();
        }
        
        public void SetTerraformType(TerraformType terraformType)
        {
            _terraformType = terraformType;
        }

        public void ToggleTerraform()
        {
            if(canTerraform) DeactivateTerraform();
            else ActivateTerraform();
        }
        
        
        public void ActivateTerraform()
        {
            Debug.Log("Activate");
            canTerraform = true;
            _terraformType = TerraformType.None;
        }
        
        public void DeactivateTerraform()
        {
            Debug.Log("Deactivate");
            canTerraform = false;
            terraformCursor.SetActive(false);
        }
    }

    public enum TerraformType
    {
        None,
        Raise,
        Lower
    }
}