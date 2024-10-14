using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Calculator
{
    public partial class Form1 : Form
    {
        private float x; //当前窗体的宽度
        private float y; //当前窗体的高度

        //运算符状态记录
        private bool guna2BtnAdd_flag = false;
        private bool guna2BtnSub_flag = false;
        private bool guna2BtnMul_flag = false;
        private bool guna2BtnDiv_flag = false;
        private bool guna2BtnEqual_flag = false;

        //private List<double> value_list = new List<double>();//存用户输入的数字
        //private List<int> operator_list = new List<int>();//存用户输入的运算符，定义+为0，-为1，×为2，÷为3
        private double num1, num2,result;
        private int sign; //记录运算符号 1->+ 2->- 3->* 4->/

        public Form1()
        {
            InitializeComponent();

            this.Text = string.Empty;
            this.ControlBox = false;
            this.MaximizedBounds = Screen.FromHandle(this.Handle).WorkingArea;

            richTBHistory.Visible = true;
            tBMemory.Visible = false;
            richTBHistory.Text = "尚无历史记录。";richTBHistory.Font = new Font("微软雅黑",9);
            tBMemory.Text = "内存中未保存任何内容。";tBMemory.Font = new Font("微软雅黑", 9);
            guna2BtnDel.Visible = false;

            //将 Form1_Load 方法订阅为窗体的 Load 事件的处理程序
            this.Load += new System.EventHandler(this.Form1_Load);
            //将 Form1_Resize 方法订阅为窗体的 Resize 事件的处理程序
            this.Resize += new System.EventHandler(this.Form1_Resize);
        }

        //页面显示效果
        #region 
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();

        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        private void guna2BtnMin_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void guna2BtnMax_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
        }

        private void guna2BtnCancel_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void panelTopBar_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void mouseHover(Guna2Button button,string msg)
        {
            ToolTip toolTip = new ToolTip();
            toolTip.AutoPopDelay = 5000;
            toolTip.InitialDelay = 500;

            toolTip.ReshowDelay = 500;
            toolTip.ShowAlways = true;

            toolTip.SetToolTip(button, msg);

            
        }
        private void guna2BtnMin_MouseHover(object sender, EventArgs e)
        {
            string msg = "最小化";
            mouseHover(this.guna2BtnMin, msg);
        }

        private void guna2BtnMax_MouseHover(object sender, EventArgs e)
        {
            string msg = "最大化";
            mouseHover(this.guna2BtnMax, msg);
        }

        private void guna2BtnCancel_MouseHover(object sender, EventArgs e)
        {
            string msg = "关闭";
            mouseHover(this.guna2BtnCancel, msg);
        }
        
        private void guna2BtnHistory_Click(object sender, EventArgs e)
        {
            tBMemory.Visible = false;
            richTBHistory.Visible = true;
        }

        private void guna2BtnMemory_Click(object sender, EventArgs e)
        {
            richTBHistory.Visible = false;
            tBMemory.Visible = true;
        }
        
        private void guna2BtnDel_Click(object sender, EventArgs e)
        {
            if(richTBHistory.Visible==true)
            {
                guna2BtnDel.Visible = false;
                richTBHistory.Text = "尚无历史记录。";
            }
            else
            {
                tBMemory.Text = "内存中未保存任何内容。";
            }
        }
        
        private void Form1_Resize(object sender, EventArgs e)
        {
            float ratioX = this.Width / x;
            float ratioY = this.Height / y;
            setControls(ratioX, ratioY, this);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            x = this.Width;
            y = this.Height;
            setTag(this);//将窗体的初始状态记录下来
        }

        private void setTag(Control cons)//传入窗体
        {
            foreach (Control con in cons.Controls)//获取窗体的控件
            {
                //将每个控件的信息存到控件的Tag属性中 宽度 高度 左边距 顶边距 字体大小
                con.Tag = con.Width + ":" + con.Height + ":" + con.Left + ":" + con.Top + ":" + con.Font.Size;
                if (con.Controls.Count > 0)//如果控件还有子控件，递归
                {
                    setTag(con);
                }
            }
        }

        private void setControls(float ratioX, float ratioY, Control cons)
        {
            //遍历窗体中的控件，重新设置控件的属性
            foreach (Control con in cons.Controls)
            {
                string[] mytag = con.Tag.ToString().Split(new char[] { ':' });
                float a = System.Convert.ToSingle(mytag[0]) * ratioX;//宽度
                con.Width = (int)a;
                a = System.Convert.ToSingle(mytag[1]) * ratioY;//高度
                con.Height = (int)a;
                a = System.Convert.ToSingle(mytag[2]) * ratioX;//左边距
                con.Left = (int)a;
                a = System.Convert.ToSingle(mytag[3]) * ratioY;//顶边距
                con.Top = (int)a;
                Single fontSize = System.Convert.ToSingle(mytag[4]) * ratioX;//字体大小
                con.Font = new Font(con.Font.Name, fontSize, con.Font.Style, con.Font.Unit);
                if (con.Controls.Count > 0)//递归设置控件的子控件
                {
                    setControls(ratioX, ratioY, con);
                }
            }
        }
        #endregion

        //数字按键处理
        #region
        private void numDown(string v)
        {
            if (guna2BtnAdd_flag || guna2BtnSub_flag || guna2BtnMul_flag || guna2BtnDiv_flag || guna2BtnEqual_flag)
            {
                tBOperationResult.Clear();
                tBOperationResult.Text += v;
                //num2 = double.Parse(tBOperationResult.Text);

                guna2BtnAdd_flag = false;
                guna2BtnSub_flag = false;
                guna2BtnMul_flag = false;
                guna2BtnDiv_flag = false;
                guna2BtnEqual_flag = false;

                guna2BtnPercent.Enabled = true;
                guna2BtnInverse.Enabled = true;
                guna2BtnSquare.Enabled = true;
                guna2BtnSqrt.Enabled = true;
                guna2BtnAdd.Enabled = true;
                guna2BtnSub.Enabled = true;
                guna2BtnMul.Enabled = true;
                guna2BtnDiv.Enabled = true;
                guna2BtnPoint.Enabled = true;
                guna2BtnOpposite.Enabled = true;
            }
            else
            {
                // 如果当前显示的是"0"且输入不是小数点，则清除文本
                if (tBOperationResult.Text == "0" && v != ".")
                {
                    tBOperationResult.Clear();
                }

                // 如果已经包含小数点
                if (tBOperationResult.Text.Contains("."))
                {
                    // 如果输入是小数点且已经包含小数点，则不进行操作
                    if (v == ".")
                    {
                        return;
                    }

                    // 分割整数和小数部分
                    string[] strArr = tBOperationResult.Text.Split('.');
                    string ip = strArr[0];
                    string dp = strArr[1];

                    // 检查是否达到最大长度限制
                    if ((ip == "0" && (ip.Length + dp.Length) == 17) ||
                        (ip != "0" && (ip.Length + dp.Length) == 16))
                    {
                        return;
                    }
                    tBOperationResult.Text += v;
                }
                else
                {
                    string num = tBOperationResult.Text.Replace(",", "");
                    // 如果没有小数点，检查总长度是否达到16位
                    if (num.Length != 16)
                    {
                        tBOperationResult.Text += v;
                        tBOperationResult.Text = FormatWithCommas(tBOperationResult.Text);
                    }
                    else
                    {
                        tBOperationResult.Text = FormatWithCommas(tBOperationResult.Text);
                        return;
                    }
                }
            }
        }
        private string FormatWithCommas(string numberStr)
        {
            numberStr = numberStr.Replace(",", "");
            if (long.TryParse(numberStr, out long number))
            {
                //MessageBox.Show($"if内：{ txtDisplay.Text}");
                return number.ToString("N0");
            }
            //MessageBox.Show($"if外：{ txtDisplay.Text}");
            return numberStr;
        }
        private void guna2Btn1_Click(object sender, EventArgs e)
        {
            numDown("1");
        }

        private void guna2Btn2_Click(object sender, EventArgs e)
        {
            numDown("2");
        }

        private void guna2Btn3_Click(object sender, EventArgs e)
        {
            numDown("3");
        }

        private void guna2Btn4_Click(object sender, EventArgs e)
        {
            numDown("4");
        }

        private void guna2Btn5_Click(object sender, EventArgs e)
        {
            numDown("5");
        }

        private void guna2Btn6_Click(object sender, EventArgs e)
        {
            numDown("6");
        }

        private void guna2Btn7_Click(object sender, EventArgs e)
        {
            numDown("7");
        }

        private void guna2Btn8_Click(object sender, EventArgs e)
        {
            numDown("8");
        }

        private void guna2Btn9_Click(object sender, EventArgs e)
        {
            numDown("9");
        }

        private void guna2Btn0_Click(object sender, EventArgs e)
        {
            numDown("0");
        }
        
        private void guna2BtnPoint_Click(object sender, EventArgs e)
        {
            numDown(".");
        }

        #endregion

        //运算符号按键处理
        #region
        private void guna2BtnC_Click(object sender, EventArgs e)
        {
            tBOperationResult.Text = "0";
            tBOperationProcedure.Clear();
            num1 = 0;num2 = 0;result = 0;

            guna2BtnPercent.Enabled = true;
            guna2BtnInverse.Enabled = true;
            guna2BtnSquare.Enabled = true;
            guna2BtnSqrt.Enabled = true;
            guna2BtnAdd.Enabled = true;
            guna2BtnSub.Enabled = true;
            guna2BtnMul.Enabled = true;
            guna2BtnDiv.Enabled = true;
            guna2BtnPoint.Enabled = true;
            guna2BtnOpposite.Enabled = true;
        }

        private void guna2BtnCE_Click(object sender, EventArgs e)
        {
            guna2BtnPercent.Enabled = true;
            guna2BtnInverse.Enabled = true;
            guna2BtnSquare.Enabled = true;
            guna2BtnSqrt.Enabled = true;
            guna2BtnAdd.Enabled = true;
            guna2BtnSub.Enabled = true;
            guna2BtnMul.Enabled = true;
            guna2BtnDiv.Enabled = true;
            guna2BtnPoint.Enabled = true;
            guna2BtnOpposite.Enabled = true;
        }

        private void guna2Btnx_Click(object sender, EventArgs e)
        {
            guna2BtnPercent.Enabled = true;
            guna2BtnInverse.Enabled = true;
            guna2BtnSquare.Enabled = true;
            guna2BtnSqrt.Enabled = true;
            guna2BtnAdd.Enabled = true;
            guna2BtnSub.Enabled = true;
            guna2BtnMul.Enabled = true;
            guna2BtnDiv.Enabled = true;
            guna2BtnPoint.Enabled = true;
            guna2BtnOpposite.Enabled = true;
        }

        private void guna2BtnAdd_Click(object sender, EventArgs e)
        {
            if (!guna2BtnAdd_flag)
            {
                //value_list.Add(double.Parse(tBOperationProcedure.Text));//将当前输入的数字存起来
                //operator_list.Add(0);
                num1 = double.Parse(tBOperationResult.Text);
                tBOperationProcedure.Text = num1.ToString() + " + ";
                //if (num2 != 0)
                //{ tBOperationProcedure.Text += num2.ToString(); }

                guna2BtnAdd_flag = true;
                guna2BtnSub_flag = true;
                guna2BtnMul_flag = true;
                guna2BtnDiv_flag = true;
                sign = 1;
            }
        }

        private void guna2BtnSub_Click(object sender, EventArgs e)
        {
            if (!guna2BtnSub_flag)
            {
                num1 = double.Parse(tBOperationResult.Text);
                tBOperationProcedure.Text = num1.ToString() + " - ";
                
                guna2BtnAdd_flag = true;
                guna2BtnSub_flag = true;
                guna2BtnMul_flag = true;
                guna2BtnMul_flag = true;
                sign = 2;
            }

        }

        private void guna2BtnMul_Click(object sender, EventArgs e)
        {
            if (!guna2BtnMul_flag)
            {
                num1 = double.Parse(tBOperationResult.Text);
                tBOperationProcedure.Text = num1.ToString() + " * ";
                
                guna2BtnAdd_flag = true;
                guna2BtnSub_flag = true;
                guna2BtnMul_flag = true;
                guna2BtnDiv_flag = true;
                sign = 3;
            }

        }

        
        private void guna2BtnDiv_Click(object sender, EventArgs e)
        {
            if (!guna2BtnDiv_flag)
            {
                num1 = double.Parse(tBOperationResult.Text);
                tBOperationProcedure.Text = num1.ToString() + " / ";

                guna2BtnAdd_flag = true;
                guna2BtnSub_flag = true;
                guna2BtnMul_flag = true;
                guna2BtnDiv_flag = true;
                sign = 4;
            }

        }

        private void guna2BtnEqual_Click(object sender, EventArgs e)
        {
            num2 = double.Parse(tBOperationResult.Text);
            if (!guna2BtnEqual_flag)
            {
                tBOperationProcedure.Text += num2.ToString();
                tBOperationProcedure.Text += " = ";
                switch (sign)
                {
                    case 1:
                        result = num1 + num2;
                        break;
                    case 2:
                        result = num1 - num2;
                        break;
                    case 3:
                        result = num1 * num2;
                        break;

                    case 4:
                        //if (num2 == 0)
                        //{ tBOperationResult.Clear(); tBOperationResult.Text = "除数不能为零"; }
                        //else
                        //{ result = num1 / num2; }
                        result = num1 / num2;
                        break;
                }
                tBOperationResult.Clear();
                if (num2 == 0&&sign==4)
                {
                    tBOperationResult.Text = "除数不能为零";
                    guna2BtnPercent.Enabled = false;
                    guna2BtnInverse.Enabled = false;
                    guna2BtnSquare.Enabled = false;
                    guna2BtnSqrt.Enabled = false;
                    guna2BtnAdd.Enabled = false;
                    guna2BtnSub.Enabled = false;
                    guna2BtnMul.Enabled = false;
                    guna2BtnDiv.Enabled = false;
                    guna2BtnPoint.Enabled = false;
                    guna2BtnOpposite.Enabled = false;
                }
                else
                { tBOperationResult.Text = result.ToString(); }
                guna2BtnEqual_flag = true;

                if(richTBHistory.Text== "尚无历史记录。")
                { richTBHistory.Clear(); }
                richTBHistory.Text += tBOperationProcedure.Text + "\n" + tBOperationResult.Text+"\n\n";
                guna2BtnDel.Visible = true;
            }
        }

    }
    #endregion


}

