using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace WindowsKeyBlocker;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
	private bool _hasDetectedProcessLastRun;

	public GlobalKeyboardHook Hook { get; set; } = new()
	{
		BlockedKeys = [Key.LWin]
	};
	public ObservableCollection<string> ProcessWatcher { get; } = ["osu!"];
	public ObservableCollection<Key> BlockedKey { get; } = [Key.LWin];
	public bool UseUnhookInstead { get; set; } = false;
	public int ProcessWatcherPollDelayMs { get; set; } = 1000;
	public bool Enabled
	{
		get => this.EnableButton.IsEnabled;
		set
		{
			this.EnableButton.IsEnabled = !value;
			this.DisableButton.IsEnabled = value;
			this.Hook.Enabled = value;

			if (value) this.Hook.Hook();

			if (this.UseUnhookInstead && !value)
			{
				this.Hook.Unhook();
			}
		}
	}

	public MainWindow()
	{
		this.InitializeComponent();
		this.Enabled = true;
		this.ProcessWatcherList.ItemsSource = this.ProcessWatcher;
		this.BlockedKeyList.ItemsSource = this.BlockedKey;
		this.ProcessWatcher.CollectionChanged += (_, e) => this.RemoveProcessButton.IsEnabled = this.ProcessWatcher.Count != 0;
		this.BlockedKey.CollectionChanged += (_, e) => this.RemoveKeyButton.IsEnabled = this.BlockedKey.Count != 0;

		this.WatchProcess();
	}

	private async void WatchProcess()
	{
		while (true)
		{
			await Task.Delay(this.ProcessWatcherPollDelayMs);
			Process[] processes = Process.GetProcesses();
			if (processes.Any(x => this.ProcessWatcher.Contains(x.ProcessName)))
			{
				this.Enabled = true;
				this._hasDetectedProcessLastRun = true;
				continue;
			}
			else if (this._hasDetectedProcessLastRun)
			{
				this.Enabled = false;
			}

			this._hasDetectedProcessLastRun = false;
		}
	}

	private void EnableButton_Click(object sender, RoutedEventArgs e) => this.Enabled = true;
	private void DisableButton_Click(object sender, RoutedEventArgs e) => this.Enabled = false;

	private void AddProcess_Click(object sender, RoutedEventArgs e)
	{
		new InputPopUp(str => { this.ProcessWatcher.Add(str); return true; }).Show();
	}
	private void RemoveProcessButton_Click(object sender, RoutedEventArgs e)
	{
		this.ProcessWatcher.Remove((string)this.ProcessWatcherList.SelectedItem);
	}

	private void AddKeyButton_Click(object sender, RoutedEventArgs e)
	{
		new InputPopUp(str =>
		{
			if (Enum.TryParse(str, out Key key))
			{
				this.BlockedKey.Add(key);
				this.Hook.BlockedKeys.Add(key);
				return true;
			}
			MessageBox.Show(this, "Invalid key specified.", "Error", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
			return false;
		}).Show();
	}
	private void RemoveKeyButton_Click(object sender, RoutedEventArgs e)
	{
		if (this.BlockedKeyList.SelectedItem == null) return;
		Key casted = (Key)this.BlockedKeyList.SelectedItem;
		this.BlockedKey.Remove(casted);
		this.Hook.BlockedKeys.Remove(casted);
	}

	private void HookInsteadCheckBox_Checked(object sender, RoutedEventArgs e)
	{
		this.UseUnhookInstead = this.UseUnhookInsteadCheckBox.IsChecked == true;
	}
}