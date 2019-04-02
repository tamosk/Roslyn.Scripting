using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DynamicType;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;

namespace DynamicGlobals.TestConsole
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Starting.");

                var start = DateTimeOffset.UtcNow;

                Console.WriteLine();
                Console.WriteLine(string.Join(string.Empty, Enumerable.Repeat('-', 100)));
                Console.ForegroundColor = ConsoleColor.Cyan;
                Run();
                Console.ResetColor();
                Console.WriteLine(string.Join(string.Empty, Enumerable.Repeat('-', 100)));

                var end = DateTimeOffset.UtcNow;
                var duration = end - start;

                Console.WriteLine();
                Console.WriteLine("Completed. Duration: {0} seconds.", duration.TotalSeconds);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine(ex.GetType().Name);
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

                Console.ResetColor();
            }

            Console.WriteLine();
            Console.WriteLine("Press enter key to exit...");

            Console.ReadLine();
        }

        private static void Run()
        {
            var globalsTypeMetadata = new TypeMetadata(
                Guid.NewGuid().ToString("N").ToUpper(),
                "Globals",
                new[]
                {
                    new FieldMetadata("int", "IntField"),
                    new FieldMetadata("string", "StringField"),
                    new FieldMetadata("List<string>", "StringArrayField"),
                });

            var namespaces = new[]
            {
                typeof(object).Namespace,
                typeof(Enumerable).Namespace,
                typeof(IEnumerable).Namespace,
                typeof(IEnumerable<>).Namespace
            };

            var references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            };

            var builder = new TypeBuilder();
            var buildResult = builder.Build(globalsTypeMetadata, namespaces, references);

            if (!buildResult.Success)
            {
                throw new Exception("Could not build Globals Type.");
            }

            var intField = buildResult.Type.GetField("IntField");
            var stringField = buildResult.Type.GetField("StringField");
            var stringArrayField = buildResult.Type.GetField("StringArrayField");

            var instance = Activator.CreateInstance(buildResult.Type);

            intField.SetValue(instance, 100);
            stringField.SetValue(instance, "Hello World!");
            stringArrayField.SetValue(instance, new List<string>());

            var scriptSource = @"
                StringArrayField.Add(StringField);
                StringArrayField.Add(IntField.ToString());
                return StringArrayField;";

            var scriptOptions = ScriptOptions.Default;

            scriptOptions = scriptOptions.WithImports(namespaces);
            scriptOptions = scriptOptions.WithReferences(references);
            scriptOptions = scriptOptions.AddReferences(buildResult.Reference);

            using (var loader = new InteractiveAssemblyLoader())
            {
                loader.RegisterDependency(buildResult.Assembly);

                var script = CSharpScript.Create(
                    scriptSource,
                    scriptOptions,
                    buildResult.Type,
                    loader);

                var scriptResult = script.RunAsync(instance);
                var returnValue = scriptResult.Result.ReturnValue as List<string>;

                Console.WriteLine(string.Join(", ", returnValue));
            }
        }
    }
}