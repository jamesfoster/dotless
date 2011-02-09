﻿namespace dotless.Core.Parser.Tree
{
    using System.Linq;
    using Infrastructure;
    using Infrastructure.Nodes;
    using Utils;
    using Plugins;

    public class Selector : Node
    {
        private string _css;
        public NodeList<Element> Elements { get; set; }

        public Selector(NodeList<Element> elements)
        {
            Elements = elements;

            if (Elements[0].Combinator.Value == "")
                Elements[0].Combinator.Value = " ";
        }

        public bool Match(Selector other)
        {
            return
                other.Elements.Count <= Elements.Count &&
                Elements[0].Value == other.Elements[0].Value;
        }

        public override string ToCSS(Env env)
        {
            if (!string.IsNullOrEmpty(_css))
                return _css;

            return _css = Elements.Select(e => e.ToCSS(env)).JoinStrings("");
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(Elements);
        }

        public override string ToString()
        {
            return ToCSS(new Env());
        }
    }
}