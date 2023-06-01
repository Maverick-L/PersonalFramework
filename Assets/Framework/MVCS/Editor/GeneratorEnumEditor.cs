using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
namespace Framework.MVC
{

    public enum EGenerator
    {
        EWindow,
        Window
    }


    /// <summary>
    /// 用来生成EWindow,Window代码,EModel,EContraller
    /// </summary>
    public static class GeneratorEnumEditor
    {
        private const string GENERATOR_ROOT_PATH ="Assets/Script/Demo/MVCExample";
        [MenuItem("Framework/MVC/Generator All")]
        private static void GeneratorAll()
        {
            GeneratorEWindow();
        }


        [MenuItem("Framework/MVC/Generator View EWindow ")]
        private static void GeneratorEWindow()
        {
            Generator(EGenerator.EWindow);

        }

        [MenuItem("Assets/MVC/Generator Window")]
        private static void GeneratorWindow()
        {
            RealGeneratWindow();
        }

        [MenuItem("GameObject/MVC/Generator Window",priority =0)]
        private static void GeneratorWithHierarchy()
        {
            RealGeneratWindow();
            GeneratorEWindow();
        }

        private static void RealGeneratWindow()
        {
            var objs = Selection.gameObjects;
            for(int i = 0; i < objs.Length; i++)
            {
                GeneratorUIWIndowEditor.OnStartCreate(objs[i]);
                AssetDatabase.Refresh();
                objs[i].AddComponent(Assembly.GetAssembly(typeof(BaseViewWindow)).GetType(objs[i].name));
                EditorUtility.SetDirty(objs[i]);
            }
            AssetDatabase.Refresh();
        }

        private static FileStream GetFilePath(EGenerator gType,string fileName = "")
        {
            string path="";
            switch (gType)
            {
                case EGenerator.EWindow:
                    path = Path.Combine(GENERATOR_ROOT_PATH, "View/EWindow.cs");break;
            }
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            
            return File.Create(path);
        }
        #region 写入

        private static void Generator(EGenerator gType,string fileName="")
        {
            FileStream stream = GetFilePath(gType, fileName);
            StreamWriter write = new StreamWriter(stream);
            WriteHead(write,gType,fileName);

            switch (gType)
            {
                case EGenerator.EWindow:
                    WriteEnumData(write, "Framework.MVC.BaseViewWindow");break;
            }

            write.WriteLine("}");
            write.Flush();
            write.Close();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 写入头字段
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="gtype"></param>
        /// <param name="fileName"></param>
        private static void WriteHead(StreamWriter writer, EGenerator gtype, string fileName = "")
        {
            writer.WriteLine("//-----------------------------------------------------");
            writer.WriteLine("//-----以下文件由自动化生成,请勿修改----------------------");
            writer.WriteLine("//-----------------------------------------------------");
            writer.WriteLine("//-----------------------------------------------------");

            switch (gtype)
            {
                case EGenerator.EWindow:
                    writer.WriteLine("public enum EWindow"); break;
            }
            writer.WriteLine("{");

        }
        /// <summary>
        /// 写入enum数据,依据反射获取所以继承baseName的文件，将文件名写入enum
        /// </summary>
        /// <param name="writer">写入流</param>
        /// <param baseName="writer">继承的父类的名字</param>
        private static void WriteEnumData(StreamWriter writer,string baseName)
        {
            HashSet<string> writeSet = new HashSet<string>();

            Assembly assembly = Assembly.GetAssembly(typeof(MVCLaunch));
            var allTypes = assembly.GetTypes();
            System.Type baseType = assembly.GetType(baseName);
            foreach(var mtype in allTypes)
            {
                if(mtype.IsClass && mtype.BaseType == baseType && mtype != baseType)
                {
                    if (writeSet.Add(mtype.Name))
                    {
                        GeneratorAttrubity att = mtype.GetCustomAttribute(typeof(GeneratorAttrubity)) as GeneratorAttrubity;
                        if (att != null) {
                            writer.WriteLine("    /// <summary>");
                            writer.WriteLine($"    /// {att.des}");
                            writer.WriteLine("    /// </summary>");
                        }

                        writer.WriteLine("    " + mtype.Name+",");
                    }
                }
            }
        }

        #endregion
    }
}