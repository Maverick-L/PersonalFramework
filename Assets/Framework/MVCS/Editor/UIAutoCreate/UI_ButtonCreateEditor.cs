using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
[GeneratorWindowAttrubity( EUINode.UI_Button, typeof(Button))]
public class UI_ButtonCreateEditor : BaseGeneratorEditor
{
    public override bool Check()
    {
        return TryGetContraller<Button>();
    }

    public override void WriteField(string tab)
    {
        _write.WriteLine($"{tab}public Button {_fieldName};");
    }

    public override void WriteMethod(string tab)
    {
        _write.WriteLine($"{tab}public void Add{_fieldFuncName}Listence(UnityEngine.Events.UnityAction action)");
        _write.WriteLine($"{tab}{{");
        _write.WriteLine($"{tab}{_tab}{_fieldName}.onClick.AddListener(action);");
        _write.WriteLine($"{tab}}}");
        MoveToNext();
        _write.WriteLine($"{tab}public void Remove{_fieldFuncName}Listence(UnityEngine.Events.UnityAction action)");
        _write.WriteLine($"{tab}{{");
        _write.WriteLine($"{tab}{_tab}{_fieldName}.onClick.RemoveListener(action);");
        _write.WriteLine($"{tab}}}");
        MoveToNext();
    }

    public override void WriteAwake(string tab)
    {
        _write.WriteLine($"{tab}{_fieldName} = transform.Find(\"{_findPath}\").GetComponent<Button>();");
    }

    public override void WriteOnDestroy(string tab)
    {
        _write.WriteLine($"{tab}{_fieldName}.onClick.RemoveAllListeners();");
    }
}
