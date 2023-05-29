using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Entities;
using Unity.Mathematics;
public struct MyInput : IComponentData
{
    public byte click;//0:为没有点击 // 1 点击左键  2 点击右键  4 点击中键
    public float3 pointPos; 
    public byte moveDir; //移动方向  8个方向 顺时针旋转 从最小位向最大位 分别是 上  右上  右   右下  下   左下  左   左上
    public byte jump; //跳
    public byte down; //蹲
    public int frame;
    public int createPlayerCount;
    public override string ToString()
    {
        return "Input:{\t Frame:" + frame + "\tclick:" + click + "\tpointPos:" + pointPos + "\tmoveDir:" + moveDir + "\tjump:" + jump + "\tdown:" + down+"\t}";
    }
}

public class InputManager : ManagerBase 
{
    private byte[] _input;
    private int _frame;
    private int _maxFrame = 500;
    private int _inputSize;
    private int _saveFrame;

    public int saveFrameCount => _saveFrame;
    public int inputFrameCount => _frame;

    //创建角色使用，
    public int createPlayerCount = 0;
    public MyInput GetInput()
    {
        if(_frame >= _saveFrame)
        {
            return new MyInput()
            {
                click = 0,
                pointPos = 0,
                moveDir = 0,
                frame = -1,
                jump = 0,
                down = 0,
                createPlayerCount = 0
            };
        }
        MyInput inputData = new MyInput();
        unsafe
        {
            fixed(byte* input = &_input[(_frame++ % _maxFrame) * _inputSize])
            {
                inputData = *(MyInput*)input;
            }
        }
        return inputData;
    }

    public override void Initialization()
    {
        var size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(MyInput));
        _inputSize = size;
        _input = new byte[size * _maxFrame];
        for(int i = 0; i < _maxFrame; i++)
        {
            unsafe
            {
                fixed (byte* input = &_input[i * _inputSize])
                {
                    *input = 0;
                }
            }
        }
        _frame = 1;
        _saveFrame = 1;
        isInit = true;
    }

    public override void OnDestoryManager()
    {
    }

    public  void Update()
    {
        MyInput input = new MyInput();
        //Bug:没有两个键的输入
        if (UnityEngine.Input.anyKeyDown || createPlayerCount != 0)
        {
            input.click = 0;
            if(UnityEngine.Input.GetKeyDown(KeyCode.Mouse0))
            {
                input.click = 1;
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Mouse1))
            {
                input.click = 2;
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Mouse2))
            {
                input.click = 4;
            }
            if(input.click != 0)
            {
                input.pointPos = UnityEngine.Input.mousePosition;
            }
            input.moveDir = 0;
            if (UnityEngine.Input.GetKeyDown(KeyCode.W) && !UnityEngine.Input.GetKeyDown(KeyCode.S))
            {
                if(UnityEngine.Input.GetKeyDown(KeyCode.A) && !UnityEngine.Input.GetKeyDown(KeyCode.D))
                {
                    input.moveDir =0b10000000;
                    Debug.LogError("输入WA");
                }
                else if (UnityEngine.Input.GetKeyDown(KeyCode.D))
                {
                    input.moveDir = 0b00000010;
                    Debug.LogError("输入WD");

                }
                else
                {
                    input.moveDir = 0b00000001;
                }
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.S))
            {
                if (UnityEngine.Input.GetKeyDown(KeyCode.A) && !UnityEngine.Input.GetKeyDown(KeyCode.D))
                {
                    input.moveDir = 0b01000000;
                    Debug.LogError("输入SA");
                }
                else if (UnityEngine.Input.GetKeyDown(KeyCode.D))
                {
                    input.moveDir = 0b00001000;
                    Debug.LogError("输入SD");

                }
                else
                {
                    input.moveDir = 0b00010000;
                }
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.A))
            {
                input.moveDir = 0b00100000;
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.D))
            {
                input.moveDir = 0b00000100;
            }
            input.jump = (byte)(UnityEngine.Input.GetKeyDown(KeyCode.Space) ? 1 : 0);
            input.down = (byte)(UnityEngine.Input.GetKeyDown(KeyCode.LeftControl) ? 1 : 0);
            input.createPlayerCount = createPlayerCount;
            createPlayerCount = 0;
            SaveFrame(input);
        }
       
    }

    private void SaveFrame(MyInput inputData)
    {
        unsafe
        {
            fixed (byte* input = &_input[(_saveFrame % _maxFrame) * _inputSize])
            {
                //input 是这一帧 存放数据的起始地址
                *(MyInput*)input = inputData;
                (*(MyInput*)input).frame = _saveFrame;
            }
        }
        _saveFrame++;
    }
}
