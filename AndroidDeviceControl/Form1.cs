using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Managed.Adb;

namespace AndroidDeviceControl
{
	public partial class Form1 : Form, IShellOutputReceiver
	{
		private AndroidDebugBridge _bridge;
		private List<Device> _devices;

		public Form1()
		{
			InitializeComponent();

			_bridge = AndroidDebugBridge.CreateBridge("adb.exe", true);
			_bridge.Start();

			_devices = AdbHelper.Instance.GetDevices(AndroidDebugBridge.SocketAddress).ToList();

			Thread thr = new Thread(GetImageThreadRunner) {IsBackground = true};
			thr.Start();
		}

		private void GetImageThreadRunner()
		{
			while (true)
			{
				Image img =
					ResizeImage(
						AdbHelper.Instance.GetFrameBuffer(AndroidDebugBridge.SocketAddress, _devices.FirstOrDefault()).ToImage());
				Invoke(() => pictureBox1.Image = img);
			}
		}

		private void Invoke(Action method)
		{
			try
			{
				if (!IsDisposed)
					base.Invoke(method);
			}
			catch (Exception ex)
			{

			}
		}

		private double calculationBase = 0;

		private Image ResizeImage(Image image)
		{
			int height = pictureBox1.Height;

			var result = new Bitmap(pictureBox1.Width, pictureBox1.Height);

			var newWidth = (int) (image.Width*(height/(double)image.Height));
			calculationBase = image.Height/(double) height;

			Graphics.FromImage(result).DrawImage(image, 0f, 0f, newWidth, height);

			return result;
		}

		private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
		{
			int x = (int) (e.X*calculationBase);
			int y = (int) (e.Y*calculationBase);

			AdbHelper.Instance.ExecuteRemoteCommand(AndroidDebugBridge.SocketAddress, String.Format("input touchscreen tap {0} {1}", x, y), _devices.FirstOrDefault(), this);
		}

		public void AddOutput(byte[] data, int offset, int length)
		{

		}

		public void Flush()
		{
			
		}

		public bool IsCancelled { get; private set; }
	}
}
