using System.Collections;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    [SerializeField] int chunkSize = 16;
    [SerializeField] GameObject chunkPrefab;
    [SerializeField] int chunkSideCount = 10;
    [SerializeField] int noiseMultiplier = 1;
    [SerializeField] float[,] heightMap;
    [SerializeField] PerlinNoise perlinNoise;
    [SerializeField] float defaultHeightMultiplier;
    [SerializeField] bool developSlope = false;
    private GameObject[,] chunks;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        perlinNoise.size = chunkSideCount*noiseMultiplier;
        chunks = new GameObject[chunkSideCount, chunkSideCount];

        heightMap = perlinNoise.createNoise(chunkSize);

        // Pass each chunk its relative information and then build the chunk
        for (int x = 0; x < chunkSideCount; x++)
        {
            for (int z = 0; z < chunkSideCount; z++)
            {
                var go = Instantiate(chunkPrefab);
                go.transform.position = new Vector3(x*chunkSize, 0, z*chunkSize);
                go.GetComponent<VoxelMeshGenerator>().heightMap = heightMap;
                go.GetComponent<VoxelMeshGenerator>().xLocation = x;
                go.GetComponent<VoxelMeshGenerator>().zLocation = z;
                go.GetComponent<VoxelMeshGenerator>().chunkSize = chunkSize;
                go.GetComponent<VoxelMeshGenerator>().heightMultiplier = defaultHeightMultiplier;
                go.GetComponent<VoxelMeshGenerator>().chunkManager = this.GetComponent<ChunkManager>();
                go.GetComponent<VoxelMeshGenerator>().BuildChunk();
                chunks[x, z] = go;
            }
            if (developSlope)
            defaultHeightMultiplier += .2f;

        }
        foreach (var chunk in chunks)
        {
            chunk.GetComponent<VoxelMeshGenerator>().GenerateMesh();
        }
    }


    public  bool CheckChunkNeighborX(int chunkX,int chunkZ, int xCoord, int yCoord, int zCoord)
    {
        if(xCoord == 0 && chunkX != 0)
        {
            return chunks[chunkX-1,chunkZ].GetComponent<VoxelMeshGenerator>().IsVoxelSolid(chunkSize-1,yCoord,zCoord);
        }
        else if(xCoord == chunkSize - 1 && chunkX < chunkSideCount-1)
        {
            return chunks[chunkX + 1, chunkZ].GetComponent<VoxelMeshGenerator>().IsVoxelSolid(0, yCoord, zCoord);
        }
        else
        {
            return false;
        }



    }

    public bool CheckChunkNeighborZ(int chunkX, int chunkZ, int xCoord, int yCoord, int zCoord)
    {
        if (zCoord == 0 && chunkZ !=0)
        {
            return chunks[chunkX, chunkZ - 1].GetComponent<VoxelMeshGenerator>().IsVoxelSolid(xCoord, yCoord, chunkSize - 1);
        }
        else if(zCoord == chunkSize-1 && chunkZ <chunkSideCount-1)
        {
            return chunks[chunkX, chunkZ + 1].GetComponent<VoxelMeshGenerator>().IsVoxelSolid(xCoord, yCoord, 0);
        }
        else
        {
            return false;
        }



    }
}
