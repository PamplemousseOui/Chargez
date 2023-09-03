using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WallSound : MonoBehaviour
{
    public WallController wallController;
    public SoundEmitter emitter;
    public GameObject emitterObject;
    public EventReference deathEvent;
    public EventReference moveEvent;
    public ParamRef distanceParam;
    public ParamRef dopplerParam;

    private float previousDist;

    private void OnEnable()
    {
        wallController.OnWallDestroyed += OnWallDestroyed;
    }

    private void OnDisable()
    {
        wallController.OnWallDestroyed -= OnWallDestroyed;
    }

    private void Update()
    {
        if (emitterObject != null)
        {
            emitterObject.transform.position = Vector3.MoveTowards(emitterObject.transform.position, GameManager.player.transform.position, 100);
            emitterObject.transform.localPosition = new Vector3(emitterObject.transform.localPosition.x, 0f, emitterObject.transform.localPosition.z);
        }
    }

    private void FixedUpdate()
    {
        float distance = (GameManager.player.transform.position - emitterObject.transform.position).magnitude;
        float relativeSpeed = (distance - previousDist) / Time.deltaTime;
        Debug.Log("Distance = " + distance);
        Debug.Log("Relative speed = " + relativeSpeed);
        emitter.SetParameter(distanceParam, distance);
        emitter.SetParameter(dopplerParam, relativeSpeed);

        previousDist = distance;
    }

    private void OnWallDestroyed()
    {
        emitter.PlayOneShot(deathEvent);
        emitter.Stop(moveEvent);
    }

    private void Start()
    {
        emitter.Play(moveEvent);

        float distance = (GameManager.player.transform.position - emitterObject.transform.position).magnitude;

        emitter.SetParameter(distanceParam, distance);

        previousDist = distance;
    }
}
