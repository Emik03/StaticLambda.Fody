// SPDX-License-Identifier: MPL-2.0
[assembly: CLSCompliant(true)]

namespace StaticLambda.Fody;

using CustomAttribute = Mono.Cecil.CustomAttribute;
using FieldDefinition = Mono.Cecil.FieldDefinition;
using MethodDefinition = Mono.Cecil.MethodDefinition;
using ModuleDefinition = Mono.Cecil.ModuleDefinition;
using OpCodes = Mono.Cecil.Cil.OpCodes;
using TypeDefinition = Mono.Cecil.TypeDefinition;

/// <summary>This weaver removes unused members within an assembly.</summary>
[CLSCompliant(false)] // ReSharper disable once ClassNeverInstantiated.Global
public sealed class ModuleWeaver : BaseModuleWeaver
{
    /// <inheritdoc />
    public override bool ShouldCleanReference => false;

    /// <summary>Executes the weaver on the <see cref="Mono.Cecil.Cil.ModuleDefinition"/>.</summary>
    /// <param name="module">The module to process.</param>
    /// <param name="onInfo">The logger at the info level.</param>
    /// <param name="onDebug">The logger at the debug level.</param>
    // ReSharper disable once CognitiveComplexity
    public static void Execute(ModuleDefinition module, Action<string>? onInfo = null, Action<string>? onDebug = null)
    {
        bool TurnStatic(MethodDefinition x)
        {
            if (x.IsConstructor)
                return true;

            onDebug?.Invoke($"Changing {x.FullName} to be a public static method.");
            x.IsPublic = true;
            x.IsStatic = true;
            return true;
        }

        bool Suitable(TypeDefinition x)
        {
            if (!x.CustomAttributes.Any(IsCompilerGenerated) ||
                !x.Fields.Any(IsSingletonField) ||
                !x.Methods.All(TurnStatic))
                return false;

            onDebug?.Invoke($"Changing {x.FullName} to be a public type.");
            x.IsNestedPublic = true;
            x.IsPublic = true;
            return true;
        }

        var types = module.Assembly.Modules.SelectMany(x => x.GetAllTypes()).Where(Suitable).ToImmutableArray();

        Instruction? Target(Instruction il) =>
            il is { OpCode.Code: Code.Ldsfld, Operand: FieldReference { FieldType.FullName: var fullName } } &&
            types.Any(x => x.FullName == fullName)
                ? il
                : null;

        foreach (var method in types.SelectMany(x => x.DeclaringType.Methods))
            while (method.Body.Instructions.Select(Target).FirstOrDefault(x => x is not null) is { } instruction)
                Replace(method, instruction, onDebug);

        if (onInfo is null)
            return;

        foreach (var type in types)
            onInfo($"Finished processing {type.FullName}!");
    }

    /// <inheritdoc />
    public override void Execute()
    {
        if (!DefineConstants.Exists(x => x.Contains("NO_STATIC_LAMBDA_FODY")))
            Execute(ModuleDefinition, WriteInfo, WriteDebug);
    }

    /// <inheritdoc />
    public override IEnumerable<string> GetAssembliesForScanning() => [];

    static void Replace(MethodDefinition method, Instruction instruction, Action<string>? onDebug)
    {
        method.Body.GetILProcessor().Replace(instruction, Instruction.Create(OpCodes.Ldnull));

        onDebug?.Invoke(
            $"Replaced {method.FullName} IL_{instruction.Offset:x4}'s {nameof(Code.Ldsfld)} to {nameof(Code.Ldnull)}."
        );
    }

    static bool IsCompilerGenerated(CustomAttribute x) =>
        x.AttributeType.FullName == typeof(CompilerGeneratedAttribute).FullName;

    static bool IsSingletonField(FieldDefinition x) => x.IsStatic && x.FieldType.FullName == x.DeclaringType.FullName;
}
