using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TextureArrayManager : Singleton<TextureArrayManager>
{
    [Serializable]
    public class TexturesForArray
    {
        public string textureName;
        public Texture2D albedo;
        public Texture2D normalMap;
        public Texture2D metallicMap;

        public TexturesForArray() { }
    }

    public List<TexturesForArray> textures;

    private Dictionary<string, int> indexLookupTable;

    public void FillUpIndexLookupTable()
    {
        indexLookupTable = new Dictionary<string, int>();

        for (int t = 0; t < textures.Count; t++)
        {
            indexLookupTable.Add(textures[t].textureName, t);
        }
    }

    public void CreateArray(out Texture2DArray albedoArray, out Texture2DArray normalArray, out Texture2DArray metallicArray)
    {
        // unified size and count!
        int width = textures[0].albedo.width;
        int height = textures[0].albedo.height;

        // albedo
        Texture2DArray array = new Texture2DArray(width, height, textures.Count, TextureFormat.RGBA32, true);

        for (int i = 0; i < textures.Count; i++) array.SetPixels(textures[i].albedo.GetPixels(), i);

        array.Apply();
        albedoArray = array;
        AssetDatabase.CreateAsset(array, $"Assets/TerrainAlbedoTextureArray.asset");

        // normal
        array = new Texture2DArray(width, height, textures.Count, TextureFormat.RGBA32, true);

        for (int i = 0; i < textures.Count; i++) array.SetPixels(textures[i].normalMap.GetPixels(), i);

        array.Apply();
        normalArray = array;
        AssetDatabase.CreateAsset(array, $"Assets/TerrainNormalTextureArray.asset");

        // metallic
        array = new Texture2DArray(width, height, textures.Count, TextureFormat.RGBA32, true);

        for (int i = 0; i < textures.Count; i++) array.SetPixels(textures[i].metallicMap.GetPixels(), i);

        array.Apply();
        metallicArray = array;
        AssetDatabase.CreateAsset(array, $"Assets/TerrainMetallicTextureArray.asset");
    }

    //O(1) complexity for finding textures in array by name
    public int GetIndexByTextureName(string textureName) => indexLookupTable[textureName];
}
