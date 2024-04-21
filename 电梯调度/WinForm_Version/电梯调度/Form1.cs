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

namespace 电梯调度
{
    public partial class Form1 : Form
    {
        private int elevatorNum;
        private int floorNum;
        private int elevatorSize;

        private NumericUpDown elevatorNumeric;
        private NumericUpDown floorNumeric;
        //private int maxSize;
        public ElevatorControl[] elevator;
        private RichTextBox richTextBox;

        private int richTextBoxHeight;
        private int richTextBoxWidth;

        private Timer algorithmTimer;

        //private NumericUpDown heightNumeric;
        //private NumericUpDown personNumeric;

        public GroupBox elevatorOuterControl;
        public GroupBox elevatorParameterControl;

        private List<ElevatorRequest> elevatorRequests;

        private Size buttonSize;
        private Size formSize;

        private Label tag;

        private Point startLocation;

        private FontFamily numberFont;

        public class ElevatorControl
        {
            public Elevator elevator;
            public Button[] floorButton;//外部按钮
            public Button warningButton;
            public Button forceOpenButton;
            public Button forceCloseButton;
        }

        public class ElevatorRequest
        {
            public int startFloor;//请求的起始楼层
            public bool up;//是否上楼
            public int elevatorIndex;//调用的电梯编号
            public Button requestButton;//触发请求的按钮
            public ElevatorRequest(int startFloor, bool up, Button requestButton)
            {
                this.startFloor = startFloor;
                this.up = up;
                elevatorIndex = -1;
                this.requestButton = requestButton;
            }
        }
        private int Max(int x, int y)
        {
            return x > y ? x : y;
        }
        public Form1(int elevatorNum = 5, int floorNum = 20)
        {
            this.elevatorNum = elevatorNum;
            this.floorNum = floorNum;
            this.elevatorSize = 120;
            this.buttonSize = new Size(60, 30);
            this.AutoScroll = true;
            this.richTextBoxHeight = 100;
            this.richTextBoxWidth = Max(25 + (elevatorNum - 1) * (elevatorSize + elevatorSize / 2) + 25, 200);
            //int y = Max(richTextBoxHeight + 20 + (floorNum + 1) / 2 * (buttonSize.Height + 5) + elevatorSize + 200, richTextBoxHeight + 20 + floorNum * (buttonSize.Height + 5));
            this.startLocation = new Point(50, richTextBoxHeight + 20 + (floorNum + 1) / 2 * (buttonSize.Height + 5) + elevatorSize);
            this.formSize = new Size(richTextBoxWidth + elevatorSize + elevatorSize / 2 + 300, richTextBoxHeight + 20 + (floorNum + 1) / 2 * (buttonSize.Height + 5) + elevatorSize + 200);
            this.Load += new EventHandler(Form1_Load);//调用Form1_Load函数
            this.FormClosed += Form1_FormClosed;
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            /// <summary>
            /// 添加第三方字体
            /// </summary>
            PrivateFontCollection pfc = new PrivateFontCollection(); // using System.Drawing.Text;
            string[] fontNames = { "myFont\\digital.ttf" };
            for (int i = 0; i < fontNames.Length; i++)
            {
                pfc.AddFontFile(fontNames[i]);
            }
            this.numberFont = pfc.Families[0];//设置私有的字体族

            //字体大小设置
            this.Size = this.formSize;

            //起始位置设置
            Point position = startLocation;

            //电梯数组初始化
            elevator = new ElevatorControl[elevatorNum + 1];

            for (int i = 0; i < elevatorNum; i++)
            {
                this.elevator[i] = new ElevatorControl
                {
                    elevator = new Elevator(this, floorNum, elevatorSize, position, numberFont),
                    floorButton = new Button[floorNum],
                };

                //起始位置初始化
                Point buttonLocation = new Point(position.X + 0, position.Y - 85);

                //警报按钮
                elevator[i].warningButton = new Button
                {
                    BackgroundImage = Image.FromFile(@"img\警报2.png"),
                    Name = i.ToString(),
                    BackgroundImageLayout = ImageLayout.Zoom,
                    //Enabled = false,
                    Location = new Point(position.X - 30, position.Y + 45),
                    Parent = this,
                    Size = new Size(30, 30),
                };
                elevator[i].warningButton.Click += WarningRequest;

                //强制开按钮
                elevator[i].forceOpenButton = new Button
                {
                    Text = "开",
                    Name = '+' + i.ToString(),
                    Location = new Point(position.X - 30, position.Y + 15),
                    Parent = this,
                    Size = new Size(30, 30),
                };
                elevator[i].forceOpenButton.Click += ForceOpen;

                //强制关按钮
                elevator[i].forceCloseButton = new Button
                {
                    Text = "关",
                    Name = '-' + i.ToString(),
                    Location = new Point(position.X - 30, position.Y + 75),
                    Parent = this,
                    Size = new Size(30, 30),
                };
                elevator[i].forceCloseButton.Click += ForceClose;

                for (int j = 0; j < floorNum; j += 2)
                {
                    elevator[i].floorButton[j] = new Button
                    {
                        Text = (j + 1).ToString(),
                        Location = buttonLocation,
                        Size = buttonSize,
                        Parent = this,
                        Name = i.ToString() + "_" + (j + 1).ToString(),
                    };
                    if (j + 1 >= floorNum)
                    {
                        break;
                    }
                    elevator[i].floorButton[j].Click += InnerRequest;//添加点击事件
                    buttonLocation.X += buttonSize.Width + 5;
                    elevator[i].floorButton[j + 1] = new Button
                    {
                        Text = (j + 2).ToString(),
                        Location = buttonLocation,
                        Size = buttonSize,
                        Parent = this,
                        Name = i.ToString() + "_" + (j + 2).ToString(),
                    };
                    elevator[i].floorButton[j + 1].Click += InnerRequest;//添加点击事件
                    buttonLocation.X -= buttonSize.Width + 5;
                    buttonLocation.Y -= buttonSize.Height + 5;
                }

                //elevator[i].warningButton.Enabled = true;
                position.X += elevatorSize + elevatorSize / 2;
            }

            richTextBox = new RichTextBox
            {
                Location = new Point(80, 40),
                Size = new Size(richTextBoxWidth, richTextBoxHeight),
                HideSelection = false,
                Text = "本程序为电梯调度程序",
                Parent = this,
                Font = new Font("宋体", 12),
            };
            int x = 80 + richTextBoxWidth + 60, y = 30;

            elevatorParameterControl = new GroupBox
            {
                Location = new Point(x, y),
                Parent = this,
                Text = "电梯参数控制",
                Font = new Font("Consolas", 12),
                Size = new Size(formSize.Width - x - 50, richTextBoxHeight),
            };

            Label elevatorNumLabel = new Label()
            {
                Text = "电梯数量",
                Font = new Font("宋体", 10),
                Parent = elevatorParameterControl,
                Location = new Point(15, 30),
                AutoSize = true,
            };

            elevatorNumeric = new NumericUpDown
            {
                Value = elevatorNum,
                Location = new Point(90, 25),
                Font = new Font(numberFont, 12),
                Minimum = 1,
                Size = new Size(50, 50),
                Parent = elevatorParameterControl
            };

            Label floorNumLabel = new Label()
            {
                Text = "高度",
                Font = new Font("宋体", 10),
                Location = new Point(145, 30),
                AutoSize = true,
                Parent = elevatorParameterControl
            };

            floorNumeric = new NumericUpDown
            {
                Value = floorNum,
                Parent = elevatorParameterControl,
                Font = new Font(numberFont, 12),
                Location = new Point(185, 25),
                Minimum = 2,
                Size = new Size(50, 50)
            };

            Button reset = new Button
            {
                //Text = elevatorParameterControl.Size.Width.ToString(),
                Text = "修改",
                Font = new Font("宋体", 12),
                Parent = elevatorParameterControl,
                Location = new Point(200, 60),
            };
            reset.Click += parameterReset;//绑定到参数修改函数

            //我的标签
            tag = new Label
            {
                Text = "操作系统作业\n2153828\n朱昱芃",
                Parent = this,
                Location = new Point(x, y + 140 + 20 + (floorNum / 2 + floorNum % 2) * (buttonSize.Height + 5) + 30),
                Size = new Size(formSize.Width - x - 50, 120),
                BorderStyle = BorderStyle.Fixed3D,
                Font = new Font("Consolas", 20),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.White,
            };

            //电梯外部控制Box初始化
            elevatorOuterControl = new GroupBox
            {
                Text = "电梯外部控制",
                Parent = this,
                Name = "电梯外部控制",
                Font = new Font("Consolas", 12),
                Size = new Size(formSize.Width - x - 50, 20 + (floorNum / 2 + floorNum % 2) * (buttonSize.Height + 5)),
                Location = new Point(x, y + 140),
            };
            elevatorOuterControl.BringToFront();

            Point tmp = new Point(20, 20);
            for (int i = floorNum - 1 + floorNum % 2; i >= 1; i -= 2)
            {
                Label floorLabelLeft = new Label
                {
                    Text = "F" + i.ToString(),
                    Parent = elevatorOuterControl,
                    Location = tmp,
                    Size = new Size(40, 30),
                    TextAlign = ContentAlignment.MiddleCenter,
                    //BorderStyle = BorderStyle.FixedSingle,
                };
                tmp.X += 45;
                if (i + 1 <= floorNum)
                {
                    Button upButtonLeft = new Button
                    {
                        Text = "",
                        Name = "+" + i.ToString(),
                        Parent = elevatorOuterControl,
                        Location = tmp,
                        BackgroundImage = Image.FromFile("img\\电梯向上.png"),
                        BackgroundImageLayout = ImageLayout.Zoom,
                        Size = new Size(35, 30),
                    };
                    upButtonLeft.Click += OuterRequest;
                }
                tmp.X += 40;
                if (i - 1 >= 1)
                {
                    Button downButtonLeft = new Button
                    {
                        Text = "",
                        Name = "-" + i.ToString(),
                        Parent = elevatorOuterControl,
                        Location = tmp,
                        BackgroundImage = Image.FromFile("img\\电梯向下.png"),
                        BackgroundImageLayout = ImageLayout.Zoom,
                        Size = new Size(35, 30),
                    };
                    downButtonLeft.Click += OuterRequest;
                }
                if (i + 1 > floorNum)
                {
                    tmp.X -= 85;
                    tmp.Y += 35;
                    continue;
                }
                i++;
                tmp.X += 50;
                Label floorLabelRight = new Label
                {
                    Text = "F" + i.ToString(),
                    Parent = elevatorOuterControl,
                    Location = tmp,
                    Size = new Size(40, 30),
                    TextAlign = ContentAlignment.MiddleCenter,
                    //BorderStyle = BorderStyle.FixedSingle,
                };
                tmp.X += 45;
                if (i + 1 <= floorNum)
                {
                    Button upButtonRight = new Button
                    {
                        Text = "",
                        Name = "+" + i.ToString(),
                        Parent = elevatorOuterControl,
                        Location = tmp,
                        BackgroundImage = Image.FromFile("img\\电梯向上.png"),
                        BackgroundImageLayout = ImageLayout.Zoom,
                        Size = new Size(30, 30),
                    };
                    upButtonRight.Click += OuterRequest;
                }
                tmp.X += 40;
                if (i - 1 >= 1)
                {
                    Button downButtonRight = new Button
                    {
                        Text = "",
                        Name = "-" + i.ToString(),
                        Parent = elevatorOuterControl,
                        Location = tmp,
                        BackgroundImage = Image.FromFile("img\\电梯向下.png"),
                        BackgroundImageLayout = ImageLayout.Zoom,
                        Size = new Size(35, 30),
                    };
                    downButtonRight.Click += OuterRequest;
                }
                i--;
                tmp.X -= 220;
                tmp.Y += 35;
            }

            //调度算法执行频率
            algorithmTimer = new Timer();
            algorithmTimer.Interval = 1;
            algorithmTimer.Tick += AlgorithmTick;
            algorithmTimer.Start();

            //请求序列初始化
            elevatorRequests = new List<ElevatorRequest>();
        }

