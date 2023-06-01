using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Reflection;

public class GeneratorUIWIndowEditor
{
    public static readonly string UI_CLASS_SAVE_PATH = Application.dataPath + "/Script/AutoUI";

    public static void OnStartCreate(GameObject target)
    {
        string classPath = Path.Combine(UI_CLASS_SAVE_PATH, target.name + ".cs");
        StreamWriter writer;
        if (!Directory.Exists(UI_CLASS_SAVE_PATH))
        {
            Directory.CreateDirectory(UI_CLASS_SAVE_PATH);
        }
        if (File.Exists(classPath))
        {
            File.Delete(classPath);
        }
        writer = new StreamWriter(File.Create(classPath));

        try
        {
            var map = GetGeneratorMap();
            List<BaseGeneratorEditor[]> generatorMap = new List<BaseGeneratorEditor[]>();
            GetGenerator(target, map, writer, ref generatorMap);
            OnWrite(writer, target, generatorMap);
            writer.Flush();
            Debug.Log("文件写入成功:" + classPath);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
        finally
        {
            writer.Close();

        }


    }
    #region 写入

    private static void OnWrite(StreamWriter writer,GameObject target, List<BaseGeneratorEditor[]> generatorMap)
    {
        //写入基础using
        OnWriteUseing(writer, generatorMap);
        //写入额外需要using内容
        writer.WriteLine();
        OnWriteClass(writer, target);
        //写入声明
        OnWriteField(generatorMap);
        //写入Unity的时间调用事件
        OnWriteUnityFunc(writer, generatorMap);
        //写入方法
        OnWriteMethod(generatorMap);
        writer.WriteLine("}");
    }
    private static void OnWriteUseing(StreamWriter write,List<BaseGeneratorEditor[]> map)
    {
        write.WriteLine($"using UnityEngine;");
        write.WriteLine($"using UnityEngine.UI;");
        write.WriteLine($"using System;");
        write.WriteLine($"using System.Collections;");
        write.WriteLine($"using System.Collections.Generic;");
        OnWriteNodeFunc(map, "WriteUsing");
    }


    /// <summary>
    /// 写入声明
    /// </summary>
    /// <param name="map"></param>
    private static void OnWriteField(List<BaseGeneratorEditor[]> map)
    {
        OnWriteNodeFunc(map, "WriteField", "    ");
    }

    private static void OnWriteMethod(List<BaseGeneratorEditor[]> map)
    {
        OnWriteNodeFunc(map, "WriteMethod", "    ");
    }
    private static void OnWriteClass(StreamWriter write,GameObject root)
    {
        write.WriteLine($"public class {root.transform.root.name} : Framework.MVC.BaseViewWindow");
        write.WriteLine(@"{");
    }

    private static void OnWriteUnityFunc(StreamWriter write, List<BaseGeneratorEditor[]> map)
    {
        OnWiretUnityEvent(write, map, "Awake");
        OnWiretUnityEvent(write, map, "OnEnable");
        OnWiretUnityEvent(write, map, "OnDisable");
        OnWiretUnityEvent(write, map, "Start");
        OnWiretUnityEvent(write, map, "OnDestroy");
        OnWiretUnityEvent(write, map, "Update");
    }

    private static void OnWiretUnityEvent(StreamWriter write,List<BaseGeneratorEditor[]> map,string funcName)
    {
        var tab = "    ";
        write.WriteLine($"{tab}public override void {funcName}()");
        write.WriteLine($"{tab}{{");
        write.WriteLine($"{tab}{tab}base.{funcName}();");
        OnWriteNodeFunc(map, $"Write{funcName}", tab + tab);
        write.WriteLine($"{tab}}}");
        write.WriteLine("\n");
    }

    private static void OnWriteNodeFunc(List<BaseGeneratorEditor[]> map,string funcName,string childTab ="")
    {
        for (int i = 0; i < map.Count; i++)
        {
            for (int j = 0; j < map[i].Length; j++)
            {
                try {
                    var method = map[i][j].GetType().GetMethod(funcName, BindingFlags.Instance | BindingFlags.Public);
                    method.Invoke(map[i][j], new object[] { childTab });
                }
                catch(Exception ex)
                {
                    Debug.LogError(funcName + "：错误");
                    throw ex;
                }
              
            }
        }
    }
    #endregion
    /// <summary>
    /// 获取写入文件的类
    /// </summary>
    /// <returns></returns>
    private static void GetGenerator(GameObject go, Dictionary<EUINode, Type> map,StreamWriter writer, ref List<BaseGeneratorEditor[]> stackMap)
    {
        Framework.MVC.GeneratorUINode uinode = go.GetComponent<Framework.MVC.GeneratorUINode>();
        if (uinode != null)
        {
            BaseGeneratorEditor[] allGenerator = new BaseGeneratorEditor[uinode.nodeTypeList.Length];
            for (int i = 0; i < uinode.nodeTypeList.Length; i++)
            {
                if (map.ContainsKey(uinode.nodeTypeList[i]))
                {
                    allGenerator[i] = Activator.CreateInstance(map[uinode.nodeTypeList[i]]) as BaseGeneratorEditor;
                    allGenerator[i].OnInit(go, writer);
                }
            }
            stackMap.Add(allGenerator);
            
        }
        var childCount = go.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            var child = go.transform.GetChild(i);
            GetGenerator(child.gameObject, map, writer, ref stackMap);
        }
    }

    private static Dictionary<EUINode, Type> GetGeneratorMap()
    {
        Assembly assembly = Assembly.GetAssembly(typeof(BaseGeneratorEditor));
        Type[] allType = assembly.GetTypes();
        Dictionary<EUINode, Type> map = new Dictionary<EUINode, Type>();

        for(int i = 0; i < allType.Length; i++)
        {
            Type t = allType[i];
            GeneratorWindowAttrubity att = t.GetCustomAttribute<GeneratorWindowAttrubity>();
            if (att != null)
            {
                map.Add(att.node, t);
            }
        }
        return map;
    }
}
