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

    [SerializeField] int pixWidth = 256;
    [SerializeField] int pixHeight = 256;

    [SerializeField] float scale = 20.0f;

    Renderer rend;
    Color[] pix;
    Texture2D noiseTex;

    float[,] heightMap;

    void Start()
    {
        rend = GetComponent<Renderer>();
        noiseTex = new Texture2D(pixWidth, pixHeight);
        noiseTex.filterMode = FilterMode.Point;
        pix = new Color[pixWidth * pixHeight];
        rend.material.mainTexture = noiseTex;

        SetHeightMap();
        CreateTexture();
        CreateMesh();
    }

    void SetHeightMap()
    {
        heightMap = new float[pixWidth + 1, pixHeight + 1];

        for (int y = 0; y <= pixHeight; y++)
        {
            for (int x = 0; x <= pixWidth; x++)
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
                    if (heightMap[x, y] <= heightColorData[i].heightValue)
                    {
                        pix[y * pixWidth + x] = heightColorData[i].heightColor;
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
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[(pixWidth + 1) * (pixHeight + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[pixWidth * pixHeight * 6];
        int t = 0;

        for (int y = 0; y <= pixHeight; y++)
        {
            for (int x = 0; x <= pixWidth; x++)
            {
                vertices[y * (pixWidth + 1) + x] = new Vector3(x, heightMap[x, y], y);
                uv[y * (pixWidth + 1) + x] = new Vector2((float)x / pixWidth, (float)y / pixHeight);

                if (x != pixWidth && y != pixHeight)
                {
                    triangles[t] = y * (pixWidth + 1) + x;
                    triangles[t + 1] = (y + 1) * (pixWidth + 1) + x;
                    triangles[t + 2] = y * (pixWidth + 1) + x + 1;
                    triangles[t + 3] = y * (pixWidth + 1) + x + 1;
                    triangles[t + 4] = (y + 1) * (pixWidth + 1) + x;
                    triangles[t + 5] = (y + 1) * (pixWidth + 1) + x + 1;
                    t += 6;
                }
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
    }
}
