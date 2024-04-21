using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Drawing;
using System.Windows.Data;
using System.Windows.Documents;


namespace WpfApp1
{
    public class Elevator
    {
        public enum ELEVATOR_STATE
        {
            CLOSE, MOVING, OPEN, WARNING
        }
        //当前楼层
        private int FloorForNow;

        //最大楼层
        private int MaxFloor;

        //电梯构建位置
        private Point BaseLocation;

        //电梯状态
        private ELEVATOR_STATE State;

        //电梯图标，但是文字
        private Label img;
        
        
    }
    public class Controller
    {
        
    }
    

}
