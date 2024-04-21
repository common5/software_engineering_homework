using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;




namespace 电梯调度
{
    public class Elevator
    {
        public enum ELEVATOR_DOOR_STATE
        {
            CLOSED, CLOSING, OPENED, OPENING,
        }

        public enum DOOR_COMMAND
        {
            CLOSE, OPEN, NONE,
        }

        public enum ELEVATOR_RUNNING_STATE
        {
            MOVING, WAITING, WARNING, STOP,
        }
        public enum ELEVATOR_MOVING_DIRECTION
        {
            UP, DOWN, NULL,
        }
        //电梯状态
        public ELEVATOR_RUNNING_STATE runningState;
        public ELEVATOR_MOVING_DIRECTION movingDirection;
        public ELEVATOR_DOOR_STATE doorState;
        private List<DOOR_COMMAND> doorCommand;

        //当前楼层
        public int floorForNow;
        private Label presentFloorLabel;

        //最大楼层
        private int maxFloor;

        //电梯构建位置
        private Point baseLocation;

        //电梯是否处于警告状态
        public bool elevatorWarning;

        //电梯图标
        private PictureBox elevatorImage;

        //电梯运行状态图标
        private PictureBox elevatorRunningState;

        //电梯按钮
        public List<Button> innerActivatedButton;
        public List<Button> outerActivatedButton;

        //标识动画计数器
        private int noticeCount;
        //电梯停靠计数器
        private int stopCount;

        private Timer doorTimer; //开关门计时器
        private Timer globalTimer;//全局计时器
        private Timer movingTimer;//移动计时器
        private Timer movingNoticeTimer;//电梯移动方向标识计时器

        FontFamily numberFont;

        //父控件
        private Form parent;

        //单层Y轴高度
        private int elevatorSize;

        //电梯需要停靠的楼层
        public bool[] stopFloor;

