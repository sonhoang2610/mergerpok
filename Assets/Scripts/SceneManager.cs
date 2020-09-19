using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EazyEngine.Tools;
using UnityEngine.Events;
using DG.Tweening;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Firebase.Analytics;
using System.Threading.Tasks;
using Firebase.Extensions;
using Facebook.Unity;

namespace Pok
{
    [System.Serializable]
    public class EventFloat : UnityEvent<float>
    {

    }

    public class SceneManager : PersistentSingleton<SceneManager>
    {
        public UIWidget fadeLayout;
        public UI2DSprite process;
        public UILabel loadingcontent;
        public UnityEvent onStartLoad;
        public EventFloat onLoadingScene;
        public UnityEvent onComplete;
        public UnityEvent onCompleteLoadAssets;
#if UNITY_EDITOR
        public CreatureItem[] items;
        [ContextMenu("abc")]
        public void abc()
        {
            foreach(var item in items)
            {
                item.ItemID = item.name;
                item.displayNameItem.normalString = item.name;
                UnityEditor.EditorUtility.SetDirty(item);
            }
        }
#endif
        
        AsyncOperation async;
        AsyncOperationHandle<IList<ScriptableObject>> asyncDatabase;
        AsyncOperationHandle<IList<UnityEngine.Object>> asyncResource;
        AsyncOperationHandle<IList<UnityEngine.AudioClip>> asyncAudio;
        bool isStart = false,loadingDatabase = false,loadResource = false;
        [System.NonSerialized]
        public string currentScene = "Home";
        [System.NonSerialized]
        public string previousScene = "Home";
        public void loadScene(string pScene)
        {
            if (pScene != currentScene)
            {
                previousScene = currentScene;
            }
            currentScene = pScene;
            Sequence pSeq = DOTween.Sequence();
            if (process && process.fillAmount >= 1)
            {
                process.fillAmount = 0;
            }
            pSeq.Append(DOTween.To(() => fadeLayout.alpha, a => fadeLayout.alpha = a, 1, 0.25f));
            pSeq.AppendCallback(delegate ()
            {
                onStartLoad.Invoke();
                isStart = true;
                if(currentScene == "MainScene")
                {
                    loadingDatabase = true;
                    asyncDatabase = LoadDataBase((a)=> {
                       
                    });
                }
                else
                {
                    async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(pScene);
                }
              
            });

            pSeq.Play();


        }

        public AsyncOperationHandle<IList<ScriptableObject>> LoadDataBase(System.Action<ScriptableObject> result)
        {
           return  Addressables.LoadAssetsAsync<ScriptableObject>("Database", result);
        }

        public AsyncOperationHandle<IList<UnityEngine.Object>> PreloadTexture(System.Action<UnityEngine.Object> result)
        {
            return Addressables.LoadAssetsAsync<UnityEngine.Object>("Texture", result);
        }
        public AsyncOperationHandle<IList<UnityEngine.AudioClip>> PreloadAudio(System.Action<UnityEngine.AudioClip> result)
        {
            return Addressables.LoadAssetsAsync<UnityEngine.AudioClip>("Audio", result);
        }
        protected Coroutine corountineFirstPool = null;

