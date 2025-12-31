using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iText.IO.Font;
using iText.IO.Source;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser.Util;
using iText.Kernel.Pdf.Extgstate;
using iText.Kernel.Pdf.Xobject;

namespace PdfRedactionRemover
{
    public class PdfProcessor
    {
        private readonly Finder _form;

        public PdfProcessor(Finder form)
        {
            _form = form;
        }

        public async Task ProcessFolderAsync(string folderPath, bool Recurse)
        {
            await Task.Run(() => ProcessFolder(folderPath, Recurse));
        }

        private void ProcessFolder(string folderPath, bool Recurse)
        {
            var pdfFiles = Directory.GetFiles(folderPath, "*.pdf", Recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            _form.LogMessage($"Found {pdfFiles.Length} PDF file(s) in folder.");

            for (int i = 0; i < pdfFiles.Length; i++)
            {
                _form.UpdateProgress(i + 1, pdfFiles.Length);
                ProcessPdfFile(pdfFiles[i]);
            }

            _form.LogMessage("All files processed.");
        }

        private void ProcessPdfFile(string pdfPath)
        {
            string fileName = System.IO.Path.GetFileNameWithoutExtension(pdfPath);
            string directory = System.IO.Path.GetDirectoryName(pdfPath) ?? "";
            string outputPdf = System.IO.Path.Combine(directory, $"{fileName}_cleaned.pdf");
            string summaryFile = System.IO.Path.Combine(directory, $"{fileName}.txt");

           
            _form.LogMessage($"Processing: {pdfPath}");

            try
            {
                var redactionInfo = new RedactionInfo();

                using (PdfReader reader = new PdfReader(pdfPath))
                using (PdfWriter writer = new PdfWriter(outputPdf))
                using (PdfDocument pdfDoc = new PdfDocument(reader, writer))
                {
                    int pageCount = pdfDoc.GetNumberOfPages();
                    _form.LogMessage($"  Pages: {pageCount}");

                    for (int pageNum = 1; pageNum <= pageCount; pageNum++)
                    {
                        PdfPage page = pdfDoc.GetPage(pageNum);

                        // Extract text and locations
                        var textExtractor = new LocationTextExtractionStrategy();
                        PdfCanvasProcessor processor = new PdfCanvasProcessor(textExtractor);
                        processor.ProcessPageContent(page);

                        // Detect and collect black rectangles
                        var rectangleDetector = new BlackRectangleDetector(page);
                        var blackRectangles = rectangleDetector.DetectBlackRectangles();

                        if (blackRectangles.Count > 0)
                        {
                            _form.LogMessage($"  Page {pageNum}: Found {blackRectangles.Count} black rectangle(s)");

                            // Find text under black rectangles
                            var textUnderRedactions = FindTextUnderRectangles(page, blackRectangles);

                            foreach (var rect in blackRectangles)
                            {
                                var pageInfo = new PageRedactionInfo
                                {
                                    PageNumber = pageNum,
                                    Rectangle = rect
                                };

                                // Check for text under this rectangle
                                if (textUnderRedactions.ContainsKey(rect))
                                {
                                    pageInfo.HiddenText = textUnderRedactions[rect];
                                    _form.LogMessage($"    - Hidden text detected: \"{pageInfo.HiddenText}\"");
                                }

                                // Check for images under this rectangle
                                var imageData = CheckForImagesUnderRectangle(page, rect);
                                if (imageData != null && imageData.Length > 0)
                                {
                                    pageInfo.HasHiddenImage = true;
                                    pageInfo.HiddenImageInfo = $"Image data found ({imageData.Length} bytes)";
                                    _form.LogMessage($"    - Hidden image detected: {imageData.Length} bytes");
                                }

                                redactionInfo.Redactions.Add(pageInfo);
                            }

                            // Remove black rectangles from the page
                            RemoveBlackRectangles(page, blackRectangles);
                        }
                    }
                }

                // Write summary file
                WriteSummaryFile(summaryFile, redactionInfo);
                _form.LogMessage($"  Created: {System.IO.Path.GetFileName(outputPdf)}");
                _form.LogMessage($"  Created: {System.IO.Path.GetFileName(summaryFile)}");
            }
            catch (Exception ex)
            {
                _form.LogMessage($"  ERROR: {ex.Message}");
            }
        }

        private Dictionary<iText.Kernel.Geom.Rectangle, string> FindTextUnderRectangles(PdfPage page, List<iText.Kernel.Geom.Rectangle> rectangles)
        {
            var result = new Dictionary<iText.Kernel.Geom.Rectangle, string>();

            try
            {
                var strategy = new LocationTextExtractionStrategy();
                PdfCanvasProcessor processor = new PdfCanvasProcessor(strategy);
                processor.ProcessPageContent(page);

                string allText = strategy.GetResultantText();
                var textChunks = GetTextChunks(page);

                foreach (var rect in rectangles)
                {
                    var textInRect = new StringBuilder();

                    foreach (var chunk in textChunks)
                    {
                        if (IsTextInRectangle(chunk, rect))
                        {
                            textInRect.Append(chunk.Text);
                        }
                    }

                    string foundText = textInRect.ToString().Trim();
                    if (!string.IsNullOrEmpty(foundText))
                    {
                        result[rect] = foundText;
                    }
                }
            }
            catch (Exception ex)
            {
                _form.LogMessage($"    Warning: Error extracting text: {ex.Message}");
            }

            return result;
        }

        private List<TextChunk> GetTextChunks(PdfPage page)
        {
            var chunks = new List<TextChunk>();
            var listener = new TextChunkLocationListener();
            PdfCanvasProcessor processor = new PdfCanvasProcessor(listener);
            processor.ProcessPageContent(page);
            return listener.Chunks;
        }

        private bool IsTextInRectangle(TextChunk chunk, iText.Kernel.Geom.Rectangle rect)
        {
            return chunk.X >= rect.GetX() &&
                   chunk.X <= rect.GetX() + rect.GetWidth() &&
                   chunk.Y >= rect.GetY() &&
                   chunk.Y <= rect.GetY() + rect.GetHeight();
        }

        private byte[]? CheckForImagesUnderRectangle(PdfPage page, iText.Kernel.Geom.Rectangle rect)
        {
            try
            {
                var imageDetector = new ImageDetector(rect);
                PdfCanvasProcessor processor = new PdfCanvasProcessor(imageDetector);
                processor.ProcessPageContent(page);
                return imageDetector.ImageData;
            }
            catch
            {
                return null;
            }
        }

        private void RemoveBlackRectangles(PdfPage page, List<iText.Kernel.Geom.Rectangle> rectangles)
        {
            if (rectangles.Count == 0) return;

            try
            {
                // Create an extended graphics state with zero fill alpha (fully transparent)
                PdfExtGState transparentState = new PdfExtGState();
                transparentState.SetFillOpacity(0f); // Make fills completely transparent

                // Add the graphics state to the page resources
                PdfDictionary extGStates = page.GetResources().GetPdfObject().GetAsDictionary(PdfName.ExtGState);
                if (extGStates == null)
                {
                    extGStates = new PdfDictionary();
                    page.GetResources().GetPdfObject().Put(PdfName.ExtGState, extGStates);
                }

                string gsName = "GS_Transparent";
                //extGStates.Put(new PdfName(gsName), transparentState.GetPdfObject());

                
                // Get the content stream and parse it to wrap black fill operations with transparency
                var contentBytes = page.GetContentBytes();
                RemoveBlackRectsFromContent(page);

                _form.LogMessage($"    Made {rectangles.Count} Removed black fill operations");
            }
            catch (Exception ex)
            {
                _form.LogMessage($"    Warning: Error processing rectangles: {ex.Message}");
                Debug.WriteLine($"Error in RemoveBlackRectangles: {ex.Message}");
                Debug.WriteLine($"Stack: {ex.StackTrace}");
            }
        }

        private static void RemoveBlackRectsFromContent(PdfPage page)
        {
            var contentBytes = page.GetContentBytes();
            var contentString = PdfEncodings.ConvertToString(contentBytes, null);

            // This regex-based approach is simplified; production code
            // should use a proper content stream parser
            var filtered = FilterBlackRectangles(contentString);
            // Replace page content
            page.GetFirstContentStream()?.SetData(PdfEncodings.ConvertToBytes(filtered, null));
        }
        //  This method doesn't break anything! 
        // Current iteration removes all rectangles with a following fill regardless of color
        private static string FilterBlackRectangles(string content)
        {
            var lines = content.Split('\n');
            int linecount = 0;
            var result = new StringBuilder();
            bool skipNextRect = false;
            float currentGray = -1;
            bool nextFill = false;

            foreach (var line in lines)
            {
                if(nextFill)
                {
                    nextFill = false;
                    linecount++;
                    continue;
                }
                if (line.EndsWith("re") && lines[linecount + 1] == "f*") 
                {
                    nextFill = true;
                    linecount++;
                    continue;
                }
                linecount++;

                result.AppendLine(line); // this is the "don't break me" magic.
            }

            return result.ToString();
        }


        private class PdfToken
        {
            public PdfTokenizer.TokenType Type { get; set; }
            public string? Value { get; set; }
            public byte[]? RawBytes { get; set; }
            public bool Written { get; set; } = false;
            public bool Skip { get; set; } = false;
        }

        private void WriteSummaryFile(string filePath, RedactionInfo info)
        {
            if(info.Redactions.Count == 0)
            {
                return; // No redactions to report
            }   
            var sb = new StringBuilder();
            sb.AppendLine("PDF REDACTION ANALYSIS SUMMARY");
            sb.AppendLine("==============================");
            sb.AppendLine($"Generated: {DateTime.Now}");
            sb.AppendLine();
            sb.AppendLine($"Total redactions found: {info.Redactions.Count}");
            sb.AppendLine();

            if (info.Redactions.Count > 0)
            {
                sb.AppendLine("REDACTION DETAILS:");
                sb.AppendLine("------------------");

                var groupedByPage = info.Redactions.GroupBy(r => r.PageNumber);
                foreach (var pageGroup in groupedByPage.OrderBy(g => g.Key))
                {
                    sb.AppendLine();
                    sb.AppendLine($"Page {pageGroup.Key}:");

                    int redactionNum = 1;
                    foreach (var redaction in pageGroup)
                    {
                        sb.AppendLine($"  Redaction #{redactionNum}:");
                        sb.AppendLine($"    Location: X={redaction.Rectangle.GetX():F2}, Y={redaction.Rectangle.GetY():F2}");
                        sb.AppendLine($"    Size: W={redaction.Rectangle.GetWidth():F2}, H={redaction.Rectangle.GetHeight():F2}");

                        if (!string.IsNullOrEmpty(redaction.HiddenText))
                        {
                            sb.AppendLine($"    Hidden Text: \"{redaction.HiddenText}\"");
                        }

                        if (redaction.HasHiddenImage)
                        {
                            sb.AppendLine($"    Hidden Image: {redaction.HiddenImageInfo}");
                        }

                        if (string.IsNullOrEmpty(redaction.HiddenText) && !redaction.HasHiddenImage)
                        {
                            sb.AppendLine($"    Content: No text or image data detected");
                        }

                        sb.AppendLine();
                        redactionNum++;
                    }
                }
            }
            else
            {
                sb.AppendLine("No redactions found in this document.");
            }

            File.WriteAllText(filePath, sb.ToString());
        }
    }

