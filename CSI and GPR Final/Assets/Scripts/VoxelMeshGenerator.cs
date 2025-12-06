//I used this video as a reference https://www.youtube.com/watch?v=lyDJPVp7Oc8 but its not copy and pasted

using JetBrains.Annotations;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class VoxelMeshGenerator : MonoBehaviour
{
    public Material voxelMaterial;
    public int[,,] voxelData;
    const int width = 1, height = 1;
    [SerializeField] public float[,] heightMap;
    [SerializeField] public int xLocation;
    [SerializeField] public int zLocation;
    [SerializeField] public int chunkSize;
    [SerializeField] public float heightMultiplier;
    public ChunkManager chunkManager;
    private Mesh mesh;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();
    [SerializeField] public Material grass;
    private List<Material> usedMaterials;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void BuildChunk()
    {
        usedMaterials = new List<Material>();
        mesh = new Mesh();
        //Generate Voxel Data
        voxelData = new int[chunkSize, chunkSize, chunkSize];
        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                int height = (chunkSize/2) + Mathf.CeilToInt(chunkSize * (heightMap[x+(xLocation*chunkSize), z+(zLocation*chunkSize)]) * heightMultiplier);
                
                for (int y = 0; y < chunkSize; y++)
                {
                    if (y < height)
                    {
                        voxelData[x,y,z] = 1;
                    }
                    else
                    {
                        voxelData[x, y, z] = 0;
                    }

                }
                
            }


        }
        //GenerateMesh();
    }

    public void GenerateMesh()
    {
        mesh.Clear();
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
      
       GetComponent<MeshFilter>().mesh = mesh;
        
        
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        // Apply material
        if (voxelMaterial != null)
        {
            GetComponent<MeshRenderer>().material = voxelMaterial;
        }
      /*  Material[] finalMaterials = new Material[usedMaterials.Count];
        for (int i = 0; i < usedMaterials.Count; i++)
        {
            finalMaterials[i] = usedMaterials[i];
        }
        GetComponent<MeshRenderer>().sharedMaterials = finalMaterials;*/
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
    public bool IsVoxelSolid(int x, int y, int z)
        {
            // Check if position is out of bounds
            if (x < 0 || x >= chunkSize || y < 0 || y >= chunkSize || z < 0 || z >= chunkSize)
            {
                return false; // Treat out of bounds as air
            }
            return voxelData[x, y, z] == 1;
        }

        bool IsNeighborSolid(int x, int y, int z){
        Debug.Log(xLocation);
        Debug.Log(zLocation);
         if(voxelData[x, y, z] == 1){
            return true;
         }else{ 
         return false; }


         }
    void CreateTopFace(int x, int y, int z)
    {
      if (y == chunkSize-1 || !IsNeighborSolid(x, y + 1, z))
      {
            mesh.subMeshCount++;
         int vertexIndex = vertices.Count;
         vertices.Add(new Vector3(x, y + 1, z));
         vertices.Add(new Vector3(x, y + 1, z + 1));
         vertices.Add(new Vector3(x + 1, y + 1, z + 1));
         vertices.Add(new Vector3(x + 1, y + 1, z));
            AddQuadTriangles(vertexIndex);
            //mesh.SetTriangles(AddSubMeshTriangles(vertexIndex), mesh.subMeshCount - 1);
            //usedMaterials.Add(grass);
         AddQuadUVs(0.5f);
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
        AddQuadUVs(1);
      }
    }

    void CreateLeftFace(int x, int y, int z)
    {
        if (x == 0)
        {
            if (!chunkManager.CheckChunkNeighborX(xLocation, zLocation, x, y, z))
            {
                int vertexIndex = vertices.Count;
                vertices.Add(new Vector3(x, y, z));
                vertices.Add(new Vector3(x, y, z + 1));
                vertices.Add(new Vector3(x, y + 1, z + 1));
                vertices.Add(new Vector3(x, y + 1, z));
                AddQuadTriangles(vertexIndex);
                AddQuadUVs(1);
                
            }
        }else if (!IsNeighborSolid(x - 1, y, z))
        {
            int vertexIndex = vertices.Count;
            vertices.Add(new Vector3(x, y, z));
            vertices.Add(new Vector3(x, y, z + 1));
            vertices.Add(new Vector3(x, y + 1, z + 1));
            vertices.Add(new Vector3(x, y + 1, z));
            AddQuadTriangles(vertexIndex);
            AddQuadUVs(1);
        }
        
    }

    void CreateRightFace(int x, int y, int z)
    {
        if (x == chunkSize - 1)
        {
            if (!chunkManager.CheckChunkNeighborX(xLocation, zLocation, x, y, z))
            {
                int vertexIndex = vertices.Count;
                vertices.Add(new Vector3(x + 1, y, z));
                vertices.Add(new Vector3(x + 1, y + 1, z));
                vertices.Add(new Vector3(x + 1, y + 1, z + 1));
                vertices.Add(new Vector3(x + 1, y, z + 1));
                AddQuadTriangles(vertexIndex);
                AddQuadUVs(1);
            }
        }else if (!IsNeighborSolid(x + 1, y, z))
        {
            int vertexIndex = vertices.Count;
            vertices.Add(new Vector3(x + 1, y, z));
            vertices.Add(new Vector3(x + 1, y + 1, z));
            vertices.Add(new Vector3(x + 1, y + 1, z + 1));
            vertices.Add(new Vector3(x + 1, y, z + 1));
            AddQuadTriangles(vertexIndex);
            AddQuadUVs(1);
        }
        
    }

    void CreateFrontFace(int x, int y, int z)
    {
        if (z == chunkSize - 1)
        {
            if (!chunkManager.CheckChunkNeighborZ(xLocation, zLocation, x, y, z))
            {
                int vertexIndex = vertices.Count;
                vertices.Add(new Vector3(x, y, z + 1));
                vertices.Add(new Vector3(x + 1, y, z + 1));
                vertices.Add(new Vector3(x + 1, y + 1, z + 1));
                vertices.Add(new Vector3(x, y + 1, z + 1));
                AddQuadTriangles(vertexIndex);
                AddQuadUVs(1);
            }
        }else if (!IsNeighborSolid(x,y,z+1))
        {int vertexIndex = vertices.Count;
        vertices.Add(new Vector3(x, y, z + 1));
        vertices.Add(new Vector3(x + 1, y, z + 1));
        vertices.Add(new Vector3(x + 1, y + 1, z + 1));
        vertices.Add(new Vector3(x, y + 1, z + 1));
        AddQuadTriangles(vertexIndex);
         AddQuadUVs(1);
      }
    }

    void CreateBackFace(int x, int y, int z)
    {
        if (z == 0)
        {
            if (!chunkManager.CheckChunkNeighborZ(xLocation, zLocation, x, y, z))
            {
                int vertexIndex = vertices.Count;
                vertices.Add(new Vector3(x, y, z));
                vertices.Add(new Vector3(x, y + 1, z));
                vertices.Add(new Vector3(x + 1, y + 1, z));
                vertices.Add(new Vector3(x + 1, y, z));
                AddQuadTriangles(vertexIndex);
                AddQuadUVs(1);
            }
        }else if ( !IsNeighborSolid(x, y, z-1))
      {
         int vertexIndex = vertices.Count;
         vertices.Add(new Vector3(x, y, z));
         vertices.Add(new Vector3(x, y + 1, z));
         vertices.Add(new Vector3(x + 1, y + 1, z));
         vertices.Add(new Vector3(x + 1, y, z));
         AddQuadTriangles(vertexIndex);
         AddQuadUVs(1);
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

    List<int> AddSubMeshTriangles(int vertexIndex) {
        List<int> tAngles = new List<int>();
        tAngles.Add(vertexIndex);
        tAngles.Add(vertexIndex + 1);
        tAngles.Add(vertexIndex + 2);

        // Second triangle
        tAngles.Add(vertexIndex);
        tAngles.Add(vertexIndex + 2);
        tAngles.Add(vertexIndex + 3);

        return tAngles;
    }



    void AddQuadUVs(float texCoord)
    {
        uvs.Add(new Vector2(texCoord-0.5f, texCoord-0.5f));
        uvs.Add(new Vector2(texCoord, texCoord - 0.5f));
        uvs.Add(new Vector2(texCoord, texCoord));
        uvs.Add(new Vector2(texCoord - 0.5f, texCoord));
    }
}
