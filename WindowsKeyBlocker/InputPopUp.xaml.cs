using System.Windows;
using System.Windows.Input;

namespace WindowsKeyBlocker;

/// <summary>
/// Interaction logic for InputPopUp.xaml
/// </summary>
public partial class InputPopUp : Window
{
	private Func<string, bool> _onOk;

	public InputPopUp(Func<string, bool> onOk)
	{
		this._onOk = onOk;
		this.InitializeComponent();
		this.Input.KeyDown += (o, e) =>
		{
			if (e.Key == Key.Enter)
				this.DoneButton_Click(o, e);
		};
	}

	private void DoneButton_Click(object sender, RoutedEventArgs e)
	{
		if (this._onOk.Invoke(this.Input.Text))
			this.Close();
	}
	private void CancelButton_Click(object sender, RoutedEventArgs e)
	{
		this.Close();
	}
}
