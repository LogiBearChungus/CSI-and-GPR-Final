using System.Collections.Generic;
using UnityEngine;

public class VoxelMeshGenerator : MonoBehaviour
{

    public Material voxelMaterial;
    public int[,,] voxelData;
    [SerializeField] int chunkSize = 16;
    const int width = 1, height = 1;

    private Mesh mesh;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Generate Voxel Data
        voxelData = new int[chunkSize, chunkSize, chunkSize];
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    if ((x + y + z) % 8 == 0)
                    {
                  if (x == 0 || x == chunkSize - 1 || y == 0 || y == chunkSize - 1 || z == 0 || z == chunkSize - 1)
                  {
                     voxelData[x, y, z] = 0;
                  }else{
                     voxelData[x, y, z] = 1;
                  }
                  
                    }
                    else
                    {
                     voxelData[x, y, z] = 1;

               }
                }
            }


        }
        GenerateMesh();
    }

    void GenerateMesh()
    {
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();

        // Loop through all voxels
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    if (voxelData[x, y, z] == 1)
                    {
                        CreateVoxel(x, y, z);
                    }
                }
            }
        }

        // Create or update the mesh
        if (mesh == null)
        {
            mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = mesh;
        }
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        // Apply material
        if (voxelMaterial != null)
        {
            GetComponent<MeshRenderer>().material = voxelMaterial;
        }
    }
    void CreateVoxel(int x, int y, int z)
    {
        // Check each face and only create it if it's exposed
        if (!IsVoxelSolid(x, y + 1, z)) CreateTopFace(x, y, z);      // Top
        if (!IsVoxelSolid(x, y - 1, z)) CreateBottomFace(x, y, z);   // Bottom
        if (!IsVoxelSolid(x - 1, y, z)) CreateLeftFace(x, y, z);     // Left
        if (!IsVoxelSolid(x + 1, y, z)) CreateRightFace(x, y, z);    // Right
        if (!IsVoxelSolid(x, y, z + 1)) CreateFrontFace(x, y, z);    // Front
        if (!IsVoxelSolid(x, y, z - 1)) CreateBackFace(x, y, z);     // Back

    }
        bool IsVoxelSolid(int x, int y, int z)
        {
            // Check if position is out of bounds
            if (x < 0 || x >= chunkSize || y < 0 || y >= chunkSize || z < 0 || z >= chunkSize)
            {
                return false; // Treat out of bounds as air
            }
            return voxelData[x, y, z] == 1;
        }

        bool IsNeighborSolid(int x, int y, int z){
         if(voxelData[x, y, z] == 1){
            return true;
         }else{ 
         return false; }


         }
    void CreateTopFace(int x, int y, int z)
    {
      if (y == chunkSize-1 || !IsNeighborSolid(x, y + 1, z))
      {
         int vertexIndex = vertices.Count;
         vertices.Add(new Vector3(x, y + 1, z));
         vertices.Add(new Vector3(x, y + 1, z + 1));
         vertices.Add(new Vector3(x + 1, y + 1, z + 1));
         vertices.Add(new Vector3(x + 1, y + 1, z));
         AddQuadTriangles(vertexIndex);
         AddQuadUVs();
      }
    }

    void CreateBottomFace(int x, int y, int z)
    {
      if(y == 0 || !IsNeighborSolid(x, y -1, z))
       { int vertexIndex = vertices.Count;
        vertices.Add(new Vector3(x, y, z));
        vertices.Add(new Vector3(x + 1, y, z));
        vertices.Add(new Vector3(x + 1, y, z + 1));
        vertices.Add(new Vector3(x, y, z + 1));
        AddQuadTriangles(vertexIndex);
         AddQuadUVs();
      }
    }

    void CreateLeftFace(int x, int y, int z)
    {
      if(x == 0 || !IsNeighborSolid(x-1,y,z))
        {int vertexIndex = vertices.Count;
        vertices.Add(new Vector3(x, y, z));
        vertices.Add(new Vector3(x, y, z + 1));
        vertices.Add(new Vector3(x, y + 1, z + 1));
        vertices.Add(new Vector3(x, y + 1, z));
        AddQuadTriangles(vertexIndex);
         AddQuadUVs();
      }
    }

    void CreateRightFace(int x, int y, int z)
    {
        if(x==chunkSize-1 || !IsNeighborSolid(x+1,y,z))
        {int vertexIndex = vertices.Count;
        vertices.Add(new Vector3(x + 1, y, z));
        vertices.Add(new Vector3(x + 1, y + 1, z));
        vertices.Add(new Vector3(x + 1, y + 1, z + 1));
        vertices.Add(new Vector3(x + 1, y, z + 1));
        AddQuadTriangles(vertexIndex);
         AddQuadUVs();
      }
    }

    void CreateFrontFace(int x, int y, int z)
    {
      if(z== chunkSize - 1 || !IsNeighborSolid(x,y,z+1))
        {int vertexIndex = vertices.Count;
        vertices.Add(new Vector3(x, y, z + 1));
        vertices.Add(new Vector3(x + 1, y, z + 1));
        vertices.Add(new Vector3(x + 1, y + 1, z + 1));
        vertices.Add(new Vector3(x, y + 1, z + 1));
        AddQuadTriangles(vertexIndex);
         AddQuadUVs();
      }
    }

    void CreateBackFace(int x, int y, int z)
    {
      if (z == 0 || !IsNeighborSolid(x, y, z-1))
      {
         int vertexIndex = vertices.Count;
         vertices.Add(new Vector3(x, y, z));
         vertices.Add(new Vector3(x, y + 1, z));
         vertices.Add(new Vector3(x + 1, y + 1, z));
         vertices.Add(new Vector3(x + 1, y, z));
         AddQuadTriangles(vertexIndex);
         AddQuadUVs();
      }
    }
    
    void AddQuadTriangles(int vertexIndex)
    {
        // First triangle
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);

        // Second triangle
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 3);
    }

    void AddQuadUVs()
    {
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(0, 1));
    }
}
