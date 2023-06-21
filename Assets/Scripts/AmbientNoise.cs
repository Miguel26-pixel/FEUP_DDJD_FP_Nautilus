using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using PlayerControls;
using UnityEngine;

public class AmbientNoise : MonoBehaviour
{
    private Player _player;
    private Transform _playerTransform;
    private Transform _cameraTransform;
    
    public EventReference windReference;
    public EventReference waveReference;
    public EventReference shoreReference;
    
    private EventInstance _shoreEvent;
    private EventInstance _waveEvent;
    private EventInstance _windEvent;

    public float shoreLine = 21f;
    public float shoreRange = 5f;
    public float windRange = 10f;
    public float waveRange = 5f;
    public float floorDistanceRay = 30f;
    
    public LayerMask floorLayerMask;
    private void Start()
    {
        _player = GameObject.FindWithTag("Player").GetComponent<Player>();
        _playerTransform = _player.transform;
        if (Camera.main != null)
        {
            _cameraTransform = Camera.main.transform;
        }
        
        InvokeRepeating(nameof(UpdateSounds), 0, 0.1f);
    }
    
    private void UpdateSounds()
    {
        _shoreEvent.getPlaybackState(out var state);
        _windEvent.getPlaybackState(out var windState);
        _waveEvent.getPlaybackState(out var waveState);
        
        if (state == PLAYBACK_STATE.STOPPED)
        {
            _shoreEvent = RuntimeManager.CreateInstance(shoreReference);
            _shoreEvent.start();
        }
        
        if (windState == PLAYBACK_STATE.STOPPED)
        {
            _windEvent = RuntimeManager.CreateInstance(windReference);
            _windEvent.start();
        }
        
        if (waveState == PLAYBACK_STATE.STOPPED)
        {
            _waveEvent = RuntimeManager.CreateInstance(waveReference);
            _waveEvent.start();
        }
        
        float floorY = _cameraTransform.position.y - floorDistanceRay;
        if (Physics.Raycast(_cameraTransform.position + Vector3.up, Vector3.down, out var hit, 1 + floorDistanceRay,
                floorLayerMask))
        {
            floorY = hit.point.y;
        }

        float distanceToShore = floorY - shoreLine;
        float absDistanceToShore = Mathf.Abs(distanceToShore);
        // 0 when below shore, 1 when on shore, 0 when above shore
        float shoreFactor = Mathf.Clamp01(1 - absDistanceToShore / shoreRange);
        float waveFactor = Mathf.Clamp01(1 - distanceToShore / waveRange);
        float windFactor = Mathf.Clamp01(distanceToShore / windRange);

        _shoreEvent.setParameterByName("Volume", shoreFactor);
        _windEvent.setParameterByName("Volume", windFactor);
        _waveEvent.setParameterByName("Volume", waveFactor);
    }
}
