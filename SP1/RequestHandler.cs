using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using SP1.Utils;
using SP1.Cache;
using System.Net;
using System.Text;

namespace SP1
{
    public static class RequestHandler
    {
        private static readonly object locker = new();
        private static readonly WorkbookCache cache = new(20);

        public static void HandleRequest(HttpListenerContext context)
        {
            Console.WriteLine("Pocela obrada zahteva");

            HttpListenerResponse response = context.Response;

            //greska u request-u
            if (context == null
                || context.Request == null
                || context.Request.RawUrl == null
                || context.Request.RawUrl.Length < 6)
            {
                RespondError(ref response, "Greska u request-u", 400, "Bad Request");
                return;
            }

            //nije .csv fajl
            if (!context.Request.RawUrl.EndsWith(".csv"))
            {
                RespondError(ref response, "Trazeni fajl nije .csv", 415, "Unsupported Media Type");
                return;
            }

            string reqFile = context.Request.RawUrl[1..];

            //provera da li je u kesu
            IWorkbook? wb = cache.TryGet(reqFile);

            //nije u kesu
            if (wb == null)
            {
                //fajl ne postoji
                if (!ConvertFile(reqFile, out wb))
                {
                    RespondError(ref response, "Trazeni fajl ne postoji", 404, "Not Found");
                    return;
                }

                //greska pri citanju
                if (wb == null)
                {
                    RespondError(ref response, "", 500, "Internal Server Error");
                    return;
                }
            }

            //azuriranje kesa
            cache.AddOrUse(wb);

            response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            wb.Write(response.OutputStream);
            response.Close();
            Console.WriteLine("Uspesna obrada zahteva");
        }

        private static bool ConvertFile(string reqFile, out IWorkbook? wb)
        {
            string[] lines;
            bool lockTaken = false;

            try
            {
                Monitor.Enter(locker, ref lockTaken);
                lines = File.ReadAllLines($"../../../CSVFiles/{reqFile}");
            }
            catch (FileNotFoundException) //fajl ne postoji
            {
                wb = null;
                return false;
            }
            catch (Exception ex) //greska pri pristupu fajlu
            {
                Console.WriteLine(ex.Message + "\n");
                wb = null;
                return true;
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(locker);
                }
            }

            int rows = 0;

            wb = new XSSFWorkbook();
            ISheet sheet = wb.CreateSheet(reqFile);

            foreach (string line in lines)
            {
                IRow row = sheet.CreateRow(rows++);

                int cols = 0;
                var vals = line.Split(',');

                foreach (string val in vals)
                {
                    ICell cell = row.CreateCell(cols++);

                    SetCellUtil.SetCellValue(ref cell, val);
                }
            }

            return true;
        }

        private static void RespondError(ref HttpListenerResponse resp, string toPrint, int code, string desc)
        {
            Console.WriteLine(toPrint);
            resp.StatusCode = code;
            resp.StatusDescription = desc;
            string responseString = $"<HTML><BODY>{code} {desc}</BODY></HTML>";
            resp.Close(Encoding.UTF8.GetBytes(responseString), false);
        }
    }
}
