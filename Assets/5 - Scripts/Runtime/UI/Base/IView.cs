using UnityEngine;

namespace UI.Base
{
    public interface IView : IWindow
    {
        public void Bind(Object model);
    }
}