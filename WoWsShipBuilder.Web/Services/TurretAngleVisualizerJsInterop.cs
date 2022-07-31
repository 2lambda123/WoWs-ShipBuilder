﻿using System.Diagnostics.CodeAnalysis;
using Microsoft.JSInterop;
using WoWsShipBuilder.Web.Data;

namespace WoWsShipBuilder.Web.Services;

public class TurretAngleVisualizerJsInterop : IAsyncDisposable
{
    private readonly IJSRuntime runtime;

    private IJSObjectReference? module;

    public TurretAngleVisualizerJsInterop(IJSRuntime runtime)
    {
        this.runtime = runtime;
    }

    public async Task DrawVisualizerAsync(IEnumerable<GunDataContainer> gunDataContainers, bool isArtillery)
    {
        await InitializeModule();
        await module.InvokeVoidAsync("drawVisualizer", gunDataContainers, isArtillery);
    }

    public async Task CleanupSubscriptions()
    {
        await InitializeModule();
        await module.InvokeVoidAsync("cleanSubscriptions");
    }

    [MemberNotNull(nameof(module))]
    private async Task InitializeModule()
    {
        // module is not null after this method but apparently, Roslyn does not want to recognize that.
#pragma warning disable CS8774
        module ??= await runtime.InvokeAsync<IJSObjectReference>("import", "/scripts/TurretAngleVisualizerHelper.js");
#pragma warning restore CS8774
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (module is not null)
        {
            try
            {
                await module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
            }
        }
    }
}
