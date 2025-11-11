using UnityEngine;

public class VoxelMeshGenerator : MonoBehaviour
{

    public Material voxelMaterial;
    public int[,,] voxelData;
    [SerializeField] int chunkSize = 16;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        voxelData = new int[chunkSize, chunkSize, chunkSize];

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    if ((x + y + z) % 2 == 0)
                    {
                        voxelData[x, y, z] = 1;
                    }
                    else
                    {
                        voxelData[x, y, z] = 0;

                    }
                }
            }


        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
