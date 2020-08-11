using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
//using System.Reflection;
using System.Globalization;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;
using UnityEngine.Networking;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AtlasLoader
{
    public Dictionary<string, Sprite> spriteDic = new Dictionary<string, Sprite>();

    //Creates new Instance only, Manually call the loadSprite function later on 
    public AtlasLoader()
    {

    }

    //Creates new Instance and Loads the provided sprites
    public AtlasLoader(string spriteBaseName)
    {
        loadSprite(spriteBaseName);
    }

    //Loads the provided sprites
    public void loadSprite(string spriteBaseName)
    {
        Sprite[] allSprites = Resources.LoadAll<Sprite>(spriteBaseName);
        if (allSprites == null || allSprites.Length <= 0)
        {
            Debug.LogError("The Provided Base-Atlas Sprite `" + spriteBaseName + "` does not exist!");
            return;
        }

        for (int i = 0; i < allSprites.Length; i++)
        {
            spriteDic.Add(allSprites[i].name, allSprites[i]);
        }
    }

    //Get the provided atlas from the loaded sprites
    public Sprite getAtlas(string atlasName)
    {
        Sprite tempSprite;

        if (!spriteDic.TryGetValue(atlasName, out tempSprite))
        {
            Debug.LogError("The Provided atlas `" + atlasName + "` does not exist!");
            return null;
        }
        return tempSprite;
    }

    //Returns number of sprites in the Atlas
    public int atlasCount()
    {
        return spriteDic.Count;
    }
}
public static class AddressableSupport
{
    public static void loadAssetWrapped(this AssetReferenceSprite loader,System.Action<Sprite> result)
    {
        if (loader.Asset)
        {
            result.Invoke((Sprite)loader.Asset);
        }
        else
        {
            var pAsync = loader.LoadAssetAsync();
            pAsync.Completed += delegate (AsyncOperationHandle<Sprite> reusltAsync)
            {
                result.Invoke(reusltAsync.Result);
            };
        }
    }

    public static void loadAssetWrapped<T>(this AssetReference loader, System.Action<T> result) where T : UnityEngine.Object
    {
        if (loader.Asset)
        {
            result.Invoke((T)loader.Asset);
        }
        else
        {
            var pAsync = loader.LoadAssetAsync<T>();
            pAsync.Completed += delegate (AsyncOperationHandle<T> reusltAsync)
            {
                result.Invoke(reusltAsync.Result);
            };
        }
    }
}
public static class MonoBehaviorExtension
{
    static List<MonoBehaviour> listMono;
    public static void changeScene()
    {
        listMono = null;
    }
    public static List<T> FindObjectsOfTypeAll<T>()
    {
        List<T> results = new List<T>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            if (s.isLoaded)
            {
                var allGameObjects = s.GetRootGameObjects();
                for (int j = 0; j < allGameObjects.Length; j++)
                {
                    var go = allGameObjects[j];
                    results.AddRange(go.GetComponentsInChildren<T>(true));
                }
            }
        }
        return results;
    }
    public static List<MonoBehaviour> Monos
    {
        get
        {
            return listMono == null ? listMono = FindObjectsOfTypeAll<MonoBehaviour>() : listMono;
        }
    }
}

public static class NGUIExtension
{
    public static void MakePixelPerfectClaimIn(this UI2DSprite sprite,Vector2Int claim)
    {
        var cacheScale = sprite.transform.localScale;
        sprite.transform.localScale = new Vector3(1, 1, 1);
        sprite.MakePixelPerfect();
        float height = sprite.height;
        float width = sprite.width;
        float factor = height / claim.y;
        var newheight = (int)claim.y;
        var newwidth = (int)(width / factor);
        if(newwidth > claim.x)
        {
            factor = width / claim.x;
            newwidth = (int)claim.x;
            newheight = (int)(height / factor);
        }
        sprite.width = newwidth;
        sprite.height = newheight;
        sprite.transform.localScale = cacheScale;
    }
}
//public static class PlayMakerExtends
//{
//    public static PlayMakerFSM FindFsmOnGameObject(GameObject go, string fsmName)
//    {
//        foreach (var fsm in PlayMakerFSM.FsmList)
//        {
//            if (fsm.gameObject == go && fsm.FsmName == fsmName)
//            {
//                return fsm;
//            }
//        }
//        return null;
//    }
//}
#if UNITY_EDITOR
public static class ScriptableObjectUtility
{
    /// <summary>
    //	This makes it easy to create, name and place unique new ScriptableObject asset files.
    /// </summary>
    public static T CreateAsset<T>(string path, string name) where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T>();
        string assetPathAndName = (path + "/" + name + ".asset");
        AssetDatabase.CreateAsset(asset, assetPathAndName);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        return asset;
    }
}
#endif

