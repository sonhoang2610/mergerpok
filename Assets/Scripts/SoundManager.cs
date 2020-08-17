using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EazyEngine.Tools;
using EazyEngine.Audio;
using Sirenix.OdinInspector;
using DG.Tweening;
using Pok;

public enum TypeNotifySfx { TurnSound, TurnMusic, PlaySound, PlayMusic,TurnVibration }
public struct SfxNotifi
{
    public object value;
    public TypeNotifySfx type;
    public SfxNotifi(TypeNotifySfx pType, object pValue)
    {
        value = pValue;
        type = pType;
    }
}
public class SoundCallerInfo
{
    public GameObject owner;
    public List<SoundInfoFromCaller> dictAudios = new List<SoundInfoFromCaller>();
}


public class SoundInfoFromCaller
{
    public string groupName;
    public string conditionstate;
    public List<AudioSource> audios = new List<AudioSource>();
}
public class SoundManager : PersistentSingleton<SoundManager>
{

    [Header("Music")]
    public bool musicOn = true;
    /// the music volume
    [Range(0, 1)]
    public float MusicVolume = 0.3f;

    [Header("Sound Effects")]
    public bool sfxOn = true;
    /// the sound fx volume
    [Range(0, 1)]
    public float SfxVolume = 1f;

    public AudioSource _backgroundMusic;
    public static List<AudioSource> PoolInGameAudios = new List<AudioSource>();
    protected GameObject parrentSound;
  //  public List<AudioClip> ingnoreClips = new List<AudioClip>();
    public List<string> states = new List<string>();
    [ShowInInspector]
    public List<SoundCallerInfo> callers = new List<SoundCallerInfo>();
    [ShowInInspector]
    public List<SoundCallerInfo> musicCallers = new List<SoundCallerInfo>();
    public AudioSource[] preloadSource;

