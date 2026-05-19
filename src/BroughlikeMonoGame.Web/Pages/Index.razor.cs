using BroughlikeMonoGame.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BroughlikeMonoGame.Web.Pages;

public partial class Index
{
    [Parameter] public string? Path { get; set; }

    private BrowserGameHost? _host;

    protected override async void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);

        if (!firstRender)
        {
            return;
        }

        await JsRuntime.InvokeAsync<object>("initRenderJS", DotNetObjectReference.Create(this));
    }

    [JSInvokable]
    public void TickDotNet()
    {
        _host ??= new BrowserGameHost();
        _host.Tick();
    }
}
