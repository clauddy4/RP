namespace Chain
{
    class Parameters
    {
        public Parameters(int listeningPort, string nextHost, int nextPort, bool isInitiator)
        {
            ListeningPort = listeningPort;
            NextHost = nextHost;
            NextPort = nextPort;
            IsInitiator = isInitiator;
        }

        public int ListeningPort { get; set; }
        public string NextHost { get; set; }
        public int NextPort { get; set; }
        public bool IsInitiator { get; set; }
    }
}
