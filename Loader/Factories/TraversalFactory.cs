using System.Collections.Generic;
using System;
using Loader.Traversals;
using Loader.Types;

namespace Loader.Factories
{
    public static class TraversalFactory
    {
        private static readonly Dictionary<TraversalStrategy, INodeTraverser> _traverseStrategies;

        static TraversalFactory()
        {
            _traverseStrategies = new Dictionary<TraversalStrategy, INodeTraverser>
            {
                {TraversalStrategy.Depth, new DepthFirstNodeTraverser() },
                {TraversalStrategy.Breadth, new BreadthFirstNodeTraverser() }
            };
        }

        public static INodeTraverser Make(TraversalStrategy strategy)
        {
            if (!_traverseStrategies.ContainsKey(strategy))
                throw new ArgumentException($" Such {nameof(strategy)} was not expected!");

            return _traverseStrategies[strategy];
        }
    }
}
