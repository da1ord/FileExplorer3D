using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController :MonoBehaviour
{
    // Hit variable for raycasting
    RaycastHit hit_;
    // Player rigidbody
    Rigidbody rb_;
    // Fileexplorer instance
    FileExplorer fileExplorer_;

    // Mouse around x-axis rotation
    float xRot_ = 0.0f;
    // Mouse sensitivity constant
    const float mouseSensitivity_ = 3.0f;
    // Movement speed
    float movementSpeed_ = 5.0f;
    // Walking speed constant
    const float walkingSpeed_ = 5.0f;
    // Running speed constant
    const float runningSpeed_ = 8.0f;

    // Initialization
    void Start()
    {
        rb_ = transform.parent.GetComponent<Rigidbody>();
        fileExplorer_ = GameObject.Find( "Canvas" ).GetComponent<FileExplorer>();
    }

    // Update call
    void Update()
    {
        if( fileExplorer_.IsFading() )
        {
            return;
        }

        if( fileExplorer_.IsShowingMore() )
        {
            if( Input.GetKey( KeyCode.Backspace ) )
            {
                fileExplorer_.StopShowingMore();
            }

            if( fileExplorer_.imagesBrowser_.IsActive() )
            {
                if( Input.GetKey( KeyCode.LeftArrow ) )
                {
                    fileExplorer_.PreviousImage();
                }
                else if( Input.GetKey( KeyCode.RightArrow ) )
                {
                    fileExplorer_.NextImage();
                }
            }
            return;
        }

        // Interaction with objects
        // Left mouse button pressed
        if( Input.GetMouseButtonDown( 0 ) )
        {
            if( Physics.Raycast( transform.position, transform.forward,
                out hit_, 3.0f, LayerMask.GetMask( "Interactable" ) ) )
            {
                ObjectProperties op = hit_.collider.gameObject.GetComponent<ObjectProperties>();
                if( hit_.collider.CompareTag( "Doors" ) )
                {
                    if( op.GetName() == "More directories ..." )//if( hit_.collider.name == "More directories ..." )
                    {
                        fileExplorer_.ShowMoreDirectories();
                    }
                    else
                    {
                        fileExplorer_.ChangeDirectory( op.GetFullName() );
                    }
                }
                else if( hit_.collider.CompareTag( "Painting" ) )
                {
                    if( op.GetName() == "More images ..." )//if( hit_.collider.name == "More images ..." )
                    {
                        fileExplorer_.ShowMoreImages();
                    }
                }
                else if( hit_.collider.CompareTag( "Statue" ) )
                {
                    if( op.GetName() == "More files ..." )//if( hit_.collider.name == "More files ..." )
                    {
                        fileExplorer_.ShowMoreFiles();
                    }
                }
            }
        }

        // Player/camera movement
        Movement();
    }

    // Player/camera movement handling
    void Movement()
    {
        // Running/walking
        if( Input.GetKey( KeyCode.LeftShift ) )
        {
            movementSpeed_ = runningSpeed_;
        }
        else
        {
            movementSpeed_ = walkingSpeed_;
        }

        // Strafing - A, D
        float axisH = Input.GetAxisRaw( "Horizontal" );
        // Forward and backward - W, S
        float axisV = Input.GetAxisRaw( "Vertical" );

        // Calculate movement vector
        Vector3 moveH = transform.right * axisH;
        Vector3 moveV = transform.forward * axisV;
        Vector3 movement = ( moveH + moveV ).normalized * movementSpeed_;

        // Move rigidbody in movement direction
        rb_.MovePosition( rb_.position + movement * Time.fixedDeltaTime );

        // Get rotation around X and Y
        float yRot = Input.GetAxis( "Mouse X" ) * mouseSensitivity_;
        xRot_ += Input.GetAxis( "Mouse Y" ) * mouseSensitivity_;
        xRot_ = Mathf.Clamp( xRot_, -90f, 90f );

        // Rotate camera along X-axis
        transform.localEulerAngles = new Vector3( -xRot_, 0, 0 );
        // Rotata rigidbody along Y-axis
        rb_.MoveRotation( rb_.rotation * Quaternion.Euler( new Vector3( 0.0f, yRot, 0.0f ) ) );
    }
}
