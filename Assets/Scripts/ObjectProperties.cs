using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectProperties : MonoBehaviour
{
    // Object name
    protected string objectName_;
    // Flag indicating if object is active
    protected bool active_;

    // Initialization
    void Start()
    {
        // Set object's name
        objectName_ = "Object";
        // Deactivate object
        active_ = false;
    }

    // Update function
    void Update()
    {
    }

    // Get object's name
    public string GetName()
    {
        return objectName_;
    }
    // Set object's name
    public void SetName( string objectName )
    {
        // Set objectName variable
        objectName_ = objectName;

        // Get object's label textmesh
        TextMesh tm = gameObject.GetComponentInChildren<TextMesh>();
        // Set object name as label text
        tm.text = objectName_;
        // Get oobject name length
        int length = objectName_.Length;

        // If short text, set max font size
        if( length < 20 )
        {
            tm.fontSize = 34;
        }
        // If text is longer than maximum, set min font size and truncate text
        else if( length > 30 )
        {
            tm.fontSize = 24;
            tm.text = objectName_.Substring( 0, 26 ) + " ...";
        }
        // Text between length boundaries, calculate the font size
        else
        {
            tm.fontSize = 54 - length;
        }
    }
    // Get object's state
    public bool IsActive()
    {
        return active_;
    }
    // Activate object
    public void Activate()
    {
        active_ = true;
        gameObject.SetActive( true );
    }
    // Deactivate object
    public void Deactivate()
    {
        active_ = false;
        gameObject.SetActive( false );
    }
}
