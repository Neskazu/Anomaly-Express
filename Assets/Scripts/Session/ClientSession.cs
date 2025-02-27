namespace Session
{
    public class ClientSession : Base.Session
    {
        public override void Start()
        {
            Network.StartClient();
        }

        public override void Stop()
        {
            Network.Shutdown();
        }
    }
}