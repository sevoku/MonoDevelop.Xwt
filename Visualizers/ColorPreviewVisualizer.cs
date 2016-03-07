//
// ColorPreviewVisualizer.cs
//
// Author:
//       Vsevolod Kukol <sevo@sevo.org>
//
// Copyright (c) 2016 Vsevolod Kukol
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using Mono.Debugging.Client;
using MonoDevelop.Components;
using MonoDevelop.Debugger;
using Xwt;
using Xwt.Drawing;

namespace MonoDevelop.Xwt
{
	public class ColorPreviewVisualizer : PreviewVisualizer
	{
		public override bool CanVisualize (ObjectValue val)
		{
			return DebuggingService.HasGetConverter<Color> (val);
		}

		public override Control GetVisualizerWidget (ObjectValue val)
		{
			var color = DebuggingService.GetGetConverter<Color> (val).GetValue (val);
			var mainBox = new HBox ();
			var colorBox = new ColorBox (color);
			var titleColor = Styles.PreviewVisualizerHeaderTextColor;
			var valueColor = Styles.PreviewVisualizerTextColor;
			var font = Font.SystemFont;
			font = font.WithSize (font.Size - 1);

			mainBox.PackStart (colorBox, hpos: WidgetPlacement.Center, vpos: WidgetPlacement.Center);

			var mainTable = new Table () {
				DefaultRowSpacing = 5,
				DefaultColumnSpacing = 5,
			};
			mainTable.SetColumnSpacing (2, 18);
			mainTable.SetColumnSpacing (4, 12);

			var titleLabel = new Label ("R:") { TextColor = titleColor, Font = font };
			mainTable.Add (titleLabel, 0, 0);
			var valueLabel = new Label (((byte)(color.Red * 255.0)).ToString ()) { TextColor = valueColor, Font = font };
			mainTable.Add (valueLabel, 1, 0);

			titleLabel = new Label ("G:") { TextColor = titleColor, Font = font };
			mainTable.Add (titleLabel, 0, 1);
			valueLabel = new Label (((byte)(color.Green * 255.0)).ToString ()) { TextColor = valueColor, Font = font };
			mainTable.Add (valueLabel, 1, 1);

			titleLabel = new Label ("B:") { TextColor = titleColor, Font = font };
			mainTable.Add (titleLabel, 0, 2);
			valueLabel = new Label (((byte)(color.Blue * 255.0)).ToString ()) { TextColor = valueColor, Font = font };
			mainTable.Add (valueLabel, 1, 2);

			titleLabel = new Label ("H:") { TextColor = titleColor, Font = font };
			mainTable.Add (titleLabel, 2, 0);
			valueLabel = new Label ((color.Hue * 360.0).ToString ("0.##") + "°") { TextColor = valueColor, Font = font };
			mainTable.Add (valueLabel, 3, 0);

			titleLabel = new Label ("S:") { TextColor = titleColor, Font = font };
			mainTable.Add (titleLabel, 2, 1);
			valueLabel = new Label ((color.Saturation * 100.0).ToString ("0.##") + "%") { TextColor = valueColor, Font = font };
			mainTable.Add (valueLabel, 3, 1);

			titleLabel = new Label ("L:") { TextColor = titleColor, Font = font };
			mainTable.Add (titleLabel, 2, 2);
			valueLabel = new Label ((color.Light * 100.0).ToString ("0.##") + "%") { TextColor = valueColor, Font = font };
			mainTable.Add (valueLabel, 3, 2);

			titleLabel = new Label ("A:") { TextColor = titleColor, Font = font };
			titleLabel.TextAlignment = Alignment.End;
			mainTable.Add (titleLabel, 4, 0);
			valueLabel = new Label (((byte)(color.Alpha * 255.0)) + " (" + (color.Alpha * 100.0).ToString ("0.##") + "%)") {
				TextColor = valueColor, Font = font
			};
			mainTable.Add (valueLabel, 5, 0);

			titleLabel = new Label ("HEX:") { TextColor = titleColor, Font = font };
			mainTable.Add (titleLabel, 4, 1);
			valueLabel = new Label ("#" + 
				((byte)(color.Red * 255.0)).ToString ("X2") +
				((byte)(color.Green * 255.0)).ToString ("X2") +
				((byte)(color.Blue * 255.0)).ToString ("X2")) {
				TextColor = valueColor,
				Font = font
			};
			if (color.Alpha < 1.0)
				valueLabel.Text += ((byte)(color.Alpha * 255.0)).ToString ("X2");
			valueLabel.TextColor = valueColor;
			mainTable.Add (valueLabel, 5, 1);

			mainBox.PackStart (mainTable, true, margin: 9);

			return new XwtControlWrapper (mainBox);
		}

		class XwtControlWrapper : Control
		{
			public Widget Widget { get; set; }

			public XwtControlWrapper (Widget widget)
			{
				Widget = widget;
			}

			protected override object CreateNativeWidget<T> ()
			{
				return Toolkit.CurrentEngine.GetNativeWidget (Widget);
			}
		}

		class ColorBox : Canvas
		{
			Color color;

			public Color Color {
				get { return color;}
				set {
					color = value;
					QueueDraw ();
				}
			}

			public ColorBox (Color color)
			{
				this.color = color;
			}

			protected override Size OnGetPreferredSize (SizeConstraint widthConstraint, SizeConstraint heightConstraint)
			{
				return new Size (40, 40);
			}

			protected override void OnDraw (Context ctx, Rectangle dirtyRect)
			{
				base.OnDraw (ctx, dirtyRect);
				var borderColor = Styles.PreviewVisualizerTextColor;

				ctx.RoundRectangle (0, 0, Bounds.Width, Bounds.Height, 2);
				ctx.SetLineWidth (1);

				ctx.SetColor (Color);
				ctx.FillPreserve ();
				ctx.SetColor (borderColor);
				ctx.Stroke ();
			}
		}
	}
}

