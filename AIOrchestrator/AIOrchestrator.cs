using AIOrchestrator.Support;

namespace AIOrchestrator
{

  public class AIOrchestrator
  {
    private readonly ChatHandler _chatHandler = new();
    private readonly FilesHelper _filesHelper = new();
    private readonly ParquetHelper _parquetHelper = new();
    public async Task StartChatAsync()
    {
      string datasetFilePath = _filesHelper.GetDatasetFilePath(DatasetFiles.RecurvMedical);
      string datasetString = await _parquetHelper.ReadParquetAsString(datasetFilePath);

      await _chatHandler.ConversationHandlerAsync();
    }
  }
}