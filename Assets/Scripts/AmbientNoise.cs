using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using PlayerControls;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class AmbientNoise : MonoBehaviour
{
    private Player _player;
    private Transform _playerTransform;
    private Transform _cameraTransform;
    
    public EventReference windReference = EventReference.Find("event:/Environment/Wind whooshing through leaves and grass");
    public EventReference waveReference = EventReference.Find("event:/Environment/Wave sound");
    public EventReference shoreReference = EventReference.Find("event:/Environment/Beach shore sound");
    public EventReference bubblingReference = EventReference.Find("event:/Environment/Underwater bubbling and rumbling");
    public EventReference emergeReference = EventReference.Find("event:/Player/emerge");
    public EventReference submergeReference = EventReference.Find("event:/Player/Submerge");
    public EventReference deepAtmosphere = EventReference.Find("event:/Environment/Scary atmosphere stings");
    public EventReference deepAmbiance = EventReference.Find("event:/Environment/Deep sounding, heavy feeling atmosphere. Scary");
    public EventReference deepGuttural = EventReference.Find("event:/Environment/Deep guttural sounds");
    
    private EventInstance _shoreEvent;
    private EventInstance _waveEvent;
    private EventInstance _windEvent;
    private EventInstance _bubblingEvent;
    private EventInstance _deepAtmosphereEvent;
    private EventInstance _deepAmbianceEvent;
    private EventInstance _deepGutturalEvent;

    private bool _above = true;

    public float shoreLine = 21f;
    public float deepLine = -14f;
    public float lowestDeepLine = -45f;
    public float shoreRange = 24f;
    public float windRange = 10f;
    public float waveRange = 5f;
    public float floorDistanceRay = 30f;
    
    public float ambianceChancePerSecond = 0.05f;
    public float gutturalsChancePerSecond = 0.05f;
    public float updateInterval = 0.1f;
    
    public LayerMask floorLayerMask;
    private void Start()
    {
        _player = GameObject.FindWithTag("Player").GetComponent<Player>();
        _playerTransform = _player.transform;
        if (Camera.main != null)
        {
            _cameraTransform = Camera.main.transform;
        }
        
        InvokeRepeating(nameof(UpdateSounds), 0, updateInterval);
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

        if (_cameraTransform.position.y > shoreLine + 1.2f)
        {
            UpdateAboveNoise();
        }
        else
        {
            UpdateBelowNoise();
        }
    }

    private void UpdateAboveNoise()
    {
        if (!_above)
        {
            RuntimeManager.CreateInstance(emergeReference).start();
            _above = true;
        }
        
        float floorY = _cameraTransform.position.y - floorDistanceRay;
        if (Physics.Raycast(_cameraTransform.position + Vector3.up, Vector3.down, out var hit, 1 + floorDistanceRay,
                floorLayerMask))
        {
            floorY = hit.point.y;
        }
        
        _bubblingEvent.getPlaybackState(out var bubblingState);
        if (bubblingState == PLAYBACK_STATE.PLAYING)
        {
            _bubblingEvent.stop(STOP_MODE.ALLOWFADEOUT);
        }
        _deepAmbianceEvent.getPlaybackState(out var deepAmbianceState);
        if (deepAmbianceState == PLAYBACK_STATE.PLAYING)
        {
            _deepAmbianceEvent.stop(STOP_MODE.ALLOWFADEOUT);
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
        _bubblingEvent.setParameterByName("Volume", 0f);
    }

    private void UpdateBelowNoise()
    {
        if (_above)
        {
            RuntimeManager.CreateInstance(submergeReference).start();
            _above = false;
        }
        
        _shoreEvent.setParameterByName("Volume", 0);
        _windEvent.setParameterByName("Volume", 0);
        _waveEvent.setParameterByName("Volume", 0);
        _bubblingEvent.setParameterByName("Volume", 1f);
        
        _bubblingEvent.getPlaybackState(out var bubblingState);
        if (bubblingState == PLAYBACK_STATE.STOPPED)
        {
            _bubblingEvent = RuntimeManager.CreateInstance(bubblingReference);
            _bubblingEvent.start();
        }
        
        _deepAmbianceEvent.getPlaybackState(out var deepAmbianceState);
        if (deepAmbianceState == PLAYBACK_STATE.STOPPED)
        {
            float rand = UnityEngine.Random.value;
            if (rand < ambianceChancePerSecond * updateInterval)
            {
                _deepAmbianceEvent = RuntimeManager.CreateInstance(deepAmbiance);
                _deepAmbianceEvent.start();
            }
        }
        
        _deepGutturalEvent.getPlaybackState(out var deepGutturalState);
        if (deepGutturalState == PLAYBACK_STATE.STOPPED)
        {
            float rand = UnityEngine.Random.value;
            if (rand < gutturalsChancePerSecond * updateInterval)
            {
                _deepGutturalEvent = RuntimeManager.CreateInstance(deepGuttural);
                _deepGutturalEvent.start();
            }
        }

        var position = _cameraTransform.position;
        float distanceToLowestDeep = position.y - lowestDeepLine;
        float lowestDeepFactor = Mathf.Clamp01(1 - distanceToLowestDeep / (shoreLine - lowestDeepLine));
        
        _deepAmbianceEvent.setParameterByName("Depth", lowestDeepFactor);
        
        float distanceToDeep = position.y - deepLine;
        float deepFactor = Mathf.Clamp01(1 - distanceToDeep / waveRange);
        
        _deepAtmosphereEvent.getPlaybackState(out var deepState);
        
        switch (deepState)
        {
            case PLAYBACK_STATE.STOPPED when deepFactor > 0.5f:
                _deepAtmosphereEvent = RuntimeManager.CreateInstance(deepAtmosphere);
                _deepAtmosphereEvent.start();
                break;
            case PLAYBACK_STATE.PLAYING when deepFactor < 0.5f:
                _deepAtmosphereEvent.stop(STOP_MODE.ALLOWFADEOUT);
                break;
            default:
                break;
        }
    }
}
