using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public static class AudioOperations
{
    public static byte[] ConvertAudioClipToWav(AudioClip clip)
    {
        using (MemoryStream stream = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);

            // Cabeçalho WAV
            writer.Write(new char[] { 'R', 'I', 'F', 'F' });
            writer.Write(36 + samples.Length * 2);
            writer.Write(new char[] { 'W', 'A', 'V', 'E' });
            writer.Write(new char[] { 'f', 'm', 't', ' ' });
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)clip.channels);
            writer.Write(clip.frequency);
            writer.Write(clip.frequency * clip.channels * 2);
            writer.Write((short)(clip.channels * 2));
            writer.Write((short)16);

            // Escreve os dados de áudio
            writer.Write(new char[] { 'd', 'a', 't', 'a' });
            writer.Write(samples.Length * 2);
            foreach (float sample in samples)
            {
                short intSample = (short)(sample * short.MaxValue);
                writer.Write(intSample);
            }

            return stream.ToArray();
        }
    }

    public static float GetLoudnessAverage(AudioClip clip, int clipPos, int sampleWindow = 64)
    {
        // sampleWindow = 3 * 16000;

        int startPosition = clipPos - sampleWindow;
        if(startPosition < 0)
        {
            return 0;
        }

        float[] data = new float[sampleWindow];
        clip.GetData(data, startPosition);

        float sample_sum = 0;
        foreach (float audio_sample in data)
        {
            //sample_sum += Mathf.Abs(audio_sample);
            sample_sum += audio_sample * audio_sample; // RMS
        }

        return Mathf.Sqrt(sample_sum / sampleWindow);
    }

    public static bool IsAnySubwindowLoud(AudioClip clip, int endPosition, int totalWindowSize, int subwindowSize, float threshold)
    {
        int startPosition = endPosition - totalWindowSize;
        if (startPosition < 0) return false;

        float[] data = new float[totalWindowSize];
        clip.GetData(data, startPosition);

        int numSubwindows = totalWindowSize / subwindowSize;

        for (int i = 0; i < numSubwindows; i++)
        {
            float sum = 0f;
            for (int j = 0; j < subwindowSize; j++)
            {
                int index = i * subwindowSize + j;
                var audio_sample = data[index];
                //sum += Mathf.Abs(data[index]);
                sum += audio_sample * audio_sample; // RMS
            }

            float avg = Mathf.Sqrt(sum / subwindowSize);
            if (avg > threshold)
            {
                return true; 
            }
        }

        return false;
    }
}
