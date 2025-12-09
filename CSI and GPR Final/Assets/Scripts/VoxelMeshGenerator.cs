//I used this video as a reference https://www.youtube.com/watch?v=lyDJPVp7Oc8 but its not copy and pasted

using JetBrains.Annotations;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class VoxelMeshGenerator : MonoBehaviour
{
    public Material voxelMaterial;
    public int[,,] voxelData;
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
    [SerializeField] public Material[] materials;
    private List<int> grassTopTriangles = new List<int>();
    private List<int> grassSideTriangles = new List<int>();
    private List<int> dirtTriangles = new List<int>();
    private List<int> stoneTriangles = new List<int>();


    //Builds voxel data for this chunk using heightmap
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

    // Converts voxelData into actual mesh geometry
    public void GenerateMesh()
    {
        // Reset mesh buffers
        mesh.Clear();
        vertices.Clear();
        grassSideTriangles.Clear();
        grassTopTriangles.Clear();
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

        // Determine how many submeshes are needed (only add ones we use)
        int subMeshCount = 0;
        if (grassSideTriangles.Count > 0) subMeshCount++;
        if (grassTopTriangles.Count > 0) subMeshCount++;
        if (dirtTriangles.Count > 0) subMeshCount++;
        if (stoneTriangles.Count > 0) subMeshCount++;

        mesh.subMeshCount = subMeshCount;

        int currentSubMesh = 0;
        List<Material> activeMaterials = new List<Material>();

        // Assign triangles to each submesh in order
        if (grassTopTriangles.Count > 0)
        {
            mesh.SetTriangles(grassTopTriangles, currentSubMesh);
            activeMaterials.Add(materials[0]);
            currentSubMesh++;
        }
        if (grassSideTriangles.Count > 0)
        {
            mesh.SetTriangles(grassSideTriangles, currentSubMesh);
            activeMaterials.Add(materials[1]);
            currentSubMesh++;
        }
        if (dirtTriangles.Count > 0)
        {
            mesh.SetTriangles(dirtTriangles, currentSubMesh);
            activeMaterials.Add(materials[2]);
            currentSubMesh++;
        }
        if (stoneTriangles.Count > 0)
        {
            mesh.SetTriangles(stoneTriangles, currentSubMesh);
            activeMaterials.Add(materials[3]);
            currentSubMesh++;
        }

        // Compute smooth lighting
        mesh.RecalculateNormals();

        // Apply to components
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().materials = activeMaterials.ToArray();
    }

    // Generates all exposed faces for a single voxel
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

    // Returns whether a voxel index is in-bounds and solid
    public bool IsVoxelSolid(int x, int y, int z)
        {
            // Check if position is out of bounds
            if (x < 0 || x >= chunkSize || y < 0 || y >= chunkSize || z < 0 || z >= chunkSize)
            {
                return false; // Treat out of bounds as air
            }
            return voxelData[x, y, z] > 0;
        }


    // Simple neighbor-check helper (used by faces)
    bool IsNeighborSolid(int x, int y, int z){
        return voxelData[x, y, z] > 0;
    }

    // --- FACE CREATION FUNCTIONS ---------------------------------------------------
    void CreateTopFace(int x, int y, int z, int materialID)
    {
        if (materialID == 1)
        {
            materialID = 0; //make sure the top of grass is grass
        }
        if (y == chunkSize-1 || !IsNeighborSolid(x, y + 1, z))
      {
         int vertexIndex = vertices.Count;
         vertices.Add(new Vector3(x, y + 1, z));
         vertices.Add(new Vector3(x, y + 1, z + 1));
         vertices.Add(new Vector3(x + 1, y + 1, z + 1));
         vertices.Add(new Vector3(x + 1, y + 1, z));

         // Add triangles to the correct material list
         AddQuadTrianglesToMaterial(vertexIndex, materialID);
         AddQuadUVs();
      }
    }

    void CreateBottomFace(int x, int y, int z, int materialID)
    {
        if (materialID == 1)
        {
            materialID = 2; //turn the bottom of grass blocks into dirt
        }
        if (y == 0 || !IsNeighborSolid(x, y -1, z))
       { int vertexIndex = vertices.Count;
        vertices.Add(new Vector3(x, y, z));
        vertices.Add(new Vector3(x + 1, y, z));
        vertices.Add(new Vector3(x + 1, y, z + 1));
        vertices.Add(new Vector3(x, y, z + 1));
        AddQuadTrianglesToMaterial(vertexIndex, materialID);
        AddQuadUVs();
      }
    }

    void CreateLeftFace(int x, int y, int z, int materialID)
    {
 
        if (x == 0)
        {
            if (!chunkManager.CheckChunkNeighborX(xLocation, zLocation, x, y, z))
            {
                int vertexIndex = vertices.Count;
                vertices.Add(new Vector3(x, y, z));   // bottom-back
                vertices.Add(new Vector3(x, y, z + 1)); // bottom-front
                vertices.Add(new Vector3(x, y + 1, z + 1)); // top-front
                vertices.Add(new Vector3(x, y + 1, z));   // top-back
                AddQuadTrianglesToMaterial(vertexIndex, materialID);
                AddQuadUVs();
                
            }
        }else if (!IsNeighborSolid(x - 1, y, z))
        {
            int vertexIndex = vertices.Count;
            vertices.Add(new Vector3(x, y, z));   // bottom-back
            vertices.Add(new Vector3(x, y, z + 1)); // bottom-front
            vertices.Add(new Vector3(x, y + 1, z + 1)); // top-front
            vertices.Add(new Vector3(x, y + 1, z));   // top-back
            AddQuadTrianglesToMaterial(vertexIndex, materialID);
            AddQuadUVs();
        }
        
    }

    void CreateRightFace(int x, int y, int z, int materialID)
    {
    
        if (x == chunkSize - 1)
        {
            if (!chunkManager.CheckChunkNeighborX(xLocation, zLocation, x, y, z))
            {
                int vertexIndex = vertices.Count;
                vertices.Add(new Vector3(x + 1, y, z + 1)); // bottom-front
                vertices.Add(new Vector3(x + 1, y, z));   // bottom-back
                vertices.Add(new Vector3(x + 1, y + 1, z));   // top-back
                vertices.Add(new Vector3(x + 1, y + 1, z + 1)); // top-front

                AddQuadTrianglesToMaterial(vertexIndex, materialID);
                AddQuadUVs();
            }
        }else if (!IsNeighborSolid(x + 1, y, z))
        {
            int vertexIndex = vertices.Count;
            vertices.Add(new Vector3(x + 1, y, z + 1)); // bottom-front
            vertices.Add(new Vector3(x + 1, y, z));   // bottom-back
            vertices.Add(new Vector3(x + 1, y + 1, z));   // top-back
            vertices.Add(new Vector3(x + 1, y + 1, z + 1)); // top-front
            AddQuadTrianglesToMaterial(vertexIndex, materialID);
            AddQuadUVs();
        }
        
    }

    void CreateFrontFace(int x, int y, int z, int materialID)
    {
       
        if (z == chunkSize - 1)
        {
            if (!chunkManager.CheckChunkNeighborZ(xLocation, zLocation, x, y, z))
            {
                int vertexIndex = vertices.Count;
                vertices.Add(new Vector3(x, y, z + 1)); // bottom-left
                vertices.Add(new Vector3(x + 1, y, z + 1)); // bottom-right
                vertices.Add(new Vector3(x + 1, y + 1, z + 1)); // top-right
                vertices.Add(new Vector3(x, y + 1, z + 1)); // top-left
                AddQuadTrianglesToMaterial(vertexIndex, materialID);
                AddQuadUVs();
            }
        }else if (!IsNeighborSolid(x,y,z+1))
        {int vertexIndex = vertices.Count;
            vertices.Add(new Vector3(x, y, z + 1)); // bottom-left
            vertices.Add(new Vector3(x + 1, y, z + 1)); // bottom-right
            vertices.Add(new Vector3(x + 1, y + 1, z + 1)); // top-right
            vertices.Add(new Vector3(x, y + 1, z + 1)); // top-left
            AddQuadTrianglesToMaterial(vertexIndex, materialID);
            AddQuadUVs();
      }
    }

    void CreateBackFace(int x, int y, int z, int materialID)
    {
       
        if (z == 0)
        {
            if (!chunkManager.CheckChunkNeighborZ(xLocation, zLocation, x, y, z))
            {
                int vertexIndex = vertices.Count;
                vertices.Add(new Vector3(x + 1, y, z));   // bottom-right
                vertices.Add(new Vector3(x, y, z));   // bottom-left
                vertices.Add(new Vector3(x, y + 1, z));   // top-left
                vertices.Add(new Vector3(x + 1, y + 1, z));   // top-right
                AddQuadTrianglesToMaterial(vertexIndex, materialID);
                AddQuadUVs();
            }
        }else if ( !IsNeighborSolid(x, y, z-1))
      {
         int vertexIndex = vertices.Count;
            vertices.Add(new Vector3(x + 1, y, z));   // bottom-right
            vertices.Add(new Vector3(x, y, z));   // bottom-left
            vertices.Add(new Vector3(x, y + 1, z));   // top-left
            vertices.Add(new Vector3(x + 1, y + 1, z));   // top-right
            AddQuadTrianglesToMaterial(vertexIndex, materialID);
            AddQuadUVs();
      }
      
    }


    // Adds UVs for a standard square
    void AddQuadUVs()
    {
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(0, 1));
    }

    // Picks material by how deep the voxel is relative to the surface
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

    // Pushes triangle indices into the correct material submesh list
    void AddQuadTrianglesToMaterial(int vertexIndex, int materialID)
    {

        List<int> targetList = null;

        if (materialID == 1) targetList = grassSideTriangles;
        else if (materialID == 0) targetList = grassTopTriangles;
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
