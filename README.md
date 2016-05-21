# MonoDevelop.Xwt
The Xwt addin for Xamarin Studio / MonoDevelop makes it easy to create new Xwt UI based cross-platform projects.

## Xwt
Xwt is a .NET framework for creating desktop applications that run on multiple platforms from the same codebase. Xwt works by exposing one unified API across all environments that is mapped to a set of native controls on each platform.

Xwt Project: https://github.com/mono/xwt

##Features
* Project Templates
  * Hybrid (all-in-one) application for Windows/MacOS/Linux
  * Backend based, one application for each backend (Wpf, Mac, Gtk, ...) sharing UI logic
  * Library (for shared UI logic, loadable into other projects)
* File Templates
  * Window
  * Widget
  * Custom Canvas
  * Custom drawn image
* Import/Reference Xwt from different sources
  * Github (checkout repository, optional submodule registration support)
  * NuGet (Xwt is not yet published on NuGet, but you can use own NuGet packages built from source)
  * Local / GAC (e.g. Xwt packaged with XamarinStudio / Monodevelop)
* Xwt.Drawing.Color Debugger Visualizer

## Installation
Open the Xamarin Studio / MonoDevelop Addin Gallery and install the "[Xwt Project Support](http://addins.monodevelop.com/Project/Index/189)" addin. Sometimes XS / MD should be restarted to get the full functionality.

## Supported project types
The addin provides different types of project templates, which can be found in the "Cross-platform" category of the XS / MD "New Project" wizard.

### Xwt Hybrid Application
This Hybrid Application is one single application targeting Windows, Mac and Linux at the same time. The wizard automatically creates different projects:
* [ProjectName] library
* [ProjectName].Desktop Application
* [ProjectName].Mac MonoMac Application (On Mac only)
* [ProjectName].XamMac XamarinMac Application (On Mac only, Xamarin.Mac license required)

All your UI code should go to the created library, which already contains a main window for your application. Additionally it contains a static ```App``` class which initializes Xwt and opens the created window. You can set which backends should be used for each platform or handle command line arguments inside ```App.cs```.
The created applications simply load the shared library and launch the Xwt application using ```App.Run()```.

The main [ProjectName].Desktop can run on all supported platforms, but for a better Mac integration the special Mac applications should be used instead (on Mac a "native" application package will be created).

### Xwt Application
The "Xwt Application" template creates separated application projects for each backend supported by Xwt and a shared library for your own implementation:
* [ProjectName] library
* [ProjectName].Wpf
* [ProjectName].Gtk2
* [ProjectName].Gtk3
* [ProjectName].Mac  (On Mac only)
* [ProjectName].XamMac (On Mac only, Xamarin.Mac license required)

All created applications load the shared library, initialize Xwt and open the ```MainWindow```.

### Xwt Library
This template simply creates a new library with a custom widget declaration. You can reference it from any other project but you need to take care of the Xwt initialization on you own.

## Xwt Reference selection
When creating a new Xwt project the template wizard asks to select a reference source to use for the created project. Following sources are supported:

### Github
The wizard creates a "Xwt" solution folder and clones the [official Xwt](https://github.com/mono/xwt) repository.

Optionally the Xwt repository can be registered as a Git submodule, if the Git version control is enabled in the last wizard step (otherwise a simple repository clone is performed).

### NuGet

The wizard adds a package reference to Xwt which will be resolved by the XS/MD package management. You can register a custom NuGet repository (XS / MD preferences) to use an own Xwt NuGet (you can build it using the nuspec files inside the Xwt repository).

### Local / GAC
A local package / GAC Xwt reference will be added. Xwt should be installed inside the GAC or the XS / MD built in version (supports only Gtk2) will be used. Select this option to reference the Xwt assemblies directly by providing a HintPath manually.

## Build Xwt addin from source
To build the addin from source the [Addin Maker](http://addins.monodevelop.com/Project/Index/87) addin is required. It can be easily installed from the Xamarin Studio / MonoDevelop Addin Gallery (Addin Maker source repository: https://github.com/mhutch/MonoDevelop.AddinMaker).
With the Addin Maker Plugin installed the project can be loaded and built.
