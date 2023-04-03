using System.Globalization;
using System.Net.Mime;
using System.Text;
using CsvHelper;

namespace Ozon_live;

public class Writer
{
    public async Task<bool> WriteToExcell(string nameFolder, string nameFile, List<OutputData> outputDatas)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var path = $"{Environment.CurrentDirectory}\\output\\{nameFolder}";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        await using var writer =
            new StreamWriter($"{path}\\{nameFile}_{DateTime.Now.Hour}_{DateTime.Now.Minute}_{DateTime.Now.Second}.csv",
                true, Encoding.GetEncoding("windows-1251"));
       
        await using var csvWriter = new CsvWriter(writer, CultureInfo.CreateSpecificCulture("ru-RU"));

        csvWriter.WriteRecords(outputDatas);

        return true;
    }
}