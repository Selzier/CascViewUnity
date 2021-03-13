using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Linq;
using SimpleSQL;
using M2Lib;
using M2Lib.m2;
using UnityEngine.Rendering;

public class CASCMain : MonoBehaviour
{
    /*public enum ChunkID
    {
        MD20 = 808600653,
        MD21 = 825377869,
        PFID = 1145652816,
        SFID = 1145652819,
        AFID = 1145652801,
        BFID = 1145652802,
        TXAC = 1128355924,
        EXPT = 1414551621,
        EXP2 = 844126277,
        PABC = 1128415568,
        PADC = 1128546640,
        PSBC = 1128420176,
        PEDC = 1128547664,
        SKID = 1145654099,
        TXID = 1145657428,
        LDV1 = 827737164,
        RPID = 1145655378,
        GPID = 1145655367,
        WFV1 = 827737687,
        WFV2 = 844514903,
        PGD1 = 826558288,
        WFV3 = 861292119,
        PFDC = 1128547920,
        EDGF = 1179075653,
        NERF = 1179796814,
        DETL = 1280591172
    }*/

    public string selectedPath;
    public string listFilePath;
    public string csvFilePath;
    //public string cascFileToExport;
    public int imageFileDataID;

    public Material testMaterial;
    public static SimpleSQLManager dbManager;

    [HideInInspector] public string outputPath;
    CASCStorage storage;
    //public static List<CASCFile> CASCFiles;
    //public static List<CASCFolder> CASCFolders;
    public static List<CASCFile> CASCFiles;
    public static List<CASCFolder> CASCFolders;

    private void OnEnable() {
        //CASCFiles = new List<CASCFile>();
        CASCFiles = new List<CASCFile>();
        dbManager = GetComponent<SimpleSQLManager>();
    }

    IEnumerator Start() {
        if (outputPath == "") {
            outputPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\ExtractedCSACPath\\";
        }
        OpenStorage();
        yield return null;
        //ExtractFile(CASCFiles);
        /*
        List<CASCFile> fileToExport = new List<CASCFile>();
        for (int i = 0; i < CASCFiles.Count; i++) {
            if (CASCFiles[i].FileName.Contains(cascFileToExport)) {
                fileToExport.Add(CASCFiles[i]);
                Debug.Log("Found: " + cascFileToExport);
            }
        }
        if (fileToExport.Count == 0) { Debug.Log("Cannot find file in CASC: " + cascFileToExport); }*/
        //ExtractAllFiles(fileToExport);

        // Mesh: 1859623

        (Mesh m2Mesh, Texture2D[] textures) = M2ToMesh(GetCascFile(imageFileDataID));
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.sharedMesh = m2Mesh;
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        renderer.material = testMaterial;
        testMaterial.mainTexture = textures[1];



        // Texture: 158918
        //Texture2D blpTexture = BlpToTexture2d(GetCascFile(imageFileDataID));
        //testMaterial.SetTexture("_MainTex", blpTexture);

        yield return null;
    }

    public ListFileStructured GetFileInfoFromDB(int fileDataID) {
        List<ListFileStructured> match = CASCMain.dbManager.Query<ListFileStructured>("SELECT * FROM files WHERE FileDataID = " + fileDataID + ";");
        if (match.Count > 0) {
            return match[0];
        }
        else {
            return null;
        }
    }

    public CASCFile GetCascFile(int fileDataID) {
        //IEnumerable<CASCFile> files = from CascFile in CASCFiles
        //                              where CascFile.FileDataID == imageFileDataID
        //                              select CascFile;
        IEnumerable<CASCFile> files = CASCFiles.Where(file => file.FileDataID == fileDataID).ToList();
        if (files.Count() > 0) {
            return files.First();
        }
        else {
            return null;
        }
    }

    private void OpenStorage() {
        CASCFiles.Clear();
        storage?.Dispose();
        try {
            storage = new CASCStorage(selectedPath, listFilePath, csvFilePath);
        }
        catch (Exception e) {
            storage?.Dispose();
            Debug.Log(e);
            Debug.LogError("Failed to open CASC Storage. Ensure that the storage is intact and exists at the given location.");
        }
    }

    /// <summary>
    /// Extract all files in FileList to the Hard Drive.
    /// Exports native CASC files, for example this will export BLP files, not PNGs.
    /// </summary>
    /// <param name="FileList">List of files to be extracted</param>
    private void ExtractAllFiles(List<CASCFile> FileList) {
        var files = FileList;
        if (files.Count == 0) {
            Debug.Log("No files provided to extract.");
            return;
        }

        StartCoroutine(ExtractCoroutine(FileList));
    }

