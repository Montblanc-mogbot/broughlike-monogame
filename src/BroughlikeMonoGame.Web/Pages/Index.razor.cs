using BroughlikeMonoGame.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.Xna.Framework.Input;

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

    [JSInvokable]
    public void QueueKeyDotNet(string key)
    {
        _host ??= new BrowserGameHost();
        if (TryMapKey(key, out var mapped))
        {
            _host.QueueBrowserKey(mapped);
        }
    }

    private static bool TryMapKey(string key, out Keys mapped)
    {
        mapped = key switch
        {
            "w" or "W" or "ArrowUp" => Keys.W,
            "s" or "S" or "ArrowDown" => Keys.S,
            "a" or "A" or "ArrowLeft" => Keys.A,
            "d" or "D" or "ArrowRight" => Keys.D,
            " " => Keys.Space,
            "Enter" => Keys.Enter,
            "1" => Keys.D1,
            "2" => Keys.D2,
            "3" => Keys.D3,
            "4" => Keys.D4,
            "5" => Keys.D5,
            "6" => Keys.D6,
            "7" => Keys.D7,
            "8" => Keys.D8,
            "9" => Keys.D9,
            _ => Keys.None,
        };

        return mapped != Keys.None;
    }
}
