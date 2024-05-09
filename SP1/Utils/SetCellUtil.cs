using NPOI.SS.UserModel;

namespace SP1.Utils
{
    public static class SetCellUtil
    {
        public static void SetCellValue(ref ICell cell, string value)
        {
            if (double.TryParse(value, out double parsedDouble))
            {
                cell.SetCellValue(parsedDouble);
            }
            else if (DateOnly.TryParse(value, out DateOnly parsedDateOnly))
            {
                cell.SetCellValue(parsedDateOnly);
            }
            else if (DateTime.TryParse(value, out DateTime parsedDateTime))
            {
                cell.SetCellValue(parsedDateTime);
            }
            else if (bool.TryParse(value, out bool parsedBool))
            {
                cell.SetCellValue(parsedBool);
            }
            else
            {
                cell.SetCellValue(value);
            }
        }
    }
}
