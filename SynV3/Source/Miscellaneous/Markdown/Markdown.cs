using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;
using Markdig;

namespace SynV3
{
    public static class MD
    {
        private static readonly MarkdownPipeline Pipeline;
        static MD()
        {
            Pipeline = new MarkdownPipelineBuilder().UseEmphasisExtras().UseGridTables().UsePipeTables().UseTaskLists().UseAutoLinks().Build();
        }
        public static void Display(FlowDocument TargetDocument, string Markdown, FontFamily? FontFamily = null)
        {
            if (TargetDocument == null)
                throw new ArgumentNullException(nameof(TargetDocument));
            if (string.IsNullOrEmpty(Markdown))
            {
                TargetDocument.Blocks.Clear();
                return;
            }
            try
            {
                var Document = Markdig.Wpf.Markdown.ToFlowDocument(Markdown, Pipeline);
                Document.Foreground = TargetDocument.Foreground;
                Document.FontSize = TargetDocument.FontSize;
                Document.FontFamily = FontFamily ?? TargetDocument.FontFamily;

                Document.PagePadding = new Thickness(0);

                foreach (var Block in Document.Blocks.ToList())
                {
                    if (Block is Section Section)
                    {
                        Section.Margin = new Thickness(0);
                        foreach (var SectionBlock in Section.Blocks.ToList())
                        {
                            if (SectionBlock is Paragraph Paragraph)
                            {
                                Paragraph.Foreground = TargetDocument.Foreground;
                                Paragraph.Margin = new Thickness(0, 0, 0, 5);

                                foreach (var Inline in Paragraph.Inlines.ToList())
                                    Inline.Foreground = TargetDocument.Foreground;
                            }
                        }
                    }
                    else if (Block is Paragraph Paragraph)
                    {
                        Paragraph.Foreground = TargetDocument.Foreground;
                        Paragraph.Margin = new Thickness(0, 0, 0, 5);

                        foreach (var Inline in Paragraph.Inlines.ToList())
                            Inline.Foreground = TargetDocument.Foreground;
                    }

                    if (Block is TextElement TextElement)
                    {
                        TextElement.Foreground = TargetDocument.Foreground;
                        if (Block is Block blockElement)
                            blockElement.Margin = new Thickness(0, 0, 0, 5);
                    }
                }

                TargetDocument.Blocks.Clear();
                TargetDocument.Blocks.AddRange(Document.Blocks.ToList());
            }
            catch (Exception ex)
            {
                throw new Exception($"Error rendering markdown: {ex.Message}", ex);
            }
        }
    }
}