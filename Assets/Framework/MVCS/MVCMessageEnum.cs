
namespace Framework.MVC
{
    public class MVCMessageAttrubite : Framework.Message.MessageAttrubity
    {
        public MVCMessageAttrubite(EMVCMessageEnum Etype)
        {
            messageType = Etype;
        }
    }

    public enum EMVCMessageEnum
    {
        /// <summary>
        /// 参数格式，第一个值为Type类型，具体的Model类型，第二个值为具体的修改的参数
        /// </summary>
        ModeifyModelMessage, 
    }
}

