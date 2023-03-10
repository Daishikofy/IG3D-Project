using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshColorTexture : MonoBehaviour
{
    public int resolution = 3;
    public int textureWidth = 100;
    public int penSize = 3;

    Color activeColor = Color.black;
    Vector3[] worldVertices;
    MeshFilter meshFilter;
    Vector2[] meshUvs;

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

        texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/3D/Texture/ProceduralTexture.asset");

        if (texture == null || texture.width != width || texture.height != height)
        {
            texture = new Texture2D(width, height);
            AssetDatabase.CreateAsset(texture, "Assets/3D/Texture/ProceduralTexture.asset");
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
        var meshFilter = GetComponent<MeshFilter>();

        var mesh = meshFilter.mesh;
        var colors = mesh.colors;

        Vector3 hitWorldPosition = Vector3.zero;

        CastRayToPosition(Input.mousePosition);
        CastRayToPosition(new Vector2(Input.mousePosition.x + penSize, Input.mousePosition.y));
        CastRayToPosition(new Vector2(Input.mousePosition.x - penSize, Input.mousePosition.y));
        CastRayToPosition(new Vector2(Input.mousePosition.x, Input.mousePosition.y + penSize));
        CastRayToPosition(new Vector2(Input.mousePosition.x, Input.mousePosition.y - penSize));

        texture.Apply();

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
        var minIds = new List<int>();
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

        if (minDist > 1)
        { return; }

        minIds.Add(minId);
        for (int i = 0; i < mesh.vertexCount; i++)
        {
            float newDist = Vector3.Distance(worldVertices[i], worldVertices[minId]);
            if (newDist <= float.Epsilon)
            {
                minIds.Add(i);
            }
        }

        Debug.DrawLine(Camera.main.transform.position, worldVertices[minId], activeColor);

        foreach (int id in minIds)
        {
            colors[id] = activeColor;
        }

        meshFilter.mesh.colors = colors;
    }

    private void CastRayToPosition(Vector2 position)
    {
        Ray ray = Camera.main.ScreenPointToRay(position);
        RaycastHit hit = new RaycastHit();
        Physics.Raycast(ray, out hit, Vector3.Distance(Camera.main.transform.position, this.transform.position));
        
        if (hit.collider != null)
            PaintTexture(hit.textureCoord, activeColor);
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
        Color[] textureColors = new Color[width * height];

        for (int k = 0; k < triangles.Length; k += 3)
        {
            if ( k % 2 != 0)
            {
                for (int i = 0; i < resolution; i++)
                {
                    for (int j = 0; j <= i; j++)
                    {
                        us.x = j + ud.x;
                        us.y = i + ud.y;

                        if (i == 0 && j == 0)
                        {
                            uv[triangles[k + 1]].x = us.x;
                            uv[triangles[k + 1]].y = us.y;
                        }
                        else if (i == (resolution - 1) && j == 0)
                        {
                            uv[triangles[k + 2]].x = us.x;
                            uv[triangles[k + 2]].y = us.y;
                        }
                        else if (i == (resolution - 1) && j == (resolution - 1))
                        {
                            uv[triangles[k + 0]].x = us.x;
                            uv[triangles[k + 0]].y = us.y;
                        }
                        else if (i == j && us.x + 1 < texture.width)
                        {
                            texture.SetPixel(us.x - 1, us.y, texture.GetPixel(us.x, us.y));
                        }
                        else if (i == (resolution - 1) && us.y + 1 < texture.height)
                        {
                            texture.SetPixel(us.x - 1, us.y, texture.GetPixel(us.x, us.y));
                        }
                        else if (j == 0 && us.x - 1 > 0)
                        {
                            texture.SetPixel(us.x - 1, us.y, texture.GetPixel(us.x, us.y));
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < resolution; i++)
                {
                    for (int j = 0; j <= i; j++)
                    {
                        us.x = j + ud.x;
                        us.y = i + ud.y;

                        if (i == 0 && j == 0)
                        {
                            int t = k + 1;

                            uv[triangles[t]].x = us.x;
                            uv[triangles[t]].y = us.y;
                        }
                        else if (i == (resolution - 1) && j == 0)
                        {
                            int t = k + 0;

                            uv[triangles[t]].x = us.x;
                            uv[triangles[t]].y = us.y;
                        }
                        else if (i == (resolution - 1) && j == (resolution - 1))
                        {
                            int t = k + 2;
                            uv[triangles[t]].x = us.x;
                            uv[triangles[t]].y = us.y;
                        }
                        else if (i == j && us.x + 1 < texture.width)
                        {
                            texture.SetPixel(us.x - 1, us.y, texture.GetPixel(us.x, us.y));
                        }
                        else if (i == (resolution - 1) && us.y + 1 < texture.height)
                        {
                            texture.SetPixel(us.x - 1, us.y, texture.GetPixel(us.x, us.y));
                        }
                        else if (j == 0 && us.x - 1 > 0)
                        {
                            texture.SetPixel(us.x - 1, us.y, texture.GetPixel(us.x, us.y));
                        }
                    }
                }
            }


            ud.x += resolution;
            if(ud.x >= width)
            {
                ud.x = 0;
                ud.y += resolution;
            }
        }

        texture.Apply();

        for (int i = 0; i < uv.Length; i++)
        {
            uv[i].x /= width - 1;
            uv[i].y /= height - 1;
        }

        File.WriteAllBytes("Assets/3D/Texture/ProceduralTexture.png", texture.EncodeToPNG());
        meshUvs = uv;

        SaveModelUV();
    }

    void PaintTexture(Vector2 uv, Color color)
    {
        int x = (int)(texture.width * uv.x);
        int y = (int)(texture.height * uv.y);

        int w = (x + penSize) < texture.width ? penSize : texture.width - x;
        int h = (y + penSize) < texture.height ? penSize : texture.height - y;

        Color[] colors = new Color[h * w];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = activeColor;
        }

        texture.SetPixels(x, y, w, h, colors);
    }

    public void SaveModelUV()
    {
        meshFilter.sharedMesh.SetUVs(0, meshUvs);

        var name = meshFilter.mesh.name;
        if(meshFilter.mesh.name.Length - 9 > 0)
        {
            name = name.Remove(meshFilter.mesh.name.Length - 9);
            Debug.Log(name);
        }

        if (! AssetDatabase.Contains(meshFilter.mesh))
        {
            AssetDatabase.CreateAsset(meshFilter.mesh, "Assets/3D/Models/" + name + ".asset");
            AssetDatabase.SaveAssets();
        }

    }

    public void SetActiveColor(string hexColor)
    {
        Color outColor;
        if (ColorUtility.TryParseHtmlString(hexColor, out outColor))
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


    private void SetPixel(Color[] texture, int x, int y, Color color)
    {
        texture[x + y * width] = color;
    }
}
