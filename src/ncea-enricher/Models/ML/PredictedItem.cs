namespace Ncea.Enricher.Models.ML;

public class PredictedItem
{
    public PredictedItem(string code, string originalValue)
    {
        Code = code;
        OriginalValue = originalValue;
    }

    public string Code { get; }
    public string OriginalValue { get; }
}
