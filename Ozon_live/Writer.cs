using System.Globalization;
using System.Net.Mime;
using System.Text;
using CsvHelper;

namespace Ozon_live;

public class Writer
{
    public async Task<bool> WriteToExcell(string nameFile, List<OutputData> outputDatas)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var path = $"F:\\output\\";
        if(!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        await using var writer = new StreamWriter($"{path}\\{nameFile}.csv", true, Encoding.GetEncoding("windows-1251"));
        
        await using var csvWriter = new CsvWriter(writer, CultureInfo.CurrentCulture);

        await csvWriter.WriteRecordsAsync(outputDatas);
        
        return true;
    }
}