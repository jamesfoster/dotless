﻿namespace dotless.Core.Parser.Tree
{
    using Infrastructure;
    using Infrastructure.Nodes;
    using Plugins;

    public class Element : Node
    {
        public Combinator Combinator { get; set; }
        public string Value { get; set; }

        public Element(Combinator combinator, string value)
        {
            Combinator = combinator ?? new Combinator("");
            Value = value.Trim();
        }

        public override string ToCSS(Env env)
        {
            return Combinator.ToCSS(env) + Value;
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(Combinator);
        }
    }
}