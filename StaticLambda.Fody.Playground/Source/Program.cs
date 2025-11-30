// SPDX-License-Identifier: MPL-2.0
// ReSharper disable once RedundantUsingDirective.Global
#pragma warning disable 1591, RCS1102
namespace StaticLambda.Fody.Playground;

public static class Program
{
    public static void Main()
    {
        var i = 0;
        Capture(StaticMethod);
        Capture(static () => 2);
        Capture(() => 32);
        Capture(() => 123);
        Capture(() => i++);
    }

    static void Capture(System.Func<int> _) { }

    static int StaticMethod() => 2;
}