//public static class NGUIAnchorExtends
//{

//    public static UIRect.AnchorPoint GetAnchorPoint(this UIRect rect,int index)
//    {
//        if(index == 0)
//        {
//            return rect.leftAnchor;
//        }else if(index == 1)
//        {
//            return rect.topAnchor;
//        }else if(index == 2)
//        {
//            return rect.rightAnchor;
//        }
//        else
//        {
//            return rect.bottomAnchor;
//        }
//    }
//}
public static class MathExtends
{
    public static T Clamp<T>(this T value, T max, T min)
    where T : System.IComparable<T>
    {
        T result = value;
        if (value.CompareTo(max) > 0)
            result = max;
        if (value.CompareTo(min) < 0)
            result = min;
        return result;
    }
    public static Vector3 paraboldDraw(Vector3 start, Vector3 end, float height, float t)
    {
        float parabolicT = t * 2 - 1;
        if (Mathf.Abs(start.y - end.y) < 0.1f)
        {
            //start and end are roughly level, pretend they are - simpler solution with less steps
            Vector3 travelDirection = end - start;
            Vector3 result = start + t * travelDirection;
            result.y += (-parabolicT * parabolicT + 1) * height;
            return result;
        }
        else
        {
            //start and end are not level, gets more complicated
            Vector3 travelDirection = end - start;
            Vector3 levelDirecteion = end - new Vector3(start.x, end.y, start.z);
            Vector3 right = Vector3.Cross(travelDirection, levelDirecteion);
            Vector3 up = Vector3.Cross(right, levelDirecteion);
            if (end.y > start.y) up = -up;
            Vector3 result = start + t * travelDirection;
            result += ((-parabolicT * parabolicT + 1) * height) * up.normalized;
            return result;
        }
    }

    public static float LerpWithOutClamp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }

    public static float InverseLerpWithOutClamp(float a, float b, float t)
    {
        if ((a - b) == 0) return 1;
        return (t - a) / Mathf.Abs(a - b);
    }

    public static float[] intersectLine(float start1, float start2, float end1, float end2)
    {
        List<float> arrayPointItrSect = new List<float>();
        if ((start1 - end1) * (start2 - end1) <= 0)
        {
            arrayPointItrSect.Add(end1);
        }
        if ((start1 - end2) * (start2 - end2) <= 0)
        {
            arrayPointItrSect.Add(end2);
        }
        return arrayPointItrSect.ToArray();
    }
}
public static class SpriteExtends
{
    public static Sprite loadSprite(string spite)
    {
        return null;
    }
}
public static class StringUtils
{
    public static string toString(this System.Numerics.BigInteger v)
    {
        return v.ToString();
    }
    public static double toDouble(this string v)
    {
        return double.Parse(v);
    }
    public static float toFloat(this string v)
    {
        return float.Parse(v);
    }
    public static int toInt(this string v)
    {
        return int.Parse(v);
    }
    public static System.Numerics.BigInteger toBigInt(this string v)
    {
        return System.Numerics.BigInteger.Parse(v.clearDot());
    }
    public static string convertMoneyAndAddDot(long i)
    {

        string money = i.ToString();
        int pCounterDot = (money.Length - 1) / 3;
        if (i < 0)
        {
            pCounterDot = (money.Length - 2) / 3;
        }

        int pIndex = money.Length;
        string newStrMoney = "";
        while (pCounterDot > 0)
        {
            newStrMoney = "." + money.Substring(pIndex - 3, 3) + newStrMoney;
            pCounterDot--;
            pIndex -= 3;
        }
        newStrMoney = money.Substring(0, pIndex) + newStrMoney;
        return newStrMoney;
    }

