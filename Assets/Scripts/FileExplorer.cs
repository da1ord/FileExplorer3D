using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

public class FileExplorer : MonoBehaviour
{
    //
    // UI components
    //
    // Application text name on menu screen
    public Text textAppName_;
    // Browse folders button
    public Button btnBrowseFolders_;
    // Return to scene button
    public Button btnReturn_;
    // Quit application button
    public Button btnQuit_;
    // Enter selected directory (if there is more subdirectories) button
    public Button btnEnterDirectory_;
    // Image name text when browsing images
    public Text imageNameText_;
    // Hint text
    public Text textHint_;
    // Image for fade in/out animations
    public Image fadeImage_;

    // Folders dropdown for printing out more directories
    public Dropdown foldersDropdown_;
    // Image for browsing more images
    public Image imagesBrowser_;
    // Scrollview for listing of more objects
    public ScrollRect filesScrollView_;

    // Actual directory info
    DirectoryInfo directoryInfo_;
    // Subdirectories info list
    List<DirectoryInfo> directories_;
    // Folder image files info list
    List<FileInfo> imageFiles_;
    // Folder other files info list
    List<FileInfo> otherFiles_;

    // Exit doors object
    DirectoryProperties exitDoorsObject_;
    // All other doors objects list
    List<DirectoryProperties> doorsObjects_;
    // Painting objects list
    List<ImageProperties> paintingsObjects_;
    // Pile of paintings object
    GameObject pileOfPaintings_;
    // Statue objects list
    List<ObjectProperties> statuesObjects_;
    // Room info text object
    TextMesh roomInfo_;
    
    // Player object 
    GameObject player_;
    // Players default position used on transitions
    Vector3 defaultPlayerPosition_ = new Vector3( -18f, 1f, 0f );
    // Players default rotation used on transitions
    Quaternion defaultPlayerRotation_ = Quaternion.AngleAxis( 90f, Vector3.up );

    // Flag indicating if fading is in process
    bool fading_;
    // Flag indicating if showing Intro screen
    bool showingIntro_;
    // Flag indicating if showing more objects (images, other files, and directories )
    bool showingMoreObjects_;
    // Index of image to show when browsing 'pile' of images
    int imageToShow_;

    // 10th painting position vector if hanging on the wall
    Vector3 lastPaintingWallPosition_ = new Vector3( 15f, 4f, -10f );
    // 10th painting rotation quaternion if hanging on the wall
    Quaternion lastPaintingWallRotation_ = Quaternion.Euler( -90f, 0f, 0f );
    // 10th painting position vector if on the floor
    Vector3 lastPaintingFloorPosition_ = new Vector3( 14f, 1.57f, -9.065f );
    // 10th painting rotation quaternion if on the floor
    Quaternion lastPaintingFloorRotation_ = Quaternion.Euler( -105f, 0f, 0f );
    
    /*TEST*/
    Texture2D m_myTexture;
    bool bFinishedCopying = false;
    int imageProcessed = 0;

    // Initialization
    void Start()
    {
        // Initialize lists of files and directories info
        directories_ = new List<DirectoryInfo>();
        imageFiles_ = new List<FileInfo>();
        otherFiles_ = new List<FileInfo>();

        // Initialize exit door object
        exitDoorsObject_ = GameObject.Find( "Scene/ExitDoors" ).GetComponentInChildren<DirectoryProperties>();
        //
        // Initialize and fill objects lists
        //
        // Initialize and fill doors objects list
        doorsObjects_ = new List<DirectoryProperties>();
        foreach( Transform child in GameObject.Find( "Scene/Doors" ).transform )
        {
            if( child.gameObject.GetComponent<DirectoryProperties>() != null )
                doorsObjects_.Add( child.gameObject.GetComponent<DirectoryProperties>() );
        }
        // Initialize and fill painting objects list
        paintingsObjects_ = new List<ImageProperties>();
        foreach( Transform child in GameObject.Find( "Scene/Paintings" ).transform )
        {
            if( child.gameObject.GetComponent<ObjectProperties>() != null )
                paintingsObjects_.Add( child.gameObject.GetComponent<ImageProperties>() );
            else
                pileOfPaintings_ = child.gameObject;
        }
        // Initialize and fill statue objects list
        statuesObjects_ = new List<ObjectProperties>();
        foreach( Transform child in GameObject.Find( "Scene/Statues" ).transform )
        {
            if( child.gameObject.GetComponent<ObjectProperties>() != null )
                statuesObjects_.Add( child.gameObject.GetComponent<ObjectProperties>() );
        }

        // Initialize room text info object
        roomInfo_ = GameObject.Find( "InfoSign" ).GetComponentInChildren<TextMesh>();

        // Initialize player object
        player_ = GameObject.Find( "Player" );

        // Clear fading flag
        fading_ = false;
        // Set showing intro flag
        showingIntro_ = true;
        // Clear showing more objects flag
        showingMoreObjects_ = false;
        // Set image to show for images browsing
        imageToShow_ = 9;

        // Hide UI elements
        foldersDropdown_.gameObject.SetActive( false );
        imagesBrowser_.gameObject.SetActive( false );
        filesScrollView_.gameObject.SetActive( false );
        btnEnterDirectory_.gameObject.SetActive( false );
        imageNameText_.gameObject.SetActive( false );
        textHint_.gameObject.SetActive( false );

        // Set up buttons listeners
        btnBrowseFolders_.onClick.AddListener( BrowseFolders );
        btnReturn_.onClick.AddListener( HideIntroScreen );
        btnQuit_.onClick.AddListener( QuitApplication );
        btnEnterDirectory_.onClick.AddListener( EnterDirectory );


        /*Test*/
        if( true )
        {
            HideIntroScreen();
            ChangeDirectory( "D:/Photos" );
        }
        else {
            // Show intro screen with selection of folder to browse
            ShowIntroScreen();
        }
    }

