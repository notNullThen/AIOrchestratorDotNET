namespace AIOrchestrator.Support
{
  using System.Text;
  using Parquet;
  public class ParquetReaderHelper
  {
    private const string _separator = "\n\n\n\n\n\n";

    public async Task<string> ReadParquetAsString(string filePath)
    {
      var options = new ParquetOptions { TreatByteArrayAsString = true };

      using (Stream fileStream = File.OpenRead(filePath))
      {
        using (var reader = await ParquetReader.CreateAsync(fileStream))
        {
          StringBuilder stringBuilder = new();
          for (int i = 0; i < reader.RowGroupCount; i++)
          {
            var group = await reader.ReadEntireRowGroupAsync(i);
            foreach (var column in group)
            {
              stringBuilder.AppendLine(string.Join(", ", ((IEnumerable<object>)column.Data).Select(d => d?.ToString() + _separator)));
            }
          }

          return stringBuilder.ToString();
        }
      }
    }
  }
}