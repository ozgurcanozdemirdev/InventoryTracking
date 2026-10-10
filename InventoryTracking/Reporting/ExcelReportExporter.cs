using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;

namespace InventoryTracking.Reporting
{
  internal static class ExcelReportExporter
  {
    private const string SpreadsheetNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

    public static void Write(string fileName, string title, string filter, string totals,
      string[] headers, IList<object[]> rows)
    {
      if (headers == null || headers.Length == 0 || headers.Length > 16384 || rows.Count > 1048572)
        throw new ArgumentException("Invalid Excel report size.");

      // .xlsx, XML parçalarından oluşan bir ZIP paketidir. Framework kitaplıkları
      // yeterlidir; kullanıcının bilgisayarında Excel veya NuGet paketi gerekmez.
      // Önce geçici dosyayı tamamlayıp sonra hedefe alırız; hata eski raporu bozmaz.
      string target = Path.GetFullPath(fileName);
      string temporary = Path.Combine(Path.GetDirectoryName(target), ".report-" + Guid.NewGuid().ToString("N") + ".tmp");
      try
      {
        using (var archive = ZipFile.Open(temporary, ZipArchiveMode.Create))
        {
          TextEntry(archive, "[Content_Types].xml", @"<?xml version=""1.0"" encoding=""utf-8""?>
<Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types""><Default Extension=""rels"" ContentType=""application/vnd.openxmlformats-package.relationships+xml""/><Default Extension=""xml"" ContentType=""application/xml""/><Override PartName=""/xl/workbook.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml""/><Override PartName=""/xl/worksheets/sheet1.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml""/><Override PartName=""/xl/styles.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml""/></Types>");
          TextEntry(archive, "_rels/.rels", @"<?xml version=""1.0"" encoding=""utf-8""?><Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships""><Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"" Target=""xl/workbook.xml""/></Relationships>");
          TextEntry(archive, "xl/workbook.xml", @"<?xml version=""1.0"" encoding=""utf-8""?><workbook xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"" xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships""><sheets><sheet name=""Report"" sheetId=""1"" r:id=""rId1""/></sheets></workbook>");
          TextEntry(archive, "xl/_rels/workbook.xml.rels", @"<?xml version=""1.0"" encoding=""utf-8""?><Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships""><Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"" Target=""worksheets/sheet1.xml""/><Relationship Id=""rId2"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles"" Target=""styles.xml""/></Relationships>");
          TextEntry(archive, "xl/styles.xml", Styles);
          using (var stream = archive.CreateEntry("xl/worksheets/sheet1.xml").Open())
          using (var writer = XmlWriter.Create(stream, new XmlWriterSettings { Encoding = new UTF8Encoding(false) }))
            WriteSheet(writer, title, filter, totals, headers, rows);
        }
        if (File.Exists(target)) File.Replace(temporary, target, null);
        else File.Move(temporary, target);
      }
      finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    private static void TextEntry(ZipArchive archive, string name, string content)
    {
      using (var writer = new StreamWriter(archive.CreateEntry(name).Open(), new UTF8Encoding(false)))
        writer.Write(content);
    }

    private static void WriteSheet(XmlWriter writer, string title, string filter, string totals,
      string[] headers, IList<object[]> rows)
    {
      string lastColumn = ColumnName(headers.Length);
      writer.WriteStartDocument(); writer.WriteStartElement("worksheet", SpreadsheetNs);
      writer.WriteStartElement("dimension"); writer.WriteAttributeString("ref", "A1:" + lastColumn + (rows.Count + 4)); writer.WriteEndElement();
      writer.WriteStartElement("sheetViews"); writer.WriteStartElement("sheetView"); writer.WriteAttributeString("workbookViewId", "0");
      writer.WriteStartElement("pane"); writer.WriteAttributeString("ySplit", "4"); writer.WriteAttributeString("topLeftCell", "A5"); writer.WriteAttributeString("activePane", "bottomLeft"); writer.WriteAttributeString("state", "frozen"); writer.WriteEndElement();
      writer.WriteEndElement(); writer.WriteEndElement();
      writer.WriteStartElement("sheetFormatPr"); writer.WriteAttributeString("defaultRowHeight", "22"); writer.WriteEndElement();
      writer.WriteStartElement("cols");
      for (int col = 0; col < headers.Length; col++)
      {
        int longest = Math.Max(headers[col].Length, rows.Take(150).Select(r => r[col]?.ToString()?.Length ?? 0).DefaultIfEmpty(0).Max());
        writer.WriteStartElement("col"); writer.WriteAttributeString("min", (col + 1).ToString()); writer.WriteAttributeString("max", (col + 1).ToString());
        writer.WriteAttributeString("width", Math.Min(45, Math.Max(18, longest + 3)).ToString()); writer.WriteAttributeString("customWidth", "1"); writer.WriteEndElement();
      }
      writer.WriteEndElement(); writer.WriteStartElement("sheetData");
      MetaRow(writer, 1, title, 4, 30); MetaRow(writer, 2, filter, 5, 38); MetaRow(writer, 3, totals, 5, 48);
      writer.WriteStartElement("row"); writer.WriteAttributeString("r", "4"); writer.WriteAttributeString("ht", "36"); writer.WriteAttributeString("customHeight", "1");
      for (int col = 0; col < headers.Length; col++) Cell(writer, ColumnName(col + 1) + "4", headers[col], 1);
      writer.WriteEndElement();
      for (int row = 0; row < rows.Count; row++)
      {
        writer.WriteStartElement("row"); writer.WriteAttributeString("r", (row + 5).ToString());
        for (int col = 0; col < headers.Length; col++)
        {
          object value = rows[row][col];
          int style = value is DateTime ? 3 : value is decimal || value is double || value is float ? 2 : IsNumber(value) ? 6 : 0;
          Cell(writer, ColumnName(col + 1) + (row + 5), value, style);
        }
        writer.WriteEndElement();
      }
      writer.WriteEndElement();
      writer.WriteStartElement("autoFilter"); writer.WriteAttributeString("ref", "A4:" + lastColumn + (rows.Count + 4)); writer.WriteEndElement();
      writer.WriteStartElement("mergeCells"); writer.WriteAttributeString("count", "3");
      for (int row = 1; row <= 3; row++) { writer.WriteStartElement("mergeCell"); writer.WriteAttributeString("ref", "A" + row + ":" + lastColumn + row); writer.WriteEndElement(); }
      writer.WriteEndElement();
      writer.WriteStartElement("pageMargins"); foreach (string side in new[] { "left", "right", "top", "bottom", "header", "footer" }) writer.WriteAttributeString(side, "0.3"); writer.WriteEndElement();
      writer.WriteStartElement("pageSetup"); writer.WriteAttributeString("orientation", "landscape"); writer.WriteAttributeString("paperSize", "9"); writer.WriteAttributeString("fitToWidth", "1"); writer.WriteAttributeString("fitToHeight", "0"); writer.WriteEndElement();
      writer.WriteEndElement(); writer.WriteEndDocument();
    }

    private static void MetaRow(XmlWriter writer, int row, string value, int style, int height)
    {
      writer.WriteStartElement("row"); writer.WriteAttributeString("r", row.ToString()); writer.WriteAttributeString("ht", height.ToString()); writer.WriteAttributeString("customHeight", "1");
      Cell(writer, "A" + row, value, style); writer.WriteEndElement();
    }

    private static bool IsNumber(object value) => value is byte || value is short || value is int || value is long ||
      value is ushort || value is uint || value is ulong || value is decimal || value is double || value is float;

    private static void Cell(XmlWriter writer, string address, object value, int style)
    {
      writer.WriteStartElement("c"); writer.WriteAttributeString("r", address); writer.WriteAttributeString("s", style.ToString());
      if (value is DateTime || IsNumber(value))
      {
        writer.WriteElementString("v", value is DateTime ? ((DateTime)value).ToOADate().ToString("R", CultureInfo.InvariantCulture) : Convert.ToString(value, CultureInfo.InvariantCulture));
      }
      else
      {
        // Kullanıcının = ile başlayan ürün adı bile metindir; Excel formülü değildir.
        writer.WriteAttributeString("t", "inlineStr"); writer.WriteStartElement("is"); writer.WriteStartElement("t");
        writer.WriteAttributeString("xml", "space", "http://www.w3.org/XML/1998/namespace", "preserve");
        writer.WriteString(CleanXml(value?.ToString() ?? "")); writer.WriteEndElement(); writer.WriteEndElement();
      }
      writer.WriteEndElement();
    }

    private static string CleanXml(string text)
    {
      var result = new StringBuilder();
      for (int i = 0; i < text.Length; i++)
      {
        if (XmlConvert.IsXmlChar(text[i])) result.Append(text[i]);
        else if (char.IsHighSurrogate(text[i]) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
        { result.Append(text[i]); result.Append(text[++i]); }
      }
      return result.ToString();
    }

    private static string ColumnName(int column)
    {
      string result = "";
      while (column > 0) { column--; result = (char)('A' + column % 26) + result; column /= 26; }
      return result;
    }

    private const string Styles = @"<?xml version=""1.0"" encoding=""utf-8""?>
<styleSheet xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"">
<numFmts count=""2""><numFmt numFmtId=""164"" formatCode=""#,##0.00 &quot;TRY&quot;""/><numFmt numFmtId=""165"" formatCode=""dd.mm.yyyy hh:mm:ss""/></numFmts>
<fonts count=""3""><font><sz val=""11""/><name val=""Calibri""/></font><font><b/><color rgb=""FFFFFFFF""/><sz val=""11""/><name val=""Calibri""/></font><font><b/><sz val=""16""/><color rgb=""FF2980B9""/><name val=""Calibri""/></font></fonts>
<fills count=""3""><fill><patternFill patternType=""none""/></fill><fill><patternFill patternType=""gray125""/></fill><fill><patternFill patternType=""solid""><fgColor rgb=""FF2980B9""/><bgColor indexed=""64""/></patternFill></fill></fills>
<borders count=""1""><border><left/><right/><top/><bottom/><diagonal/></border></borders><cellStyleXfs count=""1""><xf numFmtId=""0"" fontId=""0"" fillId=""0"" borderId=""0""/></cellStyleXfs>
<cellXfs count=""7""><xf numFmtId=""0"" fontId=""0"" fillId=""0"" borderId=""0"" xfId=""0""/><xf numFmtId=""0"" fontId=""1"" fillId=""2"" borderId=""0"" xfId=""0"" applyAlignment=""1""><alignment vertical=""center"" wrapText=""1""/></xf><xf numFmtId=""164"" fontId=""0"" fillId=""0"" borderId=""0"" xfId=""0"" applyNumberFormat=""1""/><xf numFmtId=""165"" fontId=""0"" fillId=""0"" borderId=""0"" xfId=""0"" applyNumberFormat=""1""/><xf numFmtId=""0"" fontId=""2"" fillId=""0"" borderId=""0"" xfId=""0""/><xf numFmtId=""0"" fontId=""0"" fillId=""0"" borderId=""0"" xfId=""0"" applyAlignment=""1""><alignment vertical=""center"" wrapText=""1""/></xf><xf numFmtId=""3"" fontId=""0"" fillId=""0"" borderId=""0"" xfId=""0"" applyNumberFormat=""1""/></cellXfs>
<cellStyles count=""1""><cellStyle name=""Normal"" xfId=""0"" builtinId=""0""/></cellStyles></styleSheet>";
  }
}
