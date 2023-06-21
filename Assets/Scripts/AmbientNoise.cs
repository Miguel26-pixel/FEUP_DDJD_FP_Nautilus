using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using PlayerControls;
using UnityEngine;

public class AmbientNoise : MonoBehaviour
{
    private Player player;
    private Transform playerTransform;
    private Transform cameraTransform;
    
    public EventReference windReference;
    public EventReference waveReference;
    public EventReference shoreReference;
    
    private EventInstance shoreEvent;

    public float shoreLine = 21f;
    public float shoreRange = 5f;
    public float floorDistanceRay = 30f;
    
    public LayerMask floorLayerMask;
    private void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        playerTransform = player.transform;
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        
        InvokeRepeating(nameof(UpdateSounds), 0, 0.5f);
    }
    
    private void UpdateSounds()
    {
        shoreEvent.getPlaybackState(out var state);
        
        if (state == PLAYBACK_STATE.STOPPED)
        {
            shoreEvent = RuntimeManager.CreateInstance(shoreReference);
            shoreEvent.start();
        }
        
        float floorY = cameraTransform.position.y - floorDistanceRay;
        if (Physics.Raycast(cameraTransform.position + Vector3.up, Vector3.down, out var hit, 1 + floorDistanceRay,
                floorLayerMask))
        {
            floorY = hit.point.y;
        }

        float distanceToShore = Mathf.Abs(floorY - shoreLine);
        // 0 when below shore, 1 when on shore, 0 when above shore
        float shoreFactor = Mathf.Clamp01(1 - distanceToShore / shoreRange);

        shoreEvent.setParameterByName("Volume", shoreFactor);
    }
}