    // Helper classes
    public class RedactionInfo
    {
        public List<PageRedactionInfo> Redactions { get; set; } = new List<PageRedactionInfo>();
    }

    public class PageRedactionInfo
    {
        public int PageNumber { get; set; }
        public iText.Kernel.Geom.Rectangle Rectangle { get; set; } = new iText.Kernel.Geom.Rectangle(0, 0, 0, 0);
        public string HiddenText { get; set; } = string.Empty;
        public bool HasHiddenImage { get; set; }
        public string HiddenImageInfo { get; set; } = string.Empty;
    }

    public class TextChunk
    {
        public string Text { get; set; } = string.Empty;
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
    }

    public class TextChunkLocationListener : IEventListener
    {
        public List<TextChunk> Chunks { get; } = new List<TextChunk>();

        public void EventOccurred(IEventData data, EventType type)
        {
            if (type == EventType.RENDER_TEXT)
            {
                var renderInfo = (TextRenderInfo)data;
                var baseline = renderInfo.GetBaseline();
                var ascentLine = renderInfo.GetAscentLine();

                Chunks.Add(new TextChunk
                {
                    Text = renderInfo.GetText(),
                    X = baseline.GetStartPoint().Get(0),
                    Y = baseline.GetStartPoint().Get(1),
                    Width = baseline.GetLength(),
                    Height = ascentLine.GetStartPoint().Get(1) - baseline.GetStartPoint().Get(1)
                });
            }
        }

