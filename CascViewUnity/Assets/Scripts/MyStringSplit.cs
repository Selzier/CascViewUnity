using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class MyStringSplit : MonoBehaviour
{
    public string csvPath;
    private string[] read;    
    char[] seperators = { ';', '/' };

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void ParseCsv() {
        StreamReader sr = new StreamReader(csvPath);
        string data = sr.ReadLine();

        int count = 0;

        // Adding the CASC root as first item in folder list
        CASCFolder root = new CASCFolder(0, -1, "Root");
        CASCMain.CASCFolders.Add(root);

        while ((data = sr.ReadLine()) != null) {
            read = data.Split(seperators, StringSplitOptions.None);
            //read[0] // FileDataID
            //read[read.Length -1] // Filename.xxx
            int parentID = -1;

            for (int i = 1; i < read.Length - 1; i++) {
                if (i == 1) { // First Folder, parent is root
                    parentID = 0;
                }

                /*StringBuilder sb = new StringBuilder();
                sb.Append(parentID.ToString());
                char[] chars = read[i].ToCharArray();
                foreach (char c in chars) {
                    int cIndex = char.ToUpper(c) - 64;
                    sb.Append(cIndex);
                } */

                string sID = parentID.ToString() + read[i];
                int uniqueID = sID.GetHashCode();

                //IEnumerable<CASCFolder> matchingFolders = CASCMain.CASCFolders.Where(folder => folder.ParentDirectoryID == parentID).ToList(); // Where(folder => folder.Value == value).ToList();
                /*bool addNew = false;
                foreach (CASCFolder cf in CASCMain.CASCFolders) {
                    if (cf.DirectoryID == uniqueID) {
                        addNew = false;
                    }
                    else {
                        addNew = true;
                    }
                }*/

                //if (addNew) {
                //    CASCFolder currentFolder = new CASCFolder(uniqueID, parentID, read[i]);
                //    CASCMain.CASCFolders.Add(currentFolder);
                //}

                parentID = uniqueID; // Finally set parentID so it will be used next loop
                /*if (count < 200) {
                    Debug.Log(sID);
                }*/

            }
            count++;
            
        }

        Debug.Log(CASCMain.CASCFolders.Count);
    }
}
