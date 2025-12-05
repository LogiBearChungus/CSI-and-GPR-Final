using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    [SerializeField] int chunkSize = 16;
    [SerializeField] GameObject chunkPrefab;
    [SerializeField] int chunkSideCount = 10;
    [SerializeField] int noiseMultiplier = 2;
    [SerializeField] float[,] heightMap;
    [SerializeField] PerlinNoise perlinNoise;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        perlinNoise.size = chunkSideCount*noiseMultiplier;
        GameObject[,] chunks = new GameObject[chunkSideCount, chunkSideCount];

        heightMap = perlinNoise.createNoise(chunkSize);

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
                go.GetComponent<VoxelMeshGenerator>().BuildChunk();
                chunks[x, z] = go;
            }

        }
    }
   
}
