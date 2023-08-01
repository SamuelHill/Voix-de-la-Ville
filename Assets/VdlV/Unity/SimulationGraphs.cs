using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TED.Interpreter;
using TED;
using TED.Tables;
using TED.Utilities;
using VdlV.Simulator;
using VdlV.Unity.GraphVisualization;
using VdlV.Utilities;
using VdlV.ValueTypes;

namespace VdlV.Unity {
    using PersonRelationIndex = GeneralIndex<((Person, Person), Person, Person, bool), Person>;
    using static InteractionType;
    using static VitalStatus;
    using static Randomize;
    using static GraphViz<object>;
    using static GraphVisualizer;
    using static GUIManager;
    using static Variables;
    using static VoixDeLaVille;

    public static class SimulationGraphs {

        // ********************************** Visualize Functions *********************************

        public static void VisualizeHomes() {
            var g = new GraphViz<object>();
            foreach ((var person, var place) in Home) {
                var color = PlaceColor(place);
                if (!g.Nodes.Contains(place)) {
                    g.Nodes.Add(place);
                    g.NodeAttributes[place] = new Dictionary<string, object> { { "rgbcolor", color } };
                }
                g.AddEdge(new Edge(person, place, true, null, 
                                   new Dictionary<string, object> { { "rgbcolor", color } }));
            }
            ShowGraph(g);
        }

        public static void VisualizeJobs() {
            var g = new GraphViz<object>();
            foreach (var job in Employment) {
                var place = job.Item3;
                var color = PlaceColor(place);
                if (!g.Nodes.Contains(place)) {
                    g.Nodes.Add(place);
                    g.NodeAttributes[place] = new Dictionary<string, object> { { "rgbcolor", color } };
                }
                g.AddEdge(new Edge(job.Item2, place, true, job.Item1.ToString(),
                                   new Dictionary<string, object> { { "rgbcolor", color } }));
            }
            ShowGraph(g);
        }

        public static void VisualizeRandomFriendNetwork() => VisualizeFriendNetworkOf(
            CharacterAttributes.ColumnValueFromRowNumber(person)(
                (uint)Integer(0, (int)CharacterAttributes.Length)));

        private static GraphViz<TGraph> TraceToDepth<TGraph, T>(int maxDepth, T start, 
            Func<T, IEnumerable<(T neighbor, string label, string color)>> edges) where T : TGraph {
            var g = new GraphViz<TGraph>();

            void Walk(T node, int depth) {
                if (depth > maxDepth || g.Nodes.Contains(node)) return;
                g.AddNode(node);
                foreach (var edge in edges(node)) {
                    Walk(edge.neighbor, depth + 1);
                    g.AddEdge(new GraphViz<TGraph>.Edge(node, edge.neighbor, true, edge.label,
                                                        new Dictionary<string, object> { { "color", edge.color } }));
                }
            }

            Walk(start, 0);
            return g;
        }

        private static void VisualizeFriendNetworkOf(Person p) {
            // ReSharper disable InconsistentNaming
            var FriendIndex = (PersonRelationIndex)Friend.IndexFor(person, false);
            var EnemyIndex = (PersonRelationIndex)Enemy.IndexFor(person, false);
            var RomanticInterestIndex = (PersonRelationIndex)Romantic.IndexFor(person, false);

            IEnumerable<(Person, string, string)> FriendsOf(PersonRelationIndex friendIndex)
                => friendIndex.RowsMatching(p).Select(r => (r.Item3, (string)null, "green"));
            IEnumerable<(Person, string, string)> EnemiesOf(PersonRelationIndex enemyIndex)
                => enemyIndex.RowsMatching(p).Select(r => (r.Item3, (string)null, "red"));
            IEnumerable<(Person, string, string)> RomanticInterestsOf(PersonRelationIndex romanticInterestIndex)
                => romanticInterestIndex.RowsMatching(p).Select(r => (r.Item3, (string)null, "blue"));
            IEnumerable<(Person, string, string)> ConnectionsOf(Person person) =>
                FriendsOf(FriendIndex).Concat(EnemiesOf(EnemyIndex)).Concat(RomanticInterestsOf(RomanticInterestIndex));

            var g = TraceToDepth<object, Person>(1, p, ConnectionsOf);
            var people = g.Nodes.Cast<Person>().ToArray();
            foreach (var p2 in people) {
                if (!EmploymentIndex.ContainsKey(p2)) continue;
                (var job, _, var company, _) = EmploymentIndex[p2];
                var jobColor = PlaceColor(company);
                if (!g.Nodes.Contains(company)) {
                    g.AddNode(company);
                    g.NodeAttributes[company] = new Dictionary<string, object> { { "rgbcolor", jobColor } };
                }
                g.AddEdge(new Edge(p2, company, true, job.ToString(),
                                   new Dictionary<string, object> { { "rgbcolor", jobColor } }));
            }
            ShowGraph(g);
            // ReSharper restore InconsistentNaming
        }

