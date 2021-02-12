using System.Runtime.InteropServices;

public class CASCFunc
{
    public enum CASC_STORAGE_INFO_CLASS
    {
        CascStorageLocalFileCount,
        CascStorageTotalFileCount,
        CascStorageFeatures,                      
        CascStorageInstalledLocales,              
        CascStorageProduct,                       
        CascStorageTags,                          
        CascStoragePathProduct,                   
        CascStorageInfoClassMax
    }

    public enum CASC_NAME_TYPE
    {
        CascNameFull,                              
        CascNameDataId,                            
        CascNameCKey,                              
        CascNameEKey                               
    }



    public enum CASC_FILE_INFO_CLASS
    {
        CascFileContentKey,
        CascFileEncodedKey,
        CascFileFullInfo,                           // Gives CASC_FILE_FULL_INFO structure
        CascFileSpanInfo,                           // Gives CASC_FILE_SPAN_INFO structure for each file span
        CascFileInfoClassMax
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct CASC_FIND_DATA
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public char[] szFileName;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x04)]
        byte[] temp;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] CKey;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] EKey;
        public long TagBitMask;
        public long FileSize;
        public char* szPlainName;
        public int dwFileDataId;
        public int dwLocaleFlags;
        public int dwContentFlags;
        public int dwSpanCount;
        [MarshalAs(UnmanagedType.I1)]
        public byte bFileAvailable;
        CASC_NAME_TYPE NameType;

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]

    public struct CASC_FILE_SPAN_INFO
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] CKey;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] EKey;
        public long StartOffset;                      // Starting offset of the file span
        public long EndOffset;                        // Ending offset of the file span
        public int ArchiveIndex;                         // Index of the archive
        public int ArchiveOffs;                          // Offset in the archive
        public int HeaderSize;                           // Size of encoded frame headers
        public int FrameCount;                           // Number of frames in this span
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CASC_FILE_FULL_INFO
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] CKey;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] EKey;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public char[] DataFileName;
        public long StorageOffset;                    // Offset of the file over the entire storage
        public long SegmentOffset;                    // Offset of the file in the segment file ("data.###")
        public long TagBitMask;                       // Bitmask of tags. Zero if not supported
        public long FileNameHash;                     // Hash of the file name. Zero if not supported
        public long ContentSize;                      // Content size of all spans
        public long EncodedSize;                      // Encoded size of all spans
        public int SegmentIndex;                         // Index of the segment file (aka 0 = "data.000")
        public int SpanCount;                            // Number of spans forming the file
        public int FileDataId;                           // File data ID. CASC_INVALID_ID if not supported.
        public int LocaleFlags;                          // Locale flags. CASC_INVALID_ID if not supported.
        public int ContentFlags;                         // Locale flags. CASC_INVALID_ID if not supported
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x04)]
        public byte[] temp;

    }   

    [DllImport("_ascLib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    unsafe public static extern bool CascGetFileSize64(System.IntPtr hFile, long* PtrFileSize);

    [DllImport("CascLib", CallingConvention =CallingConvention.Winapi)]
    unsafe public static extern bool CascOpenStorage(string szParams, System.UInt32 dwLocaleMask, out System.IntPtr phStorage);

    [DllImport("CascLib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Winapi)]
    unsafe public static extern bool CascGetStorageInfo(System.IntPtr hStorage, CASC_STORAGE_INFO_CLASS InfoClass, out uint pvStorageInfo, uint cbStorageInfo,uint* pcbLengthNeeded);

    [DllImport("CascLib", SetLastError =true)]
    unsafe public static extern System.IntPtr CascFindFirstFile(System.IntPtr hStorage, string szMask,out CASC_FIND_DATA pFindData, string szListFile);

    [DllImport("CascLib")]
    unsafe public static extern bool CascFindNextFile(System.IntPtr hFind, out CASC_FIND_DATA pFindData);

    [DllImport("CascLib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Winapi)]
    unsafe public static extern bool CascFindClose(System.IntPtr hFind);
    
    [DllImport("CascLib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Winapi,SetLastError =false)]
    unsafe public static extern bool  CascOpenFile(System.IntPtr hStorage, void* pvFileName, System.UInt32 dwLocaleFlags, System.UInt32 dwOpenFlags, out System.IntPtr PtrFileHandle);

    [DllImport("CascLib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Winapi)]
    unsafe public static extern bool CascCloseStorage(System.IntPtr hStorage);

    [DllImport("CascLib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Winapi)]
    unsafe public static extern bool CascReadFile(System.IntPtr hFile, System.IntPtr lpBuffer,int dwToRead,  out int pdwRead);

    [DllImport("CascLib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Winapi)]
    unsafe public static extern bool CascSetFilePointer64(System.IntPtr hFile, long DistanceToMove, out long PtrNewPos, System.UInt32 dwMoveMethod);

    [DllImport("CascLib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Winapi)]
    unsafe public static extern bool CascCloseFile(System.IntPtr hFile);

    [DllImport("CascLib", SetLastError =true, CallingConvention = CallingConvention.StdCall)]
    unsafe public static extern bool CascGetFileInfo(System.IntPtr hFile, CASC_FILE_INFO_CLASS InfoClass, out CASC_FILE_FULL_INFO pvFileInfo, uint cbFileInfo, uint* pcbLengthNeeded);   
    
    [DllImport("CascLib", SetLastError =true, CallingConvention = CallingConvention.StdCall)]
    unsafe public static extern bool CascGetFileInfo(System.IntPtr hFile, CASC_FILE_INFO_CLASS InfoClass, System.IntPtr pvFileInfo, uint cbFileInfo, uint* pcbLengthNeeded);
}