    public bool checkStateCondition(string pState)
    {
        return states.Contains(pState);
    }
    public virtual void PlayMusic(string pGroupName, bool singleton = false, GameObject pOwner = null, string conditionstate = "",float pSmoothTime = 0)
    {
        var pDatabase = AudioDatabase.Instance;
        foreach (var pGroup in pDatabase.musics)
        {
            if (pGroup.groupName == pGroupName)
            {
                PlayMusic(pGroup, singleton, pOwner, conditionstate,pSmoothTime);
            }
        }
    }
    public virtual AudioSource PlayBackgroundMusic(AudioClip Music, bool singleton, float pFactorVolume,float pSmoothTime = 0)
    {
        if (!MusicOn)
            return null;
        GameObject pExist = null;
        GameObject pExistActive = null;
        for (int i = 0; i < transform.childCount; ++i)
        {
            if (transform.GetChild(i).name == Music.name && !transform.GetChild(i).gameObject.activeSelf)
            {
                pExist = transform.GetChild(i).gameObject;
            }
            if (transform.GetChild(i).name == Music.name && transform.GetChild(i).gameObject.activeSelf)
            {
                pExistActive = transform.GetChild(i).gameObject;
            }
        }
        if (pExistActive && singleton)
        {
            return null;
        }
        // we create a temporary game object to host our audio source
        GameObject temporaryAudioHost = pExist ? pExist.gameObject : new GameObject(Music.name);
        temporaryAudioHost.transform.parent = transform;
        var audioSource = temporaryAudioHost.GetComponent<AudioSource>();
        if (!audioSource)
        {
            audioSource = temporaryAudioHost.AddComponent<AudioSource>();
        }
        // we add an audio source to that host
       // audioSource = temporaryAudioHost.AddComponent<AudioSource>() as AudioSource;
        // we set that audio source clip to the one in paramaters
        audioSource.clip = Music;
        audioSource.gameObject.SetActive(true);
        // we set the audio source volume to the one in parameters
        if (pSmoothTime == 0)
        {
            audioSource.volume = SfxVolume * pFactorVolume;
        }
        else
        {
            audioSource.volume = 0;
            DOTween.To(() => audioSource.volume, x => audioSource.volume = x, SfxVolume * pFactorVolume, pSmoothTime);
        }
        // we set our loop setting
        audioSource.loop = true;
        // we start playing the sound
        audioSource.Play();
        return audioSource;
    }
    public virtual void PlayMusic(MusicGroupInfo pInfo,bool singleton = false, GameObject pOwner = null, string conditionstate = "", float pSmoothTime = 0)
    {
        if (!string.IsNullOrEmpty(conditionstate) && !states.Contains(conditionstate)) return;
        float pFactor = pInfo.volume;
        int pTotalWeight = 0;
        for (int i = 0; i < pInfo.clips.Length; ++i)
        {
            var pClipInfo1 = pInfo.clips[i];
            pTotalWeight += pClipInfo1.weight;

        }
        int prandom = Random.Range(0, pTotalWeight);
        MusicElementInfos pClipInfo = null;
        if (preloadCache.ContainsKey(pInfo.groupName))
        {
            pClipInfo = pInfo.clips[preloadCache[pInfo.groupName]];
            preloadCache.Remove(pInfo.groupName);
        }
        pTotalWeight = 0;
        if (pClipInfo == null)
        {
            for (int i = 0; i < pInfo.clips.Length; ++i)
            {
                pTotalWeight += pInfo.clips[i].weight;
                if (prandom < pTotalWeight)
                {
                    pClipInfo = pInfo.clips[i];
                    break;
                }
            }
        }
        for (int j = 0; j < pClipInfo.elements.Length; ++j)
        {
            var pElementInfo = pClipInfo.elements[j];
            System.Action pAction = delegate
            {
                var sources = PlayBackgroundMusic(pElementInfo.Clip, singleton, pFactor * pElementInfo.volume, pSmoothTime);
                if (pOwner && sources != null)
                {
                    var pExistCaller = musicCallers.Find(x => x.owner == pOwner);
                    SoundInfoFromCaller pSoundInfo = null;
                    if (pExistCaller == null)
                    {
                        musicCallers.Add(pExistCaller = new SoundCallerInfo()
                        {
                            owner = pOwner
                        });


                    }
                    if ((pSoundInfo = pExistCaller.dictAudios.Find(x => (x.groupName == pInfo.groupName && x.conditionstate == pInfo.groupName))) == null)
                    {
                        pSoundInfo = new SoundInfoFromCaller()
                        {
                            audios = new List<AudioSource>(),
                            groupName = pInfo.groupName,
                            conditionstate = conditionstate
                        };
                        pExistCaller.dictAudios.Add(pSoundInfo);
                    }
                    pSoundInfo.audios.Add(sources);
                }
            };
            if (pElementInfo.Clip == null)
            {
                 pElementInfo.clipRef.loadAssetWrapped<AudioClip>(delegate (AudioClip a)
                {
                    pElementInfo.Clip = a;
                    if (pElementInfo.delay == 0)
                    {
                        pAction();
                    }
                    else
                    {
                        StartCoroutine(delayAction(pElementInfo.delay, pAction));
                    }
                });
              
            }
            else
            {
                if (pElementInfo.delay == 0)
                {
                    pAction();
                }
                else
                {
                    StartCoroutine(delayAction(pElementInfo.delay, pAction));
                }
            }



        }
    }
    public void StopMusicGroupName(string pGroupName, GameObject pOwner,float pTimeSmooth = 0)
    {
        if(pOwner== null)
        {
            pOwner = SoundManager.Instance.gameObject;
        }
        var pFind = musicCallers.Find(x => x.owner == pOwner);
        if (pFind != null)
        {
            SoundInfoFromCaller pInfoSound = null;
            if ((pInfoSound = pFind.dictAudios.Find(x => x.groupName == pGroupName)) != null)
            {
                foreach (var pAudio in pInfoSound.audios)
                {
                    if (pAudio.IsDestroyed()) continue;
                    if (pTimeSmooth == 0)
                    {
                        pAudio.gameObject.SetActive(false);
                    }
                    else
                    {
                        Sequence seq = DOTween.Sequence();
                        seq.Append( DOTween.To(() => pAudio.volume, x => pAudio.volume = x, 0, pTimeSmooth));
                        seq.AppendCallback(delegate {
                            pAudio.gameObject.SetActive(false);
                        });
                        seq.Play();
                    }
                }
                pFind.dictAudios.Remove(pInfoSound);
            }
        }
    }
    public void StopAllMusic( float pTimeSmooth = 0)
    {
    
           foreach( var pFind in musicCallers) {
            for(int i  = pFind.dictAudios.Count -1; i >= 0; --i) 
            {
                var pInfoSound = pFind.dictAudios[i];
                foreach (var pAudio in pInfoSound.audios)
                {
                    if (pAudio.IsDestroyed()) continue;
                    if (pTimeSmooth == 0)
                    {
                        pAudio.gameObject.SetActive(false);
                    }
                    else
                    {
                        Sequence seq = DOTween.Sequence();
                        seq.Append(DOTween.To(() => pAudio.volume, x => pAudio.volume = x, 0, pTimeSmooth));
                        seq.AppendCallback(delegate
                        {
                            pAudio.gameObject.SetActive(false);
                        });
                        seq.Play();
                    }
                }
                pFind.dictAudios.RemoveAt(i);
            }
        }
    }
    public void PlaySound(string pGroupName, GameObject pOwner = null, string conditionstate = "")
    {
        var pDatabase = AudioDatabase.Instance;
        foreach (var pGroup in pDatabase.groups)
        {
            if (pGroup.groupName == pGroupName)
            {
                playGroupSound(pGroup, pOwner, conditionstate);
            }
        }
    }

