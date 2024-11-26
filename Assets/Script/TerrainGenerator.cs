using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [System.Serializable]
    public struct HeightColorVar // Changed to public
    {
        [ColorUsage(true, true)]
        public Color heightColor;
        public float heightValue;
    }

    public HeightColorVar[] heightColorData; // Now consistent with struct accessibility

    [Header("Parameters of Terrain Generator")]
    [SerializeField] private int pixWidth = 256;
    [SerializeField] private int pixHeight = 256;
    [SerializeField] private int heightMultiplier = 0;
    [SerializeField] private float scale = 20.0f;
    
    private Renderer rend;
    private Color[] pix;
    private Texture2D noiseTex;
    private float[,] heightMap;
    
    private Vector3[] verts;
    private int[] tris;
    private Vector2[] uvs;

    void Start()
    {
        rend = GetComponent<Renderer>();
        noiseTex = new Texture2D(pixWidth, pixHeight);
        noiseTex.filterMode = FilterMode.Point;
        pix = new Color[pixWidth * pixHeight];
        rend.material.mainTexture = noiseTex;
        
        pixWidth = Random.Range(100, pixWidth);
        pixHeight = Random.Range(100, pixHeight);

        SetHeightMap();
        CreateTexture();
        CreateMesh();
    }

    void SetHeightMap()
    {
        heightMap = new float[pixWidth + 1, pixHeight + 1];

        for (int y = 0; y < pixHeight; y++)
        {
            for (int x = 0; x < pixWidth; x++)
            {
                float xCoord = (float)x / scale;
                float yCoord = (float)y / scale;
                float sample = Mathf.PerlinNoise(xCoord, yCoord);
                heightMap[x, y] = sample;
            }
        }
    }

    void CreateTexture()
    {
        for (int y = 0; y < pixHeight; y++)
        {
            for (int x = 0; x < pixWidth; x++)
            {
                for (int i = 0; i < heightColorData.Length; i++)
                {
                    if (heightMap[x, y] <= heightColorData[i].heightValue*100)
                    {
                        pix[x * pixWidth + y] = heightColorData[i].heightColor;
                        break;
                    }
                }
            }
        }

        noiseTex.SetPixels(pix);
        noiseTex.Apply();
    }

    void CreateMesh()
    {
        verts = new Vector3[(pixWidth + 1) * (pixHeight + 1)];
        uvs = new Vector2[verts.Length];
        tris = new int[pixWidth * pixHeight * 6];

        for (int y = 0; y < pixHeight; y++)
        {
            for (int x = 0; x < pixWidth; x++)
            {
                verts[y * pixWidth + x + y] = new Vector3(x, heightMap[x, y] * heightMultiplier, y);
                uvs[y * pixWidth + x + y] = new Vector2((float)y / (float)pixHeight, (float)x / (float)pixWidth);
            }
        }

        for (int y = 0; y < pixHeight; y++)
        {
            for (int x = 0; x < pixWidth; x++)
            {
                tris[(y * pixWidth + x) * 6] = y * pixWidth + x + y; 
                tris[(y * pixWidth + x) * 6 + 1] = y * pixWidth + x + y + pixWidth + 1; 
                tris[(y * pixWidth + x) * 6 + 2] = y * pixWidth + x + y + 1; 
                tris[(y * pixWidth + x) * 6 + 3] = y * pixWidth + x + y + 1; 
                tris[(y * pixWidth + x) * 6 + 4] = y * pixWidth + x + y + pixWidth + 1; 
                tris[(y * pixWidth + x) * 6 + 5] = y * pixWidth + x + y + pixWidth + 2; 
            }
        }

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.Clear();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
    }
}
 