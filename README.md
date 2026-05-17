# BioSpectra Analyzer

BioSpectra Analyzer is a WPF desktop application for visualizing and performing basic analysis of spectral data for bioorganic compounds.

The application loads spectral data from `.txt` or `.csv` files, draws an interactive plot with OxyPlot, detects local intensity peaks and compares them with predefined functional group templates.

## Features

- Load spectral data from TXT or CSV files
- Parse two-column wavelength/intensity data
- Visualize spectra in a dark WPF interface
- Detect local maxima in spectral data
- Highlight detected peaks on the plot
- Compare detected peaks with simple templates for common functional groups
- Show the best matching compound class

## Supported input format

The input file should contain at least two numeric columns:

```text
wavelength;intensity
3200;0.86
1715;0.92
1040;0.73
```

Supported separators:

```text
semicolon ;
comma ,
tab
```

Numbers are parsed using invariant culture, so decimal points should use `.`.

## Built-in templates

The current prototype contains simple peak templates for:

- Alcohol
- Ketone
- Amine
- Carboxylic Acid
- Ester
- Alkene

## Technologies

- C#
- WPF / XAML
- .NET Framework 4.8.1
- OxyPlot
- NuGet packages through `packages.config`

## Project structure

```text
BioSpectra_Analyzer.sln                 Visual Studio solution
BioSpectra_Analyzer/App.xaml            WPF app definition
BioSpectra_Analyzer/MainWindow.xaml     Main UI layout
BioSpectra_Analyzer/MainWindow.xaml.cs  Loading, plotting and analysis logic
BioSpectra_Analyzer/packages.config     NuGet package list
```

## How to run

Requirements:

- Windows
- Visual Studio 2022
- .NET Framework 4.8.1 Developer Pack

Steps:

1. Clone the repository.
2. Open `BioSpectra_Analyzer.sln` in Visual Studio.
3. Restore NuGet packages.
4. Build and run the project.
5. Load a `.txt` or `.csv` spectrum file.

## Status

Prototype / work in progress.

The project is suitable as a portfolio example of WPF data visualization and basic scientific data processing. Future improvements may include a cleaner MVVM architecture, configurable peak detection, export to reports, real reference spectra and more advanced matching algorithms.

## Author

Created by HardesFaktorProg.