    Dictionary<string, int> preloadCache = new Dictionary<string, int>();
    public void Preload(string pGroupName)
    {
        var pDatabase = AudioDatabase.Instance;
        foreach (var pGroup in pDatabase.musics)
        {
            if (pGroup.groupName == pGroupName)
            {
                int pTotalWeight = 0;
                for (int i = 0; i < pGroup.clips.Length; ++i)
                {
                    var pClipInfo = pGroup.clips[i];
                    pTotalWeight += pClipInfo.weight;

                }
                int prandom = Random.Range(0, pTotalWeight);
                pTotalWeight = 0;
                for (int i = 0; i < pGroup.clips.Length; ++i)
                {
                    pTotalWeight += pGroup.clips[i].weight;
                    var pClipInfo = pGroup.clips[i];
                    if (prandom < pTotalWeight)
                    {
                        if (preloadCache.ContainsKey(pGroupName))
                        {
                            preloadCache[pGroupName] = prandom;
                        }
                        else
                        {
                            preloadCache.Add(pGroupName, prandom);
                        }
                     
                       foreach(var pElement in pClipInfo.elements)
                        {
                            if (!pElement.Clip)
                            {
                                 pElement.clipRef.loadAssetWrapped<AudioClip>(delegate(AudioClip a) {
                                    pElement.Clip = a;
                                    pElement.Clip.LoadAudioData();
                                });
                            }
                            else
                            if (pElement.Clip.loadState != AudioDataLoadState.Loaded)
                            {
                                pElement.Clip.LoadAudioData();
                            }
                        }
                    }
                }
            }
        }
    }

    public void PlaySound(string pGroupName, Vector3 location, GameObject pOwner = null, string conditionstate = "")
    {
        PlaySound(pGroupName, pOwner, conditionstate);
    }

    public void StopSoundGroupName(string pGroupName, GameObject pOwner)
    {
        var pFind = callers.Find(x => x.owner == pOwner);
        if (pFind != null)
        {
            SoundInfoFromCaller pInfoSound = null;
            if ((pInfoSound = pFind.dictAudios.Find(x => x.groupName == pGroupName)) != null)
            {
                foreach (var pAudio in pInfoSound.audios)
                {
                    if (pAudio.IsDestroyed()) continue;
                    Destroy(pAudio.gameObject);
                }
                pFind.dictAudios.Remove(pInfoSound);
            }
        }
    }

