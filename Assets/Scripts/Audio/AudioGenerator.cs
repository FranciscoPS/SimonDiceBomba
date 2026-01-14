using UnityEngine;
public static class AudioGenerator
{
    public static AudioClip GenerateTone(float frequency, float duration, int sampleRate = 44100)
    {
        int sampleCount = Mathf.RoundToInt(duration * sampleRate);
        AudioClip clip = AudioClip.Create("GeneratedTone", sampleCount, 1, sampleRate, false);
        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * t);
            float envelope = 1f;
            float fadeTime = 0.01f; 
            int fadeSamples = Mathf.RoundToInt(fadeTime * sampleRate);
            if (i < fadeSamples)
            {
                envelope = (float)i / fadeSamples;
            }
            else if (i > sampleCount - fadeSamples)
            {
                envelope = (float)(sampleCount - i) / fadeSamples;
            }
            samples[i] *= envelope * 0.5f; 
        }
        clip.SetData(samples, 0);
        return clip;
    }
}
