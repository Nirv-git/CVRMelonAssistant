using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using static System.Windows.Forms.LinkLabel;

namespace CVRMelonAssistant
{
    public partial class ModInfoWindow : Window
    {
        public ModInfoWindow()
        {
            InitializeComponent();
        }

        public void SetMod(Mod mod)
        {
            Title = string.Format((string) FindResource("ModInfoWindow:Title"), mod.versions[0].name);

            ModDescription.Text = mod.versions[0].description ?? (string) FindResource("ModInfoWindow:NoDescription");

            var desc = mod.versions[0].description ?? (string)FindResource("ModInfoWindow:NoDescription");
            SetMarkdownText(ModDescription, desc);

            ModName.Text = mod.versions[0].name;
            ModAuthor.Text = string.Format((string) FindResource("ModInfoWindow:Author"), mod.versions[0].author ?? FindResource("ModInfoWindow:NoAuthor"));
            ModVersion.Text = mod.versions[0].modVersion;

            var dlLink = mod.versions[0].downloadLink;
            DownloadLink.Text = (string) FindResource("ModInfoWindow:DownloadLink");
            DownloadLink.Inlines.Add(new Run(" "));
            if (dlLink?.StartsWith("http") == true)
                DownloadLink.Inlines.Add(CreateHyperlink(dlLink));
            else
                DownloadLink.Inlines.Add(new Run(dlLink));

            var srcLink = mod.versions[0].sourceLink;
            SourceCodeLink.Text = (string) FindResource("ModInfoWindow:SourceCodeLink");
            SourceCodeLink.Inlines.Add(new Run(" "));
            if (srcLink?.StartsWith("http") == true)
                SourceCodeLink.Inlines.Add(CreateHyperlink(srcLink));
            else
                SourceCodeLink.Inlines.Add(new Run(srcLink));

            InternalIds.Text = string.Format((string) FindResource("ModInfoWindow:InternalIds"), mod._id, mod.versions[0]._version);
        }

        private static readonly Regex Linkish = new(
            @"\[(?<text>[^\]]+)\]\((?<url>https?://[^\s)]+)\)|<(?<auto>https?://[^>\s]+)>",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex Emphasis = new(
            @"(?<strongem>\*\*\*.+?\*\*\*|___.+?___)|(?<strong>\*\*.+?\*\*|__.+?__)|(?<em>\*(?!\s).+?\*|_(?!\s).+?_)",
            RegexOptions.Compiled);

        public void SetMarkdownText(TextBlock tb, string? text)
        {
            tb.Inlines.Clear();
            if (string.IsNullOrEmpty(text)) return;

            int last = 0;
            foreach (Match m in Linkish.Matches(text))
            {
                if (m.Index > last)
                    AddFormattedText(tb.Inlines, text.Substring(last, m.Index - last));

                if (m.Groups["url"].Success)
                {   // [text](url)
                    var url = m.Groups["url"].Value;
                    var linkText = m.Groups["text"].Value;
                    var link = CreateHyperlink(linkText, url);
                    tb.Inlines.Add(link);
                }
                else
                {   // <url>
                    string url = m.Groups["auto"].Value;
                    tb.Inlines.Add(CreateHyperlink(url));
                }
                last = m.Index + m.Length;
            }

            if (last < text.Length)
                AddFormattedText(tb.Inlines, text.Substring(last));
        }

        // Parse **bold**, *italics*, and ***bold+italics***
        private static void AddFormattedText(InlineCollection inlines, string text)
        {
            int last = 0;
            foreach (Match m in Emphasis.Matches(text))
            {
                if (m.Index > last)
                    inlines.Add(new Run(text.Substring(last, m.Index - last)));

                Inline styled;
                if (m.Groups["strongem"].Success)
                {
                    string inner = StripOuter(m.Value, 3);
                    styled = new Run(inner) { FontWeight = FontWeights.Bold, FontStyle = FontStyles.Italic };
                }
                else if (m.Groups["strong"].Success)
                {
                    string inner = StripOuter(m.Value, 2);
                    styled = new Run(inner) { FontWeight = FontWeights.Bold };
                }
                else // em
                {
                    string inner = StripOuter(m.Value, 1);
                    styled = new Run(inner) { FontStyle = FontStyles.Italic };
                }

                inlines.Add(styled);
                last = m.Index + m.Length;
            }

            if (last < text.Length)
                inlines.Add(new Run(text.Substring(last)));
        }

        private static string StripOuter(string s, int n)
        {
            if (s.Length >= 2 * n) return s.Substring(n, s.Length - 2 * n);
            return s;
        }

        private static Hyperlink CreateHyperlink(string uri)
        {
            var link = new Hyperlink { NavigateUri = new Uri(uri) };
            link.Inlines.Add(new Run(uri));
            link.RequestNavigate += HyperlinkExtensions.Hyperlink_RequestNavigate;
            return link;
        }

        private static Hyperlink CreateHyperlink(string text, string uri)
        {
            var link = new Hyperlink { NavigateUri = new Uri(uri) };
            link.Inlines.Add(new Run(text));
            link.RequestNavigate += HyperlinkExtensions.Hyperlink_RequestNavigate;
            return link;
        }
    }
}

