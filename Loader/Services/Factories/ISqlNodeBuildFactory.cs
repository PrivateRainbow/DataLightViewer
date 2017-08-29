using Loader.Components;

namespace Loader.Services.Factories
{
    public interface ISqlNodeBuildFactory
    {
        string BuildDatabase(Node node);
        string BuildTable(Node node);
        string BuildView(Node node);
        string BuildProcedure(Node node);
        string BuildColumn(Node node, bool withParent = false);
        string BuildKey(Node node);
        string BuildConstraint(Node node);
        string BuildIndex(Node node);
        string BuildProcParameter(Node node);
    }
}
