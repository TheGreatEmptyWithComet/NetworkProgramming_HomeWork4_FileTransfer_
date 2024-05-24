namespace ServerApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            FileServer fileServer = new FileServer(8800);
            fileServer.Start();
        }
    }
}