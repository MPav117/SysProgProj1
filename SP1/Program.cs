using System.Net;

namespace SP1
{
    public class XLServer
    {
        public static void Main()
        {
            HttpListener listener = new();
            try
            {
                listener.Prefixes.Add("http://localhost:7777/");
                listener.Start();
            }
            catch (HttpListenerException ex)
            {
                Console.WriteLine(ex.ErrorCode + "\n");
            }

            if (listener.IsListening)
            {
                Console.WriteLine("Server podignut. Pocinje slusanje zahteva");

                while (true)
                {
                    try
                    {
                        HttpListenerContext context = listener.GetContext();
                        Console.WriteLine("Primljen zahtev");

                        try
                        {
                            ThreadPool.QueueUserWorkItem(RequestHandler.HandleRequest, context, false);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message + "\n");
                        }
                    }
                    catch (HttpListenerException ex)
                    {
                        Console.WriteLine(ex.ErrorCode + "\n");
                    }
                }
            }
        }
    }
}