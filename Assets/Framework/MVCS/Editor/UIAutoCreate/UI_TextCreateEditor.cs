using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class UI_TextCreateEditor :BaseCreateEditor
{
    private Text _text;
    bool _have;
    public UI_TextCreateEditor(GameObject go, StreamWriter writer) : base(go, writer) {
        _have = TryGetContraller<Text>(out _text);
    }
    public override bool Check()
    {
        return _myObj.GetComponent<Text>();
    }

    public override void WriteField()
    {
        _write.WriteLine($"{_tab}public Text {_text.gameObject.name}Text;");
    }

    public override void WriteAssignment()
    {
        
    }

    public override void WriteMethod()
    {
        _write.WriteLine($"{_tab}public void Set{_text.gameObject.name}(string value)");
        _write.WriteLine($"{_tab}" + "{");
        _write.WriteLine($"{_tab}{_tab}");
        _write.WriteLine($"{_tab}" + "}");
    }
}
