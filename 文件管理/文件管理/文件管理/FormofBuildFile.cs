using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static 文件管理.FileManager;

namespace 文件管理
{
    public partial class FormofBuildFile : Form
    {
        //实体成员
        Catalog curCatalog;
        string name;
        string type;
        public enum OperationType { newfile, rename };
        public OperationType operation_type;
        public DelegateMethod.delegateFunction CallBack;
        //控件成员
        public TextBox textBox;
        public Button confirm;
        public Button cancel;
        public FormofBuildFile(ref Catalog curCatalog, string name, string type, OperationType operation_type)
        {
            InitializeComponent();
            this.curCatalog = curCatalog;
            this.name = name;
            this.type = type;
            this.operation_type = operation_type;
            this.FormClosed += formClosed;
        }
        
        private void FormofBuildFile_Load(object sender, EventArgs e)
        {
            this.AutoScroll = true;
            textBox = new TextBox
            {
                Text = name,
                Location = new Point(10, 20),
                Parent = this,
                Size = new Size(this.Size.Width - 30, 20),
                Font = new Font("宋体", 12),
            };
            textBox.Text = checkRepeatName(textBox.Text);
            confirm = new Button
            {
                Text = "确认",
                Location = new Point(this.Size.Width - 120, this.Size.Height - 75),
                Parent = this,
                Size = new Size(50, 25),
                Font = new Font("宋体", 12),
            };
            cancel = new Button
            {
                Text = "取消",
                Location = new Point(this.Size.Width - 70, this.Size.Height - 75),
                Parent = this,
                Size = new Size(50, 25),
                Font = new Font("宋体", 12),
            };
            confirm.BringToFront();
            confirm.Click += confirmEvent;
            cancel.BringToFront();
            cancel.Click += cancelEvent;
        }
        private string checkRepeatName(string name)
        {
            int cnt = 0;
            if (type == "文件夹")
            {
                for (int i = 0; i < curCatalog.nodelist.Count; i++)
                {
                    if (curCatalog.nodelist[i].nodeType == Node.NodeType.folder)
                    {
                        int pos = 0;//从左到右找到最后一个左括号，它的左边是文件的本名
                        for(int x = 0; x < curCatalog.nodelist[i].name.Length; x++)
                        {
                            if (curCatalog.nodelist[i].name[x] == '(')
                            {
                                pos = x;
                            }
                        }
                        if(pos == 0)
                        {
                            pos = curCatalog.nodelist[i].name.Length;
                        }
                        string tmp = curCatalog.nodelist[i].name.Substring(0, pos);//文件夹本名
                        if(name == tmp)
                        {
                            cnt++;
                        }
                    }
                }
                if (cnt > 0)
                {
                    name += $"({cnt})";
                }
            }
            else
            {
                for (int i = 0; i < curCatalog.nodelist.Count; i++)
                {
                    if (curCatalog.nodelist[i].nodeType == Node.NodeType.file)
                    {
                        int pos = 0;//从左到右找到最后一个左括号，它的左边是文件的本名
                        for (int x = 0; x < curCatalog.nodelist[i].name.Length; x++)
                        {
                            if (curCatalog.nodelist[i].name[x] == '(')
                            {
                                pos++;
                            }
                        }
                        string tmp = curCatalog.nodelist[i].name.Substring(0, pos);//文件夹本名
                        if (name == tmp)
                        {
                            cnt++;
                        }
                    }
                }
                if (cnt > 0)
                {
                    name += $"({cnt})";
                }
            }
            return name;
        }
        private void formClosed(object sender, FormClosedEventArgs e)
        {

        }
        private void confirmEvent(object sender, EventArgs e)
        {
            String fatherPath = curCatalog.path;
            textBox.Text = checkRepeatName(textBox.Text);//查重名
            if(operation_type == OperationType.newfile)
            {
                if(type == "文件夹")
                {
                    if (textBox.Text != "")
                    {
                        name = textBox.Text;
                    }
                    curCatalog.addNode(curCatalog, name, fatherPath);
                }
                else if(type == "文本文件")
                {
                    name = textBox.Text;
                    curCatalog.addNode(name, type, fatherPath);
                }
            }
            else if(operation_type == OperationType.rename)
            {
                if (type == "文件夹")
                {
                    for (int i = 0; i < curCatalog.nodelist.Count(); i += 1)
                    {
                        if (curCatalog.nodelist[i].name == name && curCatalog.nodelist[i].nodeType == Node.NodeType.folder)//找到同名文件夹
                        {
                            curCatalog.nodelist[i].reName(textBox.Text);
                            break;
                        }
                    }
                }
                else if (type == "文本文件")
                {
                    for (int i = 0; i < curCatalog.nodelist.Count(); i += 1)
                    {
                        if (curCatalog.nodelist[i].name == name && curCatalog.nodelist[i].nodeType == Node.NodeType.file)//找到同名文件
                        {

                            curCatalog.nodelist[i].reName(textBox.Text);
                            break;
                        }
                    }
                }
            }
            callBack();
            this.Close();
        }
        private void cancelEvent(object sender, EventArgs e)
        {
            this.Close();
        }
        private void callBack()
        {
            if (CallBack != null)
            {
                if (curCatalog.parenCatalog != null)
                {
                    curCatalog.parenCatalog.updatedTime = DateTime.Now;//文件时间
                }
                this.CallBack();
            }
        }
        public class DelegateMethod
        {
            public delegate void delegateFunction();
        }
    }
}