    // Update function
    void Update()
    {
    }

    // Show intro screen
    public void ShowIntroScreen()
    {
        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        // Set showing intro flag
        showingIntro_ = true;
        // Set black background
        fadeImage_.color = Color.black;

        // Show application name
        textAppName_.gameObject.SetActive( true );
        // Show button for browsing directories
        btnBrowseFolders_.gameObject.SetActive( true );
        // Show quit button
        btnQuit_.gameObject.SetActive( true );
        
        // If already in some directory, show return button
        if( directoryInfo_ != null )
        {
            btnReturn_.gameObject.SetActive( true );
        }
    }

    // Hide intro screen
    public void HideIntroScreen()
    {
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        // Clear showing intro flag
        showingIntro_ = false;
        // Set transparent background
        fadeImage_.color = Color.clear;

        // Hide application name
        textAppName_.gameObject.SetActive( false );
        // Hide button for browsing directories
        btnBrowseFolders_.gameObject.SetActive( false );
        // Hide quit button
        btnQuit_.gameObject.SetActive( false );

        // If already in some directory, hide return button
        if( directoryInfo_ != null )
        {
            btnReturn_.gameObject.SetActive( false );
        }
    }

    // Browse folders function
    void BrowseFolders()
    {
        // Get path from open folder panel
        string path = EditorUtility.OpenFolderPanel( "Select directory", "", "" );
        
        // Check if path was set (directory was selected)
        if( path != "" )
        {
            // Hide intro screen
            HideIntroScreen();
            // Set transparent background
            fadeImage_.color = Color.black;
            // Open selected directory
            ChangeDirectory( path );
        }
    }

    // Function for quitting the application
    void QuitApplication()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    // Fade in/out animation coroutine
    IEnumerator FadeAnimation()
    {
        // Set fading flag
        fading_ = true;

        // If fade image is already black, skip fading to black
        if( fadeImage_.color.a < 1.0f )
        {
            // Loop for almost 1 second
            for( float i = 0.0f; i <= 1.0f; i += 2 * Time.deltaTime )
            {
                // Fade from transparent to opaque
                fadeImage_.color = new Color( 0.0f, 0.0f, 0.0f, i );
                yield return null;
            }
        }
        // Set fade image to black. Just for sure
        fadeImage_.color = Color.black;

        /* TODO: Test */
        //// Get actual directory info. Load files into lists
        //GetDirectoryInfo();

        //// Hide every object in scene
        HideEverything();

        //// Show only objects that should be visible in scene
        //ShowObjects();
        /**/

        // Reset players position and rotation
        player_.GetComponent<Rigidbody>().MovePosition( defaultPlayerPosition_ );
        player_.GetComponent<Rigidbody>().MoveRotation( defaultPlayerRotation_ );

        // Wait for 2s to avoid laggy scene
        yield return new WaitForSeconds( 2 );
        // Loop for almost 1 second
        for( float i = 0.0f; i <= 1.0f; i += Time.deltaTime )
        {
            // Fade from opaque to transparent
            fadeImage_.color = new Color( 0.0f, 0.0f, 0.0f, 1.0f - i );
            yield return null;
        }
        // Set fade image to transparent. Just for sure
        fadeImage_.color = Color.clear;

        // Clear fading flag
        fading_ = false;
        yield return null;
    }

