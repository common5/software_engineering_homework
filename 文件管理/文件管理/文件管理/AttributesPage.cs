using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static 文件管理.FileManager;

namespace 文件管理
{
    public partial class AttributesPage : Form
    {
        //控件类型
        ImageList imageList;
        PictureBox icon;
        TextBox nameBox;
        Label typeLabel, typeBox;
        Label posLabel, posBox;
        Label sizeLabel, sizeBox;
        Label containLabel, containBox;
        Label createTimeLabel, createTimeBox;
        Label updateTimeLabel, updateTimeBox;
        //数据类型
        Node node;
        public AttributesPage(ref Node node)
        {
            this.node = node;
            if(node.nodeType == Node.NodeType.folder)
            {
                updateFolderSize(ref node.folder);//更新文件夹大小
            }
            InitializeComponent();
        }

        private void AttributesPage_Load(object sender, EventArgs e)
        {
            this.BackColor = Color.White;
            this.AutoScroll = true;
            imageList = new ImageList();
            imageList.ImageSize = new Size(100, 100);
            imageList.Images.Add(Image.FromFile("./properties/文件夹.png"));
            imageList.Images.Add(Image.FromFile("./properties/文本文件.png"));
            icon = new PictureBox
            {
                Image = imageList.Images[(node.nodeType == Node.NodeType.folder ? 0 : 1)],
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point(25, 25),
                Size = new Size(50, 50),
                Parent = this,
            };
            icon.BringToFront();
            nameBox = new TextBox
            {
                ReadOnly = true,
                Parent = this,
                Text = node.name + (node.nodeType == Node.NodeType.folder ? "" : ".txt"),
                Location = new Point(100, 37),
                Size = new Size(250, 25),
            };
            nameBox.BringToFront();
            typeLabel = new Label
            {
                Text = "类型: ",
                Parent = this,
                Location = new Point(30, 90),
                AutoSize = true,
                BackColor = this.BackColor,
                BorderStyle = BorderStyle.None,
                //Font = new Font("新宋体", 10),
            };
            typeLabel.BringToFront();
            typeBox = new Label
            {
                Text = (node.nodeType == Node.NodeType.folder ? "文件夹" : "TXT文件"),
                Parent = this,
                Location = new Point(100, 90),
                AutoSize = true,
                BackColor = this.BackColor,
                BorderStyle = BorderStyle.None,
            };
            typeBox.BringToFront();
            posLabel = new Label
            {
                Text = "位置: ",
                Parent = this,
                Location = new Point(30, 130),
                AutoSize = true,
                BackColor = this.BackColor,
                BorderStyle = BorderStyle.None,
            };
            posLabel.BringToFront();
            posBox = new Label
            {
                Text = node.path,
                Parent = this,
                AutoSize = true,
                Location = new Point(100, 130),
                BackColor = this.BackColor,
                BorderStyle = BorderStyle.None,
            };
            posBox.BringToFront();
            sizeLabel = new Label
            {
                Text = "大小: ",
                Parent = this,
                Location = new Point(30, 170),
                AutoSize = true,
                BackColor = this.BackColor,
                BorderStyle = BorderStyle.None,
            };
            sizeLabel.BringToFront();
            sizeBox = new Label
            {
                Text = (node.nodeType == Node.NodeType.folder ? $"{node.folder.fileSize}B" : $"{node.file.size}B"),
                Parent = this,
                AutoSize = true,
                Location = new Point(100, 170),
                BackColor = this.BackColor,
                BorderStyle = BorderStyle.None,
            };
            sizeBox.BringToFront();
            
            containLabel = new Label
            {
                Text = "包含: ",
                Parent = this,
                AutoSize = true,
                Location = new Point(30, 210),
                BackColor = this.BackColor,
                BorderStyle = BorderStyle.None,
            };
            containLabel.BringToFront();
            containBox = new Label
            {
                Text = (node.nodeType == Node.NodeType.folder ? $"{node.folder.childrenFileNum}个文件, {node.folder.childrenFolderNum}个文件夹" : "-"),
                Parent = this,
                AutoSize = true,
                Location = new Point(100, 210),
                BackColor = this.BackColor,
                BorderStyle = BorderStyle.None,
            };
            containBox.BringToFront();
            createTimeLabel = new Label
            {
                Text = "创建时间: ",
                Parent = this,
                AutoSize = true,
                Location = new Point(30, 250),
                BackColor = this.BackColor,
                BorderStyle = BorderStyle.None,
            };
            createTimeLabel.BringToFront();
            createTimeBox = new Label
            {
                Text = $"{(node.nodeType == Node.NodeType.folder ? node.folder.createdTime:node.file.createdTime)}",
                Parent = this,
                AutoSize = true,
                Location = new Point(100, 250),
                BackColor = this.BackColor,
                BorderStyle = BorderStyle.None,
            };
            createTimeBox.BringToFront();
            updateTimeLabel = new Label
            {
                Text = "修改时间: ",
                Parent = this,
                AutoSize = true,
                Location = new Point(30, 290),
                BackColor = this.BackColor,
                BorderStyle = BorderStyle.None,
            };
            updateTimeLabel.BringToFront();
            updateTimeBox = new Label
            {
                Text = $"{(node.nodeType == Node.NodeType.folder ? $"{node.folder.updatedTime}" : $"{node.file.updatedTime}")}",
                Parent = this,
                AutoSize = true,
                Location = new Point(100, 290),
                BackColor = this.BackColor,
                BorderStyle = BorderStyle.None,
            };
            updateTimeBox.BringToFront();
        }
        public void updateFolderSize(ref Catalog catalog)
        {
            catalog.fileSize = 0;
            catalog.childrenFileNum = 0;
            catalog.childrenFolderNum = 0;
            catalog.childrenNum = 0;
            for (int j = 0; j < catalog.nodelist.Count(); j++)
            {
                
                if (catalog.nodelist[j].nodeType == Node.NodeType.file)
                {
                    if (catalog.updatedTime < catalog.nodelist[j].file.updatedTime)
                    {
                        catalog.updatedTime = catalog.nodelist[j].file.updatedTime;
                    }
                    catalog.childrenFileNum += 1;
                    catalog.fileSize += catalog.nodelist[j].file.size;
                }
                else
                {
                    if (catalog.updatedTime < catalog.nodelist[j].folder.updatedTime)
                    {
                        catalog.updatedTime = catalog.nodelist[j].folder.updatedTime;
                    }
                    catalog.childrenFolderNum += 1;
                    Catalog tmp = catalog.nodelist[j].folder;
                    updateFolderSize(ref tmp);
                    catalog.childrenFolderNum += tmp.childrenFolderNum;//父目录的文件夹子节点数加等于子目录的文件夹子节点数
                    catalog.childrenFileNum += tmp.childrenFileNum;//父目录的文件子节点数加等于子目录的文件子节点数
                    catalog.childrenNum += tmp.childrenNum;//同理
                    catalog.fileSize += tmp.fileSize;
                }
                catalog.childrenNum += 1;
            }
        }
    }
}
