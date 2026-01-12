using System.ComponentModel;
using System.Runtime.CompilerServices;
using ICSharpCode.AvalonEdit;
using Microsoft.Web.WebView2.Wpf;
using SteelDesignerEngineer.WPF.Models;

namespace SteelDesignerEngineer.WPF.ViewModels;

/// <summary>
/// ViewModel для редактора HTML
/// </summary>
public class EditorViewModel : ViewModelBase
{
    private TextEditor? _editor;
    private WebView2? _webView;

    private string _currentContent = string.Empty;
    public string CurrentContent
    {
        get => _currentContent;
        set
        {
            SetProperty(ref _currentContent, value);
            UpdatePreview();
        }
    }

    private string _pageName = string.Empty;
    public string PageName
    {
        get => _pageName;
        set => SetProperty(ref _pageName, value);
    }

    private string _pageTitle = string.Empty;
    public string PageTitle
    {
        get => _pageTitle;
        set => SetProperty(ref _pageTitle, value);
    }

    private bool _isDirty;
    public bool IsDirty
    {
        get => _isDirty;
        set => SetProperty(ref _isDirty, value);
    }

    public void SetEditor(TextEditor editor)
    {
        _editor = editor;
        _editor.TextChanged += (s, e) =>
        {
            CurrentContent = _editor.Text;
            IsDirty = true;
        };
    }

    public void SetWebView(WebView2 webView)
    {
        _webView = webView;
    }

    public void LoadPage(HtmlPageModel page)
    {
        PageName = page.PageName;
        PageTitle = page.Title;
        CurrentContent = page.Content;

        if (_editor != null)
        {
            _editor.Text = page.Content;
        }

        IsDirty = false;
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        if (_webView == null) return;

        try
        {
            _webView.NavigateToString(CurrentContent);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Preview error: {ex.Message}");
        }
    }

    public HtmlPageModel GetCurrentPage()
    {
        return new HtmlPageModel
        {
            PageName = PageName,
            Title = PageTitle,
            Content = _editor?.Text ?? CurrentContent
        };
    }
}
