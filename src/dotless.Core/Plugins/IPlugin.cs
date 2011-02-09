using dotless.Core.Parser.Tree;

namespace dotless.Core.Plugins
{
    public interface IPlugin
    {
        Ruleset Apply(Ruleset tree);
    }
}