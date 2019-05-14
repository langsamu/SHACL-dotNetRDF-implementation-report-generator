namespace ReportGenerator
{
    using System;
    using System.Linq;
    using VDS.RDF;
    using VDS.RDF.Shacl;
    using VDS.RDF.Shacl.Validation;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var i = 1;

            foreach (var item in TestSuiteData.CoreFullTests.Concat(TestSuiteData.SparqlTests).Select(x => (string)x[0]).ToList())
            {
                Console.Write("{0} ", i++);

                TestSuiteData.ExtractTestData(item, out var testGraph, out var shouldFail, out var dataGraph, out var shapesGraph);

                bool conforms()
                {
                    var report = new ShapesGraph(shapesGraph).Validate(dataGraph);

                    var actual = report.Normalised;
                    var expected = Report.Parse(testGraph).Normalised;

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
