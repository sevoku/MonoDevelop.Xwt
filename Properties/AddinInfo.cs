﻿using Mono.Addins;

[assembly:Addin (
	"Xwt", 
	Namespace = "MonoDevelop",
	Version = "1.2.1"
)]

[assembly:AddinName ("Xwt Project Support")]
[assembly:AddinCategory ("IDE extensions")]
[assembly:AddinDescription (
	"Adds Xwt cross-platform UI project and file templates, " +
	"including executable projects for supported Xwt backends and " +
	"automatic platform detection.")]
[assembly:AddinAuthor ("Vsevolod Kukol")]
[assembly:AddinUrl ("https://github.com/sevoku/MonoDevelop.Xwt.git")]
