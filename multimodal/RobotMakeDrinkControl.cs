using System.Threading;

//using URControler2;
using CIRLABURControl;

namespace multimodal
{
    public class RobotMakeDrinkControl
    {
        URServerAction uRServerAction_right, uRServerAction_left;
        public RobotMakeDrinkControl(URServerAction uRServerActionLeft, URServerAction uRServerActionRight)
        {
            uRServerAction_left = uRServerActionLeft;
            uRServerAction_right = uRServerActionRight;
        }
        public void rotate_bottle(float[] bottle_r, float[] cup_r, float[] bottle_l)
        {

            if (uRServerAction_right == null)
            {
                throw new System.IO.DriveNotFoundException("完蛋了。");
            }

            uRServerAction_left.GripperOpen();
            uRServerAction_right.GripperOpen();
            uRServerAction_right.Move(RobotInitial.robot_initial_pos_r);
            uRServerAction_left.Move(RobotInitial.robot_initial_pos_l);
            
            //uRServerAction_left.GripperCloseForceMIN();

            //uRServerAction_left.Move(new float[] { bottle_l[0] + ImagePosition.image_left_x, 0.01F, bottle_l[1] + ImagePosition.image_left_y - 0.03F, 2.4F, 2.5F, 1.5F });
            //uRServerAction_left.Move(new float[] { bottle_l[0] + ImagePosition.image_left_x, 0.04F, bottle_l[1] + ImagePosition.image_left_y - 0.04F, 2.4F, 2.5F, 1.5F });

            //uRServerAction_left.ForceMode(0, 30);

            uRServerAction_right.Move(new float[] { bottle_r[0] + ImagePosition.image_right_x, 0.250F, bottle_r[1] + ImagePosition.image_right_y + 0.04F, 0.015F, 3.18F, 0.04F });
            uRServerAction_right.Move(new float[] { bottle_r[0] + ImagePosition.image_right_x, 0.250F, bottle_r[1] + ImagePosition.image_right_y+0.01F, 0.015F,3.18F, 0.04F });
            //Thread.Sleep(2000);

            //uRServerAction_left.EndForceMode();
            //uRServerAction_left.Move(new float[] { bottle_l[0] + ImagePosition.image_left_x, 0.01F, bottle_l[1] + ImagePosition.image_left_y - 0.04F, 2.4F, 2.5F, 1.5F });
            //uRServerAction_left.Move(RobotInitial.robot_initial_pos_l);
            //uRServerAction_left.GripperOpen();


            uRServerAction_right.GripperClose();

            uRServerAction_right.Move(new float[] { bottle_r[0] + ImagePosition.image_right_x, 0.110F, bottle_r[1] + ImagePosition.image_right_y, 0.015F, 3.18F, 0.04F });
            uRServerAction_right.Move(new float[] { RobotInitial.robot_initial_pos_rc[0], 0.110F, bottle_r[1] + ImagePosition.image_right_y, 0.015F, 3.18F, 0.04F });
            uRServerAction_right.Move(RobotInitial.robot_initial_pos_rc);

            RobotInitial.robot_initial_pos_lc[1] -= 0.02F;
            uRServerAction_left.Move(RobotInitial.robot_initial_pos_lc);
            RobotInitial.robot_initial_pos_lc[1] += 0.02F;
            uRServerAction_left.Move(RobotInitial.robot_initial_pos_lc);



            uRServerAction_left.TurnJoint(4, -15, 2);

            uRServerAction_left.Move(RobotInitial.robot_initial_pos_lc);

            uRServerAction_left.GripperClose();

            RobotInitial.robot_initial_pos_lc[1] -= 0.020F;

            uRServerAction_left.Move(RobotInitial.robot_initial_pos_lc);

            uRServerAction_right.Move(new float[] { cup_r[0] + 0.035F, 0.110F, cup_r[1] + 0.030F, 0.015F, 3.18F, 0.04F });
            uRServerAction_right.Move(new float[] { cup_r[0] + 0.035F, 0.140F, cup_r[1] + 0.030F, 0.015F, 3.18F, 0.04F });

            uRServerAction_right.MoveJoint(5, 1.75F);
            Thread.Sleep(3000);
            uRServerAction_right.MoveJoint(5, -1.75F);


            uRServerAction_right.Move(RobotInitial.robot_initial_pos_rc);

            RobotInitial.robot_initial_pos_lc[1] += 0.020F;
            uRServerAction_left.Move(RobotInitial.robot_initial_pos_lc);
            uRServerAction_left.GripperOpen();

            RobotInitial.robot_initial_pos_lc[1] -= 0.01F;
            uRServerAction_left.Move(RobotInitial.robot_initial_pos_lc);
            RobotInitial.robot_initial_pos_lc[1] += 0.01F;

            uRServerAction_left.TurnJoint(-4, 25, 2);

            RobotInitial.robot_initial_pos_rc[1] += 0.02F;
            uRServerAction_right.Move(RobotInitial.robot_initial_pos_rc);
            RobotInitial.robot_initial_pos_rc[1] -= 0.02F;

            uRServerAction_left.Move(RobotInitial.robot_initial_pos_l);

            uRServerAction_right.Move(new float[] { RobotInitial.robot_initial_pos_rc[0], 0.110F, bottle_r[1] + ImagePosition.image_right_y, 0.015F, 3.18F, 0.04F });
            uRServerAction_right.Move(new float[] { bottle_r[0] + ImagePosition.image_right_x, 0.110F, bottle_r[1] + ImagePosition.image_right_y+0.01F, 0.015F, 3.18F, 0.04F });

            uRServerAction_right.Move(new float[] { bottle_r[0] + ImagePosition.image_right_x, 0.249F, bottle_r[1] + ImagePosition.image_right_y+0.01F, 0.015F, 3.18F, 0.04F });
            uRServerAction_right.GripperOpen();
            uRServerAction_right.Move(new float[] { bottle_r[0] + ImagePosition.image_right_x, 0.249F, bottle_r[1] + ImagePosition.image_right_y + 0.03F, 0.015F, 3.18F, 0.04F });

            uRServerAction_right.Move(RobotInitial.robot_initial_pos_r);
            
        }
        public void open_bottle(float[] bottle_r, float[] cup_r, float[] cup_l, float[] bottle_l)
        {
            uRServerAction_left.GripperOpenAsync();
            uRServerAction_right.GripperOpen();
            uRServerAction_right.MoveAsync(RobotInitial.robot_initial_pos_r);
            uRServerAction_left.Move(RobotInitial.robot_initial_pos_l);


            uRServerAction_right.Move(new float[] { bottle_r[0] + ImagePosition.image_right_x, -0.015F, bottle_r[1] - 0.085F, 2.453F,-2.344F, -2.382F });
            uRServerAction_right.Move(new float[] { bottle_r[0] + ImagePosition.image_right_x, 0.030F, bottle_r[1] - 0.085F, 2.453F, -2.344F, -2.382F });

            uRServerAction_right.GripperCloseMAX();

            uRServerAction_right.Move(new float[] { -0.325F, -0.118F, -0.160F, 2.453F, -2.344F, -2.382F });
            uRServerAction_left.Move(new float[] { 0.330F, 0.100F, -0.080F, 0.06F, -3.127F, 0.093F });
            uRServerAction_left.Move(new float[] { 0.330F, 0.100F, -0.110F, 0.06F, -3.127F, 0.093F });
            uRServerAction_left.GripperCloseMAX();
            uRServerAction_right.GripperOpen();

            uRServerAction_right.Move(new float[] { -0.325F, -0.151F, -0.160F, 2.453F, -2.344F, -2.382F });
            //uRServerAction_right.Move(new float[] { -0.325F, -0.150F, -0.158F, 2.453F, -2.344F, -2.382F });
            uRServerAction_right.Move(RobotInitial.robot_initial_pos_r);
            uRServerAction_right.Move(new float[] { -0.331F, 0.055F,  0.030F, 0.035F, -3.117F, -0.016F });
            uRServerAction_right.Move(new float[] { -0.331F, 0.055F, -0.015F, 0.035F, -3.117F, -0.016F });           
            uRServerAction_right.GripperClose();

            uRServerAction_right.GripperOpen();
            uRServerAction_right.Move(new float[] { -0.331F, 0.055F, 0.030F, 0.035F, -3.117F, -0.016F });
            uRServerAction_right.Move(RobotInitial.robot_initial_pos_r);

            uRServerAction_left.Move(new float[] { cup_l[0] + ImagePosition.image_left_x - 0.095F, 0.150F, cup_l[1] + ImagePosition.image_left_y - 0.01F, 0.06F, -3.127F, 0.093F });

            uRServerAction_left.MoveJoint(5, -2F);
            Thread.Sleep(5000);
            uRServerAction_left.MoveJoint(5, 2F);

            uRServerAction_right.GripperCloseMAX();

            uRServerAction_left.Move(new float[] { 0.330F, 0.100F, -0.120F, 0.06F, -3.127F, 0.093F });
            
            uRServerAction_right.Move(new float[] { -0.285F, 0.030F, 0.035F, 0.044F, -3.110F, -0.143F });
            uRServerAction_right.Move(new float[] { -0.285F, 0.030F, 0.013F, 0.044F, -3.110F, -0.143F });
            uRServerAction_right.Move(new float[] { -0.270F, -0.035F, 0.013F, 0.044F, -3.110F, -0.143F });
            uRServerAction_right.Move(new float[] { -0.343F, -0.035F, 0.013F, 0.044F, -3.110F, -0.143F });

            uRServerAction_right.ForceMode(1, 28);
            Thread.Sleep(2000);
            uRServerAction_right.EndForceMode();
            uRServerAction_right.Move(new float[] { -0.335F, -0.040F, 0.040F, 0.044F, -3.110F, -0.143F });
            uRServerAction_right.GripperOpen();
            uRServerAction_right.Move(new float[] { -0.325F, -0.130F, -0.153F, 2.453F, -2.344F, -2.382F });
            uRServerAction_right.Move(new float[] { -0.325F, -0.118F, -0.153F, 2.453F, -2.344F, -2.382F });
            uRServerAction_right.GripperClose();

            uRServerAction_left.GripperOpen();
            uRServerAction_left.Move(new float[] { 0.330F, 0.100F, -0.080F, 0.06F, -3.127F, 0.093F });         
            uRServerAction_left.Move(RobotInitial.robot_initial_pos_l);
            Thread.Sleep(2000);

            uRServerAction_right.Move(new float[] { bottle_r[0] + ImagePosition.image_right_x,  0.030F, bottle_r[1] - 0.085F, 2.453F, -2.344F, -2.382F });
            Thread.Sleep(1000);
            uRServerAction_right.GripperOpen();
            uRServerAction_right.Move(new float[] { bottle_r[0] + ImagePosition.image_right_x , -0.015F, bottle_r[1] - 0.085F, 2.453F, -2.344F, -2.382F });
            uRServerAction_right.Move(RobotInitial.robot_initial_pos_r);


        }
        public void pick_bottle(float[] bottle_l, float[] cup_l,bool drink_2)
        {
            uRServerAction_left.GripperOpen();
            uRServerAction_right.GripperOpen();
            uRServerAction_right.Move(RobotInitial.robot_initial_pos_r);
            uRServerAction_left.Move(RobotInitial.robot_initial_pos_l);

            uRServerAction_left.Move(new float[] { bottle_l[0] + ImagePosition.image_left_x, 0.280F, bottle_l[1] + ImagePosition.image_left_y + 0.02F, 0.06F, -3.127F, 0.093F });
            uRServerAction_left.Move(new float[] { bottle_l[0] + ImagePosition.image_left_x , 0.280F, bottle_l[1] + ImagePosition.image_left_y - 0.04F, 0.06F, -3.127F, 0.093F });
            uRServerAction_left.GripperClose();
            Thread.Sleep(1000);
            uRServerAction_left.Move(new float[] { cup_l[0] + ImagePosition.image_left_x - 0.065F, 0.140F, cup_l[1] + ImagePosition.image_left_y - 0.016F, 0.06F, -3.127F, 0.093F });

            uRServerAction_left.MoveJoint(5, -1.5F);
            if (drink_2)
            {
                Thread.Sleep(700);
            }

            uRServerAction_left.MoveJoint(5, 1.5F);

            uRServerAction_left.Move(new float[] { bottle_l[0] + ImagePosition.image_left_x + 0.003F, 0.280F, bottle_l[1] + ImagePosition.image_left_y - 0.04F, 0.06F, -3.127F, 0.093F });
            uRServerAction_left.GripperOpen();
            uRServerAction_left.Move(new float[] { bottle_l[0] + ImagePosition.image_left_x + 0.003F, 0.280F, bottle_l[1] + ImagePosition.image_left_y + 0.01F, 0.06F, -3.127F, 0.093F });

            uRServerAction_left.Move(RobotInitial.robot_initial_pos_l);
        }
        public void pick_cup(float[] cup_r, float[] cup_l)
        {
            uRServerAction_left.GripperClose();
            uRServerAction_right.Move(new float[] { cup_r[0] + ImagePosition.image_right_x , 0.190F, cup_r[1] + 0.045F, 0.015F, 3.18F, 0.04F });
            uRServerAction_right.Move(new float[] { cup_r[0] + ImagePosition.image_right_x , 0.275F, cup_r[1] + 0.045F, 0.015F, 3.18F, 0.04F });
            uRServerAction_right.GripperClose();


            uRServerAction_right.Move(new float[] { cup_r[0] + ImagePosition.image_right_x , 0.050F, cup_r[1] + 0.05F, 0.015F, 3.18F, 0.04F });

            //uRServerAction_left.Move(new float[] { cup_l[0] + ImagePosition.image_left_x - 0.02F, 0.150F, cup_l[1] + ImagePosition.image_left_y, 0.071F, 2.94F, -0.067F});
            //uRServerAction_left.Move(new float[] { cup_l[0] + ImagePosition.image_left_x - 0.02F, 0.135F, cup_l[1] + ImagePosition.image_left_y, 0.071F, 2.94F, -0.067F });



            uRServerAction_right.Move(new float[] { -0.500F, 0.050F, -0.085F, 0.015F, 3.18F, 0.04F });
            uRServerAction_left.Move(new float[] { 0.490F, 0.150F, -0.045F, 0.012F, -3.54F, 0.038F });
            uRServerAction_left.Move(new float[] { 0.490F, 0.135F, -0.045F, 0.012F, -3.54F, 0.038F });
            Thread.Sleep(3000);
            uRServerAction_right.GripperOpen();
            uRServerAction_right.Move(new float[] { -0.500F, 0.050F, -0.025F, 0.015F, 3.18F, 0.04F });


            // uRServerAction_left.Move(new float[] { 0.500F, 0.135F, 0.050F, 0.0126F, 2.742F, -0.045F });
            uRServerAction_left.Move(RobotInitial.robot_initial_pos_l);

            //uRServerAction_right.Move(new float[] { cup_r[0] + ImagePosition.image_right_x+0.027F, 0.275F, cup_r[1] + 0.045F, 2.1F, -2.1F, -0.38F });
            //Thread.Sleep(500);
            //uRServerAction_right.GripperOpen();
            //uRServerAction_right.Move(new float[] { cup_r[0] + ImagePosition.image_right_x + 0.027F, 0.2F, cup_r[1] + 0.045F, 2.1F, -2.1F, -0.38F });
            uRServerAction_right.Move(RobotInitial.robot_initial_pos_r);
            uRServerAction_left.GripperOpen();
        }
    }
}
