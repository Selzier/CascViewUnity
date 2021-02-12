using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Runtime.InteropServices;

public class CASCMain : MonoBehaviour
{
    public string selectedPath;
    public string listFilePath;
    public string csvFilePath;


    [HideInInspector] public string outputPath;
    CASCStorage storage;
    public static List<CASCFile> CASCFiles;

    private void OnEnable()
    {
        CASCFiles = new List<CASCFile>();
    }

    IEnumerator Start()
    {

        if (outputPath == "")
        {
            outputPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)+"\\ExtractedCSACPath\\";
        }
        OpenStorage();
        yield return null;
        ExtractFile(CASCFiles);
        yield return null;
    }


    private void OpenStorage()
    {
        CASCFiles.Clear();
        storage?.Dispose();
        try
        {
            
            storage = new CASCStorage(selectedPath, listFilePath, csvFilePath);
        }
        catch(Exception e)
        {
            storage?.Dispose();
            Debug.Log(e);
            Debug.LogError("Failed to open CASC Storage. Ensure that the storage is intact and exists at the given location.");
        }
    }

    private void ExtractFile(List<CASCFile> FileList)
    {
        var files = FileList;
        if (files.Count == 0)
        {
            Debug.Log("No files provided to extract.");
            return;
        }

        StartCoroutine(ExtractCoroutine(FileList));
    }

    IEnumerator ExtractCoroutine(List<CASCFile> files)
    {

        print("Number of files to extract:" + files.Count);

        int filesProcessed = 0;
        foreach (var file in files)
        {
            yield return null;
            if (file.IsLocal == "Yes")
            {
                string outputLocation = outputPath + file.FileName.Replace("\0", String.Empty); /// For saving the files such that it respect the directory
                //string outputLocation = outputPath + ReplaceInvalidChars(file.FileName);      /// For saving the files without directory...using for testing purposes
                Directory.CreateDirectory(Path.GetDirectoryName(outputLocation));
                try
                {
                    var input = storage.OpenFile(file.FileName);
                    using (var output = File.Create(outputLocation))
                    {
                        System.IntPtr pspanPointer;
                        CASCFunc.CASC_FILE_FULL_INFO fileInfo;
                        if (input.GetInfo(out fileInfo, out pspanPointer))
                        {
                            for (int i = 0; i < fileInfo.SpanCount; i++)
                            {
                                var pspan = Marshal.PtrToStructure<CASCFunc.CASC_FILE_SPAN_INFO>(IntPtr.Add(pspanPointer,i*Marshal.SizeOf(typeof(CASCFunc.CASC_FILE_SPAN_INFO))));
                                int cbFileSpan = (int)(pspan.EndOffset - pspan.StartOffset);
                                if (cbFileSpan == 0)
                                    continue;
                                byte[] buffer;
                                int bytesRead;
                                bool readOK= input.Read(out buffer, cbFileSpan,out bytesRead);
                                if (readOK)
                                {
                                    output.Write(buffer, 0, bytesRead);
                                    if (bytesRead < cbFileSpan)
                                        break;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }
            }
            filesProcessed++;
            Debug.Log("Progress:" + filesProcessed + "/" + files.Count+" FileName:"+file.FileName);
        }
        yield return null;
    }


    private void OnDisable()
    {
        CASCFiles.Clear();
    }

    string ReplaceInvalidChars(string filename)
    {
        filename.TrimEnd();
        return string.Join("", filename.Split(Path.GetInvalidFileNameChars()));
    }




}
