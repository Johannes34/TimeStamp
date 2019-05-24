using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TimeStamp
{
    public class ExcelExport
    {
        public ExcelExport(Form1 owner)
            : this(owner, owner.Manager, owner.Settings)
        { }

        public ExcelExport(IWin32Window owner, TimeManager manager, TimeSettings settings)
        {
            m_owner = owner;
            m_manager = manager;
            m_settings = settings;
        }

        private IWin32Window m_owner;
        private TimeManager m_manager;
        private TimeSettings m_settings;

        // Create Excel Export:

        public int[] GetExportableYears()
        {
            return m_manager.StampList.Select(s => s.Day.Year).Distinct().OrderByDescending(y => y).ToArray();
        }

        public void CreateExcel(int year)
        {
            ExcelPackage excel = new ExcelPackage();

            for (int i = 1; i <= 12; i++)
            {
                CreateExcelSheet(excel, year, i);

                if (year == m_manager.Time.Today.Year && i == m_manager.Time.Today.Month)
                    break;
            }

            var sfd = new SaveFileDialog();
            sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            sfd.FileName = $"Zeiterfassung {year}.xlsx";
            if (sfd.ShowDialog(m_owner) == DialogResult.OK)
            {
                excel.SaveAs(new FileInfo(sfd.FileName));
                Process.Start(sfd.FileName);
            }
        }

        private void CreateExcelSheet(ExcelPackage excel, int year, int month)
        {
            var sheet = excel.Workbook.Worksheets.Add($"{new DateTime(year, month, 1).ToString("MMM")} {year}");

            // write header texts:
            sheet.Cells[1, 1].Value = "Tag";
            sheet.Cells[1, 2].Value = "Projektst.";
            int column = 3;
            foreach (var activity in m_settings.TrackedActivities)
                sheet.Cells[1, column++].Value = activity;

            int endColumn = column - 1;

            // gray background:
            sheet.Cells[1, 1, 1, endColumn].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[1, 1, 1, endColumn].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

            // properly sized:
            sheet.Row(1).Height = 30;
            for (int i = 1; i <= endColumn; i++)
            {
                sheet.Column(i).Width = 24;
                // border:
                sheet.Cells[1, i].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.Black);
                // allow line breaks:
                sheet.Cells[1, i].Style.WrapText = true;
                // alignment:
                sheet.Cells[1, i].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                sheet.Cells[1, i].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            // write table body:
            int row = 2;
            var inclusiveMin = new DateTime(year, month, 1);
            var exclusiveMax = new DateTime(month == 12 ? year + 1 : year, month == 12 ? 1 : month + 1, 1);
            foreach (var stamp in m_manager.StampList.Where(s => s.Day >= inclusiveMin && s.Day < exclusiveMax).OrderBy(s => s.Day).ToArray())
            {
                sheet.Cells[row, 1].Value = stamp.Day.ToString("dd.MM.yyyy");
                sheet.Cells[row, 2].Formula = $"=SUM(C{row}:H{row})"; //GetTimeForExcelCell(stamp);

                column = 3;
                foreach (var activity in m_settings.TrackedActivities)
                    sheet.Cells[row, column++].Value = GetTimeForExcelCell(stamp, activity);

                // formatting:
                sheet.Cells[row, 2, row, endColumn].Style.Numberformat.Format = "0.00";

                // border:
                sheet.Cells[row, 2].Style.Border.Right.Style = ExcelBorderStyle.Medium;
                sheet.Cells[row, 2].Style.Border.Right.Color.SetColor(Color.Black);

                row++;
            }

            int summaryRow = 24;

            // keep drawing border down to summary row:
            for (int i = row; i < summaryRow; i++)
            {
                sheet.Cells[i, 2].Style.Border.Right.Style = ExcelBorderStyle.Medium;
                sheet.Cells[i, 2].Style.Border.Right.Color.SetColor(Color.Black);
            }

            // write footer summary line formulas:
            row = summaryRow;
            sheet.Cells[row, 1].Formula = "=COUNTA(A2:A21)";
            for (int i = 2; i <= endColumn; i++)
                sheet.Cells[row, i].Formula = $"=SUM({ExcelAddress.GetAddressCol(i)}2:{ExcelAddress.GetAddressCol(i)}21)";

            // formatting:
            sheet.Cells[row, 2, row, endColumn].Style.Numberformat.Format = "0.00";

            // gray background:
            sheet.Cells[row, 1, row, endColumn].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[row, 1, row, endColumn].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

            // border:
            for (int i = 1; i <= endColumn; i++)
            {
                sheet.Cells[row, i].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.Black);
            }
        }

        private double? GetTimeForExcelCell(Stamp stamp, string activity = null)
        {
            IEnumerable<ActivityRecord> activities = stamp.ActivityRecords;

            if (activity != null)
                activities = activities.Where(r => r.Activity == activity);

            var span = TimeSpan.FromMinutes(activities.Sum(r => m_manager.Total(r).TotalMinutes));

            if (span == TimeSpan.Zero)
                return null; // String.Empty;

            var hours = Math.Floor(span.TotalHours);
            var fractional = span.TotalHours - hours;
            // Minuten runden auf 
            // 0.25 (viertelstunde): >= 7.5 && < 22.5
            // 0.50 (halbestunde): >= 22.5 && < 
            // 0.75 (dreiviertelstunde):
            // 0.00 (ganze stunde):
            if (fractional <= 0.125 || fractional > 0.875)
                fractional = 0;
            else if (fractional <= 0.375)
                fractional = 0.25;
            else if (fractional <= 0.625)
                fractional = 0.50;
            else if (fractional <= 0.875)
                fractional = 0.75;

            return hours + fractional;
            //return span.ToString("hh\\:mm");
        }

    }
}
