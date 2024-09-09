using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace T4C_Translator
{
	/// <summary>
	/// Interaction logic for DockerProgressBar.xaml
	/// </summary>
	public partial class DockerProgressBar : Window
	{
		public DockerProgressBar()
		{
			InitializeComponent();
		}
		public string Message
		{
			get => MessageTextBlock.Text;
			set => MessageTextBlock.Text = value;
		}

		public void SetProgress(double value)
		{
			FakeProgressBar.IsIndeterminate = false;
			FakeProgressBar.Value = value;
		}
	}
}
