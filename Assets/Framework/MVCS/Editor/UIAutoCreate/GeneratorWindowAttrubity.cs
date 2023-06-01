
public class GeneratorWindowAttrubity :System.Attribute
{
    public EUINode node;
    public System.Type uGUiType;
    public GeneratorWindowAttrubity(EUINode node,System.Type uGUiType)
    {
        this.node = node;
        this.uGUiType = uGUiType;
    }
}
