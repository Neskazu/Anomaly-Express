using Cysharp.Threading.Tasks;

namespace UI.Base
{
    public interface IWindow
    {
        public UniTask Show();
        public UniTask Hide();
    }
}