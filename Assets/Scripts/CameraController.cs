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
    const float runningSpeed_ = 9.0f;

    // Initialization
    void Start()
    {
        // Get player's rigidbody object
        rb_ = transform.parent.GetComponent<Rigidbody>();
        // Get fileExplorer object
        fileExplorer_ = GameObject.Find( "Canvas" ).GetComponent<FileExplorer>();
    }

    // Update function
    void Update()
    {
        // Return when during fading phase
        if( fileExplorer_.IsFading() )
        {
            return;
        }

        #if UNITY_EDITOR
            // If neither showing more objects nor intro screen, keep cursor locked
            if( !fileExplorer_.IsShowingMoreObjects() && !fileExplorer_.IsShowingIntro() )
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        #endif
        
        // Check if showing more objects (files, images, dirs)
        if( fileExplorer_.IsShowingMoreObjects() )
        {
            // ESC was pressed
            if( Input.GetKeyUp( KeyCode.Escape ) )
            {
                // Hide UI elements for browsing more objects
                fileExplorer_.StopShowingMoreObjects();
            }

            // Enable arrows to brows through more images
            if( fileExplorer_.imagesBrowser_.IsActive() )
            {
                // Show previous image
                if( Input.GetKeyUp( KeyCode.LeftArrow ) )
                {
                    fileExplorer_.PreviousImage();
                }
                // Show next image
                else if( Input.GetKeyUp( KeyCode.RightArrow ) )
                {
                    fileExplorer_.NextImage();
                }
            }
            return;
        }
        // Enable to open intro screen by pressing ESC
        else if( !fileExplorer_.IsShowingIntro() )
        {
            if( Input.GetKeyUp( KeyCode.Escape ) )
            {
                fileExplorer_.ShowIntroScreen();
                return;
            }
        }

        // Interaction with objects
        // Left mouse button pressed
        if( Input.GetMouseButtonDown( 0 ) )
        {
            // Check for interactable objects in range
            if( Physics.Raycast( transform.position, transform.forward,
                out hit_, 3.0f, LayerMask.GetMask( "Interactable" ) ) )
            {
                // Get object hit by ray
                ObjectProperties op = hit_.collider.gameObject.GetComponent<ObjectProperties>();
                // Doors hit
                if( hit_.collider.CompareTag( "Doors" ) )
                {
                    // Hit 'More ...' doors. Show dropdown list with other folders
                    if( op.GetName() == "More directories ..." )
                    {
                        fileExplorer_.ShowMoreDirectories();
                    }
                    else
                    {
                        // Interacting with doors. Change directory given by 
                        //  path stored in doors object
                        fileExplorer_.ChangeDirectory( ((DirectoryProperties)op).GetFullPath() );
                    }
                }
                // Painting hit
                else if( hit_.collider.CompareTag( "Painting" ) )
                {
                    // Hit 'More ...' painting. Show image browser
                    if( op.GetName() == "More images ..." )
                    {
                        fileExplorer_.ShowMoreImages();
                    }
                }
                // Statue hit
                else if( hit_.collider.CompareTag( "Statue" ) )
                {
                    // Hit 'More ...' files. Show textbox with other objects
                    if( op.GetName() == "More files ..." )
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
