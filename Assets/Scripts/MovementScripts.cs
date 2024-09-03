using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class MovementScripts : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float rotationSpeed = 50f;
    public GameObject pannelloAvanti;
    
    public bool meshReady = false, textureReady = false;
    
    private Transform target;
    private Transform originalTransform;
    private Quaternion initialRotation;
    
    private Renderer _renderer;

    private enum State { Moving, RotatingClockwise, RotatingCounterclockwise, RotatingToInitial, Idle }

    public List<GameObject> checkpoints = new List<GameObject>();
    private int currentCheckpointIndex = 0;
    
    private State currentState = State.Idle;
    private float rotationProgress = 0f;
    
    private Random Randomizer = new Random();

    void Start()
    {
        GameObject checkpointB = GameObject.Find("CheckpointB");
        GameObject checkpointC = GameObject.Find("CheckpointC");
        GameObject checkpointD = GameObject.Find("CheckpointD");
        
        
        if (checkpointB != null)
        {
            target = checkpointB.transform;
            initialRotation = transform.rotation;
        }
        else
        {
            Debug.LogError("Checkpoint B non trovato. Assicurati che l'oggetto esista nella scena e sia chiamato 'checkpoint B'.");
        }
        
        var checkpointA = GameObject.Find("CheckpointA");
        if (checkpointA != null)
        {
            transform.position = checkpointA.transform.position;
            transform.rotation = checkpointA.transform.rotation;
            originalTransform = checkpointA.transform;
        }
        else
        {
            Debug.LogError("Checkpoint A non trovato. Assicurati che l'oggetto esista nella scena e sia chiamato 'checkpoint A'.");
        }
        
        _renderer = GetComponentInChildren<Renderer>();
    }

    void Update()
    {
        if (meshReady && textureReady)
        {
            
            _renderer.enabled = true;
            switch (currentState)
            
            {
                case State.Moving:
                    MoveTowards(target.position, State.RotatingClockwise);
                    break;
                case State.RotatingClockwise:
                    RotateIncrementally(360f, State.RotatingCounterclockwise, true);
                    break;
                case State.RotatingCounterclockwise:
                    RotateIncrementally(360f, State.RotatingToInitial, false);
                    break;
                case State.RotatingToInitial:
                    RotateTowards(initialRotation, State.Idle);
                    
                    break;
            }
        }
        else
        {
            _renderer.enabled = false;
        }
    }

    private void MoveTowards(Vector3 targetPosition, State nextState)
    {
        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

        if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
        {
            currentState = nextState;
        }
        
        
    }

    private void RotateIncrementally(float totalDegrees, State nextState, bool clockwise)
    {
        float step = rotationSpeed * Time.deltaTime;
        float rotationStep = clockwise ? step : -step;
        transform.Rotate(0, rotationStep, 0);
        rotationProgress += Mathf.Abs(rotationStep);

        if (rotationProgress >= totalDegrees)
        {
            rotationProgress = 0f;
            currentState = nextState;
            if (nextState == State.Idle)
            {
                transform.position = originalTransform.position;
                transform.rotation = originalTransform.rotation;
                
            }
        }
    }

    private void RotateTowards(Quaternion targetRotation, State nextState)
    {
        float step = rotationSpeed * Time.deltaTime;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, step);

        if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
        {
            currentState = nextState;
            if (nextState == State.Idle)
            {
                /*currentCheckpointIndex++;
                if (currentCheckpointIndex < checkpoints.Count)
                {
                    target = checkpoints[currentCheckpointIndex].transform;
                    currentState = State.Moving;
                }
                else
                {
                    transform.position = originalTransform.position;
                    transform.rotation = originalTransform.rotation;
                    currentCheckpointIndex = 0;
                    target = checkpoints[currentCheckpointIndex].transform;
                }*/
                
                currentCheckpointIndex = Randomizer.Next(0, checkpoints.Count);
                target = checkpoints[currentCheckpointIndex].transform;
                transform.position = originalTransform.position;
                transform.rotation = originalTransform.rotation;
                pannelloAvanti.SetActive(true);

                
            }
        }
    }

    public void StartMoving()
    {
        currentState = State.Moving;
    }
}
