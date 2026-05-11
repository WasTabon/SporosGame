using UnityEngine;

public class AmbientMusic : MonoBehaviour
{
    public static AmbientMusic Instance { get; private set; }

    private const int SampleRate = 22050;
    private const float Duration = 30f;
    private const float GlobalGain = 0.18f;

    private AudioSource source;
    private AudioClip clip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        source = GetComponent<AudioSource>();
        if (source == null) source = gameObject.AddComponent<AudioSource>();
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 0f;

        GenerateDrone();
        ApplyVolume();
        Play();
    }

    private void Update()
    {
        if (SoundManager.Instance != null && source != null)
        {
            float target = SoundManager.Instance.IsMuted() ? 0f : SoundManager.Instance.GetMusicVolume() * GlobalGain;
            if (Mathf.Abs(source.volume - target) > 0.001f)
                source.volume = target;
        }
    }

    private void ApplyVolume()
    {
        if (SoundManager.Instance != null && source != null)
            source.volume = SoundManager.Instance.IsMuted() ? 0f : SoundManager.Instance.GetMusicVolume() * GlobalGain;
        else if (source != null) source.volume = GlobalGain;
    }

    private void GenerateDrone()
    {
        int samples = Mathf.CeilToInt(Duration * SampleRate);
        var data = new float[samples];

        float[] freqs = new float[] { 80f, 80.4f, 120f, 119.5f, 160f, 161f };
        float[] amps = new float[] { 0.35f, 0.3f, 0.22f, 0.2f, 0.14f, 0.12f };

        float lfoFreq = 0.1f;

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float lfo = 0.55f + 0.35f * Mathf.Sin(2f * Mathf.PI * lfoFreq * t);

            float s = 0f;
            for (int k = 0; k < freqs.Length; k++)
                s += amps[k] * Mathf.Sin(2f * Mathf.PI * freqs[k] * t);

            s *= lfo;

            float fadeIn = Mathf.Clamp01(t / 1.0f);
            float fadeOut = Mathf.Clamp01((Duration - t) / 1.0f);
            s *= Mathf.Min(fadeIn, fadeOut);

            data[i] = s * 0.7f;
        }

        clip = AudioClip.Create("AmbientDrone", samples, 1, SampleRate, false);
        clip.SetData(data, 0);
    }

    public void Play()
    {
        if (source == null || clip == null) return;
        source.clip = clip;
        source.Play();
    }

    public void Stop()
    {
        if (source != null) source.Stop();
    }
}
