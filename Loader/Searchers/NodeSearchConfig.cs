using System.Collections.Generic;

namespace Loader.Searchers
{
    public class NodeSearchConfig
    {
        public string InputSource { get; set; }
        public string TraverseStrategy { get; set; } = "depth";
        public string TargetNode { get; set; }
        public Dictionary<string, string> Targets { get; set; } = new Dictionary<string, string>();
        public string OutputFile { get; set; }

        public bool HasTargetNode() => string.IsNullOrEmpty(TargetNode);
        public bool HasTargets() => Targets == null || Targets.Count == 0;
        public bool HasAllSearchParams() => HasTargetNode() && HasTargets();
        public bool HasOutputFile() => !string.IsNullOrEmpty(OutputFile);
    }
}
