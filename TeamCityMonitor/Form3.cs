using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace workPanel
{
        public partial class Form3 : Form
        {
            Point pt = Screen.PrimaryScreen.WorkingArea.Location;
            private int _secondClose = 0;
            internal int SecondClose
            {
                get { return _secondClose; }
                set { _secondClose = value; }
            }

        public Form3(int secondToClose, string text)
            {
                InitializeComponent();
                SecondClose = secondToClose;
                textBox1.Text = text;
                this.StartPosition = FormStartPosition.Manual;
                pt.Offset(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);
                pt.Offset(-this.Width, -this.Height);
                this.Location = pt;
                this.Location = new Point(this.Location.X, this.Location.Y + this.Height);
                Task.Factory.StartNew(() =>
                {
                    for (int i = 0; i < this.Height; i++)
                    {
                        if (this.IsHandleCreated)
                        {
                            this.Invoke((Action)(() =>
                            {
                                this.Location = new Point(this.Location.X, this.Location.Y - 1);
                            }));
                        }
                        if (this.Location == pt) break;
                        Thread.Sleep(5);
                    }
                });
                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(_secondClose * 1000);
                    this.Invoke((Action)(() =>
                    {
                        ((IDisposable)this).Dispose();
                    }));
                });
            }
        }
    }

