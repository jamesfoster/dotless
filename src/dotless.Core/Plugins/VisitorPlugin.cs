using System;
using dotless.Core.Parser.Functions;
using dotless.Core.Parser.Infrastructure.Nodes;
using dotless.Core.Parser.Tree;
using dotless.Core.Utils;

namespace dotless.Core.Plugins
{
    public abstract class VisitorPlugin : IPlugin, IVisitor
    {
        public Ruleset Apply(Ruleset tree)
        {
            Visit(tree);

            return tree;
        }

        public void Visit(Node node)
        {
            if (Execute(node))
                node.Accept(this);
        }

        public abstract bool Execute(Node node);
    }
}