        public void complete()
        {
      
            fadeLayout.alpha = 1;
            Sequence pSeq = DOTween.Sequence();
            pSeq.AppendInterval(0.25f);
            pSeq.Append(DOTween.To(() => fadeLayout.alpha, a => fadeLayout.alpha = a, 0, 1));
            pSeq.Play();
            GameManager.removeDirtyState("Main");
        }
        public IEnumerator delayAction(float pDelay, System.Action pAction)
        {
            yield return new WaitForSeconds(pDelay);
            pAction();
        }
         public Firebase.FirebaseApp app = null;
        protected override void Awake()
        {

            base.Awake();
            Application.runInBackground = true;
#if !UNITY_EDITOR
            Debug.unityLogger.logEnabled = false;
#endif
            if (!FB.IsInitialized)
            {
                FB.Init();
            }
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                    // Create and hold a reference to your FirebaseApp,
                    // where app is a Firebase.FirebaseApp property of your application class.
                    app = Firebase.FirebaseApp.DefaultInstance;
                    System.Collections.Generic.Dictionary<string, object> defaults =
  new System.Collections.Generic.Dictionary<string, object>();
                    // These are the values that are used if we haven't fetched data from the
                    // server
                    // yet, or if we ask for values that the server doesn't have:
                    defaults.Add("time_bonus_evo", "300,1500");
                    defaults.Add("time_reward_ads", "300,1500");
                    defaults.Add("time_delay_ads", "200,230,240");
                    defaults.Add("time_switch_app", "300,1500");
                    defaults.Add("time_magic_case", "300");
                    Firebase.RemoteConfig.FirebaseRemoteConfig.SetDefaults(defaults);
                    // Set a flag here to indicate whether Firebase is ready to use by your app.
                }
                else
                {
                    UnityEngine.Debug.LogError(System.String.Format(
                      "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                    // Firebase Unity SDK is not safe to use here.
                }
            });
            Application.targetFrameRate = 60;
            Application.backgroundLoadingPriority = ThreadPriority.Normal;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
           
        }

   

        protected Coroutine corountineNotice;
        protected string lastAssetLoaded;

        public float PercentDatabase { get; set; }
        public float PercentScene { get; set; }
        public float PercentResource { get; set; }

        public List<AudioClip> loadingData = new List<AudioClip>();
        Tween tweenProcess;

        public float processing
        {
            get
            {
                return PercentDatabase + PercentResource + PercentScene;
            }
        }

