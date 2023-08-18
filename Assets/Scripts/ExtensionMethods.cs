using Common.DataStructures;
using UnityEngine;

namespace ExtensionMethods
{
    public static class ExtensionMethods
    {
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        private static Texture2D texture = new(256, 1);

        public static void SetGradientFixed(this LineRenderer renderer, CustomGradient gradient)
        {
            int textureWidth = 256;
            int textureHeight = 1;
            
            renderer.sharedMaterial.SetTexture(MainTex, GenerateTextureFromGradient(gradient, textureWidth, textureHeight));
        }
        
        private static Texture2D GenerateTextureFromGradient(CustomGradient gradient, int width, int height)
        {
            texture = new Texture2D(width, height);

            for (int x = 0; x < width; x++)
            {
                Color color = gradient.Evaluate((float)x / (width - 1));
                for (int y = 0; y < height; y++)
                {
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            
            return texture;
        }
    }
}