namespace CricleMainServer
{
    interface IServerManager
    {
        void OpenServer();
        void StopServer();
        void OpenListenServer();
        void StopListenServer();
    }
}