    public (Mesh, Texture2D[]) M2ToMesh(CASCFile file) {
        Mesh m2Mesh = new Mesh();
        BinaryReader br = new BinaryReader(new MemoryStream(CascFileBytes(file))); // New BinaryReader for the file in MemoryStream
        br.BaseStream.Seek(0, SeekOrigin.Begin);    // Go the the beginning of the stream

        // Manually Parsing the File             
        /*
        while (br.BaseStream.Position < br.BaseStream.Length) {                      // Advance the reader position until the end of the file
            //string chunkID = System.Text.Encoding.UTF8.GetString(br.ReadBytes(4)); // Reading first 4 bytes of a chunk will give us the ID
            //int chunkID = (int)br.ReadUInt32();                                    // Maybe better to use INT for performance, opting for string for readability
            ChunkID chunkID = (ChunkID)br.ReadUInt32();
            long chunkSize = (long)br.ReadUInt32();                                  // Get the size in bytes of the current chunk
            long nextChunkPos = br.BaseStream.Position + chunkSize;                  // The next chunk will be from the current reader's position + size of current chunk

            // Process current chunk 
            switch (chunkID) {
                case ChunkID.MD21:  // MD21 Header
                    ParseChunkMD21(); 
                    break;                
                case ChunkID.SFID:  // Skin File IDs
                    ParseChunkSFID();
                    break;
                case ChunkID.TXID:  // Texture File IDs
                    ParseChunkTXID();
                    break;
            }
            br.BaseStream.Seek(nextChunkPos, SeekOrigin.Begin);  // Advance the reader position to the the next chunk and proceed with loop
            Debug.Log("Processed ChunkID: " + chunkID);          // Name of current chunk
        }*/

        var model = new M2();
        model.Load(br);

        //Debug.Log(model.Name + " loaded");
        //Debug.Log(model.Version);
        Debug.Log("nViews: " + model.nViews);
        Debug.Log("Texture Count: " + model.Textures.Count);
        Texture2D[] textures = new Texture2D[model.Textures.Count];
        for (int i = 0; i < model.Textures.Count; i++) {
            Debug.Log("Texture File: " + model.Textures[i].Name + "FileDataID: " + model.Textures[i].FileDataID);
            Texture2D tex = BlpToTexture2d(GetCascFile(model.Textures[i].FileDataID));
            textures[i] = tex;
        }

        if (model.Views.Count > 0) {
            for (int i = 0; i < model.Views.Count; i++) {
                //var substream = stream.BaseStream as Substream;
                //var path = substream != null ? ((FileStream)substream.GetInnerStream()).Name : ((FileStream)stream.BaseStream).Name;
                //using (var skinFile = new BinaryReader(new FileStream(M2SkinProfile.SkinFileName(path, i), FileMode.Open)))
                using (var skinFile = new BinaryReader(new MemoryStream(CascFileBytes(GetCascFile(model.Views[i].FileDataID))))) {
                    model.Views[i].Load(skinFile, model.Version);
                    model.Views[i].LoadContent(skinFile, model.Version);
                }
                Debug.Log("Skin File: " + model.Views[i].FileDataID);
            }
        }

        Vector3[] vertices = new Vector3[model.GlobalVertexList.Count];
        Vector3[] normals = new Vector3[model.GlobalVertexList.Count];
        Vector2[] uv1 = new Vector2[model.GlobalVertexList.Count];
        Vector2[] uv2 = new Vector2[model.GlobalVertexList.Count];

        int index = 0;
        foreach (M2Vertex v in model.GlobalVertexList) {
            vertices[index] = new Vector3(v.Position.y, v.Position.z, -v.Position.x);
            normals[index] = new Vector3(v.Normal.y, v.Normal.z, -v.Normal.x);
            uv1[index] = v.TexCoords[0];
            uv2[index] = v.TexCoords[1];
            index++;
        }

        int[] triangles = new int[model.Views[0].Triangles.Count];
        for (int i = 0; i < model.Views[0].Triangles.Count; i++) {
            triangles[i] = (int)model.Views[0].Triangles[i];
        }

        // Compltely Wrong
        /*
        for (int i = 0; i < model.Views[0].Submeshes.Count; i++) {
            M2SkinSection sub = model.Views[0].Submeshes[i];
            SubMeshDescriptor smd = new SubMeshDescriptor();
            smd.baseVertex = sub.StartVertex;
            Bounds bounds = new Bounds(sub.CenterBoundingBox, new Vector3(sub.Radius, sub.Radius, sub.Radius)); // radius isn't correct
            smd.bounds = bounds;
            smd.firstVertex = 0; ///?
            smd.indexCount = 3;
            smd.indexStart = sub.StartVertex;
            smd.topology = MeshTopology.Triangles;
            smd.vertexCount = sub.NVertices;
            m2Mesh.SetSubMesh(i, smd, MeshUpdateFlags.Default);
        }*/

        m2Mesh.vertices = vertices;
        m2Mesh.normals = normals;
        m2Mesh.triangles = triangles.Reverse().ToArray(); // Reverses the faces
        m2Mesh.uv = uv1;
        m2Mesh.uv2 = uv2;
        return (m2Mesh, textures);
    }