    //support for 0 -> 999.999.999.999
    public static string convertMoneyAndAddText(long i)
    {
        string newString = "";
        long intOrigin = i;
        long intExcess = 0;
        int level = 0;
        if (intOrigin < 10000)
        {
            return convertMoneyAndAddDot(intOrigin);
        }
        while (intOrigin >= 1000)
        {
            intExcess = intOrigin % 1000;
            intOrigin /= 1000;
            level++;
        }
        newString += intOrigin.ToString();
        if (intExcess >= 10)
        {
            string stringExecess = ((long)(intExcess / 10)).ToString();
            newString += "." + (stringExecess.Length == 1 ? "0" + stringExecess : stringExecess);
        }
        switch (level)
        {
            case 1:
                newString += "K";
                break;
            case 2:
                newString += "M";
                break;
            case 3:
                newString += "B";
                break;
            default:
                return i.ToString();
        }
        return newString;
    }
    static CultureInfo elGR;
    public static string addDotMoney(long i)
    {
        string money = i.ToString();
        int pCounterDot = (money.Length - 1) / 3;
        if (i < 0)
        {
            pCounterDot = (money.Length - 2) / 3;
        }

        int pIndex = money.Length;
        string newStrMoney = "";
        while (pCounterDot > 0)
        {
            newStrMoney = "." + money.Substring(pIndex - 3, 3) + newStrMoney;
            pCounterDot--;
            pIndex -= 3;
        }
        newStrMoney = money.Substring(0, pIndex) + newStrMoney;
        return newStrMoney;
    }
 
    public static int convertToInt(this string v)
    {
        return int.Parse(v);
    }

    public static long convertToLong(this string v)
    {
        return long.Parse(v);
    }
    public static int convertStringDotToInt(this string v)
    {
        return v.clearDot().convertToInt();
    }
    public static string clearWhiteSpace(this string s)
    {
        for (int i = s.Length - 1; i >= 0; i--)
        {
            if (s.Substring(i, 1) == " ")
            {
                s = s.Remove(i, 1);
            }
        }
        return s;
    }
    public static string insertWhiteSpaceEvery(this string s, int pCount)
    {
        int index = 0;
        int count = 0;
        while (index < s.Length)
        {
            if (count == pCount)
            {
                s = s.Insert(index, " ");
                //index++;
                count = 0;
            }
            else
            {
                count++;
            }
            index++;
        }
        return s;
    }
    public static bool isHaveWhiteSpace(this string s)
    {
        for (int i = 0; i < s.Length; i++)
        {
            if (s.Substring(i, 1) == " ")
            {
                return true;
            }
        }
        return false;
    }

    public static bool startWithNumberic(this string s)
    {
        int n;
        return int.TryParse(s.Substring(0, 1), out n);
    }

