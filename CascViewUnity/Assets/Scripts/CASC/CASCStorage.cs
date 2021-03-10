using System;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text.RegularExpressions;

public unsafe class CASCStorage
{
    System.IntPtr storageHandle;
    string filePath;

    string listFilePath;
    string csvFilePath;

    private int uniqueDirectoryID;

    // 10 Hashsets, 1 for each depth possible
    private Dictionary<int, HashSet<CASCFolder>> folderSet;

    public unsafe CASCStorage(string path, string _listFilePath=null, string _csvFilePath=null)
    {
        System.IntPtr handle;
        uint filecount=0;
        filePath = path;
        listFilePath = _listFilePath;
        csvFilePath = _csvFilePath;
        CASCMain.CASCFiles = new List<CASCFile>();
        CASCMain.CASCFolders = new List<CASCFolder>();

        //folderSet = new Dictionary<int, HashSet<CASCFolder>>();
        //for (int i = 0; i < 10; i++) {
        //    folderSet[i] = new HashSet<CASCFolder>();
        //}

        //uniqueDirectoryID = 0;        

        //uniqueDirectoryID++;

        try
        {
            if (!CASCFunc.CascOpenStorage(path, 0, out handle))
                throw new Exception("Not able to open");

            if (!CASCFunc.CascGetStorageInfo(handle, CASCFunc.CASC_STORAGE_INFO_CLASS.CascStorageTotalFileCount, out filecount, sizeof(uint), null))
                throw new Exception("No info available");
            CASCFunc.CASC_FIND_DATA findData;
            var fileHandle = CASCFunc.CascFindFirstFile(handle, "*", out findData, GetProperFilePath(handle));
            if (fileHandle != null)
            {
                do
                {
                    /*
                    char[] delimiterChars = { '\\' };
                    string[] splitPath = fullPath.Split(delimiterChars);
                    

                    int parentID = 0;
                    
                    if (count < 10000) {
                        for (int i = 0; i < splitPath.Length - 1; i++) {
                            if (i == 0) { parentID = 0; }
                            int myID = FindOrCreateDirectory(parentID, splitPath[i], i); // i cooresponds to folder depth
                            parentID = myID;
                        }
                    }
                    count++;

                    // splitPath[0] is one of the root directories
                    // everything in between is a sub directory, with duplicate names
                    // splitPath[splitPath.Length - 1] is the filename //
                    //string fileName = splitPath[splitPath.Length - 1];
                    */
                    int fileDataID = (int)findData.dwFileDataId;
                    int parentDirectory = 0; //delme
                    string fileName = "";
                    string fullPath = new string(findData.szFileName);
                    
                    

                    CASCMain.CASCFiles.Add(new CASCFile(fullPath, fileName, parentDirectory, fileDataID, (long)findData.FileSize, findData.bFileAvailable == 1, findData.CKey, findData.EKey));
                }
                while(CASCFunc.CascFindNextFile(fileHandle, out findData));
            }            

            //for (int i = 0; i < 10; i++) {
            //    Debug.Log(folderSet[i].Count());
            //}

            CASCFunc.CascFindClose(fileHandle);
            storageHandle = handle;
        }

        catch (Exception e)
        {
            Debug.Log(CASCMain.CASCFiles.Count);
            Debug.Log(e.Source);
        }
    }
    
    /*    
    private int FindOrCreateDirectory(int parentID, string value, int depth) {
        int directoryID = 0;

        //IEnumerable<CASCFolder> folders = CASCMain.CASCFolders.Where(folder => folder.ParentDirectoryID == parentID).Where(folder => folder.Value == value).ToList();
        IEnumerable<CASCFolder> siblingFolders = folderSet[depth].Where(folder => folder.ParentDirectoryID == parentID).ToList(); // Where(folder => folder.Value == value).ToList();

        IEnumerable<CASCFolder> folders = siblingFolders.Where(folder => folder.Value == value).ToList();

        if (folders.Count() == 0) { // No Folder found
            directoryID = uniqueDirectoryID;
            uniqueDirectoryID++;
            CASCFolder thisFolder = new CASCFolder(directoryID, parentID, value);
            folderSet[depth].Add(thisFolder);
            //CASCMain.CASCFolders.Add(thisFolder);
        }
        else if (folders.Count() > 1) { // Too many folders found
            Debug.LogError("ERROR: Found more than 1 folder with name: " + value + " and ParentID: " + parentID);
        }
        else { // Correct folder found
            foreach (CASCFolder folder in folders) { // will only run once
                directoryID = folder.DirectoryID;
            }
        }

        return directoryID;
    }*/

    public CASCFileStream OpenFile(String fileName)
    {
        System.IntPtr handle=System.IntPtr.Zero;
        System.IntPtr cstring=Marshal.StringToHGlobalAnsi(fileName);
        try
        {
            if(!CASCFunc.CascOpenFile(storageHandle, (void*)cstring, 0, 0, out handle))
                throw new Exception("Failed to open CASC File");
        }
        catch(Exception e)
        {
            Debug.LogError(e);
        }
        finally
        {
            Marshal.FreeHGlobal(cstring);
        }
        return new CASCFileStream(handle);
    }

    unsafe string GetProperFilePath(System.IntPtr handle)
    {
        uint dwFeatures;
        CASCFunc.CascGetStorageInfo(handle, CASCFunc.CASC_STORAGE_INFO_CLASS.CascStorageFeatures, out dwFeatures, sizeof(uint), null);
        if ((dwFeatures & 0x00000010) != 0)
        {
            Debug.Log("CSV File is selected");
            return csvFilePath;
        }

        if(((dwFeatures & 0x00000008) != 0))
        {
            Debug.Log("ListFile is selected");
            return listFilePath;
        }

        CASCFunc.CascGetStorageInfo(handle, CASCFunc.CASC_STORAGE_INFO_CLASS.CascStorageProduct, out dwFeatures, sizeof(uint), null);

        return null;
    }

    ~CASCStorage()
    {
        CASCFunc.CascCloseStorage(storageHandle);
    }

    public void Dispose()
    {
        CASCFunc.CascCloseStorage(storageHandle);
    }
}

