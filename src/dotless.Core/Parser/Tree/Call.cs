namespace dotless.Core.Parser.Tree
{
    using System.Linq;
    using Infrastructure;
    using Infrastructure.Nodes;
    using Utils;
    using Plugins;

    public class Call : Node
    {
        public string Name { get; set; }
        public NodeList<Expression> Arguments { get; set; }

        public Call(string name, NodeList<Expression> arguments)
        {
            Name = name;
            Arguments = arguments;
        }

        protected Call()
        {
        }

        public override Node Evaluate(Env env)
        {
            var args = Arguments.Select(a => a.Evaluate(env));

            if (env != null)
            {
                var function = env.GetFunction(Name);

                if (function != null)
                {
                    function.Name = Name;
                    function.Index = Index;
                    return function.Call(env, args);
                }
            }

            return new TextNode(Name + "(" + Arguments.Select(a => a.Evaluate(env).ToCSS(env)).JoinStrings(", ") + ")");
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(Arguments);
        }
    }
}