        public Elevator(Form parent, int MaxFloor, int elevatorSize, Point location, FontFamily FML)
        {
            //电梯初始状态
            this.floorForNow = 1;
            this.runningState = ELEVATOR_RUNNING_STATE.STOP;
            this.movingDirection = ELEVATOR_MOVING_DIRECTION.NULL;
            this.doorState = ELEVATOR_DOOR_STATE.CLOSED;
            this.doorCommand = new List<DOOR_COMMAND>();
            this.elevatorWarning = false;

            //停靠计数器
            this.stopCount = 0;

            //父控件
            this.parent = parent;

            //Timer计时器初始化
            this.doorTimer = new Timer
            {
                Interval = 250,
            };
            this.doorTimer.Tick += this.DoorTimerTick;
            this.doorTimer.Interval = 500;

            //全局计数器初始化
            this.globalTimer = new Timer
            {
                Interval = 1
            };
            this.globalTimer.Tick += this.GlobalTimerTick;
            this.globalTimer.Start();

            //移动计数器初始化
            this.movingTimer = new Timer
            {
                Interval = 1000
            };
            this.movingTimer.Tick += this.MovingTimerTick;

            //电梯移动标识计时器初始化
            this.movingNoticeTimer = new Timer
            {
                Interval = 250
            };
            this.movingNoticeTimer.Tick += MovingNoticeTimerTick;
            this.movingNoticeTimer.Start();

            this.noticeCount = 0;

            this.maxFloor = MaxFloor;
            this.elevatorSize = elevatorSize;

            this.numberFont = FML;
            //Size floorSize = new Size(elevatorSize, elevatorSize);

            this.elevatorImage = new PictureBox
            {
                Image = Image.FromFile(@"img\电梯-close.png"),
                Location = location,
                Parent = parent,
                Size = new Size(elevatorSize, elevatorSize),
                SizeMode = PictureBoxSizeMode.Zoom,
            };

            //电梯所在楼层标签
            this.presentFloorLabel = new Label
            {
                Text = "01",
                Font = new Font(FML, 24),
                ForeColor = Color.Orange,
                BorderStyle = BorderStyle.None,
                BackColor = Color.Black,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(51, 39),
                Parent = parent,
                Location = new Point(location.X + 17, location.Y - 40),
            };

            this.elevatorRunningState = new PictureBox
            {
                Image = Image.FromFile(@"img\停止.png"),
                Location = new Point(location.X + 64, location.Y - 40),
                Size = new Size(39, 39),
                Parent = parent,
                BackColor = Color.Black,
                //SizeMode = PictureBoxSizeMode.Zoom,
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            elevatorRunningState.BringToFront();

            //presentFloorLabel.BringToFront();
            this.innerActivatedButton = new List<Button>();
            this.outerActivatedButton = new List<Button>();

            stopFloor = new bool[MaxFloor + 5];

        }

        private void DoorTimerTick(object sender, EventArgs e)
        {
            if(doorCommand.Count > 0)
            { 
                if (this.doorCommand[0] == DOOR_COMMAND.CLOSE)
                {
                    if (this.doorState == ELEVATOR_DOOR_STATE.CLOSED)
                    {
                        //this.doorTimer.Stop();
                        this.doorCommand.RemoveAt(0);//门完全关上，指令状态清除
                    }
                    else if (this.doorState == ELEVATOR_DOOR_STATE.CLOSING)
                    {
                        this.doorState = ELEVATOR_DOOR_STATE.CLOSED;
                        this.elevatorImage.Image = Image.FromFile(@"img\电梯-Close.png");
                        this.doorCommand.RemoveAt(0);//门完全关上，指令状态清除
                    }
                    else if (this.doorState == ELEVATOR_DOOR_STATE.OPENED)
                    {
                        this.doorState = ELEVATOR_DOOR_STATE.CLOSING;
                        this.elevatorImage.Image = Image.FromFile(@"img\电梯-Closing.png");
                    }
                }
                else if (this.doorCommand[0] == DOOR_COMMAND.OPEN)
                {
                    if (this.doorState == ELEVATOR_DOOR_STATE.OPENED)
                    {
                        //this.doorTimer.Stop();
                        this.doorCommand.RemoveAt(0);//门完全打开，指令状态清除
                    }
                    else if (this.doorState == ELEVATOR_DOOR_STATE.OPENING)
                    {
                        this.doorState = ELEVATOR_DOOR_STATE.OPENED;
                        this.elevatorImage.Image = Image.FromFile(@"img\电梯-Open.png");
                        this.doorCommand.RemoveAt(0);//门完全打开，指令状态清除
                    }
                    else if (this.doorState == ELEVATOR_DOOR_STATE.CLOSED)
                    {
                        this.doorState = ELEVATOR_DOOR_STATE.OPENING;
                        this.elevatorImage.Image = Image.FromFile(@"img\电梯-Opening.png");
                    }
                }
            }
        }

        private void MovingTimerTick(object sender, EventArgs e)
        {
            //修改楼层
            if(this.movingDirection == ELEVATOR_MOVING_DIRECTION.UP)
            {
                this.floorForNow++;
            }
            else if(this.movingDirection == ELEVATOR_MOVING_DIRECTION.DOWN)
            {
                this.floorForNow--;
            }
            else
            {
                ;
            }

            string label_text = ""; 
            if(this.floorForNow < 10)
            {
                label_text += "0" + this.floorForNow.ToString();
            }
            else
            {
                label_text += this.floorForNow.ToString();
            }
            this.presentFloorLabel.Text = label_text;
            this.presentFloorLabel.Font = new Font(this.numberFont, 24);

            if (this.stopFloor[this.floorForNow] || this.elevatorWarning)
            {
                this.movingTimer.Stop();
                this.movingNoticeTimer.Stop();
                this.doorState = ELEVATOR_DOOR_STATE.OPENING;
                DoorCommandUpdate(true);
                this.stopFloor[this.floorForNow] = false;
                elevatorRunningState.Image = Image.FromFile(@"img\停止.png");
                
            }
        }

        private void MovingNoticeTimerTick(object sender, EventArgs e)
        {
            if(this.runningState == ELEVATOR_RUNNING_STATE.WARNING)
            {
                elevatorRunningState.Image = Image.FromFile(@"img\停止.png");
                return;
            }
            if (this.movingDirection == ELEVATOR_MOVING_DIRECTION.UP)
            {
                if(noticeCount < 4)
                {
                    noticeCount++;
                    this.elevatorRunningState.Image = Image.FromFile($@"img\上升{noticeCount}.png");
                    noticeCount %= 4;
                }
            }
            else if(this.movingDirection == ELEVATOR_MOVING_DIRECTION.DOWN)
            {
                if (noticeCount < 4)
                {
                    noticeCount++;
                    this.elevatorRunningState.Image = Image.FromFile($@"img\下降{noticeCount}.png");
                    noticeCount %= 4;
                }
            }
            else
            {
                this.elevatorRunningState.Image = Image.FromFile(@"img\停止.png");

            }
        }

        private void GlobalTimerTick(object sender, EventArgs e)
        {
            for(int i = 0; i < this.outerActivatedButton.Count; i++) 
            {
                if (outerActivatedButton[i].Enabled) 
                {
                    outerActivatedButton.RemoveAt(i);
                }

            }

            if(this.runningState == ELEVATOR_RUNNING_STATE.WARNING)
            {
                //如果门未关则强制关门
                if(this.doorState != ELEVATOR_DOOR_STATE.CLOSED)
                {
                    doorCommand.Add(DOOR_COMMAND.CLOSE);
                }
            }
            else if(this.runningState == ELEVATOR_RUNNING_STATE.MOVING)
            {
                if(this.movingDirection == ELEVATOR_MOVING_DIRECTION.UP)
                {
                    //门完全关闭则可以立即移动
                    if(this.doorState == ELEVATOR_DOOR_STATE.CLOSED)
                    {
                        ArrivalCheck();
                        bool movingFinished = true;
                        
                        for (int i = this.floorForNow; i <= maxFloor; i++)
                        {
                            //如果上方还有未完成的任务，那么电梯移动就未完成
                            if (stopFloor[i])
                            {
                                movingFinished = false;
                            }
                            else
                            {
                                ;
                            }
                        }

                        if(movingFinished)
                        {
                            this.movingDirection = ELEVATOR_MOVING_DIRECTION.NULL;
                            this.runningState = ELEVATOR_RUNNING_STATE.STOP;
                            //DoorCommandUpdate(true);
                            return;
                        }
                        GoUp();
                    }
                    //门完全开则需要等待一段时间后关门
                    else if (this.doorState == ELEVATOR_DOOR_STATE.OPENED)
                    {
                        if(this.stopCount <= 0)
                        {
                            this.stopCount = 120;
                        }
                        else
                        {
                            this.stopCount--;
                            if(this.stopCount == 0)
                            {
                                DoorCommandUpdate(false);
                            }
                        }
                    }
                    else if(this.doorState == ELEVATOR_DOOR_STATE.OPENING)
                    {
                        this.stopFloor[this.floorForNow] = false;
                        Console.WriteLine(floorForNow);

                        //调用开关门函数
                        
                    }

                }

                else if(this.movingDirection == ELEVATOR_MOVING_DIRECTION.DOWN)
                {
                    //门完全关则可以立即移动
                    if(this.doorState == ELEVATOR_DOOR_STATE.CLOSED)
                    {
                        ArrivalCheck();
                        bool movingFinished = true;

                        for (int i = this.floorForNow; i >= 1; i--)
                        {
                            //如果下方还有未完成的任务，那么电梯移动就未完成
                            if (stopFloor[i])
                            {
                                movingFinished = false;
                            }
                            else
                            {
                                ;
                            }
                        }

                        if (movingFinished)
                        {
                            this.movingDirection = ELEVATOR_MOVING_DIRECTION.NULL;
                            this.runningState = ELEVATOR_RUNNING_STATE.STOP;
                            //DoorCommandUpdate(true);
                            return;
                        }
                        GoDown();
                    }
                    //门完全开则需要等待一段时间后关门
                    else if (this.doorState == ELEVATOR_DOOR_STATE.OPENED)
                    {
                        if (this.stopCount <= 0)
                        {
                            this.stopCount = 120;
                        }
                        else
                        {
                            this.stopCount--;
                            if (this.stopCount == 0)
                            {
                                DoorCommandUpdate(false);
                            }
                        }
                    }
                    else if(this.doorState == ELEVATOR_DOOR_STATE.OPENING)
                    {
                        this.stopFloor[this.floorForNow] = false;

                        //调用开关门函数
                        
                    }
                }
            }
            else
            {
                //门完全开则需要等待一段时间后关门
                if (this.doorState == ELEVATOR_DOOR_STATE.OPENED)
                {
                    if (this.stopCount <= 0)
                    {
                        this.stopCount = 120;
                    }
                    else
                    {
                        this.stopCount--;
                        if (this.stopCount == 0)
                        {
                            DoorCommandUpdate(false);
                        }
                    }
                }
                else if (this.doorState == ELEVATOR_DOOR_STATE.CLOSED)
                {
                    ArrivalCheck();

                    //查看是否有外部调度尚未执行
                    if (outerActivatedButton.Count() != 0)
                    {
                        //优先前往距离最近的楼层
                        outerActivatedButton.Sort((x, y) => (
                        Math.Abs(int.Parse(x.Name.Substring(1)) - this.floorForNow) -
                        Math.Abs(int.Parse(y.Name.Substring(1)) - this.floorForNow)));

                        int targetFloor = int.Parse(outerActivatedButton[0].Name.Substring(1));
                        if (targetFloor > this.floorForNow)
                        {
                            this.movingDirection = ELEVATOR_MOVING_DIRECTION.UP;
                            this.runningState = ELEVATOR_RUNNING_STATE.MOVING;
                        }
                        else if (targetFloor < this.floorForNow)
                        {
                            this.runningState = ELEVATOR_RUNNING_STATE.MOVING;
                            this.movingDirection = ELEVATOR_MOVING_DIRECTION.DOWN;
                        }
                        return;
                    }

                    //查看是否有内部移动请求尚未处理
                    if (innerActivatedButton.Count() != 0)
                    {
                        innerActivatedButton.Sort((x, y) => (
                        Math.Abs(int.Parse(x.Name.Substring(2)) - this.floorForNow) -
                        Math.Abs(int.Parse(y.Name.Substring(2)) - this.floorForNow)));
                        //优先前往第一个的位置
                        int targetFloor = int.Parse(innerActivatedButton[0].Name.Split('_')[1]);
                        if (targetFloor > this.floorForNow)
                        {
                            this.runningState = ELEVATOR_RUNNING_STATE.MOVING;
                            this.movingDirection = ELEVATOR_MOVING_DIRECTION.UP;
                        }
                        else if (targetFloor < this.floorForNow)
                        {
                            this.runningState = ELEVATOR_RUNNING_STATE.MOVING;
                            this.movingDirection = ELEVATOR_MOVING_DIRECTION.DOWN;
                        }
                        return;
                    }
                }
            }
        }

        private bool GoUp()
        {
            if(this.doorState != ELEVATOR_DOOR_STATE.CLOSED || this.floorForNow == this.maxFloor)
            {
                return false;
            }

            this.runningState = ELEVATOR_RUNNING_STATE.MOVING;
            this.movingDirection = ELEVATOR_MOVING_DIRECTION.UP;

            this.movingNoticeTimer.Start();
            this.movingTimer.Start();

            return true;
        }

        private bool GoDown()
        {
            if (this.doorState != ELEVATOR_DOOR_STATE.CLOSED || this.floorForNow == 1)
            {
                return false;
            }

            this.runningState = ELEVATOR_RUNNING_STATE.MOVING;
            this.movingDirection = ELEVATOR_MOVING_DIRECTION.DOWN;

            this.movingNoticeTimer.Start();
            this.movingTimer.Start();

            return true;
        }
        private void ArrivalCheck()
        {
            //检查当前所在楼层是否有外部调度已完成
            
            for(int i = 0; i < outerActivatedButton.Count; i++) 
            {
                if (int.Parse(outerActivatedButton[i].Name.Substring(1)) == floorForNow)
                {
                    outerActivatedButton[i].Enabled = true;
                    bool up = outerActivatedButton[i].Name[0] == '+';
                    outerActivatedButton[i].BackgroundImage = Image.FromFile($@"img\电梯{(up ? "向上" : "向下")}.png");

                   
                    outerActivatedButton.RemoveAt(i);
                    break;
                }
            }

            //检查当前所在楼层是否有内部调度已完成
            //当门关闭后，处理内部按钮
            for (int i = 0; i < innerActivatedButton.Count(); ++i)
            {
                if (int.Parse(this.innerActivatedButton[i].Name.Substring(2)) == this.floorForNow)
                {
                    innerActivatedButton[i].Enabled = true;
                    innerActivatedButton.RemoveAt(i);
                    break;
                }
            }
        }
        public void DoorCommandUpdate(bool open)
        {
            if (open)
            {
                doorCommand.Add(DOOR_COMMAND.OPEN);
            }
            else
            {
                doorCommand.Add(DOOR_COMMAND.CLOSE);
            }
            doorTimer.Start();
            
        }
        public void WarningState()
        {
            if(runningState == ELEVATOR_RUNNING_STATE.WARNING)
            {
                runningState = ELEVATOR_RUNNING_STATE.STOP;
            }
            else
            {
                runningState = ELEVATOR_RUNNING_STATE.WARNING;
            }
            movingDirection = ELEVATOR_MOVING_DIRECTION.NULL;
            elevatorWarning = !elevatorWarning;
        }

    }


}
/* 



*/
