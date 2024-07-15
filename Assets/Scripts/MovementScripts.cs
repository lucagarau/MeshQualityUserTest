
using System;
using UnityEngine;

public class MovementScripts : MonoBehaviour
{
     // Velocità di movimento e rotazione del GameObject
    public float moveSpeed = 5f;
    public float rotationSpeed = 100f;
    public GameObject pannelloAvanti;
    
    public bool meshReady = false, textureReady = false;
    
    // Posizione e rotazione di destinazione (checkpoint B)
    private Transform target;
    private Transform originalTransform;
    private Quaternion initialRotation;
    
    private Quaternion targetRotation;
    private Quaternion oppositeRotation;
    
    private Renderer _renderer;

    // Enum per definire lo stato dell'azione
    private enum State { Moving, RotatingToTarget, RotatingToOpposite, RotatingToInitial, Idle }
    private State currentState = State.Idle;

    void Start()
    {
        // Trova l'oggetto "checkpoint B" e assegna la sua trasformazione come destinazione
        GameObject checkpointB = GameObject.Find("CheckpointB");
        this.transform.rotation = Quaternion.Euler(0, 180, 0);
        if (checkpointB != null)
        {
            target = checkpointB.transform;
            targetRotation = target.rotation;
            oppositeRotation = Quaternion.Inverse(targetRotation) * Quaternion.Euler(180, 0, 180);
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
        
        _renderer = GetComponent<Renderer>();
    }

    void Update()
    {
        if (meshReady && textureReady)
        {
            _renderer.enabled = true;
            switch (currentState)
            {
                case State.Moving:
                    MoveTowards(target.position, State.RotatingToTarget);
                    break;
                case State.RotatingToTarget:
                    RotateTowards(targetRotation, State.RotatingToOpposite);
                    break;
                case State.RotatingToOpposite:
                    RotateTowards(oppositeRotation, State.RotatingToInitial);
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

    private void RotateTowards(Quaternion targetRotation, State nextState)
    {
        float step = rotationSpeed * Time.deltaTime;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, step);

        if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
        {
            currentState = nextState;
            Debug.Log($"Rotazione completata dello stato: {currentState}!");
            if (nextState == State.Idle)
            {
                Debug.Log($"Rotazione completata dello stato: {currentState}!");
                
                transform.position = originalTransform.position;
                transform.rotation = originalTransform.rotation;
                
                pannelloAvanti.SetActive(true);
                
            }
        }
    }

    // Funzione pubblica per avviare il movimento
    public void StartMoving()
    {
        currentState = State.Moving;
    }
    
    
}
