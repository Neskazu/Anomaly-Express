using Cysharp.Threading.Tasks;

namespace UI.Base
{
    public interface IView
    {
        public UniTask Show();
        public UniTask Hide();
    }
}