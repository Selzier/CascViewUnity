using System;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public unsafe class CASCStorage
{
    System.IntPtr storageHandle;
    string filePath;

    string listFilePath;
    string csvFilePath;

    public unsafe CASCStorage(string path,string _listFilePath=null,string _csvFilePath=null)
    {
        System.IntPtr handle;
        uint filecount=0;
        filePath = path;
        listFilePath = _listFilePath;
        csvFilePath = _csvFilePath;
        CASCMain.CASCFiles = new List<CASCFile>();

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
                    CASCMain.CASCFiles.Add(new CASCFile(new string(findData.szFileName), (long)findData.FileSize, findData.bFileAvailable == 1, findData.CKey, findData.EKey));
                    ///Add directory here////////////
                }
                while(CASCFunc.CascFindNextFile(fileHandle, out findData));
            }
            CASCFunc.CascFindClose(fileHandle);
            storageHandle = handle;
        }

        catch (Exception e)
        {
            Debug.Log(CASCMain.CASCFiles.Count);
            Debug.Log(e.Source);
        }
    }

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

