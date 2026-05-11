using System.Collections.Generic;
using UnityEngine;

public enum SfxType
{
    Click,
    Hover,
    Success,
    Fail,
    Pop,
    PopupOpen,
    PopupClose
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    private const int PoolSize = 5;
    private const int SampleRate = 44100;

    private List<AudioSource> sfxPool;
    private AudioSource musicSource;
    private Dictionary<SfxType, AudioClip> clipCache;

    private float sfxVolume = 1f;
    private float musicVolume = 0.6f;
    private bool muted;

    private const string KeySfxVolume = "spo_sfx_volume";
    private const string KeyMusicVolume = "spo_music_volume";
    private const string KeyMuted = "spo_muted";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadPrefs();
        InitPool();
        InitMusic();
        GenerateClips();
    }

    private void LoadPrefs()
    {
        sfxVolume = PlayerPrefs.GetFloat(KeySfxVolume, 1f);
        musicVolume = PlayerPrefs.GetFloat(KeyMusicVolume, 0.6f);
        muted = PlayerPrefs.GetInt(KeyMuted, 0) == 1;
    }

    private void InitPool()
    {
        sfxPool = new List<AudioSource>();
        for (int i = 0; i < PoolSize; i++)
        {
            var go = new GameObject("SfxSource_" + i);
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 0f;
            sfxPool.Add(src);
        }
    }

    private void InitMusic()
    {
        var go = new GameObject("MusicSource");
        go.transform.SetParent(transform);
        musicSource = go.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
    }

    private void GenerateClips()
    {
        clipCache = new Dictionary<SfxType, AudioClip>
        {
            { SfxType.Click,       GenerateTone(0.06f, 880f, 0f,    WaveType.Sine,     0.015f, 0.045f, 0.5f) },
            { SfxType.Hover,       GenerateTone(0.04f, 1320f, 0f,   WaveType.Sine,     0.005f, 0.035f, 0.3f) },
            { SfxType.Success,     GenerateChord(0.35f, new float[]{ 660f, 880f, 1320f }, 0.02f, 0.32f, 0.55f) },
            { SfxType.Fail,        GenerateTone(0.30f, 220f, -120f, WaveType.Triangle, 0.01f, 0.28f, 0.55f) },
            { SfxType.Pop,         GenerateTone(0.10f, 600f, 400f,  WaveType.Sine,     0.005f, 0.09f, 0.55f) },
            { SfxType.PopupOpen,   GenerateTone(0.18f, 440f, 660f,  WaveType.Sine,     0.01f, 0.16f, 0.5f) },
            { SfxType.PopupClose,  GenerateTone(0.18f, 660f, -440f, WaveType.Sine,     0.01f, 0.16f, 0.5f) }
        };
    }

    private enum WaveType { Sine, Triangle, Square }

    private AudioClip GenerateTone(float duration, float startFreq, float freqDelta, WaveType wave, float attack, float release, float amp)
    {
        int samples = Mathf.CeilToInt(duration * SampleRate);
        var data = new float[samples];
        int attackSamples = Mathf.CeilToInt(attack * SampleRate);
        int releaseSamples = Mathf.CeilToInt(release * SampleRate);
        int releaseStart = samples - releaseSamples;

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float freq = startFreq + freqDelta * (t / duration);
            float phase = 2f * Mathf.PI * freq * t;
            float s;
            switch (wave)
            {
                case WaveType.Triangle: s = Mathf.PingPong(phase / Mathf.PI, 2f) - 1f; break;
                case WaveType.Square:   s = Mathf.Sign(Mathf.Sin(phase)); break;
                default:                s = Mathf.Sin(phase); break;
            }

            float env = 1f;
            if (i < attackSamples) env = (float)i / attackSamples;
            else if (i > releaseStart) env = 1f - (float)(i - releaseStart) / releaseSamples;

            data[i] = s * env * amp;
        }

        var clip = AudioClip.Create("tone", samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip GenerateChord(float duration, float[] freqs, float attack, float release, float amp)
    {
        int samples = Mathf.CeilToInt(duration * SampleRate);
        var data = new float[samples];
        int attackSamples = Mathf.CeilToInt(attack * SampleRate);
        int releaseSamples = Mathf.CeilToInt(release * SampleRate);
        int releaseStart = samples - releaseSamples;

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float s = 0f;
            for (int f = 0; f < freqs.Length; f++)
                s += Mathf.Sin(2f * Mathf.PI * freqs[f] * t);
            s /= freqs.Length;

            float env = 1f;
            if (i < attackSamples) env = (float)i / attackSamples;
            else if (i > releaseStart) env = 1f - (float)(i - releaseStart) / releaseSamples;

            data[i] = s * env * amp;
        }

        var clip = AudioClip.Create("chord", samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    public void PlaySfx(SfxType type)
    {
        if (muted) return;
        if (!clipCache.TryGetValue(type, out var clip)) return;

        var src = GetFreeSource();
        src.clip = clip;
        src.volume = sfxVolume;
        src.pitch = 1f;
        src.Play();
    }

    private AudioSource GetFreeSource()
    {
        for (int i = 0; i < sfxPool.Count; i++)
            if (!sfxPool[i].isPlaying) return sfxPool[i];
        return sfxPool[0];
    }

    public void SetSfxVolume(float v)
    {
        sfxVolume = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat(KeySfxVolume, sfxVolume);
    }

    public void SetMusicVolume(float v)
    {
        musicVolume = Mathf.Clamp01(v);
        musicSource.volume = muted ? 0f : musicVolume;
        PlayerPrefs.SetFloat(KeyMusicVolume, musicVolume);
    }

    public void SetMuted(bool m)
    {
        muted = m;
        musicSource.volume = muted ? 0f : musicVolume;
        PlayerPrefs.SetInt(KeyMuted, muted ? 1 : 0);
    }

    public float GetSfxVolume() => sfxVolume;
    public float GetMusicVolume() => musicVolume;
    public bool IsMuted() => muted;
}