    public static string FormartString(string format, params object[] objects)
    {
        return string.Format(format, objects);
    }
    public static string FormartString1(string format, int index)
    {
        return string.Format(format, index);
    }
}
//public static class ActionCustomExtends
//{
//    public static EazyAction covertAction(this EazyActionInfo pAction)
//    {
//        EazyAction _mainAction = null;
//        if (pAction.SelectedAction.name == EazyActionContructor.Sequences.name)
//        {
//            _mainAction = EazyCustomAction.Sequences.create();
//            List<EazyAction> pListAction = new List<EazyAction>();
//            for (int i = 0; i < pAction.ListActionInfo.Count; i++)
//            {
//                pListAction.Add(pAction.ListActionInfo[i].covertAction());
//            }
//            ((EazyCustomAction.Sequences)_mainAction)._listAction = pListAction.ToArray();
//        }
//        else
//        {
//            _mainAction = (EazyAction)Activator.CreateInstance(pAction.SelectedAction.MainType);
//            if (_mainAction != null)
//            {
//                _mainAction.copyFromInfo(pAction);
//            }
//        }
//        // Activator.CreateInstance
//        //else if (pAction.SelectedAction >= ActionOption.MoveTo && pAction.SelectedAction <= ActionOption.ScaleBy)
//        //{
//        //    if (pAction.SelectedAction == ActionOption.MoveTo)
//        //    {
//        //        _mainAction = EazyMove.to(pAction.InfoStep.Vector3, pAction.Unit, pAction.UnitType == 0 ? true : false);
//        //    }
//        //    else if (pAction.SelectedAction == ActionOption.MoveBy)
//        //    {
//        //        _mainAction = EazyMove.by(pAction.InfoStep.Vector3, pAction.Unit, pAction.UnitType == 0 ? true : false);
//        //    }
//        //    else if (pAction.SelectedAction == ActionOption.RotateBy)
//        //    {
//        //        _mainAction = EazyRotate.by(pAction.InfoStep.Vector3, pAction.Unit, pAction.UnitType == 0 ? true : false);
//        //    }
//        //    else if (pAction.SelectedAction == ActionOption.RotateTo)
//        //    {
//        //        _mainAction = EazyRotate.to(pAction.InfoStep.Vector3, pAction.Unit, pAction.UnitType == 0 ? true : false);
//        //    }
//        //    else if (pAction.SelectedAction == ActionOption.ScaleBy)
//        //    {
//        //        _mainAction = EazyScale.by(pAction.InfoStep.Vector3, pAction.Unit, pAction.UnitType == 0 ? true : false);
//        //    }
//        //    else if (pAction.SelectedAction == ActionOption.ScaleTo)
//        //    {
//        //        _mainAction = EazyScale.to(pAction.InfoStep.Vector3, pAction.Unit, pAction.UnitType == 0 ? true : false);
//        //    }
//        //    if (pAction.TypeInfoIn != 0)
//        //    {
//        //        ((EazyVector3Action)_mainAction).setFrom(pAction.InfoFrom.Vector3);
//        //    }
//        //}
//        //else if (pAction.SelectedAction == ActionOption.FadeBy)
//        //{
//        //    _mainAction = EazyFade.by(pAction.InfoStep.Color.a, pAction.Unit, pAction.UnitType == 0 ? true : false);
//        //}
//        //else if (pAction.SelectedAction == ActionOption.FadeTo)
//        //{
//        //    _mainAction = EazyFade.to(pAction.InfoStep.Color.a, pAction.Unit, pAction.UnitType == 0 ? true : false);
//        //}
//        //else if (pAction.SelectedAction == ActionOption.TintTo)
//        //{
//        //    _mainAction = EazyColor4F.to(pAction.InfoStep.Color, pAction.Unit, pAction.UnitType == 0 ? true : false);
//        //}
//        //if (pAction.EaseTypePopUp == 0)
//        //{
//        //    _mainAction.setEase(pAction.EaseType);
//        //}
//        //else
//        //{
//        //    _mainAction.Curve = pAction.CurveEase;
//        //}
//        //if (pAction.LoopType == 0)
//        //{
//        //    if (pAction.LoopTime > 1)
//        //    {
//        //        _mainAction.loop(pAction.LoopTime);
//        //        //_mainAction = EazyRepeat.create(_mainAction, pAction.LoopTime);
//        //    }
//        //}
//        if (_mainAction != null)
//        {
//            _mainAction.setName(pAction.Name);
//        }
//        return _mainAction;
//    }


//    public static bool checkExist<T>(T[] ts, T t) where T : class
//    {
//        for (int i = 0; i < ts.Length; ++i)
//        {
//            if (ts[i] == t)
//            {
//                return true;
//            }
//        }
//        return false;
//    }


