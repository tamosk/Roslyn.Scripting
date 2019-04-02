using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace DynamicType
{
    public class TypeBuilder
    {
        public TypeBuildResult Build(TypeMetadata meta, string[] namespaces, MetadataReference[] references)
        {
            var result = TryGetImage(meta, namespaces, references, out var image);

            if (!result)
            {
                return TypeBuildResult.Failed();
            }

            var assembly = Assembly.Load(image.ToArray());
            var type = assembly.GetType(meta.TypeName);
            var reference = MetadataReference.CreateFromImage(image);

            return TypeBuildResult.Succeeded(assembly, reference, type);
        }

        private static EmitResult Emit(Compilation compilation, out ImmutableArray<byte> image)
        {
            using (var peStream = new MemoryStream())
            {
                var result = compilation.Emit(peStream);

                if (result.Success)
                {
                    image = ImmutableArray.Create(peStream.ToArray());
                }
                else
                {
                    image = ImmutableArray<byte>.Empty;
                }

                return result;
            }
        }

        private static CSharpCompilation GetCompilation(TypeMetadata meta, string[] namespaces, MetadataReference[] references)
        {
            var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest);
            var compOptions = GetCompilationOptions(namespaces);
            var source = GetSource(meta, namespaces);

            var syntaxTree = SyntaxFactory.ParseSyntaxTree(
                source,
                parseOptions);

            var compilation = CSharpCompilation.Create(
                meta.AssemblyName,
                new SyntaxTree[]
                {
                    syntaxTree
                },
                references,
                compOptions);

            return compilation;
        }

        private static CSharpCompilationOptions GetCompilationOptions(string[] namespaces)
        {
            var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

            options = options.WithOptimizationLevel(OptimizationLevel.Release);
            options = options.WithOverflowChecks(true);
            options = options.WithUsings(namespaces);

            return options;
        }

        private static string GetSource(TypeMetadata meta, string[] namespaces)
        {
            var buffer = new StringWriter();
            var writer = new IndentedTextWriter(buffer, IndentedTextWriter.DefaultTabString);

            foreach (var ns in namespaces)
            {
                writer.WriteLine("using {0};", ns);
            }

            writer.WriteLine();
            writer.WriteLine("public class {0}", meta.TypeName);
            writer.WriteLine("{");
            writer.Indent++;

            foreach (var prop in meta.Fields)
            {
                writer.WriteLine("public {0} {1};", prop.FieldType, prop.FieldName);
            }

            writer.Indent--;
            writer.WriteLine("}");

            return buffer.ToString();
        }

        private static bool TryGetImage(TypeMetadata meta, string[] namespaces, MetadataReference[] references, out ImmutableArray<byte> image)
        {
            var compilation = GetCompilation(meta, namespaces, references);
            var result = Emit(compilation, out image);
            return result.Success;
        }
    }
}