        public static void VisualizeFullSocialNetwork() {
            var g = new GraphViz<object>();
            foreach (var r in Friend)
                g.AddEdge(new Edge(r.Item2, r.Item3, true, null,
                    new Dictionary<string, object> { { "color", "green" } }));
            foreach (var r in Enemy)
                g.AddEdge(new Edge(r.Item2, r.Item3, true, null,
                    new Dictionary<string, object> { { "color", "red" } }));
            foreach (var r in Romantic)
                g.AddEdge(new Edge(r.Item2, r.Item3, true, null,
                    new Dictionary<string, object> { { "color", "blue" } }));
            ShowGraph(g);
        }

        public static void VisualizeFamilies() {
            var g = new GraphViz<Person>();
            foreach ((var parent, var child) in Parent) {
                if (!g.Nodes.Contains(parent)) g.AddNode(parent);
                if (!g.Nodes.Contains(child)) g.AddNode(child);
                g.AddEdge(new GraphViz<Person>.Edge(child, parent));
            }
            ShowGraph(g);
        }

        public static void VisualizeInteractions() {
            var g = new GraphViz<Person>();
            foreach (var row in Interaction)
                g.AddEdge(new GraphViz<Person>.Edge(
                    row.Item1, row.Item2, true, row.Item3.ToString(),
                    new Dictionary<string, object> { { "color", row.Item3 switch {
                        Empathizing => "purple",
                        Assisting => "green",
                        Complimenting => "green4",
                        Chatting => "white",
                        Insulting => "yellow",
                        Arguing => "orange",
                        Fighting => "red",
                        Dueling => "violet",
                        Flirting => "blue",
                        Negging => "cornflowerblue",
                        Snogging => "aquamarine",
                        _ => "black"
                    } } }));
            ShowGraph(g);
        }

        public static void VisualizeWhereTheyAt() {
            var g = new GraphViz<object>();
            foreach (var row in WhereTheyAt) {
                var place = row.Item3;
                var color = PlaceColor(place);
                if (!g.Nodes.Contains(place)) {
                    g.Nodes.Add(place);
                    g.NodeAttributes[place] = new Dictionary<string, object> { { "rgbcolor", color } };
                }
                g.AddEdge(new Edge(row.Item1, place, true, row.Item2.ToString(),
                    new Dictionary<string, object> { { "rgbcolor", color } }));
            }
            ShowGraph(g);
        }

        // ************************************* Descriptions *************************************

        public static void SetDescriptionMethods() {
            Graph.SetDescriptionMethod<TablePredicate>(TableDescription);
            Graph.SetDescriptionMethod<Person>(PersonDescription);
        }

        private static string TableDescription(TablePredicate p) {
            var b = new StringBuilder();
            b.Append("<b>");
            b.AppendLine(p.DefaultGoal.ToString().Replace("[", "</b>["));
            b.AppendFormat("{0} rows\n", p.Length);
            b.Append("<size=16>");
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (p.UpdateMode) {
                case UpdateMode.BaseTable:
                    b.Append("Base table");
                    break;
                case UpdateMode.Operator:
                    b.Append("Operator result");
                    break;
                default:
                    if (p.Rules != null)
                        foreach (var r in p.Rules) b.AppendLine(r.ToString());
                    break;
            }
            return b.ToString();
        }

        private static string PersonDescription(Person p) {
            var b = new StringBuilder();
            var info = CharacterAttributes.KeyIndex(person)[p];
            var dead = info.Item6 == Dead;
            var living = dead ? "Dead" : "Living";
            var job = "Unemployed";
            if (EmploymentIndex.ContainsKey(p)) job = EmploymentIndex[p].Item1.ToString();
            b.Append(dead ? "<color=grey>" : "");
            b.Append("<b>");
            b.Append(p.FullName);
            b.AppendLine("</b><size=24>");
            b.AppendFormat("{0} {1}, age: {2}\n", living, info.Item4.ToString().ToLower(), info.Item2);
            b.AppendLine(info.Item5.ToString());
            b.AppendLine(job);
            return b.ToString();
        }
    }
}
