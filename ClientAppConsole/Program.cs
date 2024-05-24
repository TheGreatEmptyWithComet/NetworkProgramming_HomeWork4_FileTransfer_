namespace ClientApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(1000);

            FileClient fileClient = new FileClient("localhost", 8800);
            fileClient.Send();
        }
    }
}