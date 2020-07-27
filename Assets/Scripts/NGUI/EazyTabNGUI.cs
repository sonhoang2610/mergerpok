using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[ExecuteInEditMode]
public class EazyTabNGUI : MonoBehaviour
{
    UIButton _button;
    UIWidget _widget;
    UISprite _sprite;
    UI2DSprite _sprite2D;

    bool _pressed = false;
    public GameObject target;
    public bool ignoreColor = false;
    public Color colorNormal = Color.white;
    public Color colorPressed = Color.white;
    public Sprite normalSprite2D;
    public Sprite pressedSprite2D;
    public bool includeChild = false;
    public string normalSprite;
    public string pressedSprite;
    public bool isSetParameter = false;

    [SerializeField]
    private UnityEvent eventOnPressed;
    [SerializeField]
    private UnityEvent eventOnUnPress;

    private int _index;
    public UIButton Button
    {
        get
        {
            return _button ? _button : _button = GetComponent<UIButton>();
        }
    }

    public UIWidget Widget
    {
        get
        {
            return _widget ? _widget : _widget = Target.GetComponent<UIWidget>();
        }
    }

    public bool Pressed
    {
        get
        {
            return this._pressed;
        }

        set
        {
            this._pressed = value;
            if (!ignoreColor)
            {
                Widget.color = value ? colorPressed : colorNormal;
                Button.defaultColor = value ? colorPressed : colorNormal;
            }
   
            if (includeChild)
            {
                var widgets = GetComponentsInChildren<UIWidget>();
                for (int i = 0; i < widgets.Length; ++i)
                {
                    widgets[i].color = value ? colorPressed : colorNormal;
                }
            }

            if (Sprite && pressedSprite != "" && normalSprite != "")
            {
                Sprite.spriteName = value ? pressedSprite : normalSprite;
                Button.normalSprite = value ? pressedSprite : normalSprite;
            }
            if (Sprite2D && pressedSprite2D != null && normalSprite2D != null)
            {
                Sprite2D.sprite2D = value ? pressedSprite2D : normalSprite2D;
                Button.normalSprite2D = value ? pressedSprite2D : normalSprite2D;
            }

            if (_pressed)
            {
                EventOnPressed.Invoke();
            }
            else
            {
                EventOnUnPress.Invoke();
            }
            doSomeThingOnPressed();
        }
    }



    public UISprite Sprite
    {
        get
        {
            return _sprite ? _sprite : _sprite = Target.GetComponent<UISprite>();
        }

    }

    public UI2DSprite Sprite2D
    {
        get
        {
            return _sprite2D ? _sprite2D : _sprite2D = Target. GetComponent<UI2DSprite>();
        }

    }

    public int Index
    {
        get
        {
            return _index;
        }

        set
        {
            _index = value;
        }
    }

    public UnityEvent EventOnPressed
    {
        get
        {
            return eventOnPressed;
        }

        set
        {
            eventOnPressed = value;
        }
    }

    public UnityEvent EventOnUnPress
    {
        get
        {
            return eventOnUnPress;
        }

        set
        {
            eventOnUnPress = value;
        }
    }

    public GameObject Target { get => target ? target : target =gameObject; set => target = value; }

    protected virtual void doSomeThingOnPressed()
    {

    }
    public virtual void setParrameter(EventDelegate pEvent)
    {
        if (pEvent.parameters.Length == 1)
        {
            pEvent.parameters[0].value = Index;
        }
    }
    // Use this for initialization
    void Start()
    {
    }

    //private void OnValidate()
    //{

    //}

    // Update is called once per frame
    void Update()
    {
    }
}