    // Function for directory changing
    public void ChangeDirectory( string path )
    {
        // Set directory info from given path
        directoryInfo_ = new DirectoryInfo( path );

        // Start fade coroutine
        StartCoroutine( FadeAnimation() );

        // Get actual directory info. Load files into lists
        GetDirectoryInfo();

        //// Hide every object in scene
        //HideEverything();

        //// Show only objects that should be visible in scene
        //ShowObjects();

        // Set paintings textures
        StartCoroutine( SetPaintingsTextures() );
    }

    // Get actual directory info. Fill images, other files, and directories info lists
    void GetDirectoryInfo()
    {
        // Get actual path
        string path = directoryInfo_.FullName;

        //
        // Cleanup
        //
        // Clear directories info list
        directories_.Clear();
        // Clear images info list
        imageFiles_.Clear();
        // Clear other files info list
        otherFiles_.Clear();
        
        //// Delete paintings' textures
        //for( int i = 0; i < paintingsObjects_.Count; i++ )
        //{
        //    paintingsObjects_[i].DeleteTexture();
        //}
        //// Force garbage collection
        //System.GC.Collect();

        // Fill directories info list
        foreach( string folderName in Directory.GetDirectories( path ) )
        {
            directories_.Add( new DirectoryInfo( folderName ) );
        }

        // Fill images and other files info lists
        foreach( string filename in Directory.GetFiles( path ) )
        {
            FileInfo file = new FileInfo( filename );
            if( Regex.IsMatch( filename.ToLower(), @".jpg|.png$" ) )
            {
                imageFiles_.Add( file );
            }
            else
            {
                otherFiles_.Add( file );
            }
        }

        // Set room info text 
        roomInfo_.text = 
            "Room name:\n" + directoryInfo_.Name + 
            "\n\nCreation time:\n" + directoryInfo_.CreationTime +
            "\n\nImages count: " + imageFiles_.Count +
            "\n\nStatues count: " + otherFiles_.Count;
    }

    // Hide objects in scene
    void HideEverything()
    {
        // Hide exit doors
        exitDoorsObject_.Deactivate();
        // Hide doors
        foreach( DirectoryProperties obj in doorsObjects_ )
        {
            obj.Deactivate();
        }
        // Hide paintings
        foreach( ImageProperties obj in paintingsObjects_ )
        {
            obj.Deactivate();
        }
        // Hide statues
        foreach( ObjectProperties obj in statuesObjects_ )
        {
            obj.Deactivate();
        }
        // Hide pile of paintings
        pileOfPaintings_.SetActive( false );
    }

