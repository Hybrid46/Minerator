using System.Collections.Generic;
using System.Text;
//using System.IO;
using UnityEngine;

public class PrefabHolder : Singleton<PrefabHolder>
{
    public Dictionary<string, GameObject> prefabs;
    private StringBuilder sb = new StringBuilder();

    public void Start()
    {
        /*
        DirectoryInfo dir = new DirectoryInfo(Application.dataPath + "/Resources/Prefabs/");
        FileInfo[] info = dir.GetFiles("*.prefab");

        foreach (FileInfo f in info)
        {
            Debug.Log("Loading: " + Application.dataPath + "/Resources/Prefabs/" + f.Name);
            prefabs.Add(f.Name, Resources.Load(Application.dataPath + "/Resources/Prefabs/" + f.Name) as GameObject);
        }
        */

        GameObject[] prefabObjects = Resources.LoadAll<GameObject>("Prefabs");

        prefabs = new Dictionary<string, GameObject>(prefabObjects.Length);

        for (int i = 0; i < prefabObjects.Length; i++)
        {
            sb.Clear();
            sb.Append("Loading: ");
            sb.Append(prefabObjects[i].name);
            Debug.Log(sb.ToString());
            prefabs.Add(prefabObjects[i].name, prefabObjects[i]);
        }
    }
}
