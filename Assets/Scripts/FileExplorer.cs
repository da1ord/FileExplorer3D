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

    GameObject exitDoors_;
    List<GameObject> doors_;
    List<GameObject> paintings_;
    List<GameObject> statues_;

    TextMesh roomInfo_;
    GameObject player_;
    Vector3 defaultPlayerPosition_ = new Vector3( -18f, 1f, 0f );
    Quaternion defaultPlayerRotation_ = Quaternion.AngleAxis( 90f, Vector3.up );

    bool fading_;
    bool showingMore_;
    int imageToShow_;

    // Use this for initialization
    void Start()
    {
        directories_ = new List<DirectoryInfo>();
        imageFiles_ = new List<FileInfo>();
        otherFiles_ = new List<FileInfo>();

        btnEnterDirectory_.onClick.AddListener( EnterDirectory );

        // Fill objects lists
        exitDoors_ = GameObject.Find( "ExitDoors" );
        doors_ = new List<GameObject>();
        foreach( Transform child in GameObject.Find( "Doors" ).transform )
        {
            child.gameObject.SetActive( false );
            doors_.Add( child.gameObject );
        }
        paintings_ = new List<GameObject>();
        foreach( Transform child in GameObject.Find( "Paintings" ).transform )
        {
            child.gameObject.SetActive( false );
            paintings_.Add( child.gameObject );
        }
        statues_ = new List<GameObject>();
        foreach( Transform child in GameObject.Find( "Statues" ).transform )
        {
            child.gameObject.SetActive( false );
            statues_.Add( child.gameObject );
        }

        roomInfo_ = GameObject.Find( "InfoSign" ).GetComponentInChildren<TextMesh>();

        player_ = GameObject.Find( "Player" );
        /* TEST */
        path_.text = "D:/Photos";
        
        // TODO: Get directory info from last directory dirs list
        directoryInfo_ = new DirectoryInfo( path_.text );

        fading_ = false;
        showingMore_ = false;
        imageToShow_ = 10;

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
    }

    void GetDirectoryInfo()
    {
        // TODO: Use this
        //string path = directoryInfo_.FullName;
        /* TEST */
        directoryInfo_ = new DirectoryInfo( path_.text );
        string path = directoryInfo_.FullName;


        directories_.Clear();
        imageFiles_.Clear();
        otherFiles_.Clear();
        
        // Cleanup
        for( int i = 0; i < paintings_.Count; i++ )
        {
            Destroy( paintings_[i].GetComponent<Renderer>().material.mainTexture );
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

        //Debug.Log( "----------------------------------" );
        //foreach( string file in Directory.GetFiles( path ) )
        //{

        //    Debug.Log( file );
        //}
        //Debug.Log( "----------------------------------" );
    }

    void HideEverything()
    {
        exitDoors_.SetActive( false );
        foreach( GameObject obj in doors_ )
        {
            obj.SetActive( false );
        }
        foreach( GameObject obj in paintings_ )
        {
            obj.SetActive( false );
        }
        foreach( GameObject obj in statues_ )
        {
            obj.SetActive( false );
        }
    }

    void ShowObjects()
    {
        if( directoryInfo_.Parent != null )
        {
            exitDoors_.SetActive( true );
            exitDoors_.GetComponentInChildren<TextMesh>().text = directoryInfo_.Parent.FullName;
        }

        foldersDropdown_.ClearOptions();
        for( int i = 0; i < directories_.Count; i++ )
        {
            if( i < doors_.Count )
            {
                doors_[i].SetActive( true );
                /* TEST */
                doors_[i].GetComponentInChildren<TextMesh>().text = directories_[i].Name;
                doors_[i].name = directories_[i].FullName;

                //// TODO: Write 'More directories ...' on the last door
                //doors_[i - 1].GetComponentInChildren<TextMesh>().text = "More directories ...";
                //doors_[i - 1].name = "More directories ...";

                //foldersDropdown_.ClearOptions();
                //foldersDropdown_.options.Add( new Dropdown.OptionData( directories_[i].Name ) );
                //break;
            }
            else if( i == doors_.Count )
            {
                // TODO: Write 'More directories ...' on the last door
                doors_[i - 1].GetComponentInChildren<TextMesh>().text = "More directories ...";
                doors_[i - 1].name = "More directories ...";

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
            if( i >= 10 ) // TODO: MAX_PAINTINGS
            {
                // Hide last painting on the wall
                paintings_[i - 1].SetActive( false );
                // Show pile of paintings (on floor)
                paintings_[i].SetActive( true );
                /* TEST */
                paintings_[i].GetComponentInChildren<TextMesh>().text = "More images ...";
                paintings_[i].name = "More images ...";
                // TODO: Show pile of paintings and render the 11th as the top one
                break;
            }
            paintings_[i].SetActive( true );
            /* TEST */
            paintings_[i].GetComponentInChildren<TextMesh>().text = imageFiles_[i].Name;
            paintings_[i].name = imageFiles_[i].FullName;
            //
            //SetPaintingsTextures( i );
        }

        filesScrollView_.content.GetComponent<Text>().text = "";
        for( int i = 0; i < otherFiles_.Count; i++ )
        {
            if( i < statues_.Count )
            {
                statues_[i].SetActive( true );
                /* TEST */
                statues_[i].GetComponentInChildren<TextMesh>().text = otherFiles_[i].Name;
                statues_[i].name = otherFiles_[i].FullName;
            }
            else if( i == statues_.Count )
            {
                // TODO: Write 'More files ...' on the last statue
                statues_[i - 1].GetComponentInChildren<TextMesh>().text = "More files ...";
                statues_[i - 1].name = "More files ...";
                //break;
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
        showingMore_ = true;
        fadeImage_.color = Color.black;
        imagesBrowser_.gameObject.SetActive( true );
        imageNameText_.gameObject.SetActive( true );
        imageToShow_ = 9;
        imagesBrowser_.material.mainTexture = paintings_[imageToShow_ + 1].GetComponent<Renderer>().material.mainTexture;
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
        showingMore_ = true;
        fadeImage_.color = Color.black;
        filesScrollView_.gameObject.SetActive( true );
    }

    public void ShowMoreDirectories()
    {
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

        yield return new WaitForSeconds( 0.5f );
        for( int i = 0; i < paintings_.Count; i++ )
        {
            if( !paintings_[i].activeSelf  )
            {
                if( imageFiles_.Count > 10 )
                {
                    LoadImageToTexture( i, i + 1 );
                    //www = new WWW( "file://" + imageFiles_[i].FullName );
                    //tex = new Texture2D( 2, 2 );
                    //www.LoadImageIntoTexture( tex );
                    //paintings_[i + 1].GetComponent<Renderer>().material.mainTexture = tex;
                }
                break;
            }

            LoadImageToTexture( i, i );
            //www = new WWW( "file://" + imageFiles_[i].FullName );
            //tex = new Texture2D( 2, 2 );
            //www.LoadImageIntoTexture( tex );
            //paintings_[i].GetComponent<Renderer>().material.mainTexture = tex;


            //byte[] fileData = File.ReadAllBytes( imageFiles_[i].FullName );
            //Texture2D tex = new Texture2D( 2, 2 );
            //tex.LoadImage( fileData );
            //paintings_[i].GetComponent<Renderer>().material.mainTexture = tex;

            yield return null;
        }
    }

    void LoadImageToTexture( int imageID, int paintingID )
    {
        //WWW www = new WWW( "file://" + imageFiles_[imageID].FullName );
        //Texture2D tex = new Texture2D( 2, 2 );
        //www.LoadImageIntoTexture( tex );
        paintings_[paintingID].GetComponent<Renderer>().material.mainTexture = GetImageAsTexture( imageID );//tex;
    }

    Texture2D GetImageAsTexture( int imageID )
    {
        WWW www = new WWW( "file://" + imageFiles_[imageID].FullName );
        Texture2D tex = new Texture2D( 2, 2 );
        www.LoadImageIntoTexture( tex );
        return tex;
    }
}
