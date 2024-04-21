using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using static 文件管理.FileManager;
using ListView = System.Windows.Forms.ListView;

namespace 文件管理
{
    public partial class FileManager : Form
    {
        //实体对象
        private Catalog root = new Catalog("root");//根目录
        private Catalog cur = new Catalog("");//当前目录
        private BitMap bitmap = new BitMap();//内存
        private string dir = Application.StartupPath;//应用路径
        private List<ListViewItem> folderItems = new List<ListViewItem>();//当前文件夹中子成员列表
        ImageList imageList = new ImageList();
        private Label curProjectNum;
        // 字典，实现中文按首字母排序用
        SerializableDictionary<string, string> dict = new SerializableDictionary<string, string>();

        //控件相关
        private ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
        private ToolStripMenuItem 打开, 新建, 新建文件夹, 新建文本文件, 重命名, 删除, 属性;

        private TreeNode rootNode;

        private Button backToLastCatalog;//返回上层目录
        private Button format;//格式化按钮
        private Button info;//说明

        private GroupBox routeBox;
        private PictureBox routeIcon;
        private TextBox routeText;

        private TreeView routeView;

        private ListView folderContent;

        public int nameComparison(Node x, Node y)
        {
            if (x.nodeType != Node.NodeType.file && y.nodeType == Node.NodeType.file)
            {
                return -1;
            }
            else if (x.nodeType == Node.NodeType.file && y.nodeType != Node.NodeType.file)
            {
                return 1;
            }
            else
            {
                string a = x.name, b = y.name;
                while (true)
                {
                    if (a.Length < 2 && b.Length < 2)
                    {
                        return string.Compare(a, b);
                    }
                    else
                    {
                        string t1 = a, t2 = b;
                        bool tag = true;//循环继续标志，当前仅当a b长度同时大于等于2才继续
                        if (a.Length >= 2)
                        {
                            t1 = a.Substring(0, 2);
                            a = a.Substring(2);
                        }
                        else
                        {
                            tag = false;
                        }
                        if (b.Length >= 2)
                        {
                            t2 = b.Substring(0, 2);
                            b = b.Substring(2);
                        }
                        else
                        {
                            tag = false;
                        }
                        if (t1[0] < 0)
                        {
                            if (dict.ContainsKey(t1))
                            {
                                t1 = dict[t1];
                            }
                            else
                            {
                                t1 = "";
                            }
                        }
                        if (t2[0] < 0)
                        {
                            if (dict.ContainsKey(t2))
                            {
                                t2 = dict[t2];
                            }
                            else
                            {
                                t2 = "";
                            }
                        }
                        if (tag && t1 == t2)
                        {
                            continue;
                        }
                        else
                        {
                            return string.Compare(t1, t2);
                        }
                    }
                }
            }
        }
        public int sizeComparison(Node x, Node y)
        {
            if (x.nodeType != Node.NodeType.file && y.nodeType == Node.NodeType.file)
            {
                return -1;
            }
            else if (x.nodeType == Node.NodeType.file && y.nodeType != Node.NodeType.file)
            {
                return 1;
            }
            else if (x.nodeType != Node.NodeType.file && y.nodeType != Node.NodeType.file)
            {
                return nameComparison(x, y);
            }
            else
            {
                //当且仅当x和y均为文本文件的时候才进行比较
                if (x.file.size == y.file.size)
                {
                    return 0;
                }
                else
                {
                    return x.file.size > y.file.size ? 1 : -1;
                }
            }
            //return 0;            
        }

        public FileManager()
        {
            FileStream dictStream = new FileStream("./properties/pinyin.txt", FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(dictStream);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] strings = line.Split('=');
                if (strings[1] == null || strings[1] == "")
                {
                    continue;
                }
                dict.Add(strings[0], strings[1]);
            }
            dictStream.Close();

