using System.Text;
using OfficeOpenXml;
using OfficeOpenXml.Table;

namespace OzonLive;

public class Writer
{
    public async Task WriteToExcel(string nameFolder, string nameFile, List<OutputData> outputDatas)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var path = $"{Environment.CurrentDirectory}\\output\\{nameFolder}";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        
        ExcelPackage pck = new ExcelPackage();
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        
        var wsEnum = pck.Workbook.Worksheets.Add("Goods");
        wsEnum.Cells["A1"].LoadFromCollection(outputDatas, true, TableStyles.Medium9);
        wsEnum.Cells[wsEnum.Dimension.Address].AutoFitColumns();

        var fi = new FileInfo($"{path}\\{nameFile}_{DateTime.Now.Hour}_{DateTime.Now.Minute}_{DateTime.Now.Second}.xlsx");
        if (fi.Exists)
        {
            fi.Delete();
        }
        await pck.SaveAsAsync(fi);
    }
}