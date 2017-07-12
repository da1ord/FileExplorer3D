using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class FileExplorer : MonoBehaviour
{
    public InputField path_;
    public Button btnEnterDirectory_;
    public Text imageNameText_;
    public Image fadeImage_;

    public Dropdown foldersDropdown_;
    public Image imagesBrowser_;
    public ScrollRect filesScrollView_;

    DirectoryInfo directoryInfo_;
    List<DirectoryInfo> directories_;
    List<FileInfo> imageFiles_;
    List<FileInfo> otherFiles_;

    /* Test */
    ObjectProperties exitDoorsObject_;
    List<ObjectProperties> doorsObjects_;
    List<ObjectProperties> paintingsObjects_;
    GameObject pileOfPaintings_;
    List<ObjectProperties> statuesObjects_;

    TextMesh roomInfo_;
    GameObject player_;
    Vector3 defaultPlayerPosition_ = new Vector3( -18f, 1f, 0f );
    Quaternion defaultPlayerRotation_ = Quaternion.AngleAxis( 90f, Vector3.up );

    bool fading_;
    bool showingMore_;
    int imageToShow_;

    Vector3 lastPaintingWallPosition_ = new Vector3( 15f, 4f, -10f );
    Vector3 lastPaintingFloorPosition_ = new Vector3( 14f, 1.94f, -9f );
    Quaternion lastPaintingWallRotation_ = Quaternion.Euler( -90f, 0f, 0f );
    Quaternion lastPaintingFloorRotation_ = Quaternion.Euler( -100f, 0f, 0f );
    /**TEST
     */
    Texture2D m_myTexture;
    bool bFinishedCopying = false;
    int imageProcessed = 0;

    // Use this for initialization
    void Start()
    {
        directories_ = new List<DirectoryInfo>();
        imageFiles_ = new List<FileInfo>();
        otherFiles_ = new List<FileInfo>();

        btnEnterDirectory_.onClick.AddListener( EnterDirectory );

        // Fill objects lists
        exitDoorsObject_ = GameObject.Find( "Scene/ExitDoors" ).GetComponentInChildren<ObjectProperties>();
        doorsObjects_ = new List<ObjectProperties>();
        foreach( Transform child in GameObject.Find( "Scene/Doors" ).transform )
        {
            if( child.gameObject.GetComponent<ObjectProperties>() != null )
                doorsObjects_.Add( child.gameObject.GetComponent<ObjectProperties>() );
        }
        paintingsObjects_ = new List<ObjectProperties>();
        foreach( Transform child in GameObject.Find( "Scene/Paintings" ).transform )
        {
            if( child.gameObject.GetComponent<ObjectProperties>() != null )
                paintingsObjects_.Add( child.gameObject.GetComponent<ObjectProperties>() );
            else
                pileOfPaintings_ = child.gameObject;
        }
        statuesObjects_ = new List<ObjectProperties>();
        foreach( Transform child in GameObject.Find( "Scene/Statues" ).transform )
        {
            if( child.gameObject.GetComponent<ObjectProperties>() != null )
                statuesObjects_.Add( child.gameObject.GetComponent<ObjectProperties>() );
        }

        roomInfo_ = GameObject.Find( "InfoSign" ).GetComponentInChildren<TextMesh>();

        player_ = GameObject.Find( "Player" );
        /* TEST */
        path_.text = "D:/Photos";
        
        // TODO: Get directory info from last directory dirs list
        directoryInfo_ = new DirectoryInfo( path_.text );

        fading_ = false;
        showingMore_ = false;
        imageToShow_ = 9;

        // Hide UI elements
        foldersDropdown_.gameObject.SetActive( false );
        imagesBrowser_.gameObject.SetActive( false );
        filesScrollView_.gameObject.SetActive( false );
        btnEnterDirectory_.gameObject.SetActive( false );
        imageNameText_.gameObject.SetActive( false );

        /*TEST*/
        /* ASYNC LOAD */
        //GetDirectoryInfo();

        ///* HIDE EVERYTHING */
        //HideEverything();

        ///* SHOW DOORS/PAINTINGS/STATUES */
        //ShowObjects();

        //// Set paintings textures
        //StartCoroutine( SetPaintingsTextures() );
        ChangeDirectory();
    }

    // Update is called once per frame
    void Update()
    {
    }

    IEnumerator FadeAnimation()
    {
        fading_ = true;

        //Loop 1 second
        for( float i = 0.0f; i <= 1.1f; i += 2 * Time.deltaTime )
        {
            // Fade from transparent to opaque
            fadeImage_.color = new Color( 0.0f, 0.0f, 0.0f, i );
            yield return null;
        }
        
        // Reset players position and rotation
        player_.GetComponent<Rigidbody>().MovePosition( defaultPlayerPosition_ );
        player_.GetComponent<Rigidbody>().MoveRotation( defaultPlayerRotation_ );

        yield return new WaitForSeconds( 2 );
        // Loop 1 second
        for( float i = 0.0f; i <= 1.1f; i += Time.deltaTime )
        {
            // Fade from opaque to transparent
            fadeImage_.color = new Color( 0.0f, 0.0f, 0.0f, 1.0f - i );
            yield return null;
        }

        fading_ = false;
        yield return null;
    }

    public void GoUp()
    {
        path_.text = directoryInfo_.Parent.FullName;
    }

    public void ChangeDirectory()
    {
        /* FADE OUT/IN */
        StartCoroutine( FadeAnimation() );

        /* ASYNC LOAD */
        GetDirectoryInfo();

        /* HIDE EVERYTHING */
        HideEverything();

        /* SHOW DOORS/PAINTINGS/STATUES */
        ShowObjects();

        // Set paintings textures
        StartCoroutine( SetPaintingsTextures() );

        //for( int i = 0; i < 9; i++ )
        //{
        //    StartCoroutine( downloadTexture( i ) );
        //}
    }

    void GetDirectoryInfo()
    {
        // TODO: Use this
        //string path = directoryInfo_.FullName;
        /* TEST */
        directoryInfo_ = new DirectoryInfo( path_.text );
        string path = directoryInfo_.FullName;

        // Cleanup
        directories_.Clear();
        imageFiles_.Clear();
        otherFiles_.Clear();
        
        for( int i = 0; i < paintingsObjects_.Count; i++ )
        {
            paintingsObjects_[i].DeleteTexture();
        }
        System.GC.Collect();


        foreach( string folderName in Directory.GetDirectories( path ) )
        {
            directories_.Add( new DirectoryInfo( folderName ) );
        }

        foreach( string filename in Directory.GetFiles( path ) )
        {
            FileInfo file = new FileInfo( filename );
            if( Regex.IsMatch( filename, @".jpg|.png$" ) )
            {
                imageFiles_.Add( file );
            }
            else
            {
                otherFiles_.Add( file );
            }
        }

        roomInfo_.text = 
            "Room name:\n" + directoryInfo_.Name + 
            "\n\nCreation time:\n" + directoryInfo_.CreationTime +
            "\n\nImages count: " + imageFiles_.Count +
            "\n\nStatues count: " + otherFiles_.Count;
    }

    void HideEverything()
    {
        exitDoorsObject_.Deactivate();
        foreach( ObjectProperties obj in doorsObjects_ )
        {
            obj.Deactivate();
        }
        foreach( ObjectProperties obj in paintingsObjects_ )
        {
            obj.Deactivate();
        }
        foreach( ObjectProperties obj in statuesObjects_ )
        {
            obj.Deactivate();
        }
        pileOfPaintings_.SetActive( false );
    }

    void ShowObjects()
    {
        if( directoryInfo_.Parent != null )
        {
            exitDoorsObject_.Activate();
            exitDoorsObject_.SetName( directoryInfo_.Parent.Name );
            exitDoorsObject_.SetFullName( directoryInfo_.Parent.FullName );
        }

        foldersDropdown_.ClearOptions();
        for( int i = 0; i < directories_.Count; i++ )
        {
            if( i < doorsObjects_.Count )
            {
                doorsObjects_[i].Activate();
                doorsObjects_[i].SetName( directories_[i].Name );
                doorsObjects_[i].SetFullName( directories_[i].FullName );
            }
            else if( i == doorsObjects_.Count )
            {
                doorsObjects_[i - 1].SetName( "More directories ..." );
                doorsObjects_[i - 1].SetFullName( "More directories ..." );

                foldersDropdown_.options.Add( new Dropdown.OptionData( directories_[i - 1].Name ) );
                foldersDropdown_.options.Add( new Dropdown.OptionData( directories_[i].Name ) );
                foldersDropdown_.value = 0;
                foldersDropdown_.GetComponentInChildren<Text>().text = foldersDropdown_.options[0].text;
            }
            else
            {
                foldersDropdown_.options.Add( new Dropdown.OptionData( directories_[i].Name ) );
            }
        }

        for( int i = 0; i < imageFiles_.Count; i++ )
        {
            if( i == 9 ) // Last painting // TODO: MAX_PAINTINGS
            {
                paintingsObjects_[i].Activate();
                paintingsObjects_[i].SetName( "More images ..." );
                paintingsObjects_[i].SetFullName( "More images ..." );

                if( imageFiles_.Count > 10 )
                {
                    paintingsObjects_[i].transform.position = lastPaintingFloorPosition_;
                    paintingsObjects_[i].transform.rotation = lastPaintingFloorRotation_;
                    // Show pile on floor
                    pileOfPaintings_.SetActive( true );
                }
                else
                {
                    paintingsObjects_[i].transform.position = lastPaintingWallPosition_;
                    paintingsObjects_[i].transform.rotation = lastPaintingWallRotation_;
                    // Hide pile on floor
                    pileOfPaintings_.SetActive( false );
                }
                break;
            }
            paintingsObjects_[i].Activate();
            paintingsObjects_[i].SetName( imageFiles_[i].Name );
            paintingsObjects_[i].SetFullName( imageFiles_[i].FullName );
            //
            //SetPaintingsTextures( i );
        }

        filesScrollView_.content.GetComponent<Text>().text = "";
        for( int i = 0; i < otherFiles_.Count; i++ )
        {
            if( i < statuesObjects_.Count )
            {
                statuesObjects_[i].Activate();
                statuesObjects_[i].SetName( otherFiles_[i].Name );
                statuesObjects_[i].SetFullName( otherFiles_[i].FullName );
            }
            else if( i == statuesObjects_.Count )
            {
                statuesObjects_[i - 1].SetName( "More files ..." );
                statuesObjects_[i - 1].SetFullName( "More files ..." );

                filesScrollView_.content.GetComponent<Text>().text += otherFiles_[i - 1].Name + "\n";
                filesScrollView_.content.GetComponent<Text>().text += otherFiles_[i].Name + "\n";
            }
            else
            {
                filesScrollView_.content.GetComponent<Text>().text += otherFiles_[i].Name + "\n";
            }
        }
    }

    public void SetPath( string path )
    {
        path_.text = path;
    }
    public bool IsFading()
    {
        return fading_;
    }

    public void ShowMoreImages()
    {
        Cursor.lockState = CursorLockMode.None;
        showingMore_ = true;
        fadeImage_.color = Color.black;
        imagesBrowser_.gameObject.SetActive( true );
        imageNameText_.gameObject.SetActive( true );
        imageToShow_ = 9;
        imagesBrowser_.material.mainTexture = paintingsObjects_[imageToShow_].GetTexture();
        imageNameText_.text = imageFiles_[imageToShow_].Name;
    }

    public void PreviousImage()
    {
        if( imageToShow_ > 9 )
        {
            imageToShow_--;
            imagesBrowser_.material.mainTexture = GetImageAsTexture( imageToShow_ );//paintings_[imageToShow_].GetComponent<Renderer>().material.mainTexture;
            // For proper image changing
            imagesBrowser_.gameObject.SetActive( false );
            imagesBrowser_.gameObject.SetActive( true );
            imageNameText_.text = imageFiles_[imageToShow_].Name;
        }
    }

    public void NextImage()
    {
        if( imageToShow_ < imageFiles_.Count - 1 )
        {
            imageToShow_++;
            imagesBrowser_.material.mainTexture = GetImageAsTexture( imageToShow_ );//paintings_[imageToShow_].GetComponent<Renderer>().material.mainTexture;
            // For proper image changing
            imagesBrowser_.gameObject.SetActive( false );
            imagesBrowser_.gameObject.SetActive( true );
            imageNameText_.text = imageFiles_[imageToShow_].Name;
        }
    }

    public void ShowMoreFiles()
    {
        Cursor.lockState = CursorLockMode.None;
        showingMore_ = true;
        fadeImage_.color = Color.black;
        filesScrollView_.gameObject.SetActive( true );
    }

    public void ShowMoreDirectories()
    {
        Cursor.lockState = CursorLockMode.None;
        showingMore_ = true;
        fadeImage_.color = Color.black;
        foldersDropdown_.gameObject.SetActive( true );
        btnEnterDirectory_.gameObject.SetActive( true );
    }

    public void EnterDirectory()
    {
        string folder = foldersDropdown_.GetComponentInChildren<Text>().text;
        if( folder != "" )
        {
            path_.text = directoryInfo_.FullName + "\\" + folder;
            StopShowingMore();
            ChangeDirectory();
        }
    }

    public bool IsShowingMore()
    {
        return showingMore_;
    }

    public void StopShowingMore()
    {
        Cursor.lockState = CursorLockMode.Locked;
        showingMore_ = false;
        fadeImage_.color = Color.clear;
        filesScrollView_.gameObject.SetActive( false );
        foldersDropdown_.gameObject.SetActive( false );
        imagesBrowser_.gameObject.SetActive( false );
        btnEnterDirectory_.gameObject.SetActive( false );
        imageNameText_.gameObject.SetActive( false );
    }

    IEnumerator SetPaintingsTextures()
    {
        /**/
        //WWW www;
        //Texture2D tex;

        ////yield return new WaitForSeconds( 0.5f );
        ////for( int i = 0; i < paintings_.Count; i++ )
        ////{
        ////    if( !paintings_[i].activeSelf  )
        ////    {
        ////        if( imageFiles_.Count > 10 )
        ////        {
        ////            LoadImageToTexture( i, i + 1 );

        ////            //www = new WWW( "file://" + imageFiles_[i].FullName );
        ////            //tex = new Texture2D( 2, 2 );
        ////            //www.LoadImageIntoTexture( tex );
        ////            //paintings_[i + 1].GetComponent<Renderer>().material.mainTexture = tex;
        ////        }
        ////        break;
        ////    }

        ////    LoadImageToTexture( i, i );

        ////    //www = new WWW( "file://" + imageFiles_[i].FullName );
        ////    //tex = new Texture2D( 2, 2 );
        ////    //www.LoadImageIntoTexture( tex );
        ////    //paintings_[i].GetComponent<Renderer>().material.mainTexture = tex;


        ////    //byte[] fileData = File.ReadAllBytes( imageFiles_[i].FullName );
        ////    //Texture2D tex = new Texture2D( 2, 2 );
        ////    //tex.LoadImage( fileData );
        ////    //paintings_[i].GetComponent<Renderer>().material.mainTexture = tex;

        ////    yield return null;
        ////}
        yield return new WaitForSeconds( 0.5f );
        for( int i = 0; i < paintingsObjects_.Count; i++ )
        {
            if( !paintingsObjects_[i].IsActive() )
            {
                break;
            }
            if( i == paintingsObjects_.Count - 1 )
            {
            }

            LoadImageToTexture( i, i );

            yield return null;
        }
    }

    void LoadImageToTexture( int imageID, int paintingID )
    {
        //WWW www = new WWW( "file://" + imageFiles_[imageID].FullName );
        //Texture2D tex = new Texture2D( 2, 2 );
        //www.LoadImageIntoTexture( tex );


        //paintings_[paintingID].GetComponent<Renderer>().materials[2].mainTexture = GetImageAsTexture( imageID );//tex;

        paintingsObjects_[paintingID].SetTexture( GetImageAsTexture( imageID ) );
    }

    Texture2D GetImageAsTexture( int imageID )
    {
        print( Time.realtimeSinceStartup );
        WWW www = new WWW( "file://" + imageFiles_[imageID].FullName );
        Texture2D tex = new Texture2D( 2, 2 );
        www.LoadImageIntoTexture( tex );

        //byte[] fileData = File.ReadAllBytes( imageFiles_[imageID].FullName );
        //Texture2D tex = new Texture2D( 2, 2 );
        //tex.LoadImage( fileData );

        print( Time.realtimeSinceStartup );
        return tex;
    }

    /***TEST*****************
     * 
     * 
     * 
     * 
     * 
     * *********************/

    IEnumerator downloadTexture( int imageID )
    {
        bFinishedCopying = false;
        WWW www = new WWW( "file://" + imageFiles_[imageID].FullName );
        yield return www;

        //m_myTexture = www.texture;  // Commenting this line removes frame out

        Debug.Log( "Downloaded Texture. Now copying it" );

        //Copy Texture to m_myTexture WITHOUT callback function
        //StartCoroutine(copyTextureAsync(www.texture));

        //Copy Texture to m_myTexture WITH callback function
        StartCoroutine( copyTextureAsync( www.texture, false, finishedCopying ) );
    }


    IEnumerator copyTextureAsync( Texture2D source, bool useMipMap = false, System.Action callBack = null )
    {
        const int LOOP_TO_WAIT = 400000; //Waits every 400,000 loop, Reduce this if still freezing
        int loopCounter = 0;

        int heightSize = source.height;
        int widthSize = source.width;

        //Create new Empty texture with size that matches source info
        m_myTexture = new Texture2D( widthSize, heightSize, source.format, useMipMap );

        for( int y = 0; y < heightSize; y++ )
        {
            for( int x = 0; x < widthSize; x++ )
            {
                //Get color/pixel at x,y pixel from source Texture
                Color tempSourceColor = source.GetPixel( x, y );

                //Set color/pixel at x,y pixel to destintaion Texture
                m_myTexture.SetPixel( x, y, tempSourceColor );

                loopCounter++;

                if( loopCounter % LOOP_TO_WAIT == 0 )
                {
                    //Debug.Log("Copying");
                    yield return null; //Wait after every LOOP_TO_WAIT 
                }
            }
        }
        //Apply changes to the Texture
        m_myTexture.Apply();
        bFinishedCopying = true;

        //Let our optional callback function know that we've done copying Texture
        if( callBack != null )
        {
            callBack.Invoke();
        }
    }

    void finishedCopying()
    {
        bFinishedCopying = true;
        Debug.Log( "Finished Copying Texture" );
        //paintings_[imageProcessed++].GetComponent<Renderer>().material.mainTexture = m_myTexture;
        //Do something else
    }
}