    // Show objects in scene
    void ShowObjects()
    {
        // Check if not in root directory
        if( directoryInfo_.Parent != null )
        {
            // Show exit doors and set directory name and full path
            exitDoorsObject_.Activate();
            exitDoorsObject_.SetName( directoryInfo_.Parent.Name );
            exitDoorsObject_.SetFullPath( directoryInfo_.Parent.FullName );
        }

        // Clear dropdown list containing more directories than in scene
        foldersDropdown_.ClearOptions();
        // Go through directories list
        for( int i = 0; i < directories_.Count; i++ )
        {
            // Show doors and set directory name and full path
            if( i < doorsObjects_.Count )
            {
                doorsObjects_[i].Activate();
                doorsObjects_[i].SetName( directories_[i].Name );
                doorsObjects_[i].SetFullPath( directories_[i].FullName );
            }
            // There is more directories than doors
            else if( i == doorsObjects_.Count )
            {
                // Set last doors name to 'More directories ...'
                doorsObjects_[i - 1].SetName( "More directories ..." );
                doorsObjects_[i - 1].SetFullPath( "More directories ..." );

                // Fill dropdown list and select first item
                foldersDropdown_.options.Add( new Dropdown.OptionData( directories_[i - 1].Name ) );
                foldersDropdown_.options.Add( new Dropdown.OptionData( directories_[i].Name ) );
                foldersDropdown_.value = 0;
                foldersDropdown_.GetComponentInChildren<Text>().text = foldersDropdown_.options[0].text;
            }
            else
            {
                // Fill dropdown list with more directories
                foldersDropdown_.options.Add( new Dropdown.OptionData( directories_[i].Name ) );
            }
        }

        // Go through images list
        for( int i = 0; i < imageFiles_.Count; i++ )
        {
            // Show paintings and set painting name and full path
            paintingsObjects_[i].Activate();
            paintingsObjects_[i].SetName( imageFiles_[i].Name );
            //paintingsObjects_[i].SetFullName( imageFiles_[i].FullName );

            // Last painting
            if( i == paintingsObjects_.Count - 1 )
            {
                // If there is more images than paintings
                if( imageFiles_.Count > 10 )
                {
                    // Set last painting position on floor
                    paintingsObjects_[i].transform.position = lastPaintingFloorPosition_;
                    paintingsObjects_[i].transform.rotation = lastPaintingFloorRotation_;
                    // Set last painting name to 'More images ...'
                    paintingsObjects_[i].SetName( "More images ..." );
                    //paintingsObjects_[i].SetFullName( "More images ..." );
                    // Show pile of paintings on floor
                    pileOfPaintings_.SetActive( true );
                }
                else
                {
                    // Set last painting position on wall
                    paintingsObjects_[i].transform.position = lastPaintingWallPosition_;
                    paintingsObjects_[i].transform.rotation = lastPaintingWallRotation_;
                    // Hide pile of paintings on floor
                    pileOfPaintings_.SetActive( false );
                }
                break;
            }
        }

        // Clear files list containing name of more other files in scene
        filesScrollView_.content.GetComponent<Text>().text = "";
        // Go through other files list
        for( int i = 0; i < otherFiles_.Count; i++ )
        {
            // Show statues and set statue name and full path
            if( i < statuesObjects_.Count )
            {
                statuesObjects_[i].Activate();
                statuesObjects_[i].SetName( otherFiles_[i].Name );
                //statuesObjects_[i].SetFullName( otherFiles_[i].FullName );
            }
            // There is more other files than statues
            else if( i == statuesObjects_.Count )
            {
                statuesObjects_[i - 1].SetName( "More files ..." );
                //statuesObjects_[i - 1].SetFullName( "More files ..." );

                filesScrollView_.content.GetComponent<Text>().text += otherFiles_[i - 1].Name + "\n";
                filesScrollView_.content.GetComponent<Text>().text += otherFiles_[i].Name + "\n";
            }
            else
            {
                // Fill files list with more filenames
                filesScrollView_.content.GetComponent<Text>().text += otherFiles_[i].Name + "\n";
            }
        }
    }

    // Return fading flag
    public bool IsFading()
    {
        return fading_;
    }

    // Show more images. When there is more images than paintings
    public void ShowMoreImages()
    {
        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        // Set showing more objects flag
        showingMoreObjects_ = true;
        // Set black background
        fadeImage_.color = Color.black;
        // Show image for image browsing
        imagesBrowser_.gameObject.SetActive( true );
        // Show image name text
        imageNameText_.gameObject.SetActive( true );
        // Show hint text
        textHint_.gameObject.SetActive( true );
        // Set image to show ID
        imageToShow_ = 9;
        // Load first image
        imagesBrowser_.material.mainTexture = paintingsObjects_[imageToShow_].GetTexture();
        // Rotate image properly
        RescaleImagesBrowser();
        // Set first image name
        imageNameText_.text = imageFiles_[imageToShow_].Name;
        // Set hint text
        textHint_.text = "Press ESC to go back \nUse left and right arrows to navigate through images";
    }

    // Show previous image
    public void PreviousImage()
    {
        // Check if ID is greater than paintings count - 1
        if( imageToShow_ > paintingsObjects_.Count - 1 )
        {
            // Decrease ID of image to show
            imageToShow_--;
            ShowBrowsedImage();
        }
    }

    // Show next image
    public void NextImage()
    {
        // Check if ID is lower than image files count - 1
        if( imageToShow_ < imageFiles_.Count - 1 )
        {
            // Increase ID of image to show
            imageToShow_++;
            ShowBrowsedImage();
        }
    }

    // Set browsed image texture to image object
    void ShowBrowsedImage()
    {
        // Set image texture
        imagesBrowser_.material.mainTexture = GetImageAsTexture( imageToShow_ );

        // Rotate image properly
        RescaleImagesBrowser();

        // For proper image changing
        imagesBrowser_.gameObject.SetActive( false );
        imagesBrowser_.gameObject.SetActive( true );
        // Set image name text
        imageNameText_.text = imageFiles_[imageToShow_].Name;
    }

