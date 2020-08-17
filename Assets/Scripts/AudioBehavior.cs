using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using EazyEngine.Tools;




#if UNITY_EDITOR
using Sirenix.Utilities;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
#endif
namespace EazyEngine.Audio
{

    public struct AudioEvent
    {
        public string eventID;
        public GameObject target;
        public AudioEvent(string pEvent, GameObject pTarget = null)
        {
            target = pTarget;
            eventID = pEvent;
        }
    }
    public enum AudioActionType
    {
        PlaySoundGroup,
        Playlist,
        StopSoundGroup,
        StopPlaylist,
        ChangeVolumeSoundGroup,
        ChangeVolumePLaylist,
        StopAllPlaylist,
        PreloadMusicGroup
    }
    public enum EventAudiBehaviorType
    {
        onEnable,
        onDisable,
        onStart,
        onAwake,
        onCustomEvent
    }
#if UNITY_EDITOR
    public class MyStructDrawer : OdinValueDrawer<AudioGroupSelector>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            Rect rect = EditorGUILayout.GetControlRect();

            if (label != null)
            {
                rect = EditorGUI.PrefixLabel(rect, label);
            }

            AudioGroupSelector value = this.ValueEntry.SmartValue;
            bool oldValue = value.isMusic;
            value.isMusic = EditorGUI.Toggle(rect.AlignLeft(rect.width * 0.25f).AddX(rect.width * 0.5f + 20), value.isMusic);
            var pStringArray = !value.isMusic ? AudioDatabase.Instance.ArrayGroupNameSound : AudioDatabase.Instance.ArrayGroupNameMusics;
            int pIndex = pStringArray.findIndex(value);
            if (pIndex == -1)
            {
                pIndex = 0;
            }
            var oldIndex = pIndex;
            pIndex = EditorGUI.Popup(rect.AlignLeft(rect.width * 0.5f), pIndex, pStringArray);
            value.groupName = pStringArray[pIndex];
  
   
            if(value.isMusic != oldValue || oldIndex != pIndex)
            {

                this.ValueEntry.SmartValue = value;
            }
            // GUIHelper.PopLabelWidth();

        }
    }
