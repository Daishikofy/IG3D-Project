using System.IO;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralPlane : MonoBehaviour
{
    public int resolution = 3;
    public int textureWidth = 100;

    Color activeColor = Color.black;
    Vector3[] worldVertices;
    MeshFilter meshFilter;

    Texture2D texture;
    int width, height;

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();

        Color[] colors =  new Color[meshFilter.mesh.vertexCount];

        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.white;
        }

        meshFilter.mesh.colors = colors;
        gameObject.AddComponent(typeof(MeshCollider));

        worldVertices = new Vector3[meshFilter.mesh.vertexCount];
        UpdateWorldVerticesPosition();

        int trianglesCount = meshFilter.mesh.triangles.Length / 3;

        if (trianglesCount <= textureWidth)
        {
            width = resolution * trianglesCount;
            height = resolution;
        }
        else
        {
            width = textureWidth * resolution;
            height = ((trianglesCount / textureWidth) + 1) * resolution;
        }

        texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/3D/ProceduralTexture.asset");

        if (texture == null || texture.width != width || texture.height != height)
        {
            texture = new Texture2D(width, height);
            AssetDatabase.CreateAsset(texture, "Assets/3D/ProceduralTexture.asset");
        }

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        GenerateTexture();
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            HandleInput();
        }
    }

    private void HandleInput()
    {
        Plane plane = new Plane(Vector3.back, 0);

        var meshFilter = GetComponent<MeshFilter>();

        var mesh = meshFilter.mesh;
        var colors = mesh.colors;

        Vector3 hitWorldPosition = Vector3.zero;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        var hits = Physics.RaycastAll(ray);
        float minDist = float.PositiveInfinity;
        foreach (var hit in hits)
        {
            float newDist = Vector3.Distance(Camera.main.transform.position, hit.transform.position);
            if (newDist < minDist)
            {
                minDist = newDist;
                hitWorldPosition = hit.point;
            }
        }

        minDist = Vector3.Distance(worldVertices[0], hitWorldPosition);
        int minId = 0;

        for (int i = 1; i < mesh.vertexCount; i++)
        {
            float newDist = Vector3.Distance(worldVertices[i], hitWorldPosition);

            if (newDist < minDist)
            {
                minDist = newDist;
                minId = i;
            }
        }

        if(minDist > 1)
        { return;  }

        Debug.DrawLine(Camera.main.transform.position, worldVertices[minId], activeColor);

        colors[minId] = activeColor;

        meshFilter.mesh.colors = colors;
        //OnVertexColorUpdate(minId);
    }

    public void SetActiveColor(string hexColor)
    {
        Color outColor;
        if(ColorUtility.TryParseHtmlString(hexColor, out outColor))
        {
            activeColor = outColor;
        }
        else
        {
            Debug.Log("Could not parse color");
        }
    }

    public void RotateModelVertically(int value)
    {
        transform.Rotate(Vector3.right, value);

        UpdateWorldVerticesPosition();
    }

    public void RotateModelHorizontally(int value)
    {
        transform.Rotate(Vector3.up, value);

        UpdateWorldVerticesPosition();
    }

    private void UpdateWorldVerticesPosition()
    {
        var vertices = meshFilter.mesh.vertices;
        for (int i = 0; i < worldVertices.Length; i++)
        {
            worldVertices[i] = transform.localToWorldMatrix.MultiplyPoint3x4(vertices[i]);
        }
    }

    public void GenerateTexture()
    {
        Vector2Int us = new Vector2Int(0,0);
        Vector2Int ud = new Vector2Int(0, 0);

        int[] triangles = meshFilter.mesh.triangles;
        Vector2[] uv = meshFilter.mesh.uv;
        Color[] colors = new Color[width * height];

        for (int k = 0; k < triangles.Length; k += 3)
        {
            for (int i = 0; i < resolution; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    us.x = j + ud.x;
                    us.y = i + ud.y;

                    if (i == 0 && j == 0)
                    {
                        SetPixel(colors, us.x, us.y, meshFilter.mesh.colors[triangles[k + 0]]);
                        uv[triangles[k + 0]].x = us.x;
                        uv[triangles[k + 0]].y = us.y;
                    }
                    else if (i == (resolution - 1) && j == 0)
                    {
                        SetPixel(colors, us.x, us.y, meshFilter.mesh.colors[triangles[k + 1]]);
                        SetPixel(colors, i + ud.x, j + ud.y, Color.black);
                        uv[triangles[k + 1]].x = us.x;
                        uv[triangles[k + 1]].y = us.y;
                    }
                    else if (i == (resolution - 1) && j == (resolution - 1))
                    {
                        SetPixel(colors, us.x, us.y, meshFilter.mesh.colors[triangles[k + 2]]);
                        uv[triangles[k + 2]].x = us.x;
                        uv[triangles[k + 2]].y = us.y;
                    }
                    else
                    {
                        SetPixel(colors, us.x , us.y, Color.white);
                        SetPixel(colors, i + ud.x, j + ud.y, Color.white);
                    }
                }
            }

            ud.x += resolution;
        }

        texture.SetPixels(0, 0, width, height, colors);
        texture.Apply();

        for (int i = 0; i < uv.Length; i++)
        {
            uv[i].x /= width - 1;
            uv[i].y /= height - 1;
        }
        
        for (int i = 0; i < uv.Length; i++)
        {
            Debug.Log(i + " : (" + uv[i].x + ", " + uv[i].y + ")");
        }

        SaveModelUV(uv);

        File.WriteAllBytes("Assets/3D/ProceduralTexture.png", texture.EncodeToPNG());
    }

    private void SaveModelUV(Vector2[] uv)
    {
        meshFilter.mesh.SetUVs(0, uv);
        var name = meshFilter.mesh.name;
        if(meshFilter.mesh.name.Length - 9 > 0)
        {
            name = name.Remove(meshFilter.mesh.name.Length - 9);
            Debug.Log(name);
        }
 
        AssetDatabase.SaveAssets();
        //AssetDatabase.CreateAsset(meshFilter.mesh, "Assets/3D/" + name + ".asset"); 
    }

    private void OnVertexColorUpdate(int vertex)
    {
        Vector2Int us = new Vector2Int(0, 0);
        Vector2Int ud = new Vector2Int(0, 0);

        int[] triangles = meshFilter.mesh.triangles;
        Vector2[] uv = meshFilter.mesh.uv;
        Color[] colors = meshFilter.mesh.colors;

        for (int k = 0; k < triangles.Length; k += 3)
        {
            for (int i = 0; i < resolution; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    us.x = j + ud.x;
                    us.y = i + ud.y;

                    if (i == 0 && j == 0)
                    {
                        if (vertex == triangles[k + 0])
                            SetPixel(colors, us.x, us.y, meshFilter.mesh.colors[triangles[k + 0]]);
                    }
                    else if (i == (resolution - 1) && j == 0)
                    {
                        if (vertex == triangles[k + 1])
                        {
                            SetPixel(colors, us.x, us.y, meshFilter.mesh.colors[triangles[k + 1]]);
                            SetPixel(colors, i + ud.x, j + ud.y, Color.black);
                        }
                    }
                    else if (i == (resolution - 1) && j == (resolution - 1))
                    {
                        if (vertex == triangles[k + 1])
                        {
                            SetPixel(colors, us.x, us.y, meshFilter.mesh.colors[triangles[k + 2]]);
                        }
                    }
                }
            }

            ud.x += resolution;
        }

        texture.SetPixels(0, 0, width, height, colors);
        texture.Apply();
    }

    private void SetPixel(Color[] texture, int x, int y, Color color)
    {
        texture[x + y * width] = color;
    }
}
