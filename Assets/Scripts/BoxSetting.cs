using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EazyEngine.Tools;

namespace Pok
{
    public class BoxSetting : MonoBehaviour
    {
        public UIToggle sound, music, vibra;
        public UILabel labelVersion;
        bool isInit = false;
        private void OnEnable()
        {
            if (isInit)
            {
                sound.value = (SoundManager.Instance.SfxOn);
                music.value = (SoundManager.Instance.MusicOn);
                vibra.value = (SoundManager.Instance.Vibration);
            }
            else
            {
                StartCoroutine(setup());
            }
            labelVersion.text = "Version: " + Application.version;


        }
        IEnumerator setup()
        {
            yield return new WaitForEndOfFrame();
            sound.Set(SoundManager.Instance.SfxOn);
            music.Set(SoundManager.Instance.MusicOn);
            vibra.Set(SoundManager.Instance.Vibration);
            isInit = true;
        }

        public void turnSound()
        {
            if (!isInit) return;
            SoundManager.Instance.SfxOn = sound.value;
        }
        public void turnMusic()
        {
            if (!isInit) return;
            SoundManager.Instance.MusicOn = music.value;
        }
        public void turnVibration()
        {
            if (!isInit) return;
            SoundManager.Instance.Vibration = vibra.value;
        }
    }
}
