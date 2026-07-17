// SPDX-License-Identifier: MPL-2.0
#if !NETSTANDARD2_0
static IEnumerable<string> FromStandardInput()
{
    while (Console.ReadLine() is not null and not "" and var next)
        yield return next;
}

(args is null or [] ? FromStandardInput() : args)
   .Where(File.Exists)
   .Select(Mono.Cecil.AssemblyDefinition.ReadAssembly)
   .Filter()
   .Lazily(x => ModuleWeaver.Execute(x.MainModule, onInfo: Console.WriteLine, onDebug: Console.Error.WriteLine))
   .Lazily(x => x.Write($"{nameof(StaticLambda)}.{x.MainModule?.Name}"))
   .Enumerate();
#endif
