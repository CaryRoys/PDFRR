# PDF Redaction Remover

A C# Windows Forms application that searches for PDF files in a folder, removes black rectangles and highlights (redactions), detects obscured content, and generates detailed summary reports.

## Features

- **Folder-based PDF Processing**: Select a folder and process all PDF files within it
- **Black Rectangle Detection**: Automatically identifies black rectangles and filled shapes used for redaction
- **Text Extraction**: Detects and extracts text hidden underneath black rectangles/highlights
- **Image Detection**: Identifies non-white image data obscured by redactions
- **Clean PDF Output**: Creates cleaned PDFs with redactions removed (removes black rectangles)
- **Detailed Summary Reports**: Generates `.txt` files with complete redaction analysis for each PDF

## Requirements

- .NET 8.0 or later
- Windows OS (Windows Forms application)

## NuGet Dependencies

- `itext7` (8.0.5) - PDF processing and manipulation
- `itext7.pdfhtml` (5.0.5) - Additional PDF functionality
- `Tesseract` (5.2.0) - OCR capabilities for text extraction

## Building the Application

1. Open a terminal in the project directory
2. Restore NuGet packages:
   ```
   dotnet restore
   ```
3. Build the project:
   ```
   dotnet build
   ```
4. Run the application:
   ```
   dotnet run
   ```

## How to Use

1. **Launch the Application**: Run `PdfRedactionRemover.exe` or use `dotnet run`

2. **Select Folder**:
   - Click the "Browse..." button
   - Navigate to the folder containing your PDF files
   - Click "Select Folder"

3. **Process PDFs**:
   - Click the "Process PDFs" button
   - Watch the progress bar and log window for real-time updates

4. **Review Results**:
   - For each `filename.pdf`, the application creates:
     - `filename_cleaned.pdf` - PDF with redactions removed
     - `filename.txt` - Summary report of all detected redactions

## Output Files

### Cleaned PDF Files (`*_cleaned.pdf`)
- Original PDF with black rectangles replaced by white rectangles
- Reveals any content that was underneath the redactions
- Preserves all other document content

### Summary Text Files (`*.txt`)
For each processed PDF, a detailed summary includes:
- Total number of redactions found
- Page-by-page breakdown of redactions
- For each redaction:
  - Location (X, Y coordinates)
  - Size (Width, Height)
  - Hidden text content (if any)
  - Hidden image information (if any)

Example summary format:
```
PDF REDACTION ANALYSIS SUMMARY
==============================
Generated: 2025-12-27 10:30:45 AM

Total redactions found: 3

REDACTION DETAILS:
------------------

Page 1:
  Redaction #1:
    Location: X=100.50, Y=200.75
    Size: W=150.25, H=20.00
    Hidden Text: "Confidential Information"

  Redaction #2:
    Location: X=100.50, Y=300.00
    Size: W=200.00, H=100.00
    Hidden Image: Image data found (45678 bytes)
```

## How It Works

1. **Rectangle Detection**: Scans PDF content for filled black paths (rectangles, shapes)
2. **Text Analysis**: Extracts all text from the page and identifies text within redaction boundaries
3. **Image Analysis**: Detects images positioned under or within redaction areas
4. **Removal Process**: Removes rectangle fill commands where found
5. **Report Generation**: Compiles all findings into a human-readable summary

## Limitations

- Currently does not check colors of black rectangles (current use case is to remove all rectangle fills)
- Works best with text-based PDFs (most likely to have black rectangles)

## License

This project uses the iText7 library which has its own licensing terms. Please review iText licensing for commercial use.

## Troubleshooting

**No redactions found**:
- Ensure redactions are actually black filled rectangles
- Some PDFs use image overlays instead of vector graphics

**Text not detected**:
- Text may be vectorized or part of an image
- OCR may be needed for scanned documents

**Build errors**:
- Ensure .NET 8.0 SDK is installed
- Run `dotnet restore` to download dependencies