//}
public static class objExtend
{
    public static string  toTimeHour(this int v)
    {
       var timespane = TimeSpan.FromSeconds(v);
        float hour = timespane.Hours;
        float minute = (float)timespane.Minutes / 60.0f;
        return (hour + minute).ToString(".0#") + "H";
    }
    public static string clearDot(this string v)
    {
        var str = v;
        int charEnd = 0;
        bool charShortcut = false;
        if (str.Contains('.'))
        {
            for(int i =str.Length-1; i >= 0; --i)
            {
             
                if (str[i] == '.')
                {
                    break;
                }
                charEnd++;
            }
        }
        if (str.Contains("K"))
        {
            charShortcut = true;
            charEnd--;
            str = str.Replace("K", "000");
        }
        if (str.Contains("M"))
        {
            charShortcut = true;
            charEnd--;
            str = str.Replace("M", "000000");
        }
        if (str.Contains("B"))
        {
            charShortcut = true;
            charEnd--;
            str = str.Replace("B", "000000000");
        }
        if (str.Contains("T"))
        {
            charShortcut = true;
            charEnd--;
            str = str.Replace("T", "000000000000");
        }
        if (str.Contains("a"))
        {
            charShortcut = true;
            charEnd -=2;
            str = str.Replace("a", "000000000000");
            string[] alphab = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o" };
            var index = System.Array.FindIndex(alphab, x => x == str.Substring(str.Length - 1, 1));
            str = str.Substring(0, str.Length - 1);
            for (int i = 0; i < index + 1; ++i)
            {
                str += "000";
            }
        }
        str = (charShortcut && charEnd>0) ? str.Substring(0, str.Length - charEnd) : str;
        return str.Replace(".",string.Empty);
    }
    public static string ToKMBTA(this string numstr)
    {
        numstr = numstr.clearDot();
        var num = System.Numerics.BigInteger.Parse(numstr);
        if (numstr.Length > 15)
        {
            string[] alphab = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o" };
            string format = "#,##0";
            for(int i = 0; i <(numstr.Length/3)  -(numstr.Length % 3 == 0 ? 1 : 0); ++i)
            {
                format += ",";
            }
       
            format += "a" + alphab[((numstr.Length/3) - (numstr.Length % 3 ==0 ? 1 : 0)) - 5];
            return num.ToString(format, CultureInfo.InvariantCulture);
        }else
        if (numstr.Length > 12)
        {
            return num.ToString("0,,,,.###T", CultureInfo.InvariantCulture);
        }else
        if (numstr.Length > 9)
        {
            return num.ToString("0,,,.##B", CultureInfo.InvariantCulture);
        }
        else
        if (numstr.Length > 6)
        {
            return num.ToString("0,,.##M", CultureInfo.InvariantCulture);
        }
        else
        if (numstr.Length > 3)
        {
            return num.ToString("0,.#K", CultureInfo.InvariantCulture);
        }
        else
        {
            return num.ToString(CultureInfo.InvariantCulture);
        }
    }
    public static void CopyAllTo<T>(this T source, T target)
    {
        var type = typeof(T);
        foreach (var sourceProperty in type.GetProperties())
        {
            var targetProperty = type.GetProperty(sourceProperty.Name);
            targetProperty.SetValue(target, sourceProperty.GetValue(source, null), null);
        }
        foreach (var sourceField in type.GetFields())
        {
            var targetField = type.GetField(sourceField.Name);
            targetField.SetValue(target, sourceField.GetValue(source));
        }
    }
}
public static class ArrayExtension
{
    public delegate void AddListCallBack<T>(T pObject);
    public static void Shuffle<T>(this IList<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    public static int ClosestTo(this IEnumerable<int> collection, int target)
    {
        // NB Method will return int.MaxValue for a sequence containing no elements.
        // Apply any defensive coding here as necessary.
        var closest = int.MaxValue;
        var minDifference = int.MaxValue;
        foreach (var element in collection)
        {
            var difference = Math.Abs((long)element - target);
            if (minDifference > difference)
            {
                minDifference = (int)difference;
                closest = element;
            }
        }

        return closest;
    }
    public static void addFromList<T>(this IList<T> list, T[] another, AddListCallBack<T> callback = null,bool checkExist = false)
    {
        foreach (T element in another)
        {
            if (checkExist && list.Contains(element)) continue;
            list.Add(element);
            if (callback != null)
            {
                callback(element);
            }
        }
    }

    public static bool checkExist<T>(this T[] vs, T e) where T : class
    {
        for (int i = 0; i < vs.Length; ++i)
        {
            if (vs[i] == e)
            {
                return true;
            }
        }
        return false;
    }

    public static int findIndex<T>(this T[] vs, T e) where T : class
    {
        for (int i = 0; i < vs.Length; ++i)
        {
            if (vs[i] == e)
            {
                return i;
            }
        }
        return -1;
    }

    public static int findIndex(this string[] vs, string v)
    {
        for (int i = 0; i < vs.Length; ++i)
        {
            if (v == vs[i])
            {
                return i;
            }
        }
        return 0;
    }
}
public static class GameObjectExtensions
{
    public static bool IsDestroyed<T>(this T mono) where T : UnityEngine.Object
    {
        // UnityEngine overloads the == opeator for the GameObject type
        // and returns null when the object has been destroyed, but 
        // actually the object is still there but has not been cleaned up yet
        // if we test both we can determine if the object has been destroyed.
        return mono == null || ReferenceEquals(mono, null);
    }
    public static Type[] getAllComponentStringType(this GameObject gObj)
    {
        if (gObj)
        {
            var mObjs = gObj.GetComponents<Component>();
            Type[] mTypes = new Type[mObjs.Length];
            for (int i = 0; i < mObjs.Length; ++i)
            {
                mTypes[i] = mObjs[i].GetType();
            }
            return mTypes;
        }
        return null;
    }
    public static string[] convertStrings(this Type[] v)
    {
        string[] strs = new string[v.Length];
        for (int i = 0; i < v.Length; ++i)
        {
            strs[i] = v[i].ToString();
        }
        return strs;
    }
    /// <summary>
    /// Returns all monobehaviours (casted to T)
    /// </summary>
    /// <typeparam name="T">interface type</typeparam>
    /// <param name="gObj"></param>
    /// <returns></returns>
    public static T[] GetInterfaces<T>(this GameObject gObj)
    {
        if (!typeof(T).IsInterface) throw new System.SystemException("Specified type is not an interface!");
        var mObjs = gObj.GetComponents<MonoBehaviour>();

        return (from a in mObjs where a.GetType().GetInterfaces().Any(k => k == typeof(T)) select (T)(object)a).ToArray();
    }

