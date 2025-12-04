using System;
using UnityEngine;

public class PerlinNoise
{
    private int[] perm;
    
    public PerlinNoise()
    {
        perm = new int[512];
        System.Random rand = new System.Random();

        // list 0 - 255
        int[] p = new int[256];
        for (int i = 0; i < 256; i++)
            p[i] = i;

        // Shuffle the list so the noise looks different each run
        for (int i = 0; i < 256; i++)
        {
            int swapIndex = rand.Next(256);
            int temp = p[i];
            p[i] = p[swapIndex];
            p[swapIndex] = temp;
        }

        // Duplicate the list so we can avoid wrapping logic 
        for (int i = 0; i < 512; i++)
            perm[i] = p[i % 256];
    }

    // 2D Perlin noise, returns 0–1
    public float Noise(float x, float y)
    {
        // Find the integer grid coordinates around our point
        int xi = (int)Math.Floor(x) & 255;
        int yi = (int)Math.Floor(y) & 255;

        // Distance inside the cell
        float xf = x - (int)Math.Floor(x);
        float yf = y - (int)Math.Floor(y);

        // Smooth the curve (removes sharp edges)
        float u = Fade(xf);
        float v = Fade(yf);

        // Hash the four corners
        int aa = perm[perm[xi] + yi];         //bottom left
        int ab = perm[perm[xi] + yi + 1];     //top left
        int ba = perm[perm[xi + 1] + yi];     //bottom right
        int bb = perm[perm[xi + 1] + yi + 1]; //top right

        // Get gradient dot products at each corner
        float x1 = Lerp(Grad(aa, xf, yf),
                        Grad(ba, xf - 1, yf), u);

        float x2 = Lerp(Grad(ab, xf, yf - 1),
                        Grad(bb, xf - 1, yf - 1), u);

        // Blend the top and bottom
        return (Lerp(x1, x2, v) + 1) * 0.5f;
    }

    private float Fade(float t)
    {
        // 6t^5 - 15t^4 + 10t^3
        // This curve eases in and out
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    // Linear interpolation
    private float Lerp(float a, float b, float t)
    {
        return a + t * (b - a);
    }


    // Simple gradient function using last 2 bits of hash (4 directions)
    private float Grad(int hash, float x, float y)
    {
        // Select one of 4 gradients based on hash
        switch (hash & 3)
        {
            case 0: return x + y;
            case 1: return -x + y;
            case 2: return x - y;
            case 3: return -x - y;
        }
        return 0; //should never happen
    }
}