        public ICollection<EventType> GetSupportedEvents()
        {
            return new[] { EventType.RENDER_TEXT };
        }
    }

    public class ImageDetector : IEventListener
    {
        private readonly iText.Kernel.Geom.Rectangle _targetRect;
        public byte[]? ImageData { get; private set; }

        public ImageDetector(iText.Kernel.Geom.Rectangle targetRect)
        {
            _targetRect = targetRect;
        }

        public void EventOccurred(IEventData data, EventType type)
        {
            if (type == EventType.RENDER_IMAGE)
            {
                var renderInfo = (ImageRenderInfo)data;
                var imageRect = renderInfo.GetImageCtm();

                // Check if image is within target rectangle
                float imgX = imageRect.Get(6);
                float imgY = imageRect.Get(7);

                if (imgX >= _targetRect.GetX() && imgX <= _targetRect.GetX() + _targetRect.GetWidth() &&
                    imgY >= _targetRect.GetY() && imgY <= _targetRect.GetY() + _targetRect.GetHeight())
                {
                    try
                    {
                        var image = renderInfo.GetImage();
                        if (image != null)
                        {
                            ImageData = image.GetImageBytes();
                        }
                    }
                    catch { }
                }
            }
        }

        public ICollection<EventType> GetSupportedEvents()
        {
            return new[] { EventType.RENDER_IMAGE };
        }
    }