    /// <summary>
    /// Returns the first monobehaviour that is of the interface type (casted to T)
    /// </summary>
    /// <typeparam name="T">Interface type</typeparam>
    /// <param name="gObj"></param>
    /// <returns></returns>
    public static T GetInterface<T>(this GameObject gObj)
    {
        if (!typeof(T).IsInterface) throw new SystemException("Specified type is not an interface!");
        return gObj.GetInterfaces<T>().FirstOrDefault();
    }

    /// <summary>
    /// Returns the first instance of the monobehaviour that is of the interface type T (casted to T)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="gObj"></param>
    /// <returns></returns>
    public static T GetInterfaceInChildren<T>(this GameObject gObj)
    {
        if (!typeof(T).IsInterface) throw new SystemException("Specified type is not an interface!");
        return gObj.GetInterfacesInChildren<T>().FirstOrDefault();
    }

    /// <summary>
    /// Gets all monobehaviours in children that implement the interface of type T (casted to T)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="gObj"></param>
    /// <returns></returns>
    public static T[] GetInterfacesInChildren<T>(this GameObject gObj)
    {
        if (!typeof(T).IsInterface) throw new SystemException("Specified type is not an interface!");

        var mObjs = gObj.GetComponentsInChildren<MonoBehaviour>();

        return (from a in mObjs where a.GetType().GetInterfaces().Any(k => k == typeof(T)) select (T)(object)a).ToArray();
    }

