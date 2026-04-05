using Managers;
using R3;
using UnityEngine;

namespace World
{
    public class WorldFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;

        private void Start()
        {
            TrainManager.Instance.OnNewWagon
                .Select(wagon => wagon.transform.position)
                .Subscribe(Move)
                .AddTo(this);
        }

        private void Move(Vector3 position)
        {
            position.y = target.position.y;
            target.position = position;
        }
    }
}