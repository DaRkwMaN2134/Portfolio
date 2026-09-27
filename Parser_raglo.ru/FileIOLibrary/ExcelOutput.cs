using OfficeOpenXml;
using OfficeOpenXml.Style;
using DataLibrary;
using ConfigurationLibrary;

namespace FileIOLibrary
{
    public class Excel_Output: IExcelOutput
    {
        private readonly ILogger _logger;

        public Excel_Output(ILogger logger)
        {
            _logger = logger;
        }
        public async Task ExcelOutput(List<Card> Card)
        {
            int row = 0;
            ExcelWorksheet sheet;

            if (Card == null || Card.Count == 0)
            {
                await _logger.LogErrorAsync($"Excel - Нет данных для выгрузки.");
                return;
            }
            else
            {
                ExcelPackage.License.SetNonCommercialPersonal("Learning");
                using var package = new ExcelPackage();
                sheet = package.Workbook.Worksheets.Add("Карточки");

                List<string> headeades = new List<string>{
                "Имя категории",
                "Артикль",
                "Url-картинки",
                "Цена",
                "Описание"};

                for (int i = 0; i < headeades.Count; i++)
                {
                    sheet.Cells[1, i + 1].Value = headeades[i];
                }
                sheet.Cells[2, 1].LoadFromCollection(Card, false);
                sheet.View.FreezePanes(2, 1);


                sheet.Cells[1, 1, Card.Count + 1, 5].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                sheet.Cells[1, 1, Card.Count + 1, 5].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                sheet.Cells[1, 1, Card.Count + 1, 5].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                sheet.Cells[1, 1, Card.Count + 1, 5].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                sheet.Cells[1, 1, 1, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[2, 1, Card.Count + 1, 5].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                sheet.Cells[2, 1, Card.Count + 1, 5].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;


                sheet.Cells[1, 1, Card.Count + 1, 5].AutoFilter = true;
                sheet.Cells[1, 1, Card.Count + 1, 5].AutoFitColumns();
                sheet.Column(5).Style.WrapText = true;
                sheet.Column(5).Width = 60;
                await package.SaveAsAsync(new FileInfo("Card.xlsx"));
            }
        }

        public async Task<int> AppendCardsAsync(ExcelWorksheet sheet, List<Card> cards, int startRow)
        {
            if (cards == null || cards.Count == 0) return 0;
            sheet.Cells[startRow, 1].LoadFromCollection(cards, false);
            return cards.Count;
        }
    }
}