        public int block { get; set; }

#if UNITY_EDITOR
        [ContextMenu("GenerateAFK")]
        public void GenerateAFK()
        {
            for(int i = 2; i < 7; ++i)
            {
                for(int j = 1; j< 25; ++j)
                {
                    var zone1Creature = GameDatabase.Instance.CreatureCollection.Find(x => x.RankChild == j);
                    var moneyNow = zone1Creature.goldAFKReward["Zone1"].clearDot().toBigInt();
                    var moneyPrevious = GameDatabase.Instance.CreatureCollection.Find(x => x.ItemID == "Pok1").goldAFKReward["Zone1"].clearDot().toDouble();
                    var dict = GameDatabase.Instance.CreatureCollection.Find(x => x.RankChild == j - 1).goldAFKReward;
                    if (dict.ContainsKey("Zone" + i.ToString()))
                    {
                        var moneyPrevioisCurrent = GameDatabase.Instance.CreatureCollection.Find(x => x.ItemID == "Pok1").goldAFKReward["Zone" + i.ToString()].toBigInt();
                       // var factor = moneyNow / moneyPrevious;
                        var final = moneyPrevioisCurrent * moneyNow;
                        var changeCreature = GameDatabase.Instance.CreatureCollection.FindAll(x => x.RankChild == j);
                        foreach(var creaturee in changeCreature)
                        {
                            creaturee.goldAFKReward["Zone" + i.ToString()] = final.toString();
                            UnityEditor.EditorUtility.SetDirty(creaturee);
                        }
                     
                        //if("Zone" + i.ToString() == "Zone2")
                        //{
                        Debug.Log("money zone" + i.ToString() + ": " + zone1Creature.ItemID);
                        Debug.Log("money zone" + i.ToString() + ": " + final.toString());
                        //}
                      
                    }
                    else
                    {
                        Debug.Log("Zone" + i.ToString() + "not exist");
                    }
                }
            }
        }
        [ContextMenu("dirty")]
        public void dirty()
        {
          for(int i = 0;i < GameDatabase.Instance.CreatureCollection.Count; ++i)
            {
                UnityEditor.EditorUtility.SetDirty(GameDatabase.Instance.CreatureCollection[i]);
            }
        }
        [ContextMenu("logzone1")]
        public void logzone1()
        {
            for (int i = 0; i < 25; ++i)
            {
               var creature = GameDatabase.Instance.CreatureCollection.Find(x => x.ItemID.Contains("Pok") && x.RankChild == i);
                //string total = "";
                //for(int j = 0; j < 17; j++)
                //{
                //    string money = PokUltis.calculateCreaturePrice(0,j, creature);
                //    total += money.toBigInt().toString().ToKMBTA() + ";";
                //}
                Debug.Log(creature.ItemID +": "+ creature.goldAFKReward["Zone1"].toBigInt().toString().ToKMBTA());
            }

        }
        [ContextMenu("error")]
        public void error()
        {
            for(int i = 0; i < 25; ++i)
            {
       
                for(int j =1; j < 7; ++j)
                {
                    var creatures = GameDatabase.Instance.CreatureCollection.FindAll(x => x.ItemID.Contains("Pok") && x.RankChild == i);
                    string start = "";
                    foreach(var creature in creatures)
                    {
                        if (string.IsNullOrEmpty(start))
                        {
                            start = creature.goldAFKReward["Zone" + j].toBigInt().ToString();
                        }
                        if(start !=     creature.goldAFKReward["Zone" + j].toBigInt().ToString())
                        {
                            Debug.Log("wrong" + creature.ItemID + i + j);
                        }
                        else
                        {
                            Debug.Log(creature.ItemID  + "validate");
                        }
                    }
                 
                }
            }
 
        }
#endif
        private void Update()
        {
            for(int i= loadingData.Count -1; i >=0; -- i) { 
                if(loadingData[i].loadState == AudioDataLoadState.Loaded)
                {
                    loadingData.RemoveAt(i);
                }
            }
            if (isStart)
            {
                if (loadResource)
                {
                    PercentResource = (asyncResource.PercentComplete + asyncAudio.PercentComplete) * 0.4f;
                }
                if(loadingDatabase)
                {
                    PercentDatabase = asyncDatabase.PercentComplete * 0.2f;
                    if (asyncDatabase.IsDone)
                    {
                        GameManager.addDirtyState("Main");
                        AddressableHolder.Instance.addLoadedItem<ScriptableObject>(asyncDatabase.Result);
                       var localAsync = Addressables.LoadAssetAsync<GameObject>("Prefab/ContainerDatabase");
                        loadingDatabase = false;
                        localAsync.Completed += (a) =>
                        {
                            if (a.Result)
                            {
                                for(int i =0;  i< a.Result.transform.childCount; ++i)
                                {
                                    Instantiate(a.Result.transform.GetChild(i));
                                }
                                
                            }
                          
                            async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(currentScene);
                        };
                     
                    }
                }
                if (async != null)
                {
                    if (!async.isDone)
                    {
                        PercentScene = async.progress * 0.4f;
                    }
                    else
                    {
                        PercentScene = 0.4f;
                        loadResource = true;
                        GameManager.addDirtyState("Main");
                        asyncResource = PreloadTexture((a) =>
                        {
                       
                        });
                        asyncAudio = PreloadAudio((a) =>
                        {
                            if(a.loadState != AudioDataLoadState.Loaded && a.loadState != AudioDataLoadState.Loading)
                            {
                                a.LoadAudioData();
                                loadingData.Add(a);
                            }
                        });
                        async = null;   
                    }
                }   
                if(PercentDatabase + PercentResource + PercentScene >= 1 && loadingData.Count == 0 && block <= 0)
                {
                    GameManager.removeDirtyState("Main");
                    loadResource = false;
                    isStart = false;
                    complete();
                }
                if (tweenProcess != null)
                {
                    tweenProcess.Kill();
                }
                tweenProcess = DOTween.To(() => process.fillAmount, x => process.fillAmount = x, PercentDatabase  + PercentScene+ PercentResource, 0.25f);
            }
        }
    }
}
