using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

[GeneratorWindowAttrubity(EUINode.UI_Text)]
public class UI_TextCreateEditor :BaseGeneratorEditor
{
    private Text _text;
    private string _fieldName;
    public override void OnInit(GameObject go, StreamWriter write)
    {
        base.OnInit(go, write);
        _fieldName = _myObj.name + "Text";
    }
    public override bool Check()
    {
        return TryGetContraller<Text>(out _text);
    }

    public override void WriteField(string tab)
    {
        _write.WriteLine($"{tab}public Text {_fieldName};");

    }

    public override void WriteValuation(string tab)
    {
        _write.WriteLine($"{tab}{_fieldName} = transform.Find({_findPath}).GetComponent<Text>();");
    }
    public override void WriteMethod(string tab)
    {
        _write.WriteLine($"{tab}public void Set{_fieldName}Text(string value)");
        _write.WriteLine($"{tab}{{");
        _write.WriteLine($"{tab}{_tab}{ _fieldName}.text = value;");
        _write.WriteLine($"{tab}}}");

    }
}
