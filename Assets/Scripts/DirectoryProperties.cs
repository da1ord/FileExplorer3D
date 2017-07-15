using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectoryProperties : ObjectProperties
{
    // Directory full path
    protected string fullPath_;

    // Initialization
    void Start()
    {
        // Set directory name
        objectName_ = "Object";
        // Set directory path
        fullPath_ = "Path";
        // Deactivate object
        active_ = false;
    }

    // Update function
    void Update()
    {
    }

    // Get directory path
    public string GetFullPath()
    {
        return fullPath_;
    }
    // Set directory path
    public void SetFullPath( string fullPath )
    {
        fullPath_ = fullPath;
    }
}
