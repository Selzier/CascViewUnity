
using System;
using System.Runtime.InteropServices;
using UnityEngine;

public unsafe class CASCFileStream 
{

	System.IntPtr FileHandle;

	public CASCFileStream(System.IntPtr handle)
    {
		FileHandle = handle;
    }

	public System.Int64 Length
	{
		get
		{
			System.Int64 Result = 0;
			CASCFunc.CascGetFileSize64(FileHandle, &Result);
			return Result;
		}
	}


	public bool Read(out byte[] buffer,System.Int32 count,out int bytesRead)
	{
		buffer = new byte[count];	
		IntPtr tempBuffer = Marshal.AllocHGlobal(count);
		if (tempBuffer == System.IntPtr.Zero)
        {
			Debug.LogError("Not Enough Memory");
			bytesRead = 0;
			return false;
        }
		var result=CASCFunc.CascReadFile(FileHandle, tempBuffer, count, out bytesRead);
		Marshal.Copy(tempBuffer, buffer, 0, count);
		Marshal.FreeHGlobal(tempBuffer);
		return result;
	}

	System.Int64 Seek(System.Int64 offset, System.UInt32 origin)
	{
		long result;
		CASCFunc.CascSetFilePointer64(FileHandle, offset, out result, origin);
		return result;
	}


	~CASCFileStream()
	{
		CASCFunc.CascCloseFile(FileHandle);
	}


    public bool GetInfo(out CASCFunc.CASC_FILE_FULL_INFO fileInfo,out System.IntPtr pspanInfoPointer)
    {
        if (CASCFunc.CascGetFileInfo(FileHandle, CASCFunc.CASC_FILE_INFO_CLASS.CascFileFullInfo,out fileInfo, (uint)Marshal.SizeOf(typeof(CASCFunc.CASC_FILE_FULL_INFO)),null))
        {
			pspanInfoPointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(CASCFunc.CASC_FILE_SPAN_INFO)));
			if(CASCFunc.CascGetFileInfo(FileHandle,CASCFunc.CASC_FILE_INFO_CLASS.CascFileSpanInfo, pspanInfoPointer, (uint)Marshal.SizeOf(typeof(CASCFunc.CASC_FILE_SPAN_INFO)),null))
            {
				 var pspanInfo = Marshal.PtrToStructure<CASCFunc.CASC_FILE_SPAN_INFO>(pspanInfoPointer);
				return true;
            }
        }
		pspanInfoPointer = System.IntPtr.Zero;
		return false;
    }

}
