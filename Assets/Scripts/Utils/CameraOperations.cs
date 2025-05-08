using UnityEngine;

public static class CameraOperations
{
    public static Texture2D RotateTexture(Texture2D original, int angle)
    {
        int width = original.width;
        int height = original.height;
        Texture2D rotated;

        Color32[] originalPixels = original.GetPixels32();
        Color32[] rotatedPixels;

        switch (angle)
        {
            case 90:
                rotated = new Texture2D(height, width);
                rotatedPixels = new Color32[originalPixels.Length];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        rotatedPixels[x * height + (height - y - 1)] = originalPixels[y * width + x];
                    }
                }
                break;

            case 180:
                rotated = new Texture2D(width, height);
                rotatedPixels = new Color32[originalPixels.Length];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        rotatedPixels[(height - y - 1) * width + (width - x - 1)] = originalPixels[y * width + x];
                    }
                }
                break;

            case 270:
                rotated = new Texture2D(height, width);
                rotatedPixels = new Color32[originalPixels.Length];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        rotatedPixels[(width - x - 1) * height + y] = originalPixels[y * width + x];
                    }
                }
                break;

            default:
                throw new System.ArgumentException("Angle must be 90, 180, or 270.");
        }

        rotated.SetPixels32(rotatedPixels);
        rotated.Apply();
        return rotated;
    }

    public static Texture2D RotateTexture(Color32[] pixels, int width, int height, int angle)
    {
        Texture2D rotated;

        Color32[] originalPixels = pixels;
        Color32[] rotatedPixels;

        switch (angle)
        {
            case 90:
                rotated = new Texture2D(height, width);
                rotatedPixels = new Color32[originalPixels.Length];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        rotatedPixels[x * height + (height - y - 1)] = originalPixels[y * width + x];
                    }
                }
                break;

            case 180:
                rotated = new Texture2D(width, height);
                rotatedPixels = new Color32[originalPixels.Length];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        rotatedPixels[(height - y - 1) * width + (width - x - 1)] = originalPixels[y * width + x];
                    }
                }
                break;

            case 270:
                rotated = new Texture2D(height, width);
                rotatedPixels = new Color32[originalPixels.Length];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        rotatedPixels[(width - x - 1) * height + y] = originalPixels[y * width + x];
                    }
                }
                break;

            default:
                throw new System.ArgumentException("Angle must be 90, 180, or 270.");
        }

        rotated.SetPixels32(rotatedPixels);
        rotated.Apply();
        return rotated;
    }
}