#endif
    [System.Serializable]
    public struct AudioGroupSelector
    {
        public bool isMusic;
        public string groupName;

        public override bool Equals(object obj)
        {
            return groupName.Equals(obj);
        }

        public override int GetHashCode()
        {
            return groupName.GetHashCode();
        }

        public override string ToString()
        {
            return groupName.ToString();
        }
        public static bool operator !=(AudioGroupSelector obj1, string obj2)
        {
            return obj1.groupName.Equals(obj2);
        }
        public static bool operator ==(AudioGroupSelector obj1, string obj2)
        {
            return obj1.groupName.Equals(obj2);
        }
        public static implicit operator string(AudioGroupSelector obj2)
        {
            return obj2.groupName;
        }

        public static implicit operator AudioGroupSelector(string v)
        {
            return new AudioGroupSelector() { groupName = v };
        }
    }
    [System.Serializable]
    public class AudioAction
    {

        public string conditionState;
        public AudioActionType actionType;
        [Range(-1, 1)]
        [ShowIf("visibleVolume")]
        public float volumeChange;
        [ShowIf("visibleGroupname")]
        public AudioGroupSelector groupName;
        [ShowIf("actionType", AudioActionType.Playlist)]
        public bool singleton;
        [ShowIf("visibleGroupname", AudioActionType.StopSoundGroup)]
        public GameObject owner;
        [ShowIf("visibleMusic")]
        public float smoothTime = 0;
#if UNITY_EDITOR
        private bool visibleVolume()
        {
            return actionType == AudioActionType.ChangeVolumeSoundGroup || actionType == AudioActionType.ChangeVolumePLaylist;
        }
        private bool visibleGroupname()
        {
            return actionType == AudioActionType.PlaySoundGroup || actionType == AudioActionType.Playlist || actionType == AudioActionType.StopPlaylist || actionType == AudioActionType.StopSoundGroup || actionType == AudioActionType.PreloadMusicGroup;
        }
        private bool visibleMusic()
        {
            return actionType == AudioActionType.StopPlaylist || actionType == AudioActionType.Playlist || actionType == AudioActionType.StopAllPlaylist;
        }
#endif
    }
    [System.Serializable]
    public class AudioController
    {
        public EventAudiBehaviorType eventType;
        [ShowIf("eventType", EventAudiBehaviorType.onCustomEvent)]
        public string eventID;
        public AudioAction[] action;
    }
    public class AudioBehavior : MonoBehaviour, EzEventListener<AudioEvent>
    {
#if UNITY_EDITOR
        [FoldoutGroup("Create New Group")]
        [HideLabel]
        public AudioGroupInfo newGroup;
        [FoldoutGroup("Create New Group")]
        [Button("Add Group")]
        public void createNewGroup()
        {
            var pStringArray = AudioDatabase.Instance.ArrayGroupNameSound;
            if (!string.IsNullOrEmpty(newGroup.groupName) && pStringArray.findIndex<string>(newGroup.groupName) == -1)
            {
                System.Array.Resize(ref AudioDatabase.Instance.groups, AudioDatabase.Instance.groups.Length + 1);
                AudioDatabase.Instance.groups[AudioDatabase.Instance.groups.Length - 1] = ES3.Deserialize<AudioGroupInfo>( ES3.Serialize(newGroup));
                SerializedObject pSer = new SerializedObject(AudioDatabase.Instance);
                pSer.ApplyModifiedProperties();
                AssetDatabase.Refresh();
            }
        }
#endif
        public AudioController[] controller;
        protected bool registerListenEvent = false;
        // Start is called before the first frame update
        void Start()
        {
            foreach (var pControl in controller)
            {
                if (pControl.eventType == EventAudiBehaviorType.onStart)
                {
                    excuteActionAudio(pControl.action);
                }
            }
        }
        private void OnEnable()
        {
            if (registerListenEvent)
            {
                EzEventManager.AddListener(this);
            }
            foreach (var pControl in controller)
            {
                if (pControl.eventType == EventAudiBehaviorType.onEnable)
                {
                    excuteActionAudio(pControl.action);
                }
            }
        }
        private void OnDisable()
        {
            if (registerListenEvent)
            {
                EzEventManager.RemoveListener(this);
            }
            foreach (var pControl in controller)
            {
                if (pControl.eventType == EventAudiBehaviorType.onDisable)
                {
                    excuteActionAudio(pControl.action);
                }
            }
        }
        private void Awake()
        {
            foreach (var pControl in controller)
            {
                if (pControl.eventType == EventAudiBehaviorType.onAwake)
                {
                    excuteActionAudio(pControl.action);
                }
                if (pControl.eventType == EventAudiBehaviorType.onCustomEvent)
                {
                    registerListenEvent = true;

                }
            }
            if (registerListenEvent)
            {
                EzEventManager.AddListener(this);
            }
        }

        public void excuteActionAudio(params AudioAction[] pActions)
        {
            if (pActions == null) return;
            if (SoundManager.Instance.IsDestroyed()) return;
            foreach (var pAction in pActions)
            {
                if (!string.IsNullOrEmpty(pAction.conditionState) && !SoundManager.Instance.checkStateCondition(pAction.conditionState))
                {
                    continue;
                }
                if (!SoundManager.Instance.IsDestroyed())
                {
                    switch (pAction.actionType)
                    {
                        case AudioActionType.PlaySoundGroup:
                            SoundManager.Instance.PlaySound(pAction.groupName, pAction.owner == null ? gameObject : pAction.owner, pAction.conditionState);
                            break;
                        case AudioActionType.Playlist:
                            SoundManager.Instance.PlayMusic(pAction.groupName, pAction.singleton, pAction.owner == null ? SoundManager.Instance.gameObject : pAction.owner, pAction.conditionState, pAction.smoothTime);
                            break;
                        case AudioActionType.StopSoundGroup:
                            SoundManager.Instance.StopSoundGroupName(pAction.groupName, pAction.owner == null ? gameObject : pAction.owner);
                            break;
                        case AudioActionType.StopPlaylist:
                            SoundManager.Instance.StopMusicGroupName(pAction.groupName, pAction.owner == null ? SoundManager.Instance.gameObject : pAction.owner, pAction.smoothTime);
                            break;
                        case AudioActionType.StopAllPlaylist:
                            SoundManager.Instance.StopAllMusic(pAction.smoothTime);
                            break;
                        case AudioActionType.PreloadMusicGroup:
                            SoundManager.Instance.Preload(pAction.groupName);
                            break;
                    }
                }
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnEzEvent(AudioEvent eventType)
        {
            if (eventType.target == null || eventType.target == gameObject)
            {
                foreach (var pControler in controller)
                {
                    if (pControler.eventType == EventAudiBehaviorType.onCustomEvent && pControler.eventID == eventType.eventID)
                    {
                        excuteActionAudio(pControler.action);
                    }
                }
            }
        }
    }
}
