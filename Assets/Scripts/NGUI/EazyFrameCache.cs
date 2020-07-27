using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EazyFrameCache : MonoBehaviour
{
    [SerializeField]
    Sprite[] _frameCache = null;
    [SerializeField]
    AudioClip[] audios = null;
    UI2DSprite _sprite2D;
    UISprite _sprite;
    UITexture _texture;
    UIButton _button;
    Image image;
    Color color;
    SpriteRenderer _spriteRenderer;

    public string atlas = "";
    public string frameNamePrefix = "";
    public bool isMakerPixelPerFect = false;
    public bool disableAudio = false;
    public int intialFrame = -1;
    public int _nIndexNow = 0;
    public void setBehaviorIndex(int pIndex)
    {
        setFrameIndexUnPixelPerfect(pIndex);
    }
    public SpriteRenderer SpriteRenderer
    {
        get
        {
            return _spriteRenderer ? _spriteRenderer : _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        set
        {
            _spriteRenderer = value;
        }
    }

    public UI2DSprite Sprite2D
    {
        get
        {
            return _sprite2D ? _sprite2D : _sprite2D = GetComponent<UI2DSprite>();
        }

        set
        {
            _sprite2D = value;
        }
    }

    public UISprite UISprite
    {
        get
        {
            return _sprite ? _sprite : _sprite = GetComponent<UISprite>();
        }
    }

    public Color Color
    {
        get
        {
            return color;
        }

        set
        {
            if (SpriteRenderer)
            {
                SpriteRenderer.color = value;
            }
            else if (Sprite2D)
            {
                Sprite2D.color = value;
            }
            color = value;
        }
    }

    public UITexture Texture
    {
        get
        {
            return _texture ? _texture : _texture = GetComponent<UITexture>();
        }
    }

    public UIButton Button
    {
        get
        {
            return _button ? _button : _button = GetComponent<UIButton>();
        }
    }

    public Image Image
    {
        get
        {
            return image ? image : image = GetComponent<Image>();
        }
    }

    int currentIndex = 0;
    //public void playAudio(int pIndex)
    //{
    //    if (disableAudio) return;
    //    if(audios != null && pIndex < audios.Length)
    //    {
    //        SoundManager.Instance.PlaySound(audios[pIndex],Vector3.zero);
    //    }
    //}
    public void nextIndex()
    {
        currentIndex++;
        if(_frameCache != null && currentIndex >= _frameCache.Length)
        {
            currentIndex = 0;
        }
        setFrameIndexUnPixelPerfect(currentIndex);
    }

    // Use this for initialization
    public void setFrameIndex2(params int[] index)
    {
        if (frameNamePrefix != "")
        {
            string newPAth = string.Format(frameNamePrefix, index[0], index[1]);
            setFrame(newPAth);
        }
    }

    public void setFrame(string frame)
    {
        string newPAth = frame;
        if (SpriteRenderer)
        {
            if (atlas == "")
            {
                SpriteRenderer.sprite = Resources.Load(newPAth, typeof(Sprite)) as Sprite;
            }
            else
            {
                SpriteRenderer.sprite = ResourceExtension.LoadAtlas(atlas, frame);
            }
        }
        else if (UISprite)
        {
            UISprite.spriteName = newPAth;
            if (Button)
            {
                Button.normalSprite = UISprite.spriteName;
            }
        }
        else if (Sprite2D)
        {
            if (atlas == "")
            {
                Sprite2D.sprite2D = Resources.Load(newPAth, typeof(Sprite)) as Sprite;
            }
            else
            {
                Sprite2D.sprite2D = ResourceExtension.LoadAtlas(atlas, frame);
            }
            if (Button)
            {
                Button.normalSprite2D = Sprite2D.sprite2D;
            }
        }
        else if (Texture)
        {
            if (atlas == "")
            {
                Texture.mainTexture = Resources.Load(newPAth, typeof(Texture2D)) as Texture2D;
            }
        }
        if (isMakerPixelPerFect && GetComponent<UIWidget>())
        {
            GetComponent<UIWidget>().MakePixelPerfect();
        }
    }
    public void setFrameIndexUnPixelPerfect(int index)
    {
        setFrameIndex(index, false);
    }

    public void setFrameIndex(int index)
    {
        setFrameIndex(index,true);
    }
    public void setFrameIndex(int index,bool pixelPerfect = true)
    {
        _nIndexNow = index;
       // playAudio(index);
        if (_frameCache != null && _frameCache.Length > index)
        {
            if (SpriteRenderer)
            {
                SpriteRenderer.sprite = _frameCache[index];
            }
            else if (Sprite2D)
            {
                Sprite2D.sprite2D = _frameCache[index];
                if (Button)
                {
                    Button.normalSprite2D = Sprite2D.sprite2D;
                }
                if (pixelPerfect)
                {
                    Sprite2D.MakePixelPerfect();
                }
            }
            else if (Image)
            {
                Image.sprite = _frameCache[index];
            }
        }
        else if (frameNamePrefix != "")
        {
            string newPAth = string.Format(frameNamePrefix, index);
            setFrame(newPAth);
        }
    }
    public void OnChangeFrame()
    {
        if (_frameCache.Length==2)
        {
            _nIndexNow = _nIndexNow == 0 ? 1 : 0;
            setFrameIndex(_nIndexNow);
        }
       
    }
    void Start()
    {
        if(intialFrame >=0)
        {
            setFrameIndex(intialFrame);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
