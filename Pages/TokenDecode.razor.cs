using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace JwtDecoder.Pages;

public partial class TokenDecode : ComponentBase
{
    private string DecodedPayload { get; set; } = "";
    private string SelectedText { get; set; } = "";
    private bool ShowPopup { get; set; }
    private bool IsProcessing { get; set; }
    private SelectionRect? SelectionRectangle { get; set; }
    private CancellationTokenSource _cts = new();
    private readonly JwtTokenDecoder _jwtDecoder = new();

    private void HandleInput(ChangeEventArgs e)
    {
        if (ShowPopup)
        {
            ShowPopup = false;
        }

        string input = e.Value?.ToString() ?? string.Empty;

        // Отменяем предыдущие вызовы
        _cts.Cancel();
        _cts.Dispose();
        _cts = new CancellationTokenSource();

        IsProcessing = true;
        if (string.IsNullOrWhiteSpace(input))
        {
            DecodedPayload = "";
            IsProcessing = false;
        }
        else
        {
            // Выполняем через задержку (throttle 700 мс)
            _ = Task.Delay(700, _cts.Token)
                .ContinueWith(task =>
                {
                    if (task.IsCanceled) return;

                    string? result = _jwtDecoder.TryDecode(input);
                    DecodedPayload =
                        string.IsNullOrWhiteSpace(result)
                            ? "Error: Can't decode jwt."
                            : result;
                    IsProcessing = false;

                    InvokeAsync(StateHasChanged); // Перерисовка
                }, TaskScheduler.Default);
        }
    }
    
    private async Task Copy()
    {
        await Js.InvokeVoidAsync("copyToClipboard", DecodedPayload);
    }
    
    private DotNetObjectReference<TokenDecode>? _dotNetRef;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            await Js.InvokeVoidAsync("registerPreCodeSelectionHandler", "codeBlock", _dotNetRef);
        }
    }

    [JSInvokable]
    public void OnTextSelected(string selectedText, SelectionRect rect)
    {
        SelectionRectangle = rect;

        if (!string.IsNullOrWhiteSpace(selectedText)
            && DateTimeParser.TryParse(selectedText, out var dateTime))
        {
            ShowPopup = true;
            string status = dateTime < DateTime.Now ? "Expired" : "Valid";
            SelectedText = $"{dateTime:dd-MM-yyyy hh:mm:ss} - {status}.";
        }
        else
        {
            ShowPopup = false;
            SelectedText = "";
        }

        StateHasChanged();
    }

    public void Dispose()
    {
        _dotNetRef?.Dispose();
    }
}

public class SelectionRect
{
    public double Top { get; set; }
    public double Left { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
}