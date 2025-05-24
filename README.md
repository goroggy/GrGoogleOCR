# GrGoogleOCR - Google OCR PDF Text Layer Builder

**Last Updated: May 2025**

GrGoogleOCR is a C# Windows Forms application demonstrating how to use Google Cloud Document AI for Optical Character Recognition (OCR) on PDF and image files. It removes the original text layer, if any. It extracts text and layout information from teh OCR results and rebuilds the PDF with an embedded, searchable text layer using two different PDF libraries provided in separate branches.

![image](https://github.com/user-attachments/assets/1ca639b2-e4a8-4a8f-b97a-aada4d0cd8bb)


## Overview

This project tackles the common problem of non-searchable, scanned PDFs. It provides a C# implementation for:

1.  Processing PDF files with Google's Document AI OCR service. You will bring your own ProcessorId and ProjectId.
2.  Parsing the rich OCR results (text, layout, optional styles).
3.  Embedding this data back into a PDF as a searchable text layer.

To showcase different trade-offs between features and licensing, this repository offers two main branches.

## Branches Explained

This repository uses a dual-branch approach:

1.  **PdfSharpCore Branch**
    * **Goal:** Provide a *permissively licensed* (MIT) solution that works "out of the box".
    * **Library:** Uses the open-source `PdfSharpCore` library.
    * **Features:** Creates a basic, searchable but visible text layer.
    * **Limitations:** Lacks advanced PDF features like precise character spacing to fit OCR boxes and true invisible text layers. The visual fidelity might not be perfect, but it *enables search*.
    * **Best For:** Users needing a free/permissive solution, a basic searchable PDF, or a starting point for Google AI integration.

2.  **SyncfusionPdf Branch**
    * **Goal:** Demonstrate the *full potential* with a feature-rich commercial library.
    * **Library:** Uses the `Syncfusion.Pdf` library.
    * **Features:** Creates high-fidelity PDF text layers, supports advanced features like better text placement (filling box width by character spacing) and invisible text.
    * **Limitations:** Requires a Syncfusion license for production/commercial use. **However, it runs without a license for evaluation, adding a warning banner to the generated PDFs.** Syncfusion also offers a free [Community License](https://www.syncfusion.com/products/communitylicense) and a free license for [Open Source Projects](https://www.syncfusion.com/products/communitylicense) – check their terms to see if you qualify.
    * **Best For:** Users evaluating Syncfusion, those who own a license, qualifying community/OSS users, or anyone wanting to see the best possible PDF output.

**To switch branches:** Use `git checkout syncfusionpdf` or `git checkout pdfsharpcore` or download the code from either.

## Features (Across Branches)

* **Google Document AI Integration:** Leverages Google's powerful cloud-based OCR engine.
* **PDF Processing:** Processes existing PDF files page by page.
* **Configurable OCR:** Allows selection of OCR modes (Lines, Tokens, Symbols), language hints, and style info requests.
* **JSON Caching:** Saves Google AI results locally to avoid reprocessing.
* **Searchable PDF Output:** Creates a new PDF file with the OCR text embedded.
* **Configurable Settings:** Provides a UI to manage Google Cloud details, OCR options, etc.

## Technology Stack

* **Language:** C#
* **Framework:** .NET (e.g., .NET 6/7/8 - *Please specify*)
* **UI:** Windows Forms (WinForms)
* **Google Cloud Client:** `Google.Cloud.DocumentAI.V1`
* **JSON Handling:** `System.Text.Json`
* **PDF Manipulation:** `PdfSharpCore` (`main` branch) / `Syncfusion.Pdf` (`syncfusion` branch)

## Setup & Installation

### Prerequisites

* Windows Operating System
* .NET SDK (Match your project's target framework)
* Visual Studio (Recommended for building/debugging)

### Google Cloud Setup (Crucial)

This application **requires** a Google Cloud Platform (GCP) account and Document AI setup:

1.  **Create a GCP Project:** [Google Cloud Console](https://console.cloud.google.com/).
2.  **Enable Billing:** Required for Document AI. There are generous allowances for new accounts.
3.  **Enable Document AI API:** Search for and enable it in the "APIs & Services" > "Library".
4.  **Create a Processor:** Create a "Document OCR" or "Form Parser" processor in the Document AI section. Note its **Location** and **Processor ID**.
5.  **Create a Service Account & Key:** Go to "IAM & Admin" > "Service Accounts", create an account, grant it the **"Document AI User"** role, and download its keys.

### Building the Application

1.  Clone this repository: `git clone [Your_Repo_URL]`
2.  Navigate to the directory: `cd GrGoogleOCR`
3.  **Choose your branch:**
    * For the basic version: `git checkout main` (or stay if it's default)
    * For the full-featured demo: `git checkout syncfusionpdf`
4.  Open the solution (`.sln`) file in Visual Studio.
5.  **Restore NuGet Packages:** Right-click the solution and choose "Restore NuGet Packages".
    * *Note for `syncfusion` branch:* You might need to [configure the Syncfusion NuGet feed](https://help.syncfusion.com/nuget/nuget-feeds) in Visual Studio if the packages don't restore automatically.
6.  Build the solution (`Build` > `Build Solution`).

## Usage

1.  Run the `GrGoogleOCR.exe`.
2.  Configure the settings in the Property Grid (GCP Project, Location, Processor, PDF File Path). TextIsVisible = false takes effect in the SyncfusionPdf branch only.
3.  **Note on Font/Style Info (`IsStyleInfoWanted`):** Be aware that enabling `IsStyleInfoWanted = true` asks Google Document AI for detailed font and style information. While this can improve PDF output (especially with libraries like Syncfusion), it engages a more advanced processing model that **significantly increases the cost** (potentially by 5x or more per page – always check [Google's current pricing](https://cloud.google.com/document-ai/pricing)). Only enable this if you specifically need font data and are aware of the cost implications. You may need font info if your text contains e.g. words in italics and you need to preserve this in the text layer. For a simple searchable PDF, you don't need italics in the (anyway invisible) text layer. To extract text to HTML, for instance, you may need it if it conveys significant meaning.
4.  Click the "Go" button.
5.  Output files (`.json`, `.pdf`) will appear in the input PDF's directory. The Syncfusion version will also show the results in the application.

## Speed

Google Document AI has rate limits. In my experience, launching 4-5 page OCR requests in parallel per second is usually accepted. Requesting character-level info results in a much bigger JSON so you may want to slow down.


## Licensing & Syncfusion

* The **PdfSharpCore branch** uses the permissively licensed (MIT) PdfSharCore library.
* The **SyncfusionPdf branch** uses Syncfusion's PDF library.
    * It **will run without a license** for evaluation, but it will add a **warning banner** to generated PDFs.
    * To remove the banner and for production/commercial use, a **Syncfusion license is required**.
    * Syncfusion offers a **free Community License** and a **free Open Source Project License**. We highly recommend checking if you qualify, as it provides access to the full-featured version at no cost for eligible users. Visit the [Syncfusion Licensing](https://www.syncfusion.com/sales/licensing) page for details.
* This project itself has its own license (Apache 2.0) to comply with Google sample code licensing terms.

## Contributing

Contributions are welcome! Feel free to fork the repository, create feature branches, and submit pull requests. Please specify which branch your changes apply to.
