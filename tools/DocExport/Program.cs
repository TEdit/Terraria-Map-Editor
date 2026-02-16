using TEdit.Scripting;

var outputPath = args.Length > 0 ? args[0] : "API_REFERENCE.md";
var markdown = ScriptApiDocGenerator.GenerateMarkdown();
File.WriteAllText(outputPath, markdown);
Console.WriteLine($"Wrote API reference to {outputPath} ({markdown.Length} bytes)");
