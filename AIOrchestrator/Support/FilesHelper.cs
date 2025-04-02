namespace AIOrchestrator.Support
{
  public static class DatasetFiles
  {
    public const string RecurvMedical = "Recurv-Medical-Dataset.parquet";
  }

  public class FilesHelper
  {
    private static readonly string _projectRootPath = Directory.GetParent(AppContext.BaseDirectory)?.Parent?.Parent?.Parent?.Parent?.FullName!;
    private static readonly string _datasetsFolderName = "Datasets";
    private readonly string _datasetFolderPath;

    public FilesHelper()
    {
      _datasetFolderPath = Path.Combine(_projectRootPath, _datasetsFolderName);
    }

    public string GetDatasetFilePath(string datasetFileName)
    {
      return Path.Combine(_datasetFolderPath, datasetFileName);
    }
  }
}