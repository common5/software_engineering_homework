using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

using static 文件管理.FileManager;
using static 文件管理.FormofBuildFile;

namespace 文件管理
{
    public partial class TextEditor : Form
    {
        private RichTextBox textBox;
        private Button save;
        private Button quit;
       
        //数据类型
        string title;
        string data;
        bool needSave = false;
        private BitMap bitmap;
        public File file;
        public DelegateMethod.delegateFunction CallBack;

        public TextEditor(ref BitMap bitmap, ref File file, string title = "文本编辑器")
        {
            InitializeComponent();
            this.title = title;
            this.bitmap = bitmap;
            this.file = file;
            data = file.readFile();
            this.FormClosing += formClosing;
        }

        private void TextEditor_Load(object sender, EventArgs e)
        {
            this.Text = title;
            textBox = new RichTextBox
            {
                Size = new Size(this.Width - 80, this.Height - 90),
                Parent = this,
                Location = new Point(30, 20),
                Text = file.readFile(),
            };
            textBox.BringToFront();
            textBox.TextChanged += textChanged;
            textBox.KeyUp += keyboardShortCuts;//添加快捷键, 支持ctrl+s保存
            save = new Button
            {
                Text = "保存",
                Parent = this,
                Location = new Point(this.Width - 160, this.Height - 70),
                Font = new Font("新宋体", 12),
                Size = new Size(50, 25),
            };
            save.BringToFront();
            save.Click += saveEvent;
            quit = new Button
            {
                Text = "退出",
                Parent = this,
                Location = new Point(this.Width - 100, this.Height - 70),
                Font = new Font("新宋体", 12),
                Size = new Size(50, 25),
            };
            quit.BringToFront();
            quit.Click += quitEvent;
        }
        private void formClosing(object sender, EventArgs e)
        {
            if(needSave)
            {
                if (MessageBox.Show("保存更改?", "提示信息", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    file.writeFile(textBox.Text, ref bitmap);
                    file.updatedTime = DateTime.Now;
                }
                this.callBack();
            }
        }
        private void keyboardShortCuts(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.S && e.Control)
            {
                this.Text = title;
                needSave = false;
                file.writeFile(textBox.Text, ref bitmap);
                file.updatedTime = DateTime.Now;
                data = textBox.Text;
                this.callBack();
            }
            if(e.KeyCode == Keys.W && e.Control)
            {
                this.Close();
            }
        }
        private void saveEvent(object sender, EventArgs e)
        {
            this.Text = title;
            needSave = false;
            file.writeFile(textBox.Text, ref bitmap);
            file.updatedTime = DateTime.Now;
            data = textBox.Text;
            this.callBack();
        }
        private void quitEvent(object sneder, EventArgs e)
        {
            this.Close();
        }
        private void textChanged(object sender, EventArgs e)
        {
            this.Text = title + "(未保存)";
            needSave = true;
        }
        private void callBack()
        {
            if(this.CallBack != null)
            {
                this.CallBack();
            }
        }
    }
}