        private void InnerRequest(object sender, EventArgs e)
        {
            Button requestButton = sender as Button;

            //此时按钮激活，不允许再次交互
            requestButton.Enabled = false;

            string[] s = requestButton.Name.Split('_');
            int index = int.Parse(s[0]);
            int floor = int.Parse(s[1]);

            UpdateLog($"{index + 1}号电梯内部请求移动到{floor}层");

            this.elevator[index].elevator.stopFloor[floor] = true;
            this.elevator[index].elevator.innerActivatedButton.Add(requestButton);

            requestButton.Enabled = false;

        }

        private void OuterRequest(object sender, EventArgs e)
        {
            Button requestButton = sender as Button;
            requestButton.Enabled = false;
            int startFloor = int.Parse(requestButton.Name.Substring(1));
            bool up = requestButton.Name[0] == '+';
            requestButton.BackgroundImage = Image.FromFile($@"img\电梯向{(up ? "上": "下" )}-Clicked.png");

            UpdateLog($"F{startFloor}层请求向{(up ? "上" : "下")}");

            AddRequest(startFloor, up, requestButton);
        }

        private void WarningRequest(object sender, EventArgs e)
        {
            Button requestButton = sender as Button;
            int index = int.Parse(requestButton.Name);
            Console.WriteLine(index);

            if (!elevator[index].elevator.elevatorWarning)
            {
                //按钮状态更新
                requestButton.BackgroundImage = Image.FromFile(@"img\警报1.png");
                //信息更新
                UpdateLog($"{index + 1}号电梯进入警报状态，停用所有按钮，并紧急停止");
                elevator[index].elevator.WarningState();

                for(int i = 0; i < floorNum; i++)
                {
                    elevator[index].floorButton[i].Enabled = false;
                    elevator[index].elevator.stopFloor[i] = false;
                }
                //清空内部激活按钮队列
                elevator[index].elevator.innerActivatedButton.Clear();

                //重新分配外部请求
                while (elevator[index].elevator.outerActivatedButton.Count != 0)
                {
                    Button btn = elevator[index].elevator.outerActivatedButton[0];
                    bool up = (btn.Name[0] == '+');
                    int startFloor = int.Parse(elevator[index].elevator.outerActivatedButton[0].Name.Substring(1));

                    //信息更新
                    UpdateLog($"由于{index + 1}号电梯进入警报状态，将重新为位于F{startFloor}层的{(up ? "上楼" : "下楼") }请求重新分配电梯");

                    //添加到请求列表中
                    AddRequest(startFloor, up, btn);

                    //从该电梯的调度请求中移除
                    elevator[index].elevator.outerActivatedButton.RemoveAt(0);
                }
            }
            else 
            {
                //按钮状态更新
                requestButton.BackgroundImage = Image.FromFile(@"img\警报2.png");
                //requestButton.BackgroundImageLayout 
                //信息更新
                UpdateLog($"{index + 1}号电梯解除警报状态，并恢复使用");
                elevator[index].elevator.WarningState();

                for (int i = 0; i < floorNum; i++)
                {
                    elevator[index].floorButton[i].Enabled = true;
                    elevator[index].elevator.stopFloor[i] = false;
                }
            }

            Algorithm();
        }

