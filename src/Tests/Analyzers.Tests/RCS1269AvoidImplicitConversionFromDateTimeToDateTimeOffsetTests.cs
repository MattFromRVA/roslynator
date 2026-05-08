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

    [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidImplicitConversionFromDateTimeToDateTimeOffset)]
    public async Task TestNoDiagnostic_UtcNow()
    {
        await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    void M()
    {
        DateTimeOffset offset = DateTime.UtcNow;
    }
}
");
    }

    [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidImplicitConversionFromDateTimeToDateTimeOffset)]
    public async Task TestNoDiagnostic_Now()
    {
        await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    void M()
    {
        DateTimeOffset offset = DateTime.Now;
    }
}
");
    }

    [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidImplicitConversionFromDateTimeToDateTimeOffset)]
    public async Task TestNoDiagnostic_Today()
    {
        await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    void M()
    {
        DateTimeOffset offset = DateTime.Today;
    }
}
");
    }

    [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidImplicitConversionFromDateTimeToDateTimeOffset)]
    public async Task TestNoDiagnostic_UnixEpoch()
    {
        await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    void M()
    {
        DateTimeOffset offset = DateTime.UnixEpoch;
    }
}
");
    }

    [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidImplicitConversionFromDateTimeToDateTimeOffset)]
    public async Task TestNoDiagnostic_SpecifyKindUtc()
    {
        await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    void M(DateTime dt)
    {
        DateTimeOffset offset = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
    }
}
");
    }

    [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidImplicitConversionFromDateTimeToDateTimeOffset)]
    public async Task TestNoDiagnostic_SpecifyKindLocal()
    {
        await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    void M(DateTime dt)
    {
        DateTimeOffset offset = DateTime.SpecifyKind(dt, DateTimeKind.Local);
    }
}
");
    }

    [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidImplicitConversionFromDateTimeToDateTimeOffset)]
    public async Task TestNoDiagnostic_ToUniversalTime()
    {
        await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    void M(DateTime dt)
    {
        DateTimeOffset offset = dt.ToUniversalTime();
    }
}
");
    }

    [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidImplicitConversionFromDateTimeToDateTimeOffset)]
    public async Task TestNoDiagnostic_ToLocalTime()
    {
        await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    void M(DateTime dt)
    {
        DateTimeOffset offset = dt.ToLocalTime();
    }
}
");
    }

    [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidImplicitConversionFromDateTimeToDateTimeOffset)]
    public async Task TestNoDiagnostic_NewDateTimeWithUtcKind()
    {
        await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    void M()
    {
        DateTimeOffset offset = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }
}
");
    }

    [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidImplicitConversionFromDateTimeToDateTimeOffset)]
    public async Task TestNoDiagnostic_NewDateTimeWithLocalKind()
    {
        await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    void M()
    {
        DateTimeOffset offset = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Local);
    }
}
");
    }

    [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidImplicitConversionFromDateTimeToDateTimeOffset)]
    public async Task TestNoDiagnostic_SpecifyKindUtc_NamedArgumentsReversed()
    {
        await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    void M(DateTime dt)
    {
        DateTimeOffset offset = DateTime.SpecifyKind(kind: DateTimeKind.Utc, value: dt);
    }
}
");
    }

    [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidImplicitConversionFromDateTimeToDateTimeOffset)]
    public async Task TestNoDiagnostic_ExplicitConstructor()
    {
        await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    void M(DateTime dt)
    {
        DateTimeOffset offset = new DateTimeOffset(dt, TimeSpan.Zero);
    }
}
");
    }

    [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidImplicitConversionFromDateTimeToDateTimeOffset)]
    public async Task Test_NewDateTimeWithoutKind()
    {
        await VerifyDiagnosticAsync(@"
using System;

class C
{
    void M()
    {
        DateTimeOffset offset = [|new DateTime(2025, 1, 1)|];
    }
}
");
    }

    [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidImplicitConversionFromDateTimeToDateTimeOffset)]
    public async Task Test_DateTimeParse()
    {
        await VerifyDiagnosticAsync(@"
using System;

class C
{
    void M()
    {
        DateTimeOffset offset = [|DateTime.Parse(""2025-01-01"")|];
    }
}
");
    }

    [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidImplicitConversionFromDateTimeToDateTimeOffset)]
    public async Task Test_DateTimeMinValue()
    {
        await VerifyDiagnosticAsync(@"
using System;

class C
{
    void M()
    {
        DateTimeOffset offset = [|DateTime.MinValue|];
    }
}
");
    }

    [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidImplicitConversionFromDateTimeToDateTimeOffset)]
    public async Task Test_LocalParameter()
    {
        await VerifyDiagnosticAsync(@"
using System;

class C
{
    void M(DateTime dt)
    {
        DateTimeOffset offset = [|dt|];
    }
}
");
    }

    [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidImplicitConversionFromDateTimeToDateTimeOffset)]
    public async Task Test_Assignment()
    {
        await VerifyDiagnosticAsync(@"
using System;

class C
{
    void M(DateTime dt)
    {
        DateTimeOffset offset;
        offset = [|dt|];
    }
}
");
    }

    [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidImplicitConversionFromDateTimeToDateTimeOffset)]
    public async Task Test_Argument()
    {
        await VerifyDiagnosticAsync(@"
using System;

class C
{
    void Take(DateTimeOffset offset) { }

    void M(DateTime dt)
    {
        Take([|dt|]);
    }
}
");
    }

    [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidImplicitConversionFromDateTimeToDateTimeOffset)]
    public async Task Test_Return()
    {
        await VerifyDiagnosticAsync(@"
using System;

class C
{
    DateTimeOffset M(DateTime dt)
    {
        return [|dt|];
    }
}
");
    }

    [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidImplicitConversionFromDateTimeToDateTimeOffset)]
    public async Task Test_ArrowExpression()
    {
        await VerifyDiagnosticAsync(@"
using System;

class C
{
    private readonly DateTime _dt;
    DateTimeOffset Offset => [|_dt|];
}
");
    }

    [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidImplicitConversionFromDateTimeToDateTimeOffset)]
    public async Task TestNoDiagnostic_DateTimeToDateTime()
    {
        // Plain DateTime -> DateTime. No conversion to DateTimeOffset; should not fire on any callsite.
        await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    DateTime M(DateTime dt)
    {
        return dt;
    }
}
");
    }
}
