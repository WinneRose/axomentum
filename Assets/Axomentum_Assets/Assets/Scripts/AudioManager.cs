using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource musicSource;
    public AudioClip[] worldMusicTracks;

    private int currentWorldIndex = -1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
        }

        if (musicSource != null)
        {
            musicSource.Stop();
            musicSource.loop = true;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        int newWorldIndex = -1;

        switch (scene.name)
        {
            case "scn_testScene":
                newWorldIndex = 0;
                break;
            case "scn_fireBiome":
                newWorldIndex = 1;
                break;
            case "scn_groundBiome":
                newWorldIndex = 2;
                break;
            case "scn_waterBiome":
                newWorldIndex = 3;
                break;
            case "scn_airBiome":
                newWorldIndex = 4;
                break;
            default:
                if (musicSource != null) musicSource.Stop();
                Debug.LogWarning($"Sahne '{scene.name}' için müzik atanmamýþ.");
                return;
        }

        if (newWorldIndex != -1)
        {
            PlayWorldMusic(newWorldIndex);
        }
    }

    public void PlayWorldMusic(int worldIndex)
    {
        if (worldIndex >= 0 && worldIndex < worldMusicTracks.Length && worldIndex != currentWorldIndex)
        {
            AudioClip clipToPlay = worldMusicTracks[worldIndex];

            if (clipToPlay != null && musicSource != null)
            {
                Debug.Log($"Playing music for World {worldIndex} (Scene: {SceneManager.GetActiveScene().name}): {clipToPlay.name}");
                musicSource.Stop();
                musicSource.clip = clipToPlay;
                musicSource.Play();
                currentWorldIndex = worldIndex;
            }
            else
            {
                Debug.LogWarning($"World {worldIndex} için müzik klibi veya AudioSource bulunamadý!");
            }
        }
        else if (worldIndex < 0 || worldIndex >= worldMusicTracks.Length)
        {
            Debug.LogError($"Geçersiz dünya index'i: {worldIndex}");
        }
    }
}