using System;
using System.Collections.Generic;
using Generation.Resource;
using UnityEngine;

namespace Player
{
    public class TerraformController : MonoBehaviour
    {
        public LayerMask layerMask;
        public float radius;
        public float power;
        public float succtionForce;
        public float maxDistance;
        public float dropImpulse = 200;
        public bool canTerraform = true;
        public GameObject terraformCursorPrefab;
        public GameObject vacuumArea;
        public GameObject vacuumCollection;

        private Transform _camera;
        private MeshGenerator _meshGenerator;
        private GameObject terraformCursor;
        private TerraformType _terraformType = TerraformType.Raise;
        private List<GameObject> _resourcesInRange = new List<GameObject>();
        private Player _player;

        private void Start()
        {
            if (Camera.main != null)
            {
                _meshGenerator = GameObject.Find("GenerationManager").GetComponent<MeshGenerator>();
                _camera = Camera.main.transform;
            }
            
            terraformCursor = Instantiate(terraformCursorPrefab, Vector3.zero, Quaternion.identity);
            terraformCursor.SetActive(false);
            _player = transform.parent.GetComponent<Player>();
        }

        private void Update()
        {
            if (!canTerraform)
            {
                return;
            }

            RaycastHit hit;
            HashSet<Vector3Int> alteredChunks = new HashSet<Vector3Int>();

            int numIterations = 5;

            if (_terraformType == TerraformType.Lower)
            {
                foreach (var resource in _resourcesInRange)
                {
                    Vector3 direction = (vacuumCollection.transform.position - resource.transform.position);
                    float dst = Mathf.Max(direction.sqrMagnitude, 1f);
                    dst = Mathf.Lerp(0.9f, maxDistance, dst / (maxDistance * maxDistance));
                    float force = succtionForce / (dst);

                    if (!resource.TryGetComponent(out Rigidbody rb))
                    {
                        continue;
                    }
                    
                    rb.AddForce(direction.normalized * (force * Time.deltaTime * 60));
                    rb.AddForce(-Physics.gravity * Time.deltaTime, ForceMode.Impulse);
                }
            }


            for (int i = 0; i < numIterations; i++)
            {
                float rayRadius = Mathf.Lerp(0.01f, 1f, i / (numIterations - 1f));

                if (Physics.SphereCast(_camera.position, rayRadius, _camera.forward, out hit, 200, layerMask))
                {
                    terraformCursor.SetActive(true);
                    terraformCursor.transform.position = hit.point;
                    terraformCursor.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

                    float distance = Vector3.Distance(_camera.position, hit.point);

                    if (distance > maxDistance)
                    {
                        // TODO: Add a way to show that the player can't terraform here
                        continue;
                    }

                    alteredChunks = Terraform(hit.point);
                }
                else
                {
                    terraformCursor.SetActive(false);
                    continue;
                }

                foreach (var alteredChunk in alteredChunks)
                {
                    _meshGenerator.RegenerateChunk(alteredChunk);
                }
                break;
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


        private void ActivateTerraform()
        {
            canTerraform = true;
            _terraformType = TerraformType.None;
        }

        private void DeactivateTerraform()
        {
            canTerraform = false;
            terraformCursor.SetActive(false);
        }

        public void OnPartTriggerEnter(GameObject child, GameObject other)
        {
            if (child == vacuumArea)
            {
                _resourcesInRange.Add(other);
            }
            
            if (child == vacuumCollection)
            {
                if (_terraformType == TerraformType.Lower && other.transform.parent.TryGetComponent(out Resource resource))
                {
                    bool added = _player.CollectResource(resource);

                    if (added)
                    {
                        _resourcesInRange.Remove(other);
                    }
                    else
                    {
                        Rigidbody rb = other.GetComponent<Rigidbody>();
                        rb.AddForce(vacuumCollection.transform.forward * dropImpulse, ForceMode.Impulse);
                    }
                }
            }
        }
        
        public void OnPartTriggerExit(GameObject child, GameObject other)
        {
            if (child == vacuumArea)
            {
                _resourcesInRange.Remove(other);
            }
        }
    }

    public enum TerraformType
    {
        None,
        Raise,
        Lower
    }
}