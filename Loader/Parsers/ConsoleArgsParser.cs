using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using Loader.Searchers;

namespace Loader.Parsers
{
    public sealed class ConsoleArgsParser
    {
        #region Consts

        private const string InFileConfigKey = "InputFile";
        private const string OutFileConfigKey = "OutputFile";
        private const string StrategyConfigKey = "TraverseStrategy";
        private const string NodeConfigKey = "TargetNodeName";
        private const string AttributesConfigKey = "TargetNodeAttributes";

        #endregion

        #region Data

        private string InFile, OutFile, Strategy, Node, Attributes;

        private HashSet<string> _keyTokens;
        private readonly Dictionary<string, string> _parsedTokens;
        private readonly IList<string> _tokens;
        private readonly int _tokensCount;

        private readonly StringBuilder _attrBuilder;
        private string _lastKeyToken;
        private bool _validArgs = true;
        #endregion

        #region Init

        private ConsoleArgsParser()
        {
            _attrBuilder = new StringBuilder(0);
            _parsedTokens = new Dictionary<string, string>();
        }

        public ConsoleArgsParser(string argsTokenSource, NameValueCollection keyTokenCollection)
        {
            if(string.IsNullOrEmpty(argsTokenSource))
                throw new ArgumentException($"{nameof(argsTokenSource)}");


        }

        public ConsoleArgsParser(IList<string> tokens, NameValueCollection keyTokenCollection): this()
        {
            if(keyTokenCollection == null)
                throw new ArgumentException($"{nameof(keyTokenCollection)}");

            if (tokens == null || tokens.Count == 0)
                throw new ArgumentException($"{nameof(tokens)}");

            _tokens = tokens;
            _tokensCount = _tokens.Count;

            InitializeKeyTokens(keyTokenCollection);
        }

        private void InitializeKeyTokens(NameValueCollection keyTokenCollection)
        {
            InFile = keyTokenCollection[InFileConfigKey];
            OutFile = keyTokenCollection[OutFileConfigKey];
            Strategy = keyTokenCollection[StrategyConfigKey];
            Node = keyTokenCollection[NodeConfigKey];
            Attributes = keyTokenCollection[AttributesConfigKey];

            _keyTokens = new HashSet<string>
            {
                InFile,
                OutFile,
                Strategy,
                Node,
                Attributes
            };
        }
        #endregion

        #region Helpers

        public NodeSearchConfig GetSearchConfig()
        {
            var config = new NodeSearchConfig();

            if (_parsedTokens.ContainsKey(InFile))
                config.InputSource = _parsedTokens[InFile];

            if (_parsedTokens.ContainsKey(OutFile))
                config.OutputFile = _parsedTokens[OutFile];

            if (_parsedTokens.ContainsKey(Node))
                config.TargetNode = _parsedTokens[Node];

            if (_parsedTokens.ContainsKey(Strategy))
                config.TraverseStrategy = _parsedTokens[Strategy];

            if (_parsedTokens.ContainsKey(Attributes))
            {
                var pairs = _attrBuilder.ToString().Split(' ');
                foreach (var p in pairs)
                {
                    var current = p.Split('=');
                    if (!config.Targets.ContainsKey(current[0]))
                        config.Targets.Add(current[0], current[1]);
                }
            }

            return config;
        }
        public bool TryParse()
        {
            if (!IsInitialTokenValid()) return false;

            for (var i = 0; i < _tokensCount; i++)
                if (!CanGetToken(_tokens[i], ref i)) break;

            return _validArgs;
        }

        private bool CanGetToken(string curToken, ref int i)
        {
            if (IsKeyToken(curToken) && curToken != Attributes)
            {
                if (!IsKeyTokenSequence(curToken, _tokens[i + 1])
                    && !_parsedTokens.ContainsKey(curToken))
                {
                    _parsedTokens.Add(curToken, _tokens[i + 1]);
                    _lastKeyToken = curToken;
                    i++;
                }
                else
                    _validArgs = false;
            }
            else if (IsKeyToken(curToken) && curToken == Attributes)
            {
                if (!_parsedTokens.ContainsKey(curToken))
                {
                    _parsedTokens.Add(curToken, string.Empty);
                    _lastKeyToken = curToken;
                }
                else
                    _validArgs = false;
            }
            else if (!IsKeyToken(curToken) && _lastKeyToken == Attributes)
            {
                var value = i < _tokensCount - 1 ? curToken + ' ' : curToken;
                _attrBuilder.Append(value);
            }
            else
                _validArgs = false;

            return _validArgs;
        }
        private bool IsKeyToken(string token) => _keyTokens.Contains(token);
        private bool IsInitialTokenValid() => IsKeyToken(_tokens[0]);
        private bool IsKeyTokenSequence(string current, string next) => IsKeyToken(current) && IsKeyToken(next);

        #endregion
    }
}
