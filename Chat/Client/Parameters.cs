namespace Client
{
    public struct Parameters
    {
        public Parameters(string address, int port, string message)
        {
            Address = address;
            Port = port;
            Message = message;
        }

        public string Address { get; set; }
        public int Port { get; set; }
        public string Message { get; set; } 
    }
}
