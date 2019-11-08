﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace multimodal
{
    public class RobotInitial
    {
        public static readonly float[] robot_initial_pos_r = { -0.288F, 0.069F, 0.322F, 1.23F, -2.59F, -0.83F };
        public static readonly float[] robot_initial_pos_l = { 0.395F, 0.030F, 0.172F, 0.86F, 3.19F, 0.76F };
        public static readonly float[] robot_initial_pos_rc = { -0.337F, 0.110F, -0.123F, 2.174F, -2.233F, 0.002F };
        public static readonly float[] robot_initial_pos_lc = { 0.335F, -0.100F, -0.153F, 2.447F, 2.367F, 2.475F };
    }
    public class ImagePosition
    {
        public static readonly float image_right_x = -0.065F;
        public static readonly float image_right_y = 0.01F;
        public static readonly float image_left_x = 0.05F;
        public static readonly float image_left_y = 0.04F;
    }
    public class DrinkName
    {
        public static readonly string first_drink_name = "牛奶";
        public static readonly string second_drink_name = "紅茶";
        public static readonly string third_drink_name = "抹茶";
        public static readonly string fourth_drink_name = "";
        public static readonly string gradual_name = "漸層";
    }
}