    public static GameObject findInActiveObject(this List<GameObject> pList)
    {
        for (int i = 0; i < pList.Count; ++i)
        {
            if (!pList[i].activeSelf)
            {
                return pList[i];
            }
        }
        return null;
    }
    public static T FindAndClean<T>(this List<T> v, Predicate<T> match, Predicate<T> matchClean) where T : UnityEngine.Object
    {

        for (int index = v.Count - 1; index >= 0; --index)
        {
            if (matchClean(v[index]))
            {
                v.RemoveAt(index);
                continue;
            }
            if (match(v[index]))
                return v[index];
        }
        return null;
    }
    public static void SetLayerRecursively(this GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            if (null == child)
            {
                continue;
            }
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}

[Serializable]
public struct Pos
{
    public int x;
    public int y;

    public override bool Equals(object other)
    {
        return this.x == ((Pos)other).x && this.y == ((Pos)other).y;
    }
    public override int GetHashCode()
    {
        return 0;
    }
    public Pos(int pX, int pY)
    {
        x = pX;
        y = pY;
    }
    public Vector2 vec2
    {
        get
        {
            return new Vector2(x, y);
        }
    }
    public static Pos Zero
    {
        get
        {
            Pos pZero;
            pZero.x = 0;
            pZero.y = 0;
            return pZero;
        }
    }


    public static Pos None
    {
        get
        {
            Pos pNone;
            pNone.x = -1;
            pNone.y = -1;
            return pNone;
        }
    }
    public static bool operator ==(Pos lhs, Pos rhs)
    {
        return lhs.Equals(rhs);
    }
    public static bool operator !=(Pos lhs, Pos rhs)
    {
        return !lhs.Equals(rhs);
    }
    public static Pos operator +(Pos lhs, Pos rhs)
    {
        return new Pos(lhs.x + rhs.x, lhs.y + rhs.y);
    }
    public static Pos operator *(Pos lhs, int pIndex)
    {
        return new Pos(lhs.x * pIndex, lhs.y * pIndex);
    }
    public static Pos operator *(int pIndex, Pos lhs)
    {
        return new Pos(lhs.x * pIndex, lhs.y * pIndex);
    }
    public static Pos operator -(Pos lhs, Pos rhs)
    {
        return new Pos(lhs.x - rhs.x, lhs.y - rhs.y);
    }
}
public class CacheTexture2D
{
    private List<Texture2D> _cacheTexture = new List<Texture2D>();
    public void addTexture(Texture2D pTexture)
    {
        _cacheTexture.Add(pTexture);
    }

    public Texture2D getTextureByName(string pName)
    {
        for (int i = 0; i < _cacheTexture.Count; i++)
        {
            if (_cacheTexture[i].name == pName)
            {
                Texture2D pTexture = _cacheTexture[i];
                return pTexture;
            }
        }
        return null;
    }
}
public static class TranformExtension
{
    public static void RotationLocalDirect(this Transform v, Vector2 direction)
    {
        Quaternion rotation = Quaternion.LookRotation(direction);
        v.localRotation = rotation;
    }
    public static void RotationDirect(this Transform v, Vector2 direction)
    {
        Quaternion rotation = Quaternion.LookRotation(direction);
        v.rotation = rotation;
    }

    public static void RotationDirect2D(this Transform v, Vector2 direction)
    {
        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle -= 90;
        v.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
    public static void TranposeAroundLocal2D(this Transform v, Vector2 pivot, float angle)
    {
        Vector2 pPos = v.transform.localPosition;
        pPos = pPos.Rotate2DAround(pivot, Vector2.Distance(pivot, pPos), angle);
        v.transform.localPosition = v.transform.localPosition.insert(pPos);
    }
    public static void setLocalPosition2D(this Transform v, Vector2 pos)
    {

        v.transform.localPosition = v.transform.localPosition.insert(pos);
    }
    public static void setLocaPosX(this Transform v, float pX)
    {
        Vector3 pPos = v.transform.localPosition;
        pPos.x = pX;
        v.transform.localPosition = pPos;
    }
    public static void setLocaPosY(this Transform v, float pY)
    {
        Vector3 pPos = v.transform.localPosition;
        pPos.y = pY;
        v.transform.localPosition = pPos;
    }
    public static void setLocaPosZ(this Transform v, float pZ)
    {
        Vector3 pPos = v.transform.localPosition;
        pPos.z = pZ;
        v.transform.localPosition = pPos;
    }
}
public static class VectorExtension
{


