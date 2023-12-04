﻿using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Data;
using System.Drawing;
using System.Security.Claims;
using WebApp.Models;
using WebApp.Repository.Interfaces;

namespace WebApp.Repository.Services
{
    public class ExportServices : IExport
    {
        public void exportPengaduan(string fromDate, string toDate, string filter, List<ReportProductDTO> dataExport, ref MemoryStream mem)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage p = new ExcelPackage())
            {
                string strFromDate = fromDate != "" ? DateTime.Parse(fromDate).ToString("dd MMM yyyy") : "";
                string strToDate = toDate != "" ? DateTime.Parse(toDate).ToString("dd MMM yyyy") : "";

                p.Workbook.Properties.Author = "Lubes.id";
                p.Workbook.Properties.Title = "Pengaduan";

                p.Workbook.Worksheets.Add("Pengaduan");
                ExcelWorksheet ws = p.Workbook.Worksheets[0];
                ws.Name = "Pengaduan";
                ws.Cells.Style.Font.Size = 11;
                ws.Cells.Style.Font.Name = "Calibri";

                ws.Column(1).Width = 20;
                ws.Column(2).Width = 30;
                ws.Column(3).Width = 20;
                ws.Column(4).Width = 30;
                ws.Column(5).Width = 50;
                ws.Column(6).Width = 45;

                DataTable dt = new DataTable();

                dt.Columns.Add("Tanggal");
                dt.Columns.Add("Nama Lengkap");
                dt.Columns.Add("No. Telp (Whatsapp)");
                dt.Columns.Add("Email");
                dt.Columns.Add("Nama Produk");
                dt.Columns.Add("Deskripsi Pelapor");

                //Print here
                ws.Cells[1, 1].Value = "List Pengaduan";
                ws.Cells[1, 1].Style.Font.Bold = true;
                ws.Cells[1, 1].Style.Font.Size = 14;

                ws.Cells[2, 1].Value = "Tanggal";
                ws.Cells[2, 2].Value = ": " + strFromDate + " - " + strToDate;
                ws.Cells[3, 1].Value = "Filter Data";
                ws.Cells[3, 2].Value = ": " + filter;

                // Create table header
                int rowIndex = 5;
                int colIndex = 1;

                foreach (DataColumn dc in dt.Columns)
                {
                    var cell = ws.Cells[rowIndex, colIndex];

                    cell.Style.WrapText = true;
                    cell.Style.Font.Bold = true;
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    var fill = cell.Style.Fill;
                    fill.PatternType = ExcelFillStyle.Solid;
                    fill.BackgroundColor.SetColor(Color.LightGray);

                    var border = cell.Style.Border;
                    border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                    cell.Value = dc.ColumnName;

                    colIndex++;
                }
                // End of table header
                rowIndex++;

                // Create table data
                for (int i = 0; i < dataExport.Count; i++)
                {
                    var data = dataExport[i];
                    for (colIndex = 1; colIndex <= dt.Columns.Count; colIndex++)
                    {
                        if (colIndex == 1)
                        {
                            var cell = ws.Cells[rowIndex, colIndex];

                            cell.Style.WrapText = true;
                            cell.Style.Font.Bold = false;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                            var border = cell.Style.Border;
                            border.BorderAround(ExcelBorderStyle.Thin);

                            cell.Value = data.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss");
                        }
                        else if (colIndex == 2)
                        {
                            var cell = ws.Cells[rowIndex, colIndex];

                            cell.Style.WrapText = true;
                            cell.Style.Font.Bold = false;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                            var border = cell.Style.Border;
                            border.BorderAround(ExcelBorderStyle.Thin);

                            cell.Value = data.namaLengkap;
                        }
                        else if (colIndex == 3)
                        {
                            var cell = ws.Cells[rowIndex, colIndex];

                            cell.Style.WrapText = true;
                            cell.Style.Font.Bold = false;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                            var border = cell.Style.Border;
                            border.BorderAround(ExcelBorderStyle.Thin);

                            cell.Value = data.nomorTelp;
                        }
                        else if (colIndex == 4)
                        {
                            var cell = ws.Cells[rowIndex, colIndex];

                            cell.Style.WrapText = true;
                            cell.Style.Font.Bold = false;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                            var border = cell.Style.Border;
                            border.BorderAround(ExcelBorderStyle.Thin);

                            cell.Value = data.email;
                        }
                        else if (colIndex == 5)
                        {
                            var cell = ws.Cells[rowIndex, colIndex];

                            cell.Style.WrapText = true;
                            cell.Style.Font.Bold = false;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                            var border = cell.Style.Border;
                            border.BorderAround(ExcelBorderStyle.Thin);

                            cell.Value = data.namaProduk;
                        }
                        else if (colIndex == 6)
                        {
                            var cell = ws.Cells[rowIndex, colIndex];

                            cell.Style.WrapText = true;
                            cell.Style.Font.Bold = false;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                            var border = cell.Style.Border;
                            border.BorderAround(ExcelBorderStyle.Thin);

                            cell.Value = data.descPelapor;
                        }
                    }

                    rowIndex++;

                }

                mem = new MemoryStream(p.GetAsByteArray());
            }
        }

        public void exportProduct(string filter, List<SeriesMasterDTO> dataExport, ref MemoryStream mem)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage p = new ExcelPackage())
            {
                p.Workbook.Properties.Author = "Lubes.id";
                p.Workbook.Properties.Title = "Products";

                p.Workbook.Worksheets.Add("Products");
                ExcelWorksheet ws = p.Workbook.Worksheets[0];
                ws.Name = "Products";
                ws.Cells.Style.Font.Size = 11;
                ws.Cells.Style.Font.Name = "Calibri";

                ws.Column(1).Width = 15;
                ws.Column(2).Width = 55;
                ws.Column(3).Width = 20;
                ws.Column(4).Width = 20;

                DataTable dt = new DataTable();

                dt.Columns.Add("Series ID");
                dt.Columns.Add("Product Name");
                dt.Columns.Add("Product Packaging");
                dt.Columns.Add("Product Volume");

                //Print here
                ws.Cells[1, 1].Value = "List Products";
                ws.Cells[1, 1].Style.Font.Bold = true;
                ws.Cells[1, 1].Style.Font.Size = 14;

                ws.Cells[2, 1].Value = "Filter Data";
                ws.Cells[2, 2].Value = ": " + filter;

                // Create table header
                int rowIndex = 4;
                int colIndex = 1;

                foreach (DataColumn dc in dt.Columns)
                {
                    var cell = ws.Cells[rowIndex, colIndex];

                    cell.Style.WrapText = true;
                    cell.Style.Font.Bold = true;
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    var fill = cell.Style.Fill;
                    fill.PatternType = ExcelFillStyle.Solid;
                    fill.BackgroundColor.SetColor(Color.LightGray);

                    var border = cell.Style.Border;
                    border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                    cell.Value = dc.ColumnName;

                    colIndex++;
                }
                // End of table header
                rowIndex++;

                // Create table data
                for (int i = 0; i < dataExport.Count; i++)
                {
                    var data = dataExport[i];
                    for (colIndex = 1; colIndex <= dt.Columns.Count; colIndex++)
                    {
                        if (colIndex == 1)
                        {
                            var cell = ws.Cells[rowIndex, colIndex];

                            cell.Style.WrapText = true;
                            cell.Style.Font.Bold = false;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                            var border = cell.Style.Border;
                            border.BorderAround(ExcelBorderStyle.Thin);

                            cell.Value = data.seriesID;
                        }
                        else if (colIndex == 2)
                        {
                            var cell = ws.Cells[rowIndex, colIndex];

                            cell.Style.WrapText = true;
                            cell.Style.Font.Bold = false;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                            var border = cell.Style.Border;
                            border.BorderAround(ExcelBorderStyle.Thin);

                            cell.Value = data.productName;
                        }
                        else if (colIndex == 3)
                        {
                            var cell = ws.Cells[rowIndex, colIndex];

                            cell.Style.WrapText = true;
                            cell.Style.Font.Bold = false;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                            var border = cell.Style.Border;
                            border.BorderAround(ExcelBorderStyle.Thin);

                            cell.Value = data.productPackaging;
                        }
                        else if (colIndex == 4)
                        {
                            var cell = ws.Cells[rowIndex, colIndex];

                            cell.Style.WrapText = true;
                            cell.Style.Font.Bold = false;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                            var border = cell.Style.Border;
                            border.BorderAround(ExcelBorderStyle.Thin);

                            cell.Value = data.productVolume;
                        }
                    }

                    rowIndex++;

                }

                mem = new MemoryStream(p.GetAsByteArray());
            }
        }

        public void exportScan(string filter, List<LogScanningDTO> dataExport, ref MemoryStream mem)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage p = new ExcelPackage())
            {
                p.Workbook.Properties.Author = "Lubes.id";
                p.Workbook.Properties.Title = "Scans";

                p.Workbook.Worksheets.Add("Scans");
                ExcelWorksheet ws = p.Workbook.Worksheets[0];
                ws.Name = "Scans";
                ws.Cells.Style.Font.Size = 11;
                ws.Cells.Style.Font.Name = "Calibri";

                ws.Column(1).Width = 20;
                ws.Column(2).Width = 30;
                ws.Column(3).Width = 50;
                ws.Column(4).Width = 55;

                DataTable dt = new DataTable();

                dt.Columns.Add("Tanggal Scan");
                dt.Columns.Add("QR No");
                dt.Columns.Add("Product");
                dt.Columns.Add("Lokasi Scan");

                //Print here
                ws.Cells[1, 1].Value = "List Scans";
                ws.Cells[1, 1].Style.Font.Bold = true;
                ws.Cells[1, 1].Style.Font.Size = 14;

                ws.Cells[2, 1].Value = "Filter Data";
                ws.Cells[2, 2].Value = ": " + filter;

                // Create table header
                int rowIndex = 4;
                int colIndex = 1;

                foreach (DataColumn dc in dt.Columns)
                {
                    var cell = ws.Cells[rowIndex, colIndex];

                    cell.Style.WrapText = true;
                    cell.Style.Font.Bold = true;
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    var fill = cell.Style.Fill;
                    fill.PatternType = ExcelFillStyle.Solid;
                    fill.BackgroundColor.SetColor(Color.LightGray);

                    var border = cell.Style.Border;
                    border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                    cell.Value = dc.ColumnName;

                    colIndex++;
                }
                // End of table header
                rowIndex++;

                // Create table data
                for (int i = 0; i < dataExport.Count; i++)
                {
                    var data = dataExport[i];
                    for (colIndex = 1; colIndex <= dt.Columns.Count; colIndex++)
                    {
                        if (colIndex == 1)
                        {
                            var cell = ws.Cells[rowIndex, colIndex];

                            cell.Style.WrapText = true;
                            cell.Style.Font.Bold = false;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                            var border = cell.Style.Border;
                            border.BorderAround(ExcelBorderStyle.Thin);

                            cell.Value = data.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss");
                        }
                        else if (colIndex == 2)
                        {
                            var cell = ws.Cells[rowIndex, colIndex];

                            cell.Style.WrapText = true;
                            cell.Style.Font.Bold = false;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                            var border = cell.Style.Border;
                            border.BorderAround(ExcelBorderStyle.Thin);

                            cell.Value = data.qrNo;
                        }
                        else if (colIndex == 3)
                        {
                            var cell = ws.Cells[rowIndex, colIndex];

                            cell.Style.WrapText = true;
                            cell.Style.Font.Bold = false;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                            var border = cell.Style.Border;
                            border.BorderAround(ExcelBorderStyle.Thin);

                            cell.Value = data.productId;
                        }
                        else if (colIndex == 4)
                        {
                            var cell = ws.Cells[rowIndex, colIndex];

                            cell.Style.WrapText = true;
                            cell.Style.Font.Bold = false;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                            var border = cell.Style.Border;
                            border.BorderAround(ExcelBorderStyle.Thin);

                            cell.Value = data.alamatMap;
                        }
                    }

                    rowIndex++;

                }

                mem = new MemoryStream(p.GetAsByteArray());
            }
        }


    }
}