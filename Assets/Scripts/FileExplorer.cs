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
    // Toggle for enabling of asynchronous loading of images
    public Toggle tglAsyncLoading_;
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
    // Flag indicating if fade to black is over
    bool blackScreen_;

    // 10th painting position vector if hanging on the wall
    Vector3 lastPaintingWallPosition_ = new Vector3( 15f, 4f, -10f );
    // 10th painting rotation quaternion if hanging on the wall
    Quaternion lastPaintingWallRotation_ = Quaternion.Euler( -90f, 0f, 0f );
    // 10th painting position vector if on the floor
    Vector3 lastPaintingFloorPosition_ = new Vector3( 14f, 1.57f, -9.065f );
    // 10th painting rotation quaternion if on the floor
    Quaternion lastPaintingFloorRotation_ = Quaternion.Euler( -105f, 0f, 0f );
    
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
        // Initialize scene objects
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
        // Clear black screen flag
        blackScreen_ = false;

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
        // Show async loading toggle
        tglAsyncLoading_.gameObject.SetActive( true );
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
        // Hide async loading toggle
        tglAsyncLoading_.gameObject.SetActive( false );
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

    // Show objects in scene and set their names
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

            // Last painting
            if( i == paintingsObjects_.Count - 1 )
            {
                // If there is more images than paintings
                if( imageFiles_.Count > paintingsObjects_.Count )
                {
                    // Set last painting position on floor
                    paintingsObjects_[i].transform.position = lastPaintingFloorPosition_;
                    paintingsObjects_[i].transform.rotation = lastPaintingFloorRotation_;
                    // Set last painting name to 'More images ...'
                    paintingsObjects_[i].SetName( "More images ..." );
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
            }
            // There is more other files than statues
            else if( i == statuesObjects_.Count )
            {
                statuesObjects_[i - 1].SetName( "More files ..." );

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

    // Function for directory changing
    public void ChangeDirectory( string path )
    {
        // Set directory info from given path
        directoryInfo_ = new DirectoryInfo( path );

        // Get actual directory info. Load files into lists
        GetDirectoryInfo();

        // TODO: In case of async texture loading, stop all running coroutines
        StopAllCoroutines();

        // Start fade coroutine
        StartCoroutine( FadeAnimation() );

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

    // Fade in/out animation coroutine
    IEnumerator FadeAnimation()
    {
        // Set fading flag
        fading_ = true;

        // If fade image is already black, skip fading to black part
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

        // Hide every object in scene
        HideEverything();

        // Indicate that fade to black is over
        blackScreen_ = true;

        // Reset players position and rotation
        player_.GetComponent<Rigidbody>().MovePosition( defaultPlayerPosition_ );
        player_.GetComponent<Rigidbody>().MoveRotation( defaultPlayerRotation_ );

        // If folder contains images wait some time to load them
        if( imageFiles_.Count > 0 && !tglAsyncLoading_.isOn )
        {
            // Wait in black for 2s to avoid laggy scene
            yield return new WaitForSeconds( 2.0f );
        }
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
        // Clear fade to black flag
        blackScreen_ = false;
        yield return null;
    }

    // Coroutine for loading of textures to paintings
    IEnumerator SetPaintingsTextures()
    {
        // Wait until fade to black is over to avoid seeing laggy screen
        while( !blackScreen_ )
        {
            yield return new WaitForSeconds( 0.1f );
        }

        // Delete paintings' textures
        for( int i = 0; i < paintingsObjects_.Count; i++ )
        {
            paintingsObjects_[i].DeleteTexture();
        }
        // Delete ImagesBrowser's loaded texture
        Destroy( imagesBrowser_.material.mainTexture );
        // Force garbage collection
        System.GC.Collect();

        // Show objects in scene
        ShowObjects();

        // Go through painting objects
        for( int i = 0; i < paintingsObjects_.Count; i++ )
        {
            // When reaching not active painting, return
            if( !paintingsObjects_[i].IsActive() )
            {
                break;
            }

            // Asynchronous loading of images is enabled
            if( tglAsyncLoading_.isOn )
            {
                StartCoroutine( LoadImageToTextureAsync( i ) );
            }
            // Asynchronous loading of images is disabled
            else
            {
                // Load texture and set it as painting material texture
                LoadImageToTexture( i );
            }

            yield return null;
        }
    }

    // Set texture as painting material texture
    void LoadImageToTexture( int imageID )
    {
        paintingsObjects_[imageID].SetTexture( GetImageAsTexture( imageID ) );

        // If there is more images than paintings and if this image being 
        //  processed is for the last painting, polish the painting position
        if( imageFiles_.Count > paintingsObjects_.Count && imageID == paintingsObjects_.Count - 1 )
        {
            paintingsObjects_[imageID].PolishPosition();
        }
    }

    // Load image to texture using WWW class and return the texture
    Texture2D GetImageAsTexture( int imageID )
    {
        WWW www = new WWW( "file://" + imageFiles_[imageID].FullName );
        Texture2D tex = new Texture2D( 2, 2 );
        www.LoadImageIntoTexture( tex );
        return tex;
    }

    //
    // Functions for showing/hiding UI regarding showing of more objects
    //
    #region Functions for showing/hiding UI regarding showing of more objects
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
        imagesBrowser_.material.mainTexture = GetImageAsTexture( imageToShow_ );//paintingsObjects_[imageToShow_].GetTexture();
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
    #endregion

    //
    // Getters/setters
    //
    #region Getters/setters
    // Return fading flag
    public bool IsFading()
    {
        return fading_;
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
    #endregion


    //
    // Asynchronous texture loading functions
    //
    // Set texture as painting material texture - asynchronous
    IEnumerator LoadImageToTextureAsync( int imageID )
    {
        WWW www = new WWW( "file://" + imageFiles_[imageID].FullName );
        yield return www;

        // Set painting's texture over time
        StartCoroutine( CopyTextureAsync( www.texture, imageID ) );
    }

    // Set chunks of painting's texture over time
    IEnumerator CopyTextureAsync( Texture2D srcTex, int paintingID )
    {
        // Waiting time constant. Reduce this if still laggy
        const int WAIT_TIME = 15000;
        // Initialize loop counter
        int loopCounter = 0;

        // Get texture dimensions
        int heightSize = srcTex.height;
        int widthSize = srcTex.width;

        //Create new Empty texture with size that matches source info
        Texture2D tex = new Texture2D( widthSize, heightSize );//, srcTex.format, useMipMap );

        // Go through texture and set destination texture pixels
        for( int y = 0; y < heightSize; y++ )
        {
            for( int x = 0; x < widthSize; x++ )
            {
                // Get pixel at specific pos from source texture
                Color tempSourceColor = srcTex.GetPixel( x, y );

                // Set texture's pixel at specified pos
                tex.SetPixel( x, y, tempSourceColor );

                // Increase loop counter
                loopCounter++;

                // Check if wait time elapsed
                if( loopCounter % WAIT_TIME == 0 )
                {
                    yield return null;
                }
            }
        }
        //Apply changes to the Texture
        tex.Apply();
        // Set painting's texture
        paintingsObjects_[paintingID].SetTexture( tex );

        // If there is more images than paintings and if this image being 
        //  processed is for the last painting, polish the painting position
        if( imageFiles_.Count > paintingsObjects_.Count && paintingID == paintingsObjects_.Count - 1 )
        {
            paintingsObjects_[paintingID].PolishPosition();
        }
    }
}
