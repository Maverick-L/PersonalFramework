using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

[GeneratorWindowAttrubity(EUINode.UI_Text,typeof(Text))]
public class UI_TextCreateEditor :BaseGeneratorEditor
{
    public override bool Check()
    {
        return TryGetContraller<Text>();
    }

    public override void WriteField(string tab)
    {
        _write.WriteLine($"{tab}public Text {_fieldName};");

    }

    public override void WriteAwake(string tab)
    {
        _write.WriteLine($"{tab}{_fieldName} = transform.Find(\"{_findPath}\").GetComponent<Text>();");
    }
    public override void WriteMethod(string tab)
    {
        _write.WriteLine($"{tab}public void Set{_fieldFuncName}(string value)");
        _write.WriteLine($"{tab}{{");
        _write.WriteLine($"{tab}{_tab}{ _fieldName}.text = value;");
        _write.WriteLine($"{tab}}}");
        MoveToNext();
    }
}