            if (!Directory.Exists(Path.Combine(@dir, "storage")))
            {
                Directory.CreateDirectory(Path.Combine(@dir, "storage"));
            }
            string catalogPath = Path.Combine(@dir, @"storage\rootCatalog.txt"), bitmapPath = Path.Combine(@dir, @"storage\bitmap.txt");
            FileStream f1, f2;
            BinaryFormatter bf = new BinaryFormatter();
            if (System.IO.File.Exists(catalogPath) && System.IO.File.Exists(bitmapPath))
            {
                //从路径中读取序列化后的文件系统目录，以及文件系统内容
                f1 = new FileStream(catalogPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                root = bf.Deserialize(f1) as Catalog;
                f1.Close();
                f2 = new FileStream(bitmapPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                bitmap = bf.Deserialize(f2) as BitMap;
                f2.Close();
            }
            cur = root;

            this.FormClosing += FileManageSystemQuit;

            InitializeComponent();
        }

        private void FileManager_Load(object sender, EventArgs e)
        {
            this.Size = new Size(750, 600);
            this.ContextMenuStrip = contextMenuStrip;

            info = new Button
            {
                Text = "说明",
                Location = new Point(200, 0),
                Size = new Size(100, 24),
                TextAlign = ContentAlignment.MiddleCenter,
                Parent = this,
                FlatStyle = FlatStyle.Flat,
            };
            info.BringToFront();
            info.FlatAppearance.BorderSize = 0;
            info.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(30, 136, 220);
            info.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(179, 215, 243);
            info.MouseEnter += mouseEnter;
            info.MouseLeave += mouseLeave;
            info.Click += clickInfoButton;
            backToLastCatalog = new Button
            {
                Text = "返回上层目录",
                Parent = this,
                Location = new Point(0, 0),
                Size = new Size(100, 24),
                TextAlign = ContentAlignment.MiddleCenter,
                FlatStyle = FlatStyle.Flat,
            };
            backToLastCatalog.BringToFront();
            backToLastCatalog.FlatAppearance.BorderSize = 0;
            backToLastCatalog.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(30, 136, 220);
            backToLastCatalog.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(179, 215, 243);
            backToLastCatalog.MouseEnter += mouseEnter;
            backToLastCatalog.MouseLeave += mouseLeave;
            backToLastCatalog.Click += clickBackToLastCatalogButton;

            format = new Button
            {
                Text = "格式化",
                Parent = this,
                Location = new Point(100, 0),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(100, 24),
                FlatStyle = FlatStyle.Flat,
            };
            format.BringToFront();
            format.FlatAppearance.BorderSize = 0;
            format.FlatAppearance.BorderColor = Color.FromArgb(30, 136, 220);
            format.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(179, 215, 243);
            format.MouseEnter += mouseEnter;
            format.MouseLeave += mouseLeave;
            format.Click += clickFormatButton;

            routeBox = new GroupBox
            {
                Parent = this,
                Location = new Point(10, 24),
                Size = new Size(this.Width - 30, 36),
            };
            routeBox.BringToFront();

            routeIcon = new PictureBox
            {
                Size = new Size(24, 24),
                Location = new Point(5, 9),
                Parent = routeBox,
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = Image.FromFile(@"properties\文件夹.png"),
            };
            routeIcon.BringToFront();

            routeText = new TextBox
            {
                Text = cur.path,
                Parent = routeBox,
                Size = new Size(this.Width - 75, 24),
                BackColor = Color.White,
                Location = new Point(35, 15),
                BorderStyle = BorderStyle.None,
            };
            routeText.BringToFront();
            routeText.ReadOnly = true;

            imageList.Images.Add(Image.FromFile("./properties/文件夹.png"));
            imageList.Images.Add(Image.FromFile("./properties/文本文件.png"));
            routeView = new TreeView
            {
                Parent = this,
                Location = new Point(10, 65),
                Size = new Size(150, 480),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                ImageList = imageList,
                ImageIndex = 0,
            };

            folderContent = new ListView
            {
                Parent = this,
                Size = new Size(550, 480),
                Location = new Point(160, 65),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                SmallImageList = imageList,
                View = View.Details,
            };
            folderContent.Columns.Add("名称", 137);
            folderContent.Columns.Add("修改日期", 137);
            folderContent.Columns.Add("类型", 137);
            folderContent.Columns.Add("大小", 137);
            folderContent.MouseDoubleClick += folderContentDoubleClickEvent;
            //folderContent.ColumnClick += sort;


            打开 = new ToolStripMenuItem
            {
                Text = "打开(O)",
            };
            打开.Click += menuOpen;
            新建 = new ToolStripMenuItem
            {
                Text = "新建(W)",
            };
            新建文件夹 = new ToolStripMenuItem
            {
                Text = "新建文件夹(F)",
            };
            新建文本文件 = new ToolStripMenuItem
            {
                Text = "新建文本文件",
            };
            新建文件夹.Click += menuBuildFolder;
            新建文本文件.Click += menuBuildText;
           
            新建.DropDownItems.Add(新建文件夹);
            新建.DropDownItems.Add(新建文本文件);
            新建.DropDown.KeyUp += shortCutsInDropDown;

            删除 = new ToolStripMenuItem("删除(D)");
            删除.Click += remove;

            重命名 = new ToolStripMenuItem("重命名(M)");
            重命名.Click += rename;

            属性 = new ToolStripMenuItem("属性(R)");
            属性.Click += attributes;

            contextMenuStrip.Items.AddRange(new ToolStripItem[] { 打开, 新建, 删除, 重命名, 属性 });
            contextMenuStrip.KeyUp += keyboardShortCuts;

            folderContent.KeyUp += folderShortCuts;
            folderContent.SelectedIndexChanged += itemSelectedEvent;

            curProjectNum = new Label
            {
                Text = $"{cur.nodelist.Count}个项目",
                Parent = this,
                AutoSize = true,
                Location = new Point(20, this.Size.Height - 55),
            };
            curProjectNum.BringToFront();

            updateRouteView();
            updateListView();
        }
        private void FileManageSystemQuit(object sender, EventArgs e)
        {
            FileStream f1, f2;
            BinaryFormatter bf = new BinaryFormatter();
            f1 = new FileStream(System.IO.Path.Combine(dir, @"storage\rootCatalog.txt"), FileMode.Create);
            bf.Serialize(f1, root);
            f1.Close();
            f2 = new FileStream(System.IO.Path.Combine(dir, @"storage\bitmap.txt"), FileMode.Create);
            bf.Serialize(f2, bitmap);
            f2.Close();
        }
        private void folderContentDoubleClickEvent(object sender, EventArgs e)
        {
            ListViewItem item = sender as ListViewItem;
            if (folderContent.Items.Count != 0)
            {
                item = folderContent.SelectedItems[0];
                for (int i = 0; i < cur.nodelist.Count(); i++)
                {
                    if (folderItems[i] == item)
                    {
                        Node current_node = cur.nodelist[i];
                        open(ref current_node);
                        break;
                    }
                }
            }
        }
        private void updateView()
        {
            routeText.Text = cur.path;
            updateRouteView();
            updateListView();
        }
        private void updateRouteView()
        {
            routeView.Nodes.Clear();
            rootNode = new TreeNode("root");
            addTreeNode(rootNode, root);
            routeView.Nodes.Add(rootNode);
            routeView.ExpandAll();
        }
        private void updateListView()
        {
            folderContent.Items.Clear();
            folderItems = new List<ListViewItem>();
            if (cur.nodelist != null)
            {
                folderItems = new List<ListViewItem>();
                Comparison<Node> myComparison = nameComparison;
                cur.nodelist.Sort(myComparison);
                for (int i = 0; i < cur.nodelist.Count(); i++)
                {
                    ListViewItem file;
                    if (cur.nodelist[i].nodeType == Node.NodeType.folder)
                    {
                        file = new ListViewItem(new string[]{
                            cur.nodelist[i].folder.name,
                            cur.nodelist[i].folder.updatedTime.ToString(),
                            "文件夹",
                            "",
                            //$"{cur.nodelist[i].folder.childrenNum}",考虑到现实文件系统中，结构及其复杂，不可能实时更新文件子项数量，因此不显示
                        });
                        file.ImageIndex = 0;
                        folderItems.Add(file);
                        folderContent.Items.Add(file);
                    }
                    else if (cur.nodelist[i].nodeType == Node.NodeType.file)
                    {
                        file = new ListViewItem(new string[] {
                            cur.nodelist[i].file.name + ".txt",
                            cur.nodelist[i].file.updatedTime.ToString(),
                            "文本文件",
                            $"{cur.nodelist[i].file.size}B",
                        });
                        file.ImageIndex = 1;
                        folderItems.Add(file);
                        folderContent.Items.Add(file);
                    }
                }
            }
            updateCurrentProjectNum();
        }

        private void updateCurrentProjectNum()
        {
            curProjectNum.Text = $"{cur.nodelist.Count}个项目";
        }
        //将catalog目录下的所有子结点加入到node结点下
        private void addTreeNode(TreeNode node, Catalog catalog)
        {
            if (catalog.nodelist != null)
            {
                for (int i = 0; i < catalog.nodelist.Count(); i++)
                {
                    if (catalog.nodelist[i].nodeType == Node.NodeType.folder)
                    {
                        TreeNode newNode = new TreeNode(catalog.nodelist[i].name);
                        addTreeNode(newNode, catalog.nodelist[i].folder);
                        node.Nodes.Add(newNode);
                    }
                }
            }
        }

        private void open(ref Node node)
        {
            if (node.nodeType == Node.NodeType.folder)
            {
                cur = node.folder;//当前目录修改为进入的文件夹
                routeText.Text = cur.path;//修改上方的当前路径
                updateView();
            }
            else if (node.nodeType == Node.NodeType.file)
            {
                TextEditor textEditor = new TextEditor(ref bitmap, ref node.file, node.name);
                textEditor.Show();
                textEditor.CallBack = updateListView;
            }
        }
        private void openFile()
        {
            ListViewItem listViewItem = new ListViewItem();
            if (folderContent.SelectedItems.Count != 0)
            {
                listViewItem = folderContent.SelectedItems[0];
                for (int i = 0; i < folderContent.Items.Count; i++)
                {
                    if (listViewItem == folderContent.Items[i])
                    {
                        Node curNode = cur.nodelist[i];
                        open(ref curNode);
                    }
                }
            }
        }
        private void menuOpen(object sender, EventArgs e)
        {
            openFile();
        }
        private void buildFolder()
        {
            string name = "新建文件夹";
            string type = "文件夹";
            FormofBuildFile.OperationType otype = FormofBuildFile.OperationType.newfile;
            checkRepeatName(ref name, type, cur);
            FormofBuildFile buildFolder = new FormofBuildFile(ref cur, "新建文件夹", "文件夹", otype);
            buildFolder.Show();
            buildFolder.CallBack = updateView;
        }
        private void menuBuildFolder(object sender, EventArgs e)
        {
            buildFolder();
        }
        private void buildText()
        {
            string name = "新建文本文件";
            string type = "文本文件";
            FormofBuildFile.OperationType otype = FormofBuildFile.OperationType.newfile;
            checkRepeatName(ref name, type, cur);
            FormofBuildFile buildText = new FormofBuildFile(ref cur, name, type, otype);
            buildText.Show();
            buildText.CallBack = updateView;
        }
        private void itemSelectedEvent(object sender, EventArgs e)
        {
            curProjectNum.Text = $"{folderContent.Items.Count}个项目";
            if (folderContent.SelectedItems.Count != 0)
            {
                curProjectNum.Text += $"    {folderContent.SelectedItems.Count}个选中项目";
            }
        }
        private void menuBuildText(object sender, EventArgs e)
        {
            buildText();
        }
        private void removeItem()
        {
            ListViewItem chosenItem = new ListViewItem();
            if (folderContent.SelectedItems.Count != 0)
            {
                chosenItem = folderContent.SelectedItems[0];
                for (int i = 0; i < folderContent.Items.Count; i++)
                {
                    if (chosenItem == folderContent.Items[i])
                    {
                        cur.updatedTime = DateTime.Now;
                        delete(ref cur.nodelist, i);
                        updateFolderSize(ref cur);
                        updateView();
                    }
                }
            }
        }
        private void remove(object sender, EventArgs e)
        {
            removeItem();
        }
        private void renameFile()
        {
            ListViewItem chosenItem = new ListViewItem();
            if (folderContent.SelectedItems.Count != 0)
            {
                chosenItem = folderContent.SelectedItems[0];
                for (int i = 0; i < folderContent.Items.Count; i++)
                {
                    if (folderContent.Items[i] == chosenItem)
                    {
                        string name = cur.nodelist[i].name;
                        string type = cur.nodelist[i].nodeType == Node.NodeType.folder ? "文件夹" : "文本文件";
                        FormofBuildFile.OperationType otype = FormofBuildFile.OperationType.rename;
                        FormofBuildFile buildFolder = new FormofBuildFile(ref cur, name, type, otype);
                        buildFolder.Show();
                        buildFolder.CallBack = updateView;
                    }
                }
            }
        }
        private void rename(object sender, EventArgs e)
        {
            renameFile();
        }
        private void openAttributesPage()
        {
            ListViewItem chosenItem = new ListViewItem();
            if (folderContent.SelectedItems.Count != 0)
            {
                chosenItem = folderContent.SelectedItems[0];
                for (int i = 0; i < cur.nodelist.Count; i++)
                {
                    if (folderContent.Items[i] == chosenItem)
                    {
                        Node node = cur.nodelist[i];
                        AttributesPage attributePage = new AttributesPage(ref node);
                        attributePage.Show();
                    }
                }
            }
        }
        private void attributes(object sender, EventArgs e)
        {
            openAttributesPage();
        }
        private void checkRepeatName(ref string name, string type, Catalog curCatalog)
        {
            int cnt = 0;
            if (type == "文件夹")
            {
                for (int i = 0; i < curCatalog.nodelist.Count; i++)
                {
                    if (curCatalog.nodelist[i].nodeType == Node.NodeType.folder)
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
        }
        private void formatSystem()
        {
            if (root.nodelist.Count() != 0)
            {
                for (int i = 0; i < root.nodelist.Count(); i++)
                {
                    delete(ref root.nodelist, i);
                }
            }
            root = new Catalog("root");
            cur = root;
            updateView();
        }
        private void backToParentCatalog()
        {
            if (cur != root)
            {
                cur = cur.parenCatalog;
                routeText.Text = cur.path;
                updateView();
            }
        }
        private void delete(ref List<Node> nodelist, int pos)
        {
            if (nodelist.Count != 0)
            {
                if (nodelist[pos].nodeType == Node.NodeType.file)
                {
                    nodelist.RemoveAt(pos);
                }
                else if (nodelist[pos].nodeType == Node.NodeType.folder)
                {
                    if (nodelist[pos].folder.nodelist != null)
                    {
                        for (int i = 0; i < nodelist[pos].folder.nodelist.Count; i++)
                        {
                            delete(ref nodelist[pos].folder.nodelist, i);//递归删除文件夹内部结点
                        }
                        nodelist.RemoveAt(pos);//删除文件夹本身结点
                    }
                    else
                    {
                        nodelist.RemoveAt(pos);
                    }
                }
            }
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
                    catalog.childrenFileNum += 1;
                    catalog.fileSize += catalog.nodelist[j].file.size;
                }
                else
                {
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
        private void folderShortCuts(object sedner, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Delete)
            {
                removeItem();
            }
        }
        private void shortCutsInDropDown(object sender, KeyEventArgs e)
        {
            //子菜单内的快捷键
            if(e.KeyCode == Keys.F)
            {
                buildFolder();
            }
        }
        private void keyboardShortCuts(object sender, KeyEventArgs e)
        {
            //MessageBox.Show("111");
            if(e.KeyCode == Keys.W)
            {
                ToolStripDropDown dropDown = 新建.DropDown;
                Point location = new Point(新建.Bounds.Right, 新建.Bounds.Top);
                dropDown.Show(新建.Owner, location);
                新建.Select();
            }
            else if(e.KeyCode == Keys.O)
            {
                //打开
                openFile();
            }
            else if (e.KeyCode == Keys.M)
            {
                renameFile();
            }
            else if(e.KeyCode == Keys.R)
            {
                openAttributesPage();
            }
            else if(e.KeyCode == Keys.D)
            {
                removeItem();
            }
        }
        //鼠标进入按钮
        void mouseEnter(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            btn.FlatAppearance.BorderSize = 1;
        }

        //鼠标离开按钮
        void mouseLeave(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            btn.FlatAppearance.BorderSize = 0;
        }

        //单击backToLastCatalog按钮
        void clickBackToLastCatalogButton(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            backToParentCatalog();
        }

        //单击格式化按钮
        void clickFormatButton(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            formatSystem();
        }
        void clickInfoButton(object sender, EventArgs e)
        {
            MessageBox.Show("本系统作者：2153828朱昱芃\n本系统菜单支持快捷键，菜单后的提示字母即为快捷键\n文本编辑器支持\"Ctrl+S\"一键保存、\"ctrl+W\"一键退出");
        }
        [Serializable]
        public class File
        {
            public const int TXT = 0;

            public int type;   //类型
            public int size;  // 大小
            public String name;  // 文件名
            public DateTime createdTime;  // 创建时间
            public DateTime updatedTime;  // 修改时间
            public List<Block> blockList;  // 文件内容
            public String path;

            public File(String name, String type, String fatherPath)
            {
                this.type = TXT;
                this.name = name;
                createdTime = DateTime.Now;
                updatedTime = DateTime.Now;
                size = 0;
                blockList = new List<Block>();
                path = fatherPath + @"\" + name;
            }

            public void setEmpty(ref BitMap bitmap)
            {
                for (int i = 0; i < blockList.Count(); i += 1)
                {
                    bitmap.setFree(bitmap.findFreeBlock());
                }
                blockList.Clear();
                size = 0;
            }

            public void writeFile(String data, ref BitMap bitmap)
            {
                setEmpty(ref bitmap);
                while (data.Count() > 512)
                {
                    bitmap.blocks[bitmap.findFreeBlock()] = new Block();
                    bitmap.blocks[bitmap.findFreeBlock()].setData(data.Substring(0, 512));
                    blockList.Add(bitmap.blocks[bitmap.findFreeBlock()]);
                    bitmap.setOccupy(bitmap.findFreeBlock());
                    size += 512;
                    data = data.Remove(0, 512);
                }
                bitmap.blocks[bitmap.findFreeBlock()] = new Block();
                bitmap.blocks[bitmap.findFreeBlock()].setData(data);
                blockList.Add(bitmap.blocks[bitmap.findFreeBlock()]);
                bitmap.setOccupy(bitmap.findFreeBlock());
                size += data.Length;
                updatedTime = DateTime.Now;
            }

            public String readFile()
            {
                string content = "";
                for (int i = 0; i < blockList.Count(); i += 1)
                {
                    content += blockList[i].getData();
                }
                return content;
            }


        }
        [Serializable]
        public class BitMap
        {
            public const int byteSize = 8;
            public const int maxCapacity = 100 * 100;
            public const int byteNum = 100 * 100 / 8;
            public Block[] blocks = new Block[maxCapacity];
            public bool[] bitmap = new bool[maxCapacity];

            public BitMap()
            {
                for (int i = 0; i < maxCapacity; i++)
                {
                    bitmap[i] = true;
                }
            }

            public int findFreeBlock()
            {
                int bytePos = 0, bitPos = 0;
                while (bytePos < byteNum)
                {
                    if (bitmap[bytePos * byteSize + bitPos])
                    {
                        return (bytePos * byteSize + bitPos);
                    }
                    else
                    {
                        bitPos += 1;
                        if (bitPos == byteSize)
                        {
                            bitPos = bitPos % byteSize;
                            bytePos += 1;
                        }
                    }
                }
                return -1;
            }

            public void setFree(int i)
            {
                bitmap[i] = true;
            }

            public void setOccupy(int i)
            {
                bitmap[i] = false;
            }
        }
        [Serializable]
        public class Block
        {
            public const int BLOCKSIZE = 512;
            public char[] data;
            public int length;

            public Block()
            {
                data = new char[BLOCKSIZE];
            }

            public void setData(String newData)
            {
                length = (newData.Length > 512) ? 512 : newData.Length;
                for (int i = 0; i < length; i++)
                {
                    data[i] = newData[i];
                }
            }

            public String getData()
            {
                String temp = new String(data);
                return temp;
            }
        }
        [Serializable]
        public class Catalog
        {
            public List<Node> nodelist;//子项结点列表
            public int childrenNum;//子项数量
            public int childrenFolderNum;
            public int childrenFileNum;
            public String name;//名称
            public String path;//路径
            public int fileSize;//大小
            public DateTime createdTime;//创建时间
            public DateTime updatedTime;//更新时间
            public Catalog parenCatalog = null;//父目录


            public Catalog(String namedata, String fatherPath)
            {
                nodelist = new List<Node>();
                name = namedata;
                path = fatherPath + @"\" + namedata;
                createdTime = DateTime.Now;
                updatedTime = DateTime.Now;
                fileSize = 0;
                childrenNum = 0;
                childrenFolderNum = 0;
                childrenFileNum = 0;
            }


            public Catalog(String namedata)
            {
                nodelist = new List<Node>();
                name = namedata;
                path = namedata + ":";
                createdTime = DateTime.Now;
                updatedTime = DateTime.Now;
                fileSize = 0;
                childrenNum = 0;
            }


            public void addNode(Catalog catalog, String namedata, String fatherPath)
            {
                Node node = new Node(namedata, fatherPath);
                node.folder.parenCatalog = catalog;
                nodelist.Add(node);
                childrenNum += 1;
                childrenFolderNum += 1;
                updatedTime = DateTime.Now;
            }


            public void addNode(String namedata, String fileType, String fatherPath)
            {
                Node node = new Node(namedata, fileType, fatherPath);
                nodelist.Add(node);
                childrenNum += 1;
                childrenFileNum += 1;
                updatedTime = DateTime.Now;
            }
        }

        [Serializable]
        public class Node
        {
            public enum NodeType { folder, file };//类型：文件夹/文件
            public NodeType nodeType;//结点类型
            public File file;//文件
            public Catalog folder;//文件夹
            public String path;//路径
            public String name;//名称

            public Node(String namedata, String fatherPath)   //文件夹结点
            {
                nodeType = NodeType.folder;
                path = fatherPath + @"\" + namedata;
                name = namedata;
                folder = new Catalog(namedata, fatherPath);
            }

            public Node(String namedata, String fileType, String fatherPath)    //文件结点
            {
                nodeType = NodeType.file;
                path = fatherPath + @"\" + namedata;
                name = namedata;
                file = new File(name, fileType, fatherPath);
            }

            public void reName(String newName)
            {
                name = newName;
                if (nodeType == Node.NodeType.folder)
                {
                    folder.path = folder.path.Remove(folder.path.Length - folder.name.Length - 1, folder.name.Length + 1);
                    folder.name = newName;
                    folder.path = folder.path + @"\" + folder.name;
                }
                else
                {
                    file.path = file.path.Remove(file.path.Length - file.name.Length - 1, file.name.Length + 1);
                    file.name = newName;
                    file.path = file.path + @"\" + file.name;
                }
            }
        }
    }
}
