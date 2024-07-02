using Microsoft.Extensions.ML;
using Microsoft.Extensions.Primitives;
using Microsoft.ML;

namespace Ncea.Enricher.Utils;

public class AzureMLModelLoader : ModelLoader
{
    private IServiceProvider _provider;
    public AzureMLModelLoader(IServiceProvider provider)
    {
        _provider = provider;
    }

    public override IChangeToken GetReloadToken()
    {
        //do azure stuff here
        throw new NotImplementedException();
    }

    public override ITransformer GetModel()
    {
        //do azure stuff here
        throw new NotImplementedException();
    }
}