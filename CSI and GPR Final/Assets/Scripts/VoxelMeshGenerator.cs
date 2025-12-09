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
    [SerializeField] public Material[] materials;
    private List<int> grassTriangles = new List<int>();
    private List<int> dirtTriangles = new List<int>();
    private List<int> stoneTriangles = new List<int>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void BuildChunk()
    {
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
                        voxelData[x,y,z] = GetMaterialIDByHeight(y, height);
                    }
                    else
                    {
                        voxelData[x, y, z] = 0; //air
                    }

                }
                
            }


        }
    }

    public void GenerateMesh()
    {
        mesh.Clear();
        vertices.Clear();
        grassTriangles.Clear();
        dirtTriangles.Clear();
        stoneTriangles.Clear();
        uvs.Clear();

        // Loop through all voxels
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    if (voxelData[x, y, z] > 0)
                    {
                        CreateVoxel(x, y, z, voxelData[x,y,z]);
                    }
                }
            }
        }

        // Assign mesh data
        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();

        int subMeshCount = 0;
        if (grassTriangles.Count > 0) subMeshCount++;
        if (dirtTriangles.Count > 0) subMeshCount++;
        if (stoneTriangles.Count > 0) subMeshCount++;

        mesh.subMeshCount = subMeshCount;

        int currentSubMesh = 0;
        List<Material> activeMaterials = new List<Material>();

        if (grassTriangles.Count > 0)
        {
            mesh.SetTriangles(grassTriangles, currentSubMesh);
            activeMaterials.Add(materials[0]);
            currentSubMesh++;
        }
        if (dirtTriangles.Count > 0)
        {
            mesh.SetTriangles(dirtTriangles, currentSubMesh);
            activeMaterials.Add(materials[1]);
            currentSubMesh++;
        }
        if (stoneTriangles.Count > 0)
        {
            mesh.SetTriangles(stoneTriangles, currentSubMesh);
            activeMaterials.Add(materials[2]);
            currentSubMesh++;
        }

        mesh.RecalculateNormals();

        // Apply to components
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().materials = activeMaterials.ToArray();
    }
    void CreateVoxel(int x, int y, int z, int materialID)
    {
        // Check each face and only create it if it's exposed
        if (!IsVoxelSolid(x, y + 1, z)) CreateTopFace(x, y, z, materialID);      // Top
        if (!IsVoxelSolid(x, y - 1, z)) CreateBottomFace(x, y, z, materialID);   // Bottom
        if (!IsVoxelSolid(x - 1, y, z)) CreateLeftFace(x, y, z, materialID);     // Left
        if (!IsVoxelSolid(x + 1, y, z)) CreateRightFace(x, y, z, materialID);    // Right
        if (!IsVoxelSolid(x, y, z + 1)) CreateFrontFace(x, y, z, materialID);    // Front
        if (!IsVoxelSolid(x, y, z - 1)) CreateBackFace(x, y, z, materialID);     // Back

    }
    public bool IsVoxelSolid(int x, int y, int z)
        {
            // Check if position is out of bounds
            if (x < 0 || x >= chunkSize || y < 0 || y >= chunkSize || z < 0 || z >= chunkSize)
            {
                return false; // Treat out of bounds as air
            }
            return voxelData[x, y, z] > 0;
        }

        bool IsNeighborSolid(int x, int y, int z){
         if(voxelData[x, y, z] > 0){
            return true;
         }else{ 
         return false; }


         }
    void CreateTopFace(int x, int y, int z, int materialID)
    {
      if (y == chunkSize-1 || !IsNeighborSolid(x, y + 1, z))
      {
         int vertexIndex = vertices.Count;
         vertices.Add(new Vector3(x, y + 1, z));
         vertices.Add(new Vector3(x, y + 1, z + 1));
         vertices.Add(new Vector3(x + 1, y + 1, z + 1));
         vertices.Add(new Vector3(x + 1, y + 1, z));

         // Add triangles to the correct material list
         AddQuadTrianglesToMaterial(vertexIndex, materialID);
         AddQuadUVs(0.5f);
      }
    }

    void CreateBottomFace(int x, int y, int z, int materialID)
    {
      if(y == 0 || !IsNeighborSolid(x, y -1, z))
       { int vertexIndex = vertices.Count;
        vertices.Add(new Vector3(x, y, z));
        vertices.Add(new Vector3(x + 1, y, z));
        vertices.Add(new Vector3(x + 1, y, z + 1));
        vertices.Add(new Vector3(x, y, z + 1));
        AddQuadTrianglesToMaterial(vertexIndex, materialID);
        AddQuadUVs(1);
      }
    }

    void CreateLeftFace(int x, int y, int z, int materialID)
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
                AddQuadTrianglesToMaterial(vertexIndex, materialID);
                AddQuadUVs(1);
                
            }
        }else if (!IsNeighborSolid(x - 1, y, z))
        {
            int vertexIndex = vertices.Count;
            vertices.Add(new Vector3(x, y, z));
            vertices.Add(new Vector3(x, y, z + 1));
            vertices.Add(new Vector3(x, y + 1, z + 1));
            vertices.Add(new Vector3(x, y + 1, z));
            AddQuadTrianglesToMaterial(vertexIndex, materialID);
            AddQuadUVs(1);
        }
        
    }

    void CreateRightFace(int x, int y, int z, int materialID)
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
                AddQuadTrianglesToMaterial(vertexIndex, materialID);
                AddQuadUVs(1);
            }
        }else if (!IsNeighborSolid(x + 1, y, z))
        {
            int vertexIndex = vertices.Count;
            vertices.Add(new Vector3(x + 1, y, z));
            vertices.Add(new Vector3(x + 1, y + 1, z));
            vertices.Add(new Vector3(x + 1, y + 1, z + 1));
            vertices.Add(new Vector3(x + 1, y, z + 1));
            AddQuadTrianglesToMaterial(vertexIndex, materialID);
            AddQuadUVs(1);
        }
        
    }

    void CreateFrontFace(int x, int y, int z, int materialID)
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
                AddQuadTrianglesToMaterial(vertexIndex, materialID);
                AddQuadUVs(1);
            }
        }else if (!IsNeighborSolid(x,y,z+1))
        {int vertexIndex = vertices.Count;
        vertices.Add(new Vector3(x, y, z + 1));
        vertices.Add(new Vector3(x + 1, y, z + 1));
        vertices.Add(new Vector3(x + 1, y + 1, z + 1));
        vertices.Add(new Vector3(x, y + 1, z + 1));
            AddQuadTrianglesToMaterial(vertexIndex, materialID);
            AddQuadUVs(1);
      }
    }

    void CreateBackFace(int x, int y, int z, int materialID)
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
                AddQuadTrianglesToMaterial(vertexIndex, materialID);
                AddQuadUVs(1);
            }
        }else if ( !IsNeighborSolid(x, y, z-1))
      {
         int vertexIndex = vertices.Count;
         vertices.Add(new Vector3(x, y, z));
         vertices.Add(new Vector3(x, y + 1, z));
         vertices.Add(new Vector3(x + 1, y + 1, z));
         vertices.Add(new Vector3(x + 1, y, z));
            AddQuadTrianglesToMaterial(vertexIndex, materialID);
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

    int GetMaterialIDByHeight(int currentY, int surfaceHeight)
    {
        // If we're at or near the surface, use grass
        if (currentY >= surfaceHeight - 1)
        {
            return 1; // Grass
        }
        // A few blocks below surface, use dirt
        else if (currentY >= surfaceHeight - 4)
        {
            return 2; // Dirt
        }
        // Everything else is stone
        else
        {
            return 3; // Stone
        }
    }

    void AddQuadTrianglesToMaterial(int vertexIndex, int materialID)
    {
        // Add to the appropriate triangle list based on material
        List<int> targetList = null;

        if (materialID == 1) targetList = grassTriangles;
        else if (materialID == 2) targetList = dirtTriangles;
        else if (materialID == 3) targetList = stoneTriangles;

        if (targetList != null)
        {
            targetList.Add(vertexIndex);
            targetList.Add(vertexIndex + 1);
            targetList.Add(vertexIndex + 2);

            targetList.Add(vertexIndex);
            targetList.Add(vertexIndex + 2);
            targetList.Add(vertexIndex + 3);
        }
    }
}
