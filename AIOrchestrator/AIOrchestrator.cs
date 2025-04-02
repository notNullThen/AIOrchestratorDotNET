using AIOrchestrator.Support;

namespace AIOrchestrator
{

  public class AIOrchestrator
  {
    private readonly ChatHandler _chatHandler = new();
    private readonly FilesHelper _filesHelper = new();
    private readonly ParquetReaderHelper _parquetReaderHelper = new();
    public async Task StartChatAsync()
    {
      string datasetFilePath = _filesHelper.GetDatasetFilePath(DatasetFiles.RecurvMedical);
      string datasetString = await _parquetReaderHelper.ReadParquetAsString(datasetFilePath);

      await _chatHandler.ConversationHandlerAsync();
    }
  }
}