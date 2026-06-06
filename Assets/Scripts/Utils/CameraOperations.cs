using UnityEngine;

public static class CameraOperations
{
    // Método principal que será chamado pelo CameraController
    public static Texture2D GetCroppedAndRotatedTexture(WebCamTexture webCam)
    {
        int originalWidth = webCam.width;
        int originalHeight = webCam.height;

        // 1. Descobre o tamanho do quadrado (o menor lado da resolução da câmera)
        int size = Mathf.Min(originalWidth, originalHeight);

        // 2. Calcula de onde começar a cortar para pegar exatamente o centro
        int startX = (originalWidth - size) / 2;
        int startY = (originalHeight - size) / 2;

        // 3. Pega APENAS os pixels do quadrado central
        Color[] croppedPixels = webCam.GetPixels(startX, startY, size, size);

        // 4. Calcula a rotação necessária
        int angle = (360 - webCam.videoRotationAngle) % 360;

        // 5. Rotaciona apenas os pixels do quadrado
        Color[] finalPixels = RotateSquarePixels(croppedPixels, size, angle);

        // 6. Cria a textura final quadrada
        Texture2D finalTexture = new Texture2D(size, size);
        finalTexture.SetPixels(finalPixels);
        finalTexture.Apply();

        return finalTexture;
    }

    // Método auxiliar otimizado para rotacionar um array quadrado
    private static Color[] RotateSquarePixels(Color[] pixels, int size, int angle)
    {
        if (angle == 0 || angle == 360) return pixels;

        Color[] rotated = new Color[pixels.Length];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int sourceIndex = y * size + x;
                int targetIndex;

                switch (angle)
                {
                    case 90:
                        targetIndex = x * size + (size - y - 1);
                        break;
                    case 180:
                        targetIndex = (size - y - 1) * size + (size - x - 1);
                        break;
                    case 270:
                        targetIndex = (size - x - 1) * size + y;
                        break;
                    default: 
                        return pixels; 
                }

                rotated[targetIndex] = pixels[sourceIndex];
            }
        }
        return rotated;
    }
}