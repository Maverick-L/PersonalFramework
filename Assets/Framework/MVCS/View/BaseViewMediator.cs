using System.Collections;
using System.Collections.Generic;
using Framework.Message;
namespace Framework.MVC
{
    public class BaseWindowParams
    {
        public bool _isback;
    }
    public abstract class BaseViewMediator : IMessage
    {
        protected BaseViewWindow _window;
        private BaseWindowParams _params;
        /// <summary>
        /// 获取页面参数
        /// </summary>
        /// <typeparam name="TParams"></typeparam>
        /// <param name="param"></param>
        /// <returns></returns>
        protected bool GetParams<TParams>(out TParams param) where TParams : BaseWindowParams
        {
            param = _params as TParams;
            if(_params == null)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 获取具体的Window实例
        /// </summary>
        /// <typeparam name="TWindow"></typeparam>
        /// <returns></returns>
        protected TWindow Get<TWindow>() where TWindow : BaseViewWindow
        {
            return _window as TWindow;
        }

        /// <summary>
        /// 面向ViewManager 的创建
        /// </summary>
        /// <param name="window"></param>
        /// <param name="param"></param>
        public  void OnCreate(BaseViewWindow window,BaseWindowParams param =null)
        {
            _window = window;
            _params = param;
            MessageManager.instance.Regist(this);
            OnRealCreate();
        }
        /// <summary>
        /// 面向ViewManager 的摧毁
        /// </summary>
        public  void OnDestory()
        {
            _window = null;
            _params = null;
            MessageManager.instance.UnRegist(this);
            OnRealDestory();
        }

        /// <summary>
        /// 具体实例的创建初始化
        /// </summary>
        protected abstract void OnRealCreate();
        /// <summary>
        /// 具体实例的摧毁初始化
        /// </summary>
        protected abstract void OnRealDestory();

    }
}