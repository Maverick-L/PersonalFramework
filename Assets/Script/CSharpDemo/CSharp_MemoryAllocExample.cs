using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.CSharp
{
    public unsafe class CSharp_MemoryAllocExampleClass
    {
       public int e;
       public int f;
    }
    struct CSharp_MemoryAllocExampleStruct
    {
        int g;
        int h;
    }
    class CSharp_MemoryAllocExample : UnityEngine.MonoBehaviour
    {

        public int a;
        public int b;

        private void Reset()
        {
            Awake();
        }

        private void Awake()
        {
            int c;
            int d;
            CSharp_MemoryAllocExampleClass classA = new CSharp_MemoryAllocExampleClass(); ;
            CSharp_MemoryAllocExampleStruct structA;
            unsafe
            {

                fixed (int* adress = &a,  bAdress = &b,eAdress = &classA.e,fAdress = & classA.f)
                {
                    string log = "全局变量地址A:{0:X}----全局变量地址C:{1:X}---局部变量地址c:{2:X}---局部变量地址d:{3:X}----classE:{4:X}---classG:{5:X}---struct:{6:X}---structSize:{7:X}";
                    UnityEngine.Debug.LogFormat(log,(int)adress,(int)bAdress,(int)&c,(int)&d,(int)eAdress,(int)fAdress,(int)&structA,(int)sizeof(CSharp_MemoryAllocExampleStruct));

                }
            }
        }
    }

}