    public void StopSoundGroupState(string pState, GameObject pOwner)
    {
        if (string.IsNullOrEmpty(pState)) return;
        var pFind = callers.Find(x => x.owner == pOwner);
        if (pFind != null)
        {
            SoundInfoFromCaller pInfoSound = null;
            if ((pInfoSound = pFind.dictAudios.Find(x => x.conditionstate == pState)) != null)
            {
                foreach (var pAudio in pInfoSound.audios)
                {
                    if (pAudio.IsDestroyed()) continue;
                    Destroy(pAudio.gameObject);
                }
                pFind.dictAudios.Remove(pInfoSound);
            }
        }
    }
    public void playGroupSound(AudioGroupInfo pInfo, GameObject pOwner = null, string conditionstate = "")
    {
        if (!string.IsNullOrEmpty(conditionstate) && !states.Contains(conditionstate)) return;
        float pFactor = pInfo.volume;
        int pTotalWeight = 0;
        for (int i = 0; i < pInfo.clips.Length; ++i)
        {
            var pClipInfo1 = pInfo.clips[i];
            pTotalWeight += pClipInfo1.weight;

        }

        int prandom = Random.Range(0, pTotalWeight);
        AudioElementInfos pClipInfo = null;

        pTotalWeight = 0;
        for (int i = 0; i < pInfo.clips.Length; ++i)
        {
            pTotalWeight += pInfo.clips[i].weight;
            if (prandom < pTotalWeight)
            {
                pClipInfo = pInfo.clips[i];
                break;
            }
        }
        for (int j = 0; j < pClipInfo.elements.Length; ++j)
        {
            var pElementInfo = pClipInfo.elements[j];

            System.Action pAction = delegate
            {

                var sources = PlaySound(pElementInfo.Clip, Vector3.zero, pElementInfo.isLoop, pFactor * pElementInfo.volume);
                if (pOwner && sources && sources.loop)
                {
                    var pExistCaller = callers.Find(x => x.owner == pOwner);
                    SoundInfoFromCaller pSoundInfo = null;
                    if (pExistCaller == null)
                    {
                        callers.Add(pExistCaller = new SoundCallerInfo()
                        {
                            owner = pOwner
                        });


                    }
                    if ((pSoundInfo = pExistCaller.dictAudios.Find(x => (x.groupName == pInfo.groupName && x.conditionstate == pInfo.groupName))) == null)
                    {
                        pSoundInfo = new SoundInfoFromCaller()
                        {
                            audios = new List<AudioSource>(),
                            groupName = pInfo.groupName,
                            conditionstate = conditionstate
                        };
                        pExistCaller.dictAudios.Add(pSoundInfo);
                    }
                    pSoundInfo.audios.Add(sources);
                }
            };
            if (pElementInfo.Clip == null)
            {
               pElementInfo.clipRef.loadAssetWrapped<AudioClip>(delegate (AudioClip a) {
                   pElementInfo.Clip = a;
                   if (pElementInfo.delay == 0)
                   {
                       pAction();
                   }
                   else
                   {
                       StartCoroutine(delayAction(pElementInfo.delay, pAction));
                   }
               });
            }
            else
            {
                if (pElementInfo.delay == 0)
                {
                    pAction();
                }
                else
                {
                    StartCoroutine(delayAction(pElementInfo.delay, pAction));
                }
            }



        }
    }
    protected override void Awake()
    {
        base.Awake();
        sfxOn = PlayerPrefs.GetInt("Sound", 1) == 1 ? true : false;
        musicOn = PlayerPrefs.GetInt("Music", 1) == 1 ? true : false;
        vibaration = PlayerPrefs.GetInt("Vibration", 1) == 1 ? true : false;
    }
    public void vib()
    {
        if (PlayerPrefs.GetInt("Vibration",1) == 1)
        {
            Handheld.Vibrate();
        }
    }
    private void Start()
    {
        foreach(var pAudio in preloadSource)
        {
            pAudio.gameObject.SetActive(false);
        }
    }
    public bool SfxOn
    {
        get
        {
            return sfxOn;
        }

        set
        {
            bool isChange = sfxOn != value;
            sfxOn = value;
            PlayerPrefs.SetInt("Sound", sfxOn ? 1 : 0);
            if (isChange)
            {
                EzEventManager.TriggerEvent(new SfxNotifi(TypeNotifySfx.TurnSound, value));
            }
        }
    }
    bool vibaration = true;
    public bool Vibration
    {
        get
        {
            return vibaration;
        }

        set
        {
            bool isChange = vibaration != value;
            vibaration = value;
            PlayerPrefs.SetInt("Vibration", vibaration ? 1 : 0);
            if (isChange)
            {
                EzEventManager.TriggerEvent(new SfxNotifi(TypeNotifySfx.TurnVibration, value));
            }
        }
    }

    public bool MusicOn
    {
        get
        {
            return musicOn;
        }

        set
        {
            bool isChange = musicOn != value;
            musicOn = value;
            PlayerPrefs.SetInt("Music", musicOn ? 1 : 0);
            if (isChange)
            {
                EzEventManager.TriggerEvent(new SfxNotifi(TypeNotifySfx.TurnMusic, value));
            }
            if (!value)
            {
                var audios = GameObject.FindObjectsOfType<AudioSource>();
                foreach (var pAudio in audios)
                {
                    pAudio.Stop();
                }
            }
            else
            {
                var audios = GameObject.FindObjectsOfType<AudioSource>();
                foreach (var pAudio in audios)
                {
                    pAudio.Play();
                }
            }
        }
    }
    public void cleanAudio()
    {
        for (int i = callers.Count - 1; i >= 0; --i)
        {
            if (callers[i].owner.IsDestroyed())
            {
                var pAudioInfos = callers[i].dictAudios;
                foreach (var pAudioInfo in pAudioInfos)
                {
                    foreach (var pAudio in pAudioInfo.audios)
                    {
                        if (pAudio.IsDestroyed()) continue;
                        Destroy(pAudio);
                    }
                }
                callers.RemoveAt(i);
            }
        }
    }