    // Rescale the ImagesBrowser according to image's orientation
    void RescaleImagesBrowser()
    {
        // Vertical orientation
        if( imagesBrowser_.material.mainTexture.height > imagesBrowser_.material.mainTexture.width )
        {
            imagesBrowser_.rectTransform.anchorMin = new Vector2( 0.3125f, 0.167f );
            imagesBrowser_.rectTransform.anchorMax = new Vector2( 0.6875f, 0.833f );
        }
        // Horizontal orientation
        else
        {
            imagesBrowser_.rectTransform.anchorMin = new Vector2( 0.25f, 0.25f );
            imagesBrowser_.rectTransform.anchorMax = new Vector2( 0.75f, 0.75f );
        }
    }

    // Show more files. When there is more other files than statues
    public void ShowMoreFiles()
    {
        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        // Set showing more objects flag
        showingMoreObjects_ = true;
        // Set black background
        fadeImage_.color = Color.black;
        // Show files list
        filesScrollView_.gameObject.SetActive( true );
        // Show hint text
        textHint_.gameObject.SetActive( true );
        // Set hint text
        textHint_.text = "Press ESC to go back";
    }

    // Show more directories. When there is more directories than doors
    public void ShowMoreDirectories()
    {
        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        // Set showing more objects flag
        showingMoreObjects_ = true;
        // Set black background
        fadeImage_.color = Color.black;
        // Show folders dropdown list
        foldersDropdown_.gameObject.SetActive( true );
        // Show button for entering the directory
        btnEnterDirectory_.gameObject.SetActive( true );
        // Show hint text
        textHint_.gameObject.SetActive( true );
        // Set hint text
        textHint_.text = "Press ESC to go back";
    }

    // Enter directory selected from dropdown list
    public void EnterDirectory()
    {
        // Get selected folder from dropdown list
        string folder = foldersDropdown_.GetComponentInChildren<Text>().text;
        // Check if folder was selected
        if( folder != "" )
        {
            // Hide UI elements used when showing more files/directories
            StopShowingMoreObjects();
            // Change directory to selected one
            ChangeDirectory( directoryInfo_.FullName + "/" + folder );
        }
    }

    // Return flag if showing intro screen
    public bool IsShowingIntro()
    {
        return showingIntro_;
    }

    // Return flag if showing more files/directories
    public bool IsShowingMoreObjects()
    {
        return showingMoreObjects_;
    }

    // Hide UI elements for showing more objects files/directories
    public void StopShowingMoreObjects()
    {
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        // Clear showing more objects flag
        showingMoreObjects_ = false;
        // Clear black background
        fadeImage_.color = Color.clear;
        // Hide image for image browsing
        imagesBrowser_.gameObject.SetActive( false );
        // Hide image name text
        imageNameText_.gameObject.SetActive( false );
        // Hide files list
        filesScrollView_.gameObject.SetActive( false );
        // Hide folders dropdown list
        foldersDropdown_.gameObject.SetActive( false );
        // Hide button for entering the directory
        btnEnterDirectory_.gameObject.SetActive( false );
        // Hide hint text
        textHint_.gameObject.SetActive( false );
    }

    // Coroutine for loading of textures to paintings
    IEnumerator SetPaintingsTextures()
    {
        // Delete paintings' textures
        for( int i = 0; i < paintingsObjects_.Count; i++ )
        {
            paintingsObjects_[i].DeleteTexture();
        }
        // Force garbage collection
        System.GC.Collect();

        // Initial wait for fading to black
        yield return new WaitForSeconds( 0.5f );
        ShowObjects();

        // Go through painting objects
        for( int i = 0; i < paintingsObjects_.Count; i++ )
        {
            // When reaching not active painting, return
            if( !paintingsObjects_[i].IsActive() )
            {
                break;
            }
            // Load texture and set it as painting material texture
            LoadImageToTexture( i, i );

            yield return null;
        }

        // If there is more images than paintings, polish last painting position
        if( imageFiles_.Count > paintingsObjects_.Count )
        {
            paintingsObjects_[paintingsObjects_.Count - 1].PolishPosition();
        }
    }

    // Set texture as painting material texture
    void LoadImageToTexture( int imageID, int paintingID )
    {
        paintingsObjects_[paintingID].SetTexture( GetImageAsTexture( imageID ) );
    }

    // Load image to texture and return the texture
    Texture2D GetImageAsTexture( int imageID )
    {
        WWW www = new WWW( "file://" + imageFiles_[imageID].FullName );
        Texture2D tex = new Texture2D( 2, 2 );
        www.LoadImageIntoTexture( tex );
        return tex;
    }

    /***TEST*****************
     * 
     * 
     * testing code for asynchronous texture loading
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
