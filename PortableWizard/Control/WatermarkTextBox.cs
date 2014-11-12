using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace PortableWizard.Control
{
	public class WatermarkTextBox : TextBox
	{
		public string Watermark
		{
			get { return (string)GetValue(WaterMarkProperty); }
			set { SetValue(WaterMarkProperty, value); }
		}
		public static readonly DependencyProperty WaterMarkProperty =
			DependencyProperty.Register("Watermark", typeof(string), typeof(WatermarkTextBox), new PropertyMetadata(new PropertyChangedCallback(OnWatermarkChanged)));

		private bool _isWatermarked = false;
		private Binding _textBinding = null;

		public WatermarkTextBox()
		{
			Loaded += (s, ea) => ShowWatermark();
		}

		protected override void OnGotFocus(RoutedEventArgs e)
		{
			base.OnGotFocus(e);
			HideWatermark();
		}

		protected override void OnLostFocus(RoutedEventArgs e)
		{
			base.OnLostFocus(e);
			ShowWatermark();
		}
		protected override void OnTextChanged(TextChangedEventArgs e)
		{
			base.OnTextChanged(e);
			if (!string.IsNullOrEmpty(Text) && Text != Watermark)
			{
				HideWatermark(Text);
			}
		}

		private static void OnWatermarkChanged(DependencyObject sender, DependencyPropertyChangedEventArgs ea)
		{
			var tbw = sender as WatermarkTextBox;
			if (tbw == null || !tbw.IsLoaded) return; //needed to check IsLoaded so that we didn't dive into the ShowWatermark() routine before initial Bindings had been made
			tbw.ShowWatermark();
		}

		private void ShowWatermark()
		{
			if (string.IsNullOrEmpty(Text) && !string.IsNullOrEmpty(Watermark))
			{
				_isWatermarked = true;

				//save the existing binding so it can be restored
				_textBinding = BindingOperations.GetBinding(this, TextProperty);

				//blank out the existing binding so we can throw in our Watermark
				BindingOperations.ClearBinding(this, TextProperty);

				//set the signature watermark gray
				Foreground = new SolidColorBrush(Colors.Gray);

				//display our watermark text
				Text = Watermark;
			}
		}

		private void HideWatermark(string defaultText = "")
		{
			if (_isWatermarked)
			{
				_isWatermarked = false;
				ClearValue(ForegroundProperty);
				Text = defaultText;
				if (_textBinding != null) SetBinding(TextProperty, _textBinding);
			}
		}
	}
}
