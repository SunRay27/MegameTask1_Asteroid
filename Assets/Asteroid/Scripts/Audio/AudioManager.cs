using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


[RequireComponent(typeof(AudioPool))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance => instance;
    private static AudioManager instance;
    private AudioPool pool;

    private List<AudioClip> fastClips = new List<AudioClip>();
    private List<AudioSource> fastAS = new List<AudioSource>();
    private List<float> _indexTimers = new List<float>();


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
            return;
        }
        pool = GetComponent<AudioPool>();
    }
    private void Update()
    {
        for (int i = 0; i < _indexTimers.Count; i++)
            _indexTimers[i] += Time.unscaledDeltaTime;
    }

    public void PlayFast2D(AudioClip clip, float volume = 1f)
    {
        int index = fastClips.IndexOf(clip);
        if (index != -1)
        {
            //we already played that clip, so find corresponding audiosource and play it
            if (!(_indexTimers[index] < 0.05f))
            {
                _indexTimers[index] = 0f;
                fastAS[index].spatialBlend = 0;
                fastAS[index].volume = volume;
                fastAS[index].Play();
            }
            return;
        }

        //create AS, assign clip, play and add clip and AS to lists
        AudioSource newAS = Instantiate(pool.Prefab, Vector3.zero, Quaternion.identity).GetComponent<AudioSource>();
        newAS.volume = volume;
        newAS.rolloffMode = AudioRolloffMode.Linear;
        newAS.spatialBlend = 0;
        newAS.playOnAwake = false;
        newAS.clip = clip;
        newAS.Play();

        DontDestroyOnLoad(newAS);
        fastClips.Add(clip);
        fastAS.Add(newAS);
        _indexTimers.Add(0f);
    }
}
