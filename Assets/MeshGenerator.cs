using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    public int width, height;
    public string meshName;

    public void GenerateMesh()
    {
        Mesh mesh = GeneratePlane();
        AssetDatabase.CreateAsset(mesh, "Assets/3D/" + meshName + ".asset");
    }

    private Mesh GeneratePlane()
    {
        var mesh = new Mesh();

        var vertices = new Vector3[width * height];
        var triangles = new List<int>();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                vertices[j + i * width] = new Vector3(j, i);
            }
        }

        for (int i = 1; i < width; i++)
        {
            for (int j = 1; j < height; j++)
            {
                int v0, v1, v2, v3;


                v0 = (j - 1) + (i - 1) * width;
                v1 = (j - 1) + i * width;
                v2 = j + (i - 1) * width;
                v3 = j  + i * width;

                triangles.Add(v0);
                triangles.Add(v1);
                triangles.Add(v2);

                triangles.Add(v2);        
                triangles.Add(v1);        
                triangles.Add(v3);        
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles.ToArray();

        return mesh;
    }
}
