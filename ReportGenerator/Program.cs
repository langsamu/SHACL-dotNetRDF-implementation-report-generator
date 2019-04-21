namespace ReportGenerator
{
    using System;
    using System.Linq;
    using VDS.RDF;
    using VDS.RDF.Parsing;
    using VDS.RDF.Shacl;
    using VDS.RDF.Shacl.Validation;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var i = 1;

            foreach (var item in TestSuiteData.TestNames.ToList())
            {
                Console.Write("{0} ",i++);

                if (item == "core/path/path-complex-002.ttl")
                {
                    Console.WriteLine("{0}=False", item);
                    continue;
                }

                TestSuiteData.ExtractTestData(item, out var testGraph, out var shouldFail, out var dataGraph, out var shapesGraph);

                bool conforms()
                {
                    new ShapesGraph(shapesGraph).Validate(dataGraph, out var report);

                    var actual = ExtractReportGraph(report.Graph);
                    var expected = ExtractReportGraph(testGraph);

                    RemoveUnnecessaryResultMessages(actual, expected);

                    return expected.Equals(actual);
                }

                if (shouldFail)
                {
                    try
                    {
                        conforms();
                    }
                    catch
                    {
                        Console.WriteLine("{0}=True", item);
                        continue;
                    }

                    Console.WriteLine("{0}=False", item);
                }
                else
                {
                    Console.WriteLine("{0}={1}", item, conforms());
                }
            }
        }

        private static IGraph ExtractReportGraph(IGraph g)
        {
            var q = new SparqlQueryParser().ParseFromString(@"
PREFIX sh: <http://www.w3.org/ns/shacl#> 

DESCRIBE ?s
WHERE {
    ?s a sh:ValidationReport .
}
");
            q.Describer = new ReportDescribeAlgorithm();

            return (IGraph)g.ExecuteQuery(q);
        }

        private static void RemoveUnnecessaryResultMessages(IGraph resultReport, IGraph testReport)
        {
            foreach (var t in resultReport.GetTriplesWithPredicate(Vocabulary.ResultMessage).ToList())
            {
                if (!testReport.GetTriplesWithPredicateObject(Vocabulary.ResultMessage, t.Object).Any())
                {
                    resultReport.Retract(t);
                }
            }
        }
    }
}
