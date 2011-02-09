namespace dotless.Core.Parser.Tree
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Infrastructure;
    using Infrastructure.Nodes;
    using Utils;
    using Plugins;

    public class Ruleset : Node
    {
        public NodeList<Selector> Selectors { get; set; }
        public List<Node> Rules { get; set; }

        private Dictionary<string, List<Closure>> _lookups;
        private Dictionary<string, Rule> _variables;


        public Ruleset(NodeList<Selector> selectors, List<Node> rules)
            : this()
        {
            Selectors = selectors;
            Rules = rules;
        }

        protected Ruleset()
        {
            _lookups = new Dictionary<string, List<Closure>>();
        }

        public Rule Variable(string name, Node startNode)
        {
            return Rules
                .TakeWhile(r => r != startNode)
                .OfType<Rule>()
                .Where(r => r.Variable)
                .Reverse()
                .FirstOrDefault(r => r.Name == name);
        }

        public List<Ruleset> Rulesets()
        {
            return Rules.OfType<Ruleset>().ToList();
        }

        public List<Closure> Find(Env env, Selector selector, Ruleset self)
        {
            self = self ?? this;
            var rules = new List<Closure>();
            var key = selector.ToCSS(env);

            if (_lookups.ContainsKey(key))
                return _lookups[key];

            foreach (var rule in Rulesets().Where(rule => rule != self))
            {
                if (rule.Selectors && rule.Selectors.Any(selector.Match))
                {
                    if (selector.Elements.Count > 1)
                    {
                        var remainingSelectors = new Selector(new NodeList<Element>(selector.Elements.Skip(1)));
                        var closures = rule.Find(env, remainingSelectors, self);

                        foreach (var closure in closures)
                        {
                            closure.Context.Insert(0, rule);
                        }

                        rules.AddRange(closures);
                    }
                    else
                        rules.Add(new Closure { Ruleset = rule, Context = new List<Ruleset> { rule } });
                }
            }
            return _lookups[key] = rules;
        }

        public virtual bool MatchArguements(NodeList<Expression> arguements, Env env)
        {
            return arguements == null || arguements.Count == 0;
        }

        public override Node Evaluate(Env env)
        {
            if (this is Root)
            {
                env = env ?? new Env();

                NodeHelper.ExpandNodes<Import>(env, this.Rules);
            }

            EvaluateRules(env);

            return this;
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(Selectors);

            Rules.ForEach(visitor.Visit);
        }

        public List<Node> EvaluateRules(Env env)
        {
            env.Frames.Push(this);

            NodeHelper.ExpandNodes<MixinCall>(env, this.Rules);

            for (var i = 0; i < Rules.Count; i++)
            {
                Rules[i] = Rules[i].Evaluate(env);
            }

            env.Frames.Pop();

            return Rules;
        }

        public string ToCSS()
        {
            return ToCSS(new Env());
        }

        public override string ToCSS(Env env)
        {
            if (!Rules.Any())
                return "";

            Evaluate(env);

            var css = ToCSS(env, new Context());

            if (env.Compress)
                return Regex.Replace(css, @"(\s)+", " ");

            return css;
        }

        protected virtual string ToCSS(Env env, Context context)
        {
            var css = new List<string>(); // The CSS output
            var rules = new List<string>(); // node.Rule instances
            var rulesets = new List<string>(); // node.Ruleset instances
            var paths = new Context(); // Current selectors

            if (!(this is Root))
            {
                paths.AppendSelectors(context, Selectors);
            }

            foreach (var rule in Rules)
            {
                if (rule.IgnoreOutput())
                    continue;

                if(rule is Comment && ((Comment)rule).Silent)
                    continue;

                if (rule is Ruleset)
                {
                    var ruleset = (Ruleset) rule;
                    rulesets.Add(ruleset.ToCSS(env, paths));
                }
                else if (rule is Rule)
                {
                    var r = (rule as Rule);

                    if (!r.Variable)
                        rules.Add(r.ToCSS(env));
                }
                else
                {
                  if (this is Root)
                    rulesets.Add(rule.ToCSS(env));
                  else
                    rules.Add(rule.ToCSS(env));
                }
            }

            var rulesetsStr = rulesets.JoinStrings("");

            // If this is the root node, we don't render
            // a selector, or {}.
            // Otherwise, only output if this ruleset has rules.
            if (this is Root)
                css.Add(rules.JoinStrings(env.Compress ? "" : "\n"));
            else
            {
                if (rules.Count > 0)
                {
                    css.Add(paths.ToCSS(env));

                    css.Add(
                        (env.Compress ? "{" : " {\n  ") +
                        rules.JoinStrings(env.Compress ? "" : "\n  ") +
                        (env.Compress ? "}" : "\n}\n"));
                }
            }
            css.Add(rulesetsStr);

            return css.JoinStrings("");
        }

        public override string ToString()
        {
            var format = "{0}{{{1}}}";
            return Selectors != null && Selectors.Count > 0
                       ? string.Format(format, Selectors.Select(s => s.ToCSS(new Env())).JoinStrings(""), Rules.Count)
                       : string.Format(format, "*", Rules.Count);
        }
    }
}