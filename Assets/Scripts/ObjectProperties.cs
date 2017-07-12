using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectProperties : MonoBehaviour
{
    string objectName_;
    string fullName_;
    bool active_;

    Texture2D tex_;
    bool orientation_;

	// Use this for initialization
	void Start()
    {
        objectName_ = "Object";
        fullName_ = "Object";
        active_ = false;

        tex_ = new Texture2D( 2, 2 );
        orientation_ = false; // horizontal
    }
	
	// Update is called once per frame
	void Update()
    {
    }

    public string GetName()
    {
        return objectName_;
    }
    public void SetName( string objectName )
    {
        objectName_ = objectName;
        gameObject.GetComponentInChildren<TextMesh>().text = objectName_;
    }
    public string GetFullName()
    {
        return fullName_;
    }
    public void SetFullName( string fullName )
    {
        fullName_ = fullName;
    }
    public bool IsActive()
    {
        return active_;
    }
    public void Activate()
    {
        active_ = true;
        // parent active
        gameObject.SetActive( true );
    }
    public void Deactivate()
    {
        active_ = false;
        // parent not active
        gameObject.SetActive( false );
    }
    public void SetTexture( Texture2D tex )
    {
        tex_ = tex;
        gameObject.GetComponent<Renderer>().materials[2].mainTexture = tex_;
    }
    public Texture2D GetTexture()
    {
        return tex_;
    }
    public void DeleteTexture()
    {
        Destroy( tex_ );
    }
    public void SetHorizontal()
    {
        orientation_ = false;
    }
    public void SetVertical()
    {
        orientation_ = true;
    }
}
