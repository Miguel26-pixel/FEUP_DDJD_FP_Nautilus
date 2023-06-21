using System;
using System.Collections.Generic;
using Generation.Resource;
using UnityEngine;

namespace PlayerControls
{
    public class TerraformController : MonoBehaviour
    {
        public LayerMask layerMask;
        public float radius;
        public float power;
        public float suctionForce;
        public float maxDistance;
        public float dropImpulse = 200;
        public bool canTerraform = true;
        public GameObject terraformCursorPrefab;
        public GameObject vacuumArea;
        public GameObject vacuumCollection;
        private readonly List<GameObject> _resourcesInRange = new();

        private Transform _camera;
        private MeshGenerator _meshGenerator;
        private Player _player;
        private GameObject _terraformCursor;
        private TerraformType _terraformType = TerraformType.Raise;

        private void Start()
        {
            if (Camera.main != null)
            {
                _meshGenerator = GameObject.Find("GenerationManager").GetComponent<MeshGenerator>();
                _camera = Camera.main.transform;
            }

            _terraformCursor = Instantiate(terraformCursorPrefab, Vector3.zero, Quaternion.identity);
            _terraformCursor.SetActive(false);
            _player = transform.parent.GetComponent<Player>();
        }

        private void LateUpdate()
        {
            var rotation = _camera.rotation;
            var playerRotation = _player.transform.rotation;
            vacuumArea.transform.localRotation = Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y - playerRotation.eulerAngles.y, 0);
            vacuumCollection.transform.localRotation = Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y - playerRotation.eulerAngles.y, 0);
        }

        private void Update()
        {
            if (!canTerraform)
            {
                return;
            }

            const int numIterations = 5;

            if (_terraformType == TerraformType.Lower)
            {
                List<GameObject> destroyedResources = new();

                foreach (GameObject resource in _resourcesInRange)
                {
                    if (resource == null)
                    {
                        destroyedResources.Add(resource);
                        continue;
                    }

                    Vector3 direction = vacuumCollection.transform.position - resource.transform.position;
                    float dst = direction.sqrMagnitude;
                    dst = Mathf.Lerp(1f, 5f, dst / (maxDistance * maxDistance));
                    float force = suctionForce / dst;

                    if (!resource.TryGetComponent(out Rigidbody rb))
                    {
                        continue;
                    }


                    rb.AddForce(direction.normalized * (force * Time.deltaTime * 60));
                    rb.AddForce(-Physics.gravity * Time.deltaTime, ForceMode.Impulse);
                }

                foreach (GameObject destroyedResource in destroyedResources)
                {
                    _resourcesInRange.Remove(destroyedResource);
                }
            }


            for (int i = 0; i < numIterations; i++)
            {
                float rayRadius = Mathf.Lerp(0.01f, 1f, i / (numIterations - 1f));

                HashSet<Vector3Int> alteredChunks;
                if (Physics.SphereCast(vacuumCollection.transform.position, rayRadius, _camera.forward, out RaycastHit hit, 40,
                        layerMask))
                {
                    _terraformCursor.SetActive(true);
                    _terraformCursor.transform.position = hit.point;
                    _terraformCursor.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

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
                    _terraformCursor.SetActive(false);
                    continue;
                }

                foreach (Vector3Int alteredChunk in alteredChunks)
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
                    var altered = _meshGenerator.Terraform(position, -power, radius);
                    _player.CollectSoil(radius * power / 4 * Time.deltaTime);
                    return altered;
                case TerraformType.Raise:
                    var removed = _player.RemoveSoil(radius * power / 4 * Time.deltaTime);
                    
                    return removed ? _meshGenerator.Terraform(position, power, radius) : new HashSet<Vector3Int>();
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
            if (canTerraform)
            {
                DeactivateTerraform();
            }
            else
            {
                ActivateTerraform();
            }
        }

        private void ActivateTerraform()
        {
            canTerraform = true;
            _terraformType = TerraformType.None;
        }

        public void DeactivateTerraform()
        {
            canTerraform = false;
            _terraformCursor.SetActive(false);
        }

        public void OnPartTriggerEnter(GameObject child, GameObject other)
        {
            if(other.CompareTag("Weapon"))
                return;

            if (child == vacuumArea)
            {
                if (other.transform.parent.TryGetComponent(out Resource resource) && resource.dropped)
                {
                    _resourcesInRange.Add(other);
                }
            }

            if (child == vacuumCollection)
            {
                if (_terraformType == TerraformType.Lower &&
                    other.transform.parent.TryGetComponent(out Resource resource) && resource.dropped)
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