using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private CharacterController _characterController;
    
    public Camera mainCamera;
    
    public InputActionReference moveAction;
   
  
    
    public int moveSpeed;
    
    private Vector3 _movementDirection;
    private Vector3 _cameraForward;
    private Vector3 _cameraRight;
    

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        CalculateMovementDirection();
    }


    private void FixedUpdate()
    {
        _characterController.SimpleMove(_movementDirection * moveSpeed);
    }

    
    private void CalculateMovementDirection()
    {
        _cameraForward = mainCamera.transform.forward;
        _cameraForward.y = 0;
        _cameraForward.Normalize();
        _cameraRight = mainCamera.transform.right;
        _cameraRight.y = 0;
        _cameraRight.Normalize();
        _movementDirection = _cameraForward * moveAction.action.ReadValue<Vector2>().y + _cameraRight * moveAction.action.ReadValue<Vector2>().x;
        _movementDirection.Normalize();
      
        
      
    }
 

    }


  
   
    


   

