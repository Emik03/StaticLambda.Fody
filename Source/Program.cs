// SPDX-License-Identifier: MPL-2.0
#if !NETSTANDARD2_0
var readLine = Console.ReadLine;

args
   .DefaultIfEmpty(readLine.Forever().Select(Invoke).TakeUntil(string.IsNullOrWhiteSpace))
   .Where(File.Exists)
   .Select(Mono.Cecil.AssemblyDefinition.ReadAssembly)
   .Filter()
   .Lazily(x => ModuleWeaver.Execute(x.MainModule, onInfo: Console.WriteLine, onDebug: Console.Error.WriteLine))
   .Lazily(x => x.Write($"{nameof(StaticLambda)}.{x.MainModule?.Name}"))
   .Enumerate();
#endif
