using UnityEngine;

namespace Anomalies
{
    public class SplitControlAnomaly : AnomalyBase
    {
        public static bool IsSplitActive { get; private set; } = false;

        protected override void OnActivate()
        {
            IsSplitActive = true;
        }

        protected override void OnDeactivate()
        {
            IsSplitActive = false;
        }
        private void OnDestroy()
        {
            IsSplitActive = false;
        }
    }
}