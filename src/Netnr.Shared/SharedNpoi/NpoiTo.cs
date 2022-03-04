#if Full || Npoi

using System;
using System.Data;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Formula.Eval;
using System.Collections.Generic;
using NPOI.XSSF.UserModel;

namespace Netnr.SharedNpoi
{
    /// <summary>
    /// NPOI操作Excel
    /// </summary>
    public class NpoiTo
    {
        /// <summary>
        /// DataTable生成Excel
        /// </summary>
        /// <param name="dt">数据表</param>
        /// <param name="fullPathName">物理路径 + 文件名称 + 格式</param>
        /// <param name="isAutoSizeColumn">是否自适应列宽，默认否，开启自适应列宽需要成倍的时间</param>
        /// <returns>返回生成状态</returns>
        public static bool DataTableToExcel(DataTable dt, string fullPathName, bool isAutoSizeColumn = false)
        {
            var dc = new Dictionary<string, DataTable>() { { "Sheet1", dt } };
            return DataTableToExcel(dc, fullPathName, isAutoSizeColumn);
        }

        /// <summary>
        /// 导出多个工作簿
        /// </summary>
        /// <param name="dicSheet">工作簿名：数据表</param>
        /// <param name="fullPathName">物理路径 + 文件名称 + 格式</param>
        /// <param name="isAutoSizeColumn">是否自适应列宽，默认否，开启自适应列宽需要成倍的时间</param>
        /// <returns></returns>
        public static bool DataTableToExcel(Dictionary<string, DataTable> dicSheet, string fullPathName, bool isAutoSizeColumn = false)
        {
            try
            {
                IWorkbook workbook = new HSSFWorkbook();

                if (fullPathName.ToLower().Contains(".xlsx"))
                    workbook = new XSSFWorkbook();

                foreach (var sheetitem in dicSheet.Keys)
                {
                    var dt = dicSheet[sheetitem];

                    ISheet sheet = workbook.CreateSheet(sheetitem);

                    //标题样式
                    ICellStyle hStyle = workbook.CreateCellStyle();
                    hStyle.BorderBottom = BorderStyle.Thin;
                    hStyle.BorderLeft = BorderStyle.Thin;
                    hStyle.BorderRight = BorderStyle.Thin;
                    hStyle.BorderTop = BorderStyle.Thin;
                    //水平垂直居中
                    hStyle.Alignment = HorizontalAlignment.Center;
                    hStyle.VerticalAlignment = VerticalAlignment.Center;
                    //背景颜色
                    hStyle.FillForegroundColor = 9;
                    hStyle.FillPattern = FillPattern.SolidForeground;

                    ////用column name 作为列名
                    int icolIndex = 0;
                    IRow headerRow = sheet.CreateRow(0);
                    headerRow.Height = 20 * 18;
                    foreach (DataColumn item in dt.Columns)
                    {
                        ICell cell = headerRow.CreateCell(icolIndex);
                        cell.SetCellValue(item.ColumnName);

                        //单元格字体
                        IFont font = workbook.CreateFont();
                        font.FontHeightInPoints = 10;
                        font.Color = 8;
                        font.IsBold = true;

                        hStyle.SetFont(font);

                        cell.CellStyle = hStyle;
                        icolIndex++;
                    }

                    //单元格样式
                    ICellStyle cellStyle = workbook.CreateCellStyle();

                    //创建CellStyle与DataFormat并加载格式样式
                    IDataFormat dataformat = workbook.CreateDataFormat();
                    //为避免日期格式被Excel自动替换，所以设定 format 为 『@』 表示一率当成text來看
                    cellStyle.DataFormat = dataformat.GetFormat("@");
                    cellStyle.BorderBottom = BorderStyle.Thin;
                    cellStyle.BorderLeft = BorderStyle.Thin;
                    cellStyle.BorderRight = BorderStyle.Thin;
                    cellStyle.BorderTop = BorderStyle.Thin;
                    cellStyle.VerticalAlignment = VerticalAlignment.Center;

                    //建立内容行
                    int iRowIndex = 1;
                    foreach (DataRow Rowitem in dt.Rows)
                    {
                        IRow DataRow = sheet.CreateRow(iRowIndex++);
                        DataRow.Height = 20 * 14;
                        int iCellIndex = 0;
                        foreach (DataColumn Colitem in dt.Columns)
                        {
                            ICell cell = DataRow.CreateCell(iCellIndex++);
                            cell.SetCellValue(Rowitem[Colitem].ToString());
                            cell.CellStyle = cellStyle;
                        }
                    }

                    //自适应列宽度（开启自适应列宽 或 少量数据行时自动开启）
                    //随着数据的增加需要的时间会越来越久
                    if (isAutoSizeColumn || dt.Rows.Count < 999)
                    {
                        for (int i = 0; i < icolIndex; i++)
                        {
                            sheet.AutoSizeColumn(i);
                        }
                    }
                }

                //写Excel
                using (FileStream file = new(fullPathName, FileMode.OpenOrCreate))
                {
                    workbook.Write(file);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return false;
            }
        }

        /// <summary>
        /// 读取Excel为DataTable
        /// </summary>
        /// <param name="workbook"></param>
        /// <param name="iSheetIndex">工作薄索引</param>
        /// <param name="extName">文件格式后缀</param>
        /// <param name="skipRow">跳过指定行开始读取，默认0</param>
        /// <returns></returns>
        public static DataTable ExcelToDataTable(IWorkbook workbook, int iSheetIndex, string extName, int skipRow = 0)
        {
            DataTable dt = new();

            ISheet sheet = workbook.GetSheetAt(iSheetIndex);

            //列头
            foreach (ICell item in sheet.GetRow(sheet.FirstRowNum + skipRow).Cells)
            {
                _ = dt.Columns.Add(item.ToString(), typeof(string));
            }

            //写入内容
            System.Collections.IEnumerator rows = sheet.GetRowEnumerator();
            while (rows.MoveNext())
            {
                IRow row = null;
                if (extName.Equals(".xls"))
                {
                    row = (HSSFRow)rows.Current;
                }
                if (extName.Equals(".xlsx"))
                {
                    row = (XSSFRow)rows.Current;
                }
                if (row.RowNum <= sheet.FirstRowNum + skipRow)
                {
                    continue;
                }

                DataRow dr = dt.NewRow();
                foreach (ICell item in row.Cells)
                {
                    switch (item.CellType)
                    {
                        case CellType.Boolean:
                            dr[item.ColumnIndex] = item.BooleanCellValue;
                            break;
                        case CellType.Error:
                            dr[item.ColumnIndex] = ErrorEval.GetText(item.ErrorCellValue);
                            break;
                        case CellType.Formula:
                            switch (item.CachedFormulaResultType)
                            {
                                case CellType.Boolean:
                                    dr[item.ColumnIndex] = item.BooleanCellValue;
                                    break;
                                case CellType.Error:
                                    dr[item.ColumnIndex] = ErrorEval.GetText(item.ErrorCellValue);
                                    break;
                                case CellType.Numeric:
                                    if (DateUtil.IsCellDateFormatted(item))
                                    {
                                        dr[item.ColumnIndex] = item.DateCellValue.ToString("yyyy-MM-dd hh:MM:ss");
                                    }
                                    else
                                    {
                                        dr[item.ColumnIndex] = item.NumericCellValue;
                                    }
                                    break;
                                case CellType.String:
                                    string str = item.StringCellValue;
                                    if (!string.IsNullOrEmpty(str))
                                    {
                                        dr[item.ColumnIndex] = str.ToString();
                                    }
                                    else
                                    {
                                        dr[item.ColumnIndex] = null;
                                    }
                                    break;
                                case CellType.Unknown:
                                case CellType.Blank:
                                default:
                                    dr[item.ColumnIndex] = string.Empty;
                                    break;
                            }
                            break;
                        case CellType.Numeric:
                            if (DateUtil.IsCellDateFormatted(item))
                            {
                                dr[item.ColumnIndex] = item.DateCellValue.ToString("yyyy-MM-dd hh:MM:ss");
                            }
                            else
                            {
                                dr[item.ColumnIndex] = item.NumericCellValue;
                            }
                            break;
                        case CellType.String:
                            string strValue = item.StringCellValue;
                            if (!string.IsNullOrEmpty(strValue))
                            {
                                dr[item.ColumnIndex] = strValue.ToString();
                            }
                            else
                            {
                                dr[item.ColumnIndex] = null;
                            }
                            break;
                        case CellType.Unknown:
                        case CellType.Blank:
                        default:
                            dr[item.ColumnIndex] = string.Empty;
                            break;
                    }
                }
                dt.Rows.Add(dr);
            }

            return dt;
        }

        /// <summary>
        /// 读取Excel为DataTable
        /// </summary>
        /// <param name="fullPathName">Excel文件目录地址</param>
        /// <param name="iSheetIndex">Excel sheet index</param>
        /// <param name="skipRow">跳过指定行开始读取，默认0</param>
        /// <returns></returns>
        public static DataTable ExcelToDataTable(string fullPathName, int iSheetIndex, int skipRow = 0)
        {
            IWorkbook workbook = null;

            string strExtName = Path.GetExtension(fullPathName);
            using (FileStream file = new(fullPathName, FileMode.Open, FileAccess.Read))
            {
                if (strExtName.Equals(".xls"))
                {
                    workbook = new HSSFWorkbook(file);
                }
                if (strExtName.Equals(".xlsx"))
                {
                    workbook = new XSSFWorkbook(file);
                }
            }

            return ExcelToDataTable(workbook, iSheetIndex, strExtName, skipRow);
        }

        /// <summary>
        /// 读取Excel为DataTable
        /// </summary>
        /// <param name="s">流</param>
        /// <param name="iSheetIndex">工作薄索引</param>
        /// <param name="extName">文件格式后缀</param>
        /// <param name="skipRow">跳过指定行开始读取，默认0</param>
        /// <returns></returns>
        public static DataTable ExcelToDataTable(Stream s, int iSheetIndex, string extName, int skipRow = 0)
        {
            IWorkbook workbook = null;

            if (extName.Equals(".xls"))
            {
                workbook = new HSSFWorkbook(s);
            }
            if (extName.Equals(".xlsx"))
            {
                workbook = new XSSFWorkbook(s);
            }

            return ExcelToDataTable(workbook, iSheetIndex, extName, skipRow);
        }
    }
}

#endif