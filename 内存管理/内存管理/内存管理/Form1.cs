using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 内存管理
{
    public partial class Form1 : Form
    {

        private int initialMemorySize;
        private GroupBox memoryParameterControl;
        public NumericUpDown initialMemorySizeNumeric;
        private Button memorySizeModifyButton;

        private int requestedMemorySize;
        private GroupBox requestMemoryControl;
        public NumericUpDown requestedMemorySizeNumeric;
        public ComboBox requestChoice;
        private Button memoryRequestButton;

        private GroupBox removeTaskControl;
        public NumericUpDown removeTaskNumNumeric;
        public ComboBox removeChoice;
        private Button removeTaskButton;

        private Size formSize;

        private GroupBox firstFitGroupBox;
        private RichTextBox firstFitBox;
        private Label firstFitBoxLabel;
        private Label firstFitSpaceLabel;
        int[] firstFitSpace;
        private GroupBox bestFitGroupBox;
        private RichTextBox bestFitBox;
        private Label bestFitBoxLabel;
        private Label bestFitSpaceLabel;
        int[] bestFitSpace;

        int ffnum = 1;
        int bfnum = 1;

        List<MemoryBlock> firstFitMemoryBlocks;
        List<MemoryBlock> bestFitMemoryBlocks;

        public Form1(int initialMemorySize = 640)
        {
            this.initialMemorySize = initialMemorySize;
            this.requestedMemorySize = 0;
            this.formSize = new Size(Math.Max(initialMemorySize + 44, 520), 800);
            this.Load += Form1_Load;
            this.AutoScroll = true;
            this.FormClosed += Form1_FormClosed;
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Size = formSize;

            memoryParameterControl = new GroupBox
            {
                Text = "总内存大小控制",
                Parent = this,
                Font = new Font("Consolas", 12),
                Location = new Point(10, 10),
                Size = new Size(240, 80)
            };
            memoryParameterControl.BringToFront();

            initialMemorySizeNumeric = new NumericUpDown
            {
                Minimum = 480,
                Maximum = 800,
                Value = initialMemorySize,
                Parent = memoryParameterControl,
                Location = new Point(10, 30),
                Enabled = true,
                ReadOnly = false,
            };

            memorySizeModifyButton = new Button
            {
                Text = "修改",
                Parent = memoryParameterControl,
                AutoSize = true,
                Font = new Font("宋体", 12),
                Location = new Point(150, 30),
            };
            memorySizeModifyButton.Click += initialMemorySizeModify;

            requestMemoryControl = new GroupBox
            {
                Text = "内存空间申请",
                Parent = this,
                Font = new Font("Consolas", 12),
                Location = new Point(10, 110),
                Size = new Size(240, 120)
            };
            requestMemoryControl.BringToFront();

            requestedMemorySizeNumeric = new NumericUpDown
            {
                Value = requestedMemorySize,
                Parent = requestMemoryControl,
                Location = new Point(10, 30),
                Size = new Size(130, 30),
                Minimum = 0,
                Maximum = initialMemorySize,
            };

            requestChoice = new ComboBox
            {
                Text = "选择算法",
                Parent = requestMemoryControl,
                Location = new Point(10, 70),
                Size = new Size(130, 30),
            };
            requestChoice.Items.Add("首次适应算法");
            requestChoice.Items.Add("最佳适应算法");

            memoryRequestButton = new Button
            {
                Text = "申请",
                Parent = requestMemoryControl,
                AutoSize = true,
                Font = new Font("宋体", 12),
                Location = new Point(150, 68),
            };
            memoryRequestButton.Click += memoryRequest;

            removeTaskControl = new GroupBox
            {
                Text = "要释放的任务的编号",
                Parent = this,
                Font = new Font("Consolas", 12),
                Location = new Point(270, 110),
                Size = new Size(240, 120)
            };
            removeTaskControl.BringToFront();

            removeTaskNumNumeric = new NumericUpDown
            {
                Value = 0,
                Parent = removeTaskControl,
                Location = new Point(10, 30),
                Minimum = 1,
                Maximum = initialMemorySize,
                Size = new Size(130, 30),
            };

            removeChoice = new ComboBox
            { 
                Text = "选择算法",
                Parent = removeTaskControl,
                Location = new Point(10, 70),
                Size = new Size(130, 30),
            };
            removeChoice.Items.Add("首次适应算法");
            removeChoice.Items.Add("最佳适应算法");
            removeTaskButton = new Button
            {
                Text = "释放",
                Parent = removeTaskControl,
                AutoSize = true,
                Font = new Font("宋体", 12),
                Location = new Point(150, 68),
            };
            removeTaskButton.Click += removeRequest;

            firstFitMemoryBlocks = new List<MemoryBlock>();
            bestFitMemoryBlocks = new List<MemoryBlock>();

            firstFitSpace = new int[this.initialMemorySize + 5];
            bestFitSpace = new int[this.initialMemorySize + 5];
            for (int i = 0; i < this.initialMemorySize; i++)
            {
                firstFitSpace[i] = 0;
            }
            for (int i = 0; i < this.initialMemorySize; i++)
            {
                bestFitSpace[i] = 0;
            }

            firstFitGroupBox = new GroupBox
            {
                Location = new Point(10, 240),
                Parent = this,
                Text = "首次适应算法",
                Font = new Font("Consolas", 12),
                Size = new Size(this.formSize.Width - 20, 250),
            };
            firstFitGroupBox.BringToFront();

            firstFitBoxLabel = new Label
            {
                Parent = firstFitGroupBox,
                BackColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                Location = new Point(10, 25),
                Size = new Size(firstFitGroupBox.Width - 20, 100),
            };

            firstFitBox = new RichTextBox
            {
                Text = "",
                Parent = firstFitGroupBox,
                BorderStyle = BorderStyle.None,
                Location = new Point(10, 135),
                Size = new Size(firstFitGroupBox.Width - 20, 100),
            };

            bestFitGroupBox = new GroupBox
            {
                Location = new Point(10, 500),
                Parent = this,
                Text = "最佳适应算法",
                Font = new Font("Consolas", 12),
                Size = new Size(this.formSize.Width - 20, 250),
            };
            bestFitGroupBox.BringToFront();

            bestFitBoxLabel = new Label
            {
                Parent = bestFitGroupBox,
                BackColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                Location = new Point(10, 25),
                Size = new Size(bestFitGroupBox.Width - 20, 100),
            };

            bestFitBox = new RichTextBox
            {
                Text = "",
                Parent = bestFitGroupBox,
                BorderStyle = BorderStyle.None,
                Location = new Point(10, 135),
                Size = new Size(bestFitGroupBox.Width - 20, 100),
            };
        }
        void initialMemorySizeModify(object sender, EventArgs e)
        {
            int newSize = int.Parse(initialMemorySizeNumeric.Value.ToString());
            MessageBox.Show(newSize.ToString());
            if (newSize == initialMemorySize)
            {
                MessageBox.Show("总内存空间未改变！");
                return;
            }
            Form1 form = new Form1(newSize);
            Hide();//隐藏旧窗体
            form.TopMost = true;
            form.Show();
        }
        void memoryRequest(object sender, EventArgs e)
        {
            int[] ffsum = new int[this.initialMemorySize + 1];
            int[] bfsum = new int[this.initialMemorySize + 1];
            int size = int.Parse(requestedMemorySizeNumeric.Value.ToString());
            string choice = requestChoice.Text.ToString();
            for (int i = 0; i <= this.initialMemorySize; i++)
            {
                ffsum[i] = (i > 0 ? firstFitSpace[i - 1] + ffsum[i - 1] : 0);
                bfsum[i] = (i > 0 ? bestFitSpace[i - 1] + bfsum[i - 1] : 0);
            }
            if (choice == "首次适应算法")
            {
                for (int i = 0; i < this.initialMemorySize; i++)
                {
                    if (i + size > this.initialMemorySize)
                    {
                        firstFitBox.AppendText("没有足够的可分配空间！\n");
                        break;
                    }
                    if (ffsum[i + size] - ffsum[i] == 0)
                    {
                        for (int j = i; j < i + size; j++)
                        {
                            firstFitSpace[j] = 1;
                        }
                        MemoryBlock ffmemoryBlock = new MemoryBlock(ffnum, size, firstFitBoxLabel.Height, 12 + i, 25, this.firstFitGroupBox);
                        ffmemoryBlock.BringToFront();
                        firstFitMemoryBlocks.Add(ffmemoryBlock);
                        firstFitBox.AppendText($"编号为: {ffnum}的任务分配完成！\n");
                        ffnum++;
                        break;
                    }
                }
            }
            else if(choice == "最佳适应算法")
            {
                int bestX = -1, minFitSpace = 1234567;
                for(int i = 0; i < this.initialMemorySize; i++)
                {
                    int space = 1234567, end = 0;
                    for (int j = i + size; j < this.initialMemorySize; j++)
                    {
                        if (bfsum[i + size] - bfsum[i] != 0)
                        {
                            break;
                        }
                        space = j - i;
                        end = j;
                    }
                    if(space < minFitSpace)
                    {
                        bestX = i;
                        minFitSpace = space;
                        i = end;
                    }
                }
                if(bestX == -1)
                {
                    bestFitBox.AppendText("没有足够的可分配空间！\n");
                    return;
                }
                else
                {
                    for(int i = bestX; i < bestX + size; i++)
                    {
                        bestFitSpace[i] = 1;
                    }
                    MemoryBlock memoryBlock = new MemoryBlock(bfnum, size, bestFitBoxLabel.Height, 12 + bestX, 25, bestFitGroupBox);
                    memoryBlock.BringToFront();
                    bestFitMemoryBlocks.Add(memoryBlock);
                    bestFitBox.AppendText($"编号为: {bfnum}的任务分配完成！\n");
                    bfnum++;
                }
            }
            else
            {
                MessageBox.Show("未选择需要使用的算法！");
            }
        }
        void removeRequest(object sender, EventArgs e)
        {
            string choice = removeChoice.Text;
            int num = int.Parse(removeTaskNumNumeric.Value.ToString());
            if(choice == "首次适应算法")
            {
                int pos = -1;
                for(int i = 0; i < firstFitMemoryBlocks.Count(); i++)
                {
                    if (firstFitMemoryBlocks[i].getNum() == num)
                    {
                        pos = i;
                        break;
                    }
                }
                if(pos == -1)
                {
                    MessageBox.Show($"编号为{num}的任务不存在！");
                    return;
                }
                int start = firstFitMemoryBlocks[pos].getPosX();
                int size = firstFitMemoryBlocks[pos].getWidth();
                for (int j = start; j < start + size; j++)
                {
                    firstFitSpace[j] = 0;
                }
                firstFitMemoryBlocks[pos].remove();
                firstFitMemoryBlocks.RemoveAt(pos);


            }
            else if(choice == "最佳适应算法")
            {
                int pos = -1;
                for (int i = 0; i < bestFitMemoryBlocks.Count(); i++)
                {
                    if (bestFitMemoryBlocks[i].getNum() == num)
                    {
                        pos = i;
                        break;
                    }
                }
                if (pos == -1)
                {
                    MessageBox.Show($"编号为{num}的任务不存在！");
                    return;
                }
                int start = bestFitMemoryBlocks[pos].getPosX();
                int size = bestFitMemoryBlocks[pos].getWidth();
                for (int j = start; j < start + size; j++)
                {
                    bestFitSpace[j] = 0;
                }
                bestFitMemoryBlocks[pos].remove();
                bestFitMemoryBlocks.RemoveAt(pos);
            }
            else
            {
                MessageBox.Show("未选择需要使用的算法！");
            }
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();//释放内存
        }
    }
}
