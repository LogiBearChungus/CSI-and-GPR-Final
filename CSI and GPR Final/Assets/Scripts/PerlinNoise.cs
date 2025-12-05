using System;
using Unity.VisualScripting;
using UnityEngine;



public class PerlinNoise : MonoBehaviour
{
    static int size = 10;
    public Vector2[,] gradients = new Vector2[size,size];
    const double twoPi = 2 * Math.PI;


    void generateGradients()
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                //randomly generates an angle for each point in grid and makes a gradient vector for it
                gradients[x, y] = CreateGradient(UnityEngine.Random.Range(0.0f, (float)twoPi));
                Debug.Log(gradients[x, y]);
            }

        }
    }
        

    Vector2 CreateGradient(float angle)
    {
        //Creates a unit vector from a given angle
        Vector2 gradient = new Vector2((float)Math.Cos(angle),(float)Math.Sin(angle));
        return gradient;
    }  

 
    // 2D Perlin noise, returns 0–1
    public float[,] createNoise(int chunkSize)
    {
        generateGradients();
        int totalVoxels = size * chunkSize;
        float[,] heightMap = new float[totalVoxels,totalVoxels];

        for(int x = 0; x < totalVoxels; x++)
        {
            for(int y = 0; y < totalVoxels; y++)
            {
                heightMap[x,y] = findHeight(x/16, y/16);
            }
        }


        return heightMap;
        
    }

    float findHeight(float x, float y)
    {
        
        int floorX = Mathf.FloorToInt(x);
        int floorY = Mathf.FloorToInt(y);
        int ceilingX = Mathf.CeilToInt(x);
        int ceilingY = Mathf.CeilToInt(y);

        Vector2 bottomLeft = new Vector2(x-floorX,y-floorY);
        Vector2 bottomRight = new Vector2(ceilingX-x,y-floorY);
        Vector2 topLeft = new Vector2(x - floorX, ceilingY-y);
        Vector2 topRight = new Vector2(ceilingX - x, ceilingY - y);

        float bottomLerp = Lerp(Vector2.Dot(bottomLeft, gradients[floorX,floorY]), Vector2.Dot(bottomRight, gradients[ceilingX,floorY]), Fade(x - floorX));
        
        float topLerp = Lerp(Vector2.Dot(topLeft, gradients[floorX,ceilingY]), Vector2.Dot(topRight, gradients[ceilingX,ceilingY]), Fade(x - floorX));
        
        
        return Lerp(bottomLerp, topLerp, Fade(y - floorY));
    }

    private float Fade(float t)
    {
        // 6t^5 - 15t^4 + 10t^3
        // This curve eases in and out
        //quintic fade function
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    // Linear interpolation
    private float Lerp(float a, float b, float t)
    {
        return a + t * (b - a);
    }
    
}
