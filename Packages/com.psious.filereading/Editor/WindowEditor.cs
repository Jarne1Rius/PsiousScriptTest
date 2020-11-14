using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

public class WindowEditor : EditorWindow
{
    private string m_FileName = "DirectoriesPsious";
    private string m_SpacingDirectoryNames = "\t";
    [MenuItem("Tools/FileTester")]

    public static void ShowWindow()
    {
        AssetDatabase.Refresh();
        EditorWindow window = EditorWindow.GetWindow(typeof(WindowEditor));
        window.maxSize = new Vector2(520, 100);
        window.minSize = new Vector2(520, 100);
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 0, 500, 100));
        GUILayout.Label("Compare from file", EditorStyles.boldLabel);
        m_FileName = EditorGUILayout.TextField("Name of the file", m_FileName);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Spacing between directories in directories in Unity");
        m_SpacingDirectoryNames = EditorGUILayout.TextField(m_SpacingDirectoryNames);
        GUILayout.EndHorizontal();
        
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("CheckFiles"))
        {
            string[] result = AssetDatabase.FindAssets(m_FileName);
            foreach (string files in result)
            {
                CompareFolderStructure(ReadFromFile(AssetDatabase.GUIDToAssetPath(files)), GetFolders("Assets"));
            }
        }
        GUILayout.EndArea();
    }

    List<string> ReadFromFile(string path)
    {
        List<string> directoryStructure = new List<string>();
        StreamReader reader = new StreamReader(path);
        string rootDirectory = "";
        string prefRootDirectory = "";
        int sizeOfPreviousDirectory = 0;

        while (!reader.EndOfStream)
        {
            string readLine = reader.ReadLine();
            Regex test = new Regex("(" + m_SpacingDirectoryNames + "*)(.*)");
            Match match = Regex.Match(readLine, test.ToString());
            if (match.Success)
            {
                int sizeToRootDirectory = match.Groups[1].Length;
                //If object is two or more folders up( does not exist )
                if (sizeOfPreviousDirectory == 0 && sizeToRootDirectory > 1)
                {
                    readLine = match.Groups[2].ToString();
                }
                else
                {
                    if (sizeToRootDirectory > sizeOfPreviousDirectory)
                    {
                        sizeOfPreviousDirectory = sizeToRootDirectory;
                        rootDirectory += prefRootDirectory + '\\';
                    }
                    else if (sizeToRootDirectory < sizeOfPreviousDirectory)
                    {
                        rootDirectory = rootDirectory.Substring(0, rootDirectory.IndexOf('\\') + 1);
                        sizeOfPreviousDirectory = sizeToRootDirectory;
                    }
                    if (sizeToRootDirectory > 0)
                    {
                        readLine = rootDirectory + match.Groups[2];
                    }
                    prefRootDirectory = match.Groups[2].ToString();
                }
                directoryStructure.Add(readLine);
            }
        }

        reader.Close();

        return directoryStructure;
    }

    List<string> GetFolders(string folderParent)
    {
        List<string> dir = new List<string>();
        dir.Add(folderParent);
        string[] directories = Directory.GetDirectories(folderParent);
        foreach (string directory in directories)
        {
            dir.AddRange(GetFolders(directory));
        }
        return dir;
    }

    void CompareFolderStructure(List<String> directoriesInFile, List<string> directoriesInUnity)
    {
        foreach (string directoryUnity in directoriesInUnity)
        {
            foreach (string directoryFile in directoriesInFile)
            {
                if (directoryFile == directoryUnity)
                {
                    directoriesInFile.Remove(directoryFile);
                    break;
                }
            }
        }

        //Send to new Window with errors
        string totErrors = "";
        foreach (string folder in directoriesInFile)
        {
            totErrors += folder + " Not in folder structure\n";
        }
        WindowError errorWindow = CreateWindow<WindowError>();
        errorWindow.Errors = totErrors;

        if (directoriesInFile.Count == 0)
        {
            errorWindow.Complete = "Folders correspond to the file";
        }
    }
}

public class WindowError : EditorWindow
{
    public string Errors { get; set; }
    public string Complete { get; set; }

    private void OnGUI()
    {
        GUI.contentColor = Color.red;
        if (Errors != "")
            GUILayout.Label(Errors);
        GUI.contentColor = Color.green;
        GUILayout.Label(Complete);
    }
}