    /// <summary>
    /// Plays a background music.
    /// Only one background music can be active at a time.
    /// </summary>
    /// <param name="Clip">Your audio clip.</param>
    public virtual void PlayBackgroundMusic(AudioSource Music)
    {
        if (_backgroundMusic != null && Music.clip == _backgroundMusic.clip) return;
        // if the music's been turned off, we do nothing and exit
        if (!sfxOn)
            return;
        if (_backgroundMusic != null && _backgroundMusic.clip.name == Music.clip.name)
        {
            return;
        }
        // if we already had a background music playing, we stop it
        if (_backgroundMusic != null)
        {
            _backgroundMusic.Stop();
        }
        // we set the background music clip
        _backgroundMusic = Music;
        // we set the music's volume
        _backgroundMusic.volume = MusicVolume;
        // we set the loop setting to true, the music will loop forever
        _backgroundMusic.loop = true;
        // we start playing the background music
        _backgroundMusic.Play();
    }

    private void OnApplicationPause(bool pause)
    {
        AudioListener.volume = pause ? 0 : (SfxOn ? 1 :0);
    }
    //Dictionary<AudioClip,GameObject> sounds = new List<AudioClip,GameObject>();
    /// <summary>
    /// Plays a sound
    /// </summary>
    /// <returns>An audiosource</returns>
    /// <param name="sfx">The sound clip you want to play.</param>
    /// <param name="location">The location of the sound.</param>
    /// <param name="loop">If set to true, the sound will loop.</param>
    public virtual AudioSource PlaySound(AudioClip sfx, Vector3 location, bool loop = false, float pFactorSpeed = 1)
    {
        if (!SfxOn || !sfx)
            return null;


        //if (ingnoreClips.Contains(sfx)) return null;
        if (!parrentSound)
        {
            parrentSound = new GameObject();
            parrentSound.name = "Sound";
        }
        AudioSource audioSource = PoolInGameAudios.FindAndClean<AudioSource>(x => (!x.gameObject.activeSelf && x.clip == sfx), x => x.IsDestroyed());
        if (!audioSource)
        {
            // we create a temporary game object to host our audio source
            GameObject temporaryAudioHost = new GameObject(sfx.name);
            temporaryAudioHost.transform.parent = parrentSound.transform;
            // we set the temp audio's position
            temporaryAudioHost.transform.position = location;
            // we add an audio source to that host
            audioSource = temporaryAudioHost.AddComponent<AudioSource>() as AudioSource;
            // we set that audio source clip to the one in paramaters
            audioSource.clip = sfx;
            if (!loop)
            {
                PoolInGameAudios.Add(audioSource);
            }
        }
        audioSource.gameObject.SetActive(true);
        // we set the audio source volume to the one in parameters
        audioSource.volume = SfxVolume * pFactorSpeed;
        // we set our loop setting
        audioSource.loop = loop;
        // we start playing the sound
        audioSource.Play();

        if (!loop)
        {
            //if (GameManager.Instance.inGame)
            //{
            //    ingnoreClips.Add(sfx);
            //}

            //StartCoroutine(delayAction(0.1f, delegate
            //{
            //    ingnoreClips.Remove(sfx);
            //}));
            StartCoroutine(delayAction(sfx.length, delegate
            {
                if (PoolInGameAudios.Contains(audioSource))
                {
                    if (!audioSource.IsDestroyed())
                    {
                        audioSource.gameObject.SetActive(false);
                    }
                    else
                    {
                        PoolInGameAudios.Remove(audioSource);
                    }

                }
                else if (!audioSource.IsDestroyed())
                {
                    Destroy(audioSource.gameObject);
                }
            }));
        }

        // we return the audiosource reference
        return audioSource;
    }
    private IEnumerator delayAction(float pDelay, System.Action pAction)
    {
        yield return new WaitForSeconds(pDelay);
        pAction();
    }
    /// <summary>
    /// Stops the looping sounds if there are any
    /// </summary>
    /// <param name="source">Source.</param>
    public virtual void StopLoopingSound(AudioSource source)
    {
        if (source != null)
        {
            Destroy(source.gameObject);
        }
    }

    private void LateUpdate()
    {
        //if (_backgroundMusic && MusicOn && !_backgroundMusic.isPlaying)
        //{
        //    AudioSource oldMusic = _backgroundMusic;
        //    _backgroundMusic = Instantiate(_backgroundMusic.gameObject, _backgroundMusic.transform.parent).GetComponent<AudioSource>();
        //    Destroy(oldMusic.gameObject);
        //}
    }
}
