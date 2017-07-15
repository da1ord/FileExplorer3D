using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageProperties : ObjectProperties
{
    // Texture used on paintings
    Texture2D tex_;
    // Orientation of image (horizontal - false/vertical - true)
    bool orientation_;
    // Painting local scale. Used for painting rescaling according to image 
    //  dimensions
    Vector3 localScale_ = new Vector3( 220f, 10f, 165f );
    // Last painting floor position. If the last painting os on floor, used for 
    //  polishing the position in case the image has vertical orientation
    Vector3 lastPaintingFloorPosition_ = new Vector3( 14f, 1.57f, -9.065f );

    // Initialization
    void Start()
    {
        // Set object's name
        objectName_ = "Object";
        // Deactivate object
        active_ = false;

        // Initialize texture
        tex_ = new Texture2D( 2, 2 );
        // Set image orientation to horizontal
        orientation_ = false;
    }
	
	// Update function
	void Update()
    {
    }

    // Set texture and set it on painting
    public void SetTexture( Texture2D tex )
    {
        tex_ = tex;
        // Set painting texture
        gameObject.GetComponent<Renderer>().materials[2].mainTexture = tex_;

        // Set painting orientation according to image dimensions
        if( tex.height > tex.width )
        {
            SetVertical();
        }
        else
        {
            SetHorizontal();
        }
    }
    // Get texture
    public Texture2D GetTexture()
    {
        return tex_;
    }
    // Delete texture
    public void DeleteTexture()
    {
        Destroy( tex_ );
    }
    // Set horizontal orientation (scale)
    public void SetHorizontal()
    {
        orientation_ = false;
        transform.localScale = localScale_;
    }
    // Set vertical orientation (scale)
    public void SetVertical()
    {
        orientation_ = true;
        transform.localScale = new Vector3( localScale_.z, localScale_.y, localScale_.x );
    }
    // Polish the floor painting position in case of vertical orientation
    public void PolishPosition()
    {
        // Vertical position
        if( orientation_ )
        {
            transform.position = new Vector3( lastPaintingFloorPosition_.x, lastPaintingFloorPosition_.y + 0.5f, lastPaintingFloorPosition_.z - 0.12f );
        }
        // Horizontal position
        else
        {
            transform.position = lastPaintingFloorPosition_;
        }
    }
}
