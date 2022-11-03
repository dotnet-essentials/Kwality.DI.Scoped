// =====================================================================================================================
// == LICENSE:       Copyright (c) 2022 Kevin De Coninck
// ==
// ==                Permission is hereby granted, free of charge, to any person
// ==                obtaining a copy of this software and associated documentation
// ==                files (the "Software"), to deal in the Software without
// ==                restriction, including without limitation the rights to use,
// ==                copy, modify, merge, publish, distribute, sublicense, and/or sell
// ==                copies of the Software, and to permit persons to whom the
// ==                Software is furnished to do so, subject to the following
// ==                conditions:
// ==
// ==                The above copyright notice and this permission notice shall be
// ==                included in all copies or substantial portions of the Software.
// ==
// ==                THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// ==                EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// ==                OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// ==                NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// ==                HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// ==                WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// ==                FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// ==                OTHER DEALINGS IN THE SOFTWARE.
// =====================================================================================================================
namespace Kwality.DI.Scoped.Tests;

using System.Diagnostics.CodeAnalysis;

using FluentAssertions;

using Kwality.DI.Scoped.Models;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

using static Kwality.DI.Scoped.Tests.Properties.Traits.TraitTypes;
using static Kwality.DI.Scoped.Tests.Properties.Traits.TraitValues;

[Trait(Functionality, Core)]
public sealed class ServicesResolverTests
{
    [Fact(DisplayName = "When the requested service is NOT found, an exception is raised.")]
    internal void RaisesAnExceptionWhenTheServiceIsNotFound()
    {
        // ARRANGE.
        IServiceProvider serviceProvider = new ServiceCollection()
            .BuildServiceProvider(true);

        var serviceResolver = new ServicesResolver(serviceProvider);

        // ACT / ASSERT.
        Func<ScopedService<SingletonServiceWithScopedService>> act = () =>
            serviceResolver.GetRequiredService<SingletonServiceWithScopedService>();

        _ = act.Should()
            .Throw<Exception>();
    }

    [Fact(DisplayName = "It's possible to resolve a `Singleton` service, which uses a `Scoped` service.")]
    internal void ItIsPossibleToResolveASingletonServiceThatUsesAScopedService()
    {
        // ARRANGE.
        IServiceProvider serviceProvider = new ServiceCollection()
            .AddSingleton<SingletonServiceWithScopedService>()
            .AddScoped<ScopedService>()
            .BuildServiceProvider(true);

        var serviceResolver = new ServicesResolver(serviceProvider);

        // ACT / ASSERT.
        Func<ScopedService<ScopedService>> act = () => serviceResolver.GetRequiredService<ScopedService>();

        _ = act.Should()
            .NotThrow<Exception>();
    }

    [Fact(DisplayName = "When resolving a `Singleton` service twice, the same instance is returned.")]
    internal void ResolvingASingletonServiceTwiceResultsInTheSameInstance()
    {
        // ARRANGE.
        IServiceProvider serviceProvider = new ServiceCollection()
            .AddSingleton<SingletonService>()
            .BuildServiceProvider(true);

        var serviceResolver = new ServicesResolver(serviceProvider);

        // ACT.
        using ScopedService<SingletonService> instanceOne = serviceResolver.GetRequiredService<SingletonService>();
        using ScopedService<SingletonService> instanceTwo = serviceResolver.GetRequiredService<SingletonService>();

        // ASSERT.
        _ = instanceOne.Service.Should()
            .BeSameAs(instanceTwo.Service);
    }

    [Fact(DisplayName = "When resolving a `Scoped` service twice, different instances are returned.")]
    internal void ResolvingAScopedTwiceResultsInDifferentInstances()
    {
        // ARRANGE.
        IServiceProvider serviceProvider = new ServiceCollection()
            .AddScoped<ScopedService>()
            .BuildServiceProvider();

        var serviceResolver = new ServicesResolver(serviceProvider);

        // ACT.
        using ScopedService<ScopedService> instanceOne = serviceResolver.GetRequiredService<ScopedService>();
        using ScopedService<ScopedService> instanceTwo = serviceResolver.GetRequiredService<ScopedService>();

        // ASSERT.
        _ = instanceOne.Service.Should()
            .NotBeSameAs(instanceTwo.Service);
    }

    [Fact(DisplayName = "When disposing an already disposed `ScopedService{TService}`, an exception should NOT be " +
        "raised.")]
    internal void DisposingAnAlreadyDisposedScopedServiceShouldNotRaiseAnException()
    {
        // ARRANGE.
        IServiceProvider serviceProvider = new ServiceCollection()
            .AddScoped<ScopedService>()
            .BuildServiceProvider();

        var serviceResolver = new ServicesResolver(serviceProvider);

        // ACT.
        using ScopedService<ScopedService> instanceOne = serviceResolver.GetRequiredService<ScopedService>();

        // ACT / ASSERT.
        Action act = () => instanceOne.Dispose();

        _ = act.Should()
            .NotThrow<Exception>();
    }

    [SuppressMessage("", "CA1812", Justification = "Used as a generic argument inside a UT.")]
    private sealed class SingletonServiceWithScopedService
    {
        public SingletonServiceWithScopedService(ScopedService _)
        {
            // NOTE: Intentionally left blank.
        }
    }

    [SuppressMessage("", "CA1812", Justification = "Used as a generic argument inside a UT.")]
    private sealed class SingletonService
    {
        // NOTE: Intentionally left blank.
    }

    [SuppressMessage("", "CA1812", Justification = "Used as a generic argument inside a UT.")]
    private sealed class ScopedService
    {
        // NOTE: Intentionally left blank.
    }
}
