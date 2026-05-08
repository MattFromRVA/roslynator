// Copyright (c) .NET Foundation and Contributors. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Testing;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests;

public class RCS1269AvoidImplicitConversionFromDateTimeToDateTimeOffsetTests : AbstractCSharpDiagnosticVerifier<AvoidImplicitConversionFromDateTimeToDateTimeOffsetAnalyzer, EmptyCodeFixProvider>
{
    public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.AvoidImplicitConversionFromDateTimeToDateTimeOffset;

    [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidImplicitConversionFromDateTimeToDateTimeOffset)]
    public async Task Test_SpecifyKindUnspecified_AssignedToDateTimeOffset()
    {
        await VerifyDiagnosticAsync(@"
using System;

class C
{
    void M()
    {
        DateTime utc = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        DateTimeOffset offset = [|utc|];
    }
}
");
    }
}
