using System.IO;
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
}
