using dotless.Core.Parser.Infrastructure.Nodes;

namespace dotless.Core.Plugins
{
    public interface IVisitor
    {
        void Visit(Node node);
    }
}