using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TriangleMesh : MonoBehaviour
{
    void Awake()
    {
        CreateTriangle();
        // Hide the triangle initially
        GetComponent<MeshRenderer>().enabled = false;
    }

    void Start()
    {
        // Show triangle after random delay between 1-5 seconds
        float delay = Random.Range(1f, 5f);
        StartCoroutine(ShowAfterDelay(delay));
    }

    System.Collections.IEnumerator ShowAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        GetComponent<MeshRenderer>().enabled = true;
    }

    void CreateTriangle()
    {
        Mesh mesh = new Mesh();
        mesh.name = "Triangle"; // Name the mesh

        // Define vertices for a 2D triangle (equilateral, centered at origin)
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(0f, 0.5f, 0f),      // Top
            new Vector3(-0.5f, -0.5f, 0f),  // Bottom left
            new Vector3(0.5f, -0.5f, 0f)    // Bottom right
        };

        // Define triangles (front and back faces for 2D visibility from both sides)
        int[] triangles = new int[]
        {
            0, 2, 1,  // Front face
            0, 1, 2   // Back face
        };

        // UVs for texturing
        Vector2[] uvs = new Vector2[]
        {
            new Vector2(0.5f, 1f),
            new Vector2(0f, 0f),
            new Vector2(1f, 0f)
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
    }
}
