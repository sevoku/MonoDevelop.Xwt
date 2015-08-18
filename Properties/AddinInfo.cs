using Mono.Addins;

[assembly:Addin (
	"MonoDevelop.Xwt", 
	Namespace = "MonoDevelop.Xwt",
	Version = "1.0"
)]

[assembly:AddinName ("Xwt Project Support")]
[assembly:AddinCategory ("IDE extensions")]
[assembly:AddinDescription (
	"Adds Xwt cross-platform UI project and file templates, " +
	"including executable projects for supported Xwt backends and " +
	"automatic platform detection.")]
[assembly:AddinAuthor ("Vsevolod Kukol")]
[assembly:AddinUrl ("https://github.com/sevoku/MonoDevelop.Xwt.git")]
