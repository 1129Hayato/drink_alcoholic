using UnityEngine;

namespace DrinkAlcoholic
{
    /// <summary>BGM/SE の再生と音量保存。初回アクセス時に自動生成され、シーンをまたいで残る。</summary>
    public class AudioManager : MonoBehaviour
    {
        const string BgmVolumeKey = "Volume.Bgm";
        const string SeVolumeKey = "Volume.Se";

        static AudioManager instance;

        public static AudioManager Instance
        {
            get
            {
                if (instance == null)
                {
                    new GameObject(nameof(AudioManager)).AddComponent<AudioManager>();
                }
                return instance;
            }
        }

        AudioSource bgmSource;
        AudioSource seSource;

        public float BgmVolume { get; private set; } = 0.8f;
        public float SeVolume { get; private set; } = 0.8f;

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);

            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            seSource = gameObject.AddComponent<AudioSource>();
            seSource.playOnAwake = false;

            Load();
        }

        public void SetBgmVolume(float v)
        {
            BgmVolume = Mathf.Clamp01(v);
            bgmSource.volume = BgmVolume;
            Save();
        }

        public void SetSeVolume(float v)
        {
            SeVolume = Mathf.Clamp01(v);
            seSource.volume = SeVolume;
            Save();
        }

        public void PlayBgm(AudioClip clip)
        {
            if (clip == null || (bgmSource.clip == clip && bgmSource.isPlaying)) return;
            bgmSource.clip = clip;
            bgmSource.Play();
        }

        public void PlaySe(AudioClip clip)
        {
            if (clip == null) return;
            seSource.PlayOneShot(clip);
        }

        void Load()
        {
            BgmVolume = PlayerPrefs.GetFloat(BgmVolumeKey, BgmVolume);
            SeVolume = PlayerPrefs.GetFloat(SeVolumeKey, SeVolume);
            bgmSource.volume = BgmVolume;
            seSource.volume = SeVolume;
        }

        void Save()
        {
            PlayerPrefs.SetFloat(BgmVolumeKey, BgmVolume);
            PlayerPrefs.SetFloat(SeVolumeKey, SeVolume);
        }
    }
}
