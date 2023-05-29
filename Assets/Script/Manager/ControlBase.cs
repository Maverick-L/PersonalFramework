using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ControllerType
{
    FFP,//流场类型
}

public class ControllerAttrubite : System.Attribute
{
    public ControllerType type;
    public ControllerAttrubite(ControllerType type)
    {
        this.type = type;
    }
}

public abstract class ControlBase :IControl
{
    public virtual IEnumerator AsynInitlization() { yield return null; }

    public abstract void OnDestoryControl();
    public abstract void OnInitlization();
    public abstract void OnLogicUpdate();
    public abstract void OnRenderUpdate();
}

public interface  IControl {

      void OnInitlization();

      void OnLogicUpdate();

      void OnRenderUpdate();

      void OnDestoryControl();
}