    /// <summary>
    /// Create a Texture2D in memory from a BLP image file
    /// </summary>
    /// <param name="file"></param>
    public Texture2D BlpToTexture2d(CASCFile file) {
        Texture2D blpTex = null;
        using (var blp = new BlpFile(new MemoryStream(CascFileBytes(file)))) {
            blpTex = blp.GetTexture2d(0); // getting mipmap 0, TODO: get all mipmaps
        }
        return blpTex;
    }

    /// <summary>
    /// Returns a byte array representing the bytes of a specific CASC file. Used to create new MemoryStreams.
    /// </summary>
    /// <param name="file">This CASCFile should come from the CASCFiles List.</param>
    /// <returns></returns>
    public byte[] CascFileBytes(CASCFile file) {
        try {
            var input = storage.OpenFile(file.FullPath);
            using (var output = new MemoryStream()) {
                System.IntPtr pspanPointer;
                CASCFunc.CASC_FILE_FULL_INFO fileInfo;
                if (input.GetInfo(out fileInfo, out pspanPointer)) {
                    for (int i = 0; i < fileInfo.SpanCount; i++) {
                        var pspan = Marshal.PtrToStructure<CASCFunc.CASC_FILE_SPAN_INFO>(IntPtr.Add(pspanPointer, i * Marshal.SizeOf(typeof(CASCFunc.CASC_FILE_SPAN_INFO))));
                        int cbFileSpan = (int)(pspan.EndOffset - pspan.StartOffset);
                        if (cbFileSpan == 0)
                            continue;
                        byte[] buffer;
                        int bytesRead;
                        bool readOK = input.Read(out buffer, cbFileSpan, out bytesRead);
                        if (readOK) {
                            output.Write(buffer, 0, bytesRead);
                            if (bytesRead < cbFileSpan)
                                break;
                        }
                        else {
                            break;
                        }
                    }
                }
                output.Position = 0;
                byte[] bytes = output.ToArray();
                return bytes;
            }
        }
        catch (Exception ex) {
            Debug.LogError(ex);
            return null;
        }
    }

    /// <summary>
    /// Extract files from CASC in their native format.
    /// </summary>
    /// <param name="files"></param>
    /// <returns></returns>
    IEnumerator ExtractCoroutine(List<CASCFile> files) {
        Debug.Log("Number of files to extract:" + files.Count);
        int filesProcessed = 0;
        foreach (var file in files) {
            yield return null;
            if (file.IsLocal == "Yes") {
                string outputLocation = outputPath + file.FullPath.Replace("\0", String.Empty); /// For saving the files such that it respect the directory
                //string outputLocation = outputPath + ReplaceInvalidChars(file.FileName);      /// For saving the files without directory...using for testing purposes
                Directory.CreateDirectory(Path.GetDirectoryName(outputLocation));
                try {
                    var input = storage.OpenFile(file.FullPath);
                    using (var output = File.Create(outputLocation, 128, FileOptions.SequentialScan)) {
                        System.IntPtr pspanPointer;
                        CASCFunc.CASC_FILE_FULL_INFO fileInfo;
                        if (input.GetInfo(out fileInfo, out pspanPointer)) {
                            for (int i = 0; i < fileInfo.SpanCount; i++) {
                                var pspan = Marshal.PtrToStructure<CASCFunc.CASC_FILE_SPAN_INFO>(IntPtr.Add(pspanPointer, i * Marshal.SizeOf(typeof(CASCFunc.CASC_FILE_SPAN_INFO))));
                                int cbFileSpan = (int)(pspan.EndOffset - pspan.StartOffset);
                                if (cbFileSpan == 0)
                                    continue;
                                byte[] buffer;
                                int bytesRead;
                                bool readOK = input.Read(out buffer, cbFileSpan, out bytesRead);
                                if (readOK) {
                                    output.Write(buffer, 0, bytesRead);
                                    if (bytesRead < cbFileSpan)
                                        break;
                                }
                                else {
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex) {
                    Debug.LogError(ex);
                }
            }
            filesProcessed++;
            Debug.Log("Progress:" + filesProcessed + "/" + files.Count + " FileName:" + file.FullPath);
        }
        yield return null;
    }

    private void OnDisable() {
        CASCFiles.Clear();
        dbManager.Dispose();
        dbManager.Close();
    }

    string ReplaceInvalidChars(string filename) {
        filename.TrimEnd();
        return string.Join("", filename.Split(Path.GetInvalidFileNameChars()));
    }
}