    public class BlackRectangleDetector : IEventListener
    {
        private readonly PdfPage _page;
        private readonly List<iText.Kernel.Geom.Rectangle> _blackRectangles = new List<iText.Kernel.Geom.Rectangle>();
        private iText.Kernel.Geom.Rectangle? _currentRect;
        int rectCount = 0;
        private bool _isBlack;

        public BlackRectangleDetector(PdfPage page)
        {
            _page = page;
        }

        public List<iText.Kernel.Geom.Rectangle> DetectBlackRectangles()
        {
            try
            {
                PdfCanvasProcessor processor = new PdfCanvasProcessor(this);
                processor.ProcessPageContent(_page);
            }
            catch { }

            return _blackRectangles;
        }

        public void EventOccurred(IEventData data, EventType type)
        {
            if (type == EventType.RENDER_PATH)
            {
                var pathInfo = (PathRenderInfo)data;
                var path = pathInfo.GetPath();

                // Check if filled and black
                int operation = pathInfo.GetOperation();
                if ((operation & PathRenderInfo.FILL) != 0)
                {
                    var fillColor = pathInfo.GetFillColor();

                    if (IsBlackColor(fillColor))
                    {
                        // Get path bounds
                        var ctm = pathInfo.GetCtm();
                        var subpaths = path.GetSubpaths();

                        foreach (var subpath in subpaths)
                        {
                            iText.Kernel.Geom.Rectangle? rect = GetRectangleFromPath(subpath, ctm);
                            if (rect != null && rect.GetWidth() > 5 && rect.GetHeight() > 5)
                            {
                                _blackRectangles.Add(rect);
                                rectCount++;
                                Debug.WriteLine("Rect count {0}", rectCount);
                            }
                        }
                    }
                }
            }
        }

        private bool IsBlackColor(iText.Kernel.Colors.Color? color)
        {
            if (color == null) return false;

            var colorValues = color.GetColorValue();
            if (colorValues == null || colorValues.Length == 0) return false;

            // For grayscale, RGB, or CMYK - check if it's close to black
            if (color is DeviceGray)
            {
                return colorValues[0] < 0.1f;
            }
            else if (color is DeviceRgb)
            {
                return colorValues[0] < 0.1f && colorValues[1] < 0.1f && colorValues[2] < 0.1f;
            }
            else if (color is DeviceCmyk)
            {
                return colorValues[3] > 0.9f; // High black value
            }

            return false;
        }

        private iText.Kernel.Geom.Rectangle? GetRectangleFromPath(iText.Kernel.Geom.Subpath subpath, iText.Kernel.Geom.Matrix ctm)
        {
            try
            {
                var segments = subpath.GetSegments();
                Debug.WriteLine($"Subpath has {segments.Count} segments");

                // Don't require minimum segments - even simple rectangles can vary
                if (segments.Count == 0) return null;

                float minX = float.MaxValue, minY = float.MaxValue;
                float maxX = float.MinValue, maxY = float.MinValue;
                int pointCount = 0;

                foreach (var segment in segments)
                {
                    var points = segment.GetBasePoints();
                    foreach (var point in points)
                    {
                        // In iText7, Point uses x and y properties directly
                        Vector vector = new Vector((float)point.GetX(), (float)point.GetY(), 1);
                        Vector transformed = vector.Cross(ctm);

                        float x = transformed.Get(0);
                        float y = transformed.Get(1);

                        minX = Math.Min(minX, x);
                        maxX = Math.Max(maxX, x);
                        minY = Math.Min(minY, y);
                        maxY = Math.Max(maxY, y);
                        pointCount++;
                    }
                }

                Debug.WriteLine($"Processed {pointCount} points, bounds: ({minX},{minY}) to ({maxX},{maxY})");

                if (minX < maxX && minY < maxY && pointCount >= 2)
                {
                    var rect = new iText.Kernel.Geom.Rectangle(minX, minY, maxX - minX, maxY - minY);
                    Debug.WriteLine($"Created rectangle: {rect.GetWidth()}x{rect.GetHeight()}");
                    return rect;
                }
                else
                {
                    Debug.WriteLine($"Failed bounds check: minX={minX}, maxX={maxX}, minY={minY}, maxY={maxY}, points={pointCount}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in GetRectangleFromPath: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            return null;
        }

        public ICollection<EventType> GetSupportedEvents()
        {
            return new[] { EventType.RENDER_PATH };
        }
    }
}
