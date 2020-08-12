using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EazyEngine.Tools;
using UnityEngine.Events;
using DG.Tweening;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

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
        bool isStart = false,loadingDatabase = false;
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
        protected Coroutine corountineFirstPool = null;

        public void complete()
        {
           // GameManager.addDirtyState("Main");
            var async = PreloadTexture((a)=> {
                //GameManager.removeDirtyState("Main");
            });
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

        protected override void Awake()
        {

            base.Awake();
            Application.targetFrameRate = 60;
            Application.backgroundLoadingPriority = ThreadPriority.Low;
#if UNITY_EDITOR
            Debug.unityLogger.logEnabled = true;
#else
            Debug.unityLogger.logEnabled = false;
#endif
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        protected Coroutine corountineNotice;
        protected string lastAssetLoaded;



        private void Update()
        {
            if (isStart)
            {
                if(loadingDatabase)
                {
                    DOTween.To(() => process.fillAmount, x => process.fillAmount = x, asyncDatabase.PercentComplete, 0.25f);
                    if (asyncDatabase.IsDone)
                    {
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
                        if (process)
                        {
                            DOTween.To(() => process.fillAmount, x => process.fillAmount = x, async.progress, 0.25f);
                        }
                    }
                    else
                    {
                      
                        Sequence pSeq = DOTween.Sequence();

                        pSeq.Append(DOTween.To(() => process.fillAmount, x => process.fillAmount = x, 1, 0.25f));
                        pSeq.AppendCallback(delegate
                        {

                            isStart = false;
                            complete();
                        });
                        async = null;   
                    }
                }
            }
        }
    }
}
