namespace ReportGenerator
{
    using VDS.RDF;
    using VDS.RDF.Shacl;
    using VDS.RDF.Writing;

    internal class Program
    {
        private static void Main(string[] args)
        {
            ImplementationReport.Generate().SaveToFile(@"..\..\..\..\dotnetrdf-shacl-earl.ttl", new CompressingTurtleWriter());
        }
    }
}
