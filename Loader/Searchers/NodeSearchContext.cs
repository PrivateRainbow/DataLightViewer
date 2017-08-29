using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Loader.Searchers
{
    public sealed class NodeSearchContext
    {
        public IList<string> InputConsoleArgs { get; }
        public NameValueCollection KeyTokenCollection { get; }

        public NodeSearchContext(IList<string> inputConsoleArgs, NameValueCollection keyArgsNamedCollection)
        {
            if (inputConsoleArgs == null || inputConsoleArgs.Count == 0)
                throw new ArgumentException($"{inputConsoleArgs}");

            if(keyArgsNamedCollection == null || keyArgsNamedCollection.Count == 0)
                throw new ArgumentException($"{inputConsoleArgs}");

            InputConsoleArgs = inputConsoleArgs;
            KeyTokenCollection = keyArgsNamedCollection;
        }
    }
}
