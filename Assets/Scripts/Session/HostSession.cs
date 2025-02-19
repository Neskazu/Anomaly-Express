using Session.Base;
using Unity.Netcode;

namespace Session
{
    public class HostSession : Base.Session
    {
        private readonly IConnectionValidator validator;

        public HostSession()
        {
            Network.ConnectionApprovalCallback += ApproveConnection;
        }

        public HostSession(IConnectionValidator validator)
        {
            this.validator = validator;

            Network.ConnectionApprovalCallback += ApproveConnection;
        }

        public override void Dispose()
        {
            Network.ConnectionApprovalCallback -= ApproveConnection;
            base.Dispose();
        }

        public override void Start()
        {
            Network.StartHost();
        }

        public override void Stop()
        {
            Network.Shutdown();
        }

        private void ApproveConnection(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            if (!validator.Validate(request, out string reason))
            {
                response.Reason = reason;

                response.Approved = false;
                response.Pending = false;

                return;
            }

            response.Approved = true;
            response.Pending = false;
        }
    }
}