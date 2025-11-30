# StaticLambda.Fody

[![NuGet package](https://img.shields.io/nuget/v/StaticLambda.Fody.svg?color=50fa7b&logo=NuGet&style=for-the-badge)](https://www.nuget.org/packages/StaticLambda.Fody)
[![License](https://img.shields.io/github/license/Emik03/StaticLambda.Fody.svg?color=6272a4&style=for-the-badge)](https://github.com/Emik03/StaticLambda.Fody/blob/main/LICENSE)

This is an add-in for [Fody](https://github.com/Fody/Fody) which turns lambda methods static if possible.

This project has a dependency to [Emik.Morsels](https://github.com/Emik03/Emik.Morsels), if you are building this project, refer to its [README](https://github.com/Emik03/Emik.Morsels/blob/main/README.md) first.

---

- [Why](#why)
- [Installation](#installation)
- [Configuration](#configuration)
- [Example](#example)
- [Contribute](#contribute)
- [License](#license)

---

## Why

This weaver exists because there is no way to guarantee that a lambda function will compile as a static method, even if you use the `static` keyword.

> "No guarantee is made as to whether a static anonymous function definition is emitted as a `static` method in metadata. This is left up to the compiler implementation to optimize." - [source](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-9.0/static-anonymous-functions)

The reason is because delegates by nature are at their fastest invoking instanced methods, however in some cases it may be useful to make these methods static, mainly for reflection and interop purposes.

Of course, you can force it to be static by defining your own method to be static:

```cs
Foo(ThisInsteadOfALambda);

static void ThisInsteadOfALambda() { }
```

However, this may be an unsatisfactory solution as it creates far more verbose code, particularly when the parameter types are complicated and you use an API that has them inferred.

Installing this weaver will turn every applicable lambda into a `static` method. You do not need to specify `static` for this to take effect. Runtime performance is very likely barely worse with this weaver, but chances are the reflection that facillitates the need for `static` methods already makes this difference negligible.

## Installation

- Install the NuGet packages [`Fody`](https://www.nuget.org/packages/Fody) and [`StaticLambda.Fody`](https://www.nuget.org/packages/StaticLambda.Fody). Installing `Fody` explicitly is needed to enable weaving.

  ```
  PM> Install-Package Fody
  PM> Install-Package StaticLambda.Fody
  ```

- Add the `PrivateAssets="all"` metadata attribute to the `<PackageReference />` items of `Fody` and `StaticLambda.Fody` in your project file, so they won't be listed as dependencies.

- If you already have a `FodyWeavers.xml` file in the root directory of your project, add the `<StaticLambda />` tag there. This file will be created on the first build if it doesn't exist:

```xml
<Weavers>
    <StaticLambda />
</Weavers>
```

See [Fody usage](https://github.com/Fody/Home/blob/master/pages/usage.md) for general guidelines, and [Fody Configuration](https://github.com/Fody/Home/blob/master/pages/configuration.md) for additional options.

## Configuration

Define the compiler constant `NO_STATIC_LAMBDA_FODY` to skip this weaver. Useful to be passed as a command-line argument: e.g. `dotnet build -define NO_STATIC_LAMBDA_FODY`.

There is currently **no way** to define which classes or methods are affected. If you do require this level of granular control, please raise an issue, and preferably your use case.

## Example

What you write:

```csharp
Capture(() => 42);

static void Capture(System.Func<int> _) { }
```

What gets compiled:

```csharp
// The Func<int> target normally is the <>c singleton (which no longer exists)
Capture(Program.<>c.<>9__0_0 ?? (Program.<>c.<>9__0_0 = new Func<int>(null, __methodptr(<Main>b__0_0))));

static void Capture(Func<int> _) { }

[CompilerGenerated, Serializable]
private sealed class <>c
{
    public static Func<int> <>9__0_0;

    // This method is normally non-static.
    internal static int <Main>b__0_0() => 42;
}
```

## Contribute

Issues and pull requests are welcome to help this repository be the best it can be.

## License

This repository falls under the [MPL-2 license](https://www.mozilla.org/en-US/MPL/2.0/).
