using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rails : MonoBehaviour
{

    
    public float railSpeed;
    public Transform endpoint;
    
    public Transform playerCheck;
    public LayerMask playerMask;
    private float playerCheckRadius = .5f;
    private bool railRideKeyWasPressed;
    private bool endPointReached;

    void Start()
    {
        
    }
        private void Update()
    {
        endPointReached = Physics.CheckSphere(playerCheck.position, playerCheckRadius, playerMask, QueryTriggerInteraction.Collide);
       if (Input.GetKey (KeyCode.Mouse1))
       {
           railRideKeyWasPressed = true;

            if (endPointReached)
            {
                railRideKeyWasPressed = false;
            }
        }
       if (Input.GetKeyUp (KeyCode.Mouse1))
        {
            railRideKeyWasPressed = false;
         }
      

        
    }
    private void OnTriggerStay (Collider player)
    {
        if (railRideKeyWasPressed)
        {
            player.transform.position = Vector3.MoveTowards(player.transform.position, endpoint.position, railSpeed * Time.deltaTime);
            
        }

        
        ;
        
    }
}