        private void ForceOpen(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            bool open = btn.Name[0] == '+';
            int index = int.Parse(btn.Name.Substring(1));
            elevator[index].elevator.DoorCommandUpdate(open);
        }

        private void ForceClose(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            bool open = btn.Name[0] == '+';
            int index = int.Parse(btn.Name.Substring(1));
            elevator[index].elevator.DoorCommandUpdate(open);
        }

        private void Algorithm()
        {
            for (int i = 0; i < elevatorRequests.Count; i++)
            {
                //过滤已经处理的请求
                if (elevatorRequests[i].elevatorIndex != -1)
                {
                    continue;
                }

                int minDistance = floorNum + 1, index = -1;
                for (int j = 0; j < elevatorNum; j++)
                {
                    //电梯处于警告状态就跳过
                    if (elevator[j].elevator.elevatorWarning == true)
                    {
                        continue;
                    }

                    if (elevator[j].elevator.runningState == Elevator.ELEVATOR_RUNNING_STATE.STOP)
                    {
                        //静止的电梯直接调用
                        minDistance = Math.Abs(elevator[j].elevator.floorForNow - elevatorRequests[i].startFloor);
                        index = j;
                    }
                    else if (elevator[j].elevator.runningState == Elevator.ELEVATOR_RUNNING_STATE.MOVING
                         && elevator[j].elevator.movingDirection == Elevator.ELEVATOR_MOVING_DIRECTION.UP
                        && elevator[j].elevator.floorForNow <= elevatorRequests[i].startFloor && elevatorRequests[i].up)
                    {
                        if (elevatorRequests[i].startFloor - elevator[j].elevator.floorForNow < minDistance)
                        {
                            minDistance = elevatorRequests[i].startFloor - elevator[j].elevator.floorForNow;
                            index = j;
                        }
                    }
                    else if (elevator[j].elevator.runningState == Elevator.ELEVATOR_RUNNING_STATE.MOVING 
                        && elevator[j].elevator.movingDirection == Elevator.ELEVATOR_MOVING_DIRECTION.DOWN
                        && elevator[j].elevator.floorForNow >= elevatorRequests[i].startFloor && !elevatorRequests[i].up)
                    {
                        if (elevator[j].elevator.floorForNow - elevatorRequests[i].startFloor < minDistance)
                        {
                            minDistance = elevator[j].elevator.floorForNow - elevatorRequests[i].startFloor;
                            index = j;
                        }
                    }

                    if(index != -1)
                    {
                        //添加到电梯调度列表内
                        elevatorRequests[i].elevatorIndex = index;
                        elevator[index].elevator.stopFloor[elevatorRequests[i].startFloor] = true;
                        elevator[index].elevator.outerActivatedButton.Add(elevatorRequests[i].requestButton);

                        //日志更新
                        UpdateLog($"对于{elevatorRequests[i].startFloor}楼的{(elevatorRequests[i].up ? "上楼" : "下楼")}请求，为其调度了{index + 1}号电梯");

                        if (elevator[index].elevator.runningState == Elevator.ELEVATOR_RUNNING_STATE.STOP)
                        {
                            if (elevatorRequests[i].startFloor != elevator[index].elevator.floorForNow)
                            {
                                ;
                            }
                            else
                            {
                                if (elevator[index].elevator.doorState == Elevator.ELEVATOR_DOOR_STATE.CLOSED)
                                {
                                    elevator[index].elevator.DoorCommandUpdate(true);
                                }
                                else
                                {
                                    elevator[index].elevator.DoorCommandUpdate(false);
                                }
                            }
                        }
                        else
                        {
                            ;
                        }
                        elevatorRequests.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        void AddRequest(int startFloor, bool up, Button requestButton)
        {
            ElevatorRequest request = new ElevatorRequest(startFloor, up, requestButton);
            elevatorRequests.Add(request);
        }

        void AlgorithmTick(object sender, EventArgs e)
        {
            //调用处理算法
            Algorithm();
        }

        private void parameterReset(object sender, EventArgs e)
        {
            int newNum = int.Parse(elevatorNumeric.Value.ToString());
            int newFloor = int.Parse(floorNumeric.Value.ToString());
            if (newNum == elevatorNum && newFloor == floorNum)
            {
                MessageBox.Show("电梯数量和楼层高度未变化");
                return;
            }
            Form1 form = new Form1(newNum, newFloor);
            Hide();//隐藏旧窗体
            form.TopMost = true;
            form.Show();
        }

        private void UpdateLog(String Text)
        {
            richTextBox.AppendText(Text + "\n");
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();//释放内存
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }
    }
}
