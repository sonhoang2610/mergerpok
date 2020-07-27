using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class I2String 
{
    public string normalString;
    public bool isI2String;
    public string value;

    public void refresh()
    {
        if (isI2String)
        {
            value = I2.Loc.LocalizationManager.GetTranslation(normalString);

        }
    }
    public string Value
    {
        get
        {
            return isI2String ? value = I2.Loc.LocalizationManager.GetTranslation(normalString) : normalString;
        }
    }
}