    public static Vector3 insert(this Vector3 v, Vector2 v1)
    {
        v.x = v1.x;
        v.y = v1.y;
        return v;
    }

    public static Vector2 Rotate(this Vector2 v, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);
        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }
    //public static float getRotate(this Vector2 v)
    //{
    //    var angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
    //   return Quaternion.AngleAxis(angle, Vector3.forward);
    //}

    public static Pos ConvetPos(this Vector2 v)
    {
        return new Pos((int)v.x, (int)v.y);
    }
    public static float Angle(this Vector2 p_vector2)
    {
        if (p_vector2.x < 0)
        {
            return 360 - (Mathf.Atan2(p_vector2.x, p_vector2.y) * Mathf.Rad2Deg * -1);
        }
        else
        {
            return Mathf.Atan2(p_vector2.x, p_vector2.y) * Mathf.Rad2Deg;
        }
    }
    public static Vector2 Rotate2DAround(this Vector2 v, Vector2 pivot, float radius, float angle)
    {
        float constX = (float)Math.Sin(angle * Math.PI / 180);
        float constY = (float)Math.Cos(angle * Math.PI / 180);
        Vector2 newPoint = new Vector2(constX, constY);
        newPoint = pivot + newPoint * radius;
        v.x = newPoint.x;
        v.y = newPoint.y;
        return v;
    }
    public static Vector3 changeDestinationByDirection(this Vector3 v, Vector3 to, float distance)
    {
        float ratio = distance / Vector3.Distance(v, to);
        Vector3 next = (to - v) * ratio;
        return next + v;
    }
}

public static class TimeExtension
{
    public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        return dtDateTime;
    }
    public static IEnumerator GetNetTime(System.Action<string, string> pResult)
    {

        string url = "http://34.94.28.183:1235/gettime/gettime";
        UnityWebRequest www = new UnityWebRequest();
        Debug.Log(url);
        www.url = url;
        www.method = UnityWebRequest.kHttpVerbPOST;
        www.downloadHandler = new DownloadHandlerBuffer();
        www.timeout = 5;
        www.SetRequestHeader("Content-Type", "applicatopn/json");
        yield return www.SendWebRequest();

        if (www.error == null)
        {

            string myContent = www.downloadHandler.text;
            pResult.Invoke(myContent, "");
        }
        else
        {
            pResult.Invoke("", www.error);
        }
    }
}
public static class ColorExtension
{
    public static Color setAlpha(this Color color, float a)
    {
        color.a = a;
        return color;
    }
    public static Color converFloatToColorA(this float v)
    {
        return new Color(0, 0, 0, v);
    }

    public static Vector3 convertColortoVector3(this Color v)
    {
        return new Vector3(v.r, v.g, v.b);
    }

    public static Color coppyFromVector3(this Color v, Vector3 vec)
    {
        v = new Color(vec.x, vec.y, vec.z, v.a);
        return v;
    }
}
public static class CameraExtension
{
    public static Camera getCameraByName(string name)
    {
        Camera[] allCamera = Camera.allCameras;
        foreach (var cam in allCamera)
        {
            if (cam.name == name)
            {
                return cam;
            }
        }
        return null;
    }
}

public static class ResourceExtension
{
    public static Sprite LoadAtlas(string atlas, string spritename)
    {
        Sprite[] textures = Resources.LoadAll<Sprite>(atlas);
        return textures.Where(t => t.name == spritename).First<Sprite>();
    }
}

