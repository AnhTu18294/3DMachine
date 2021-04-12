using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace CT3DMachine.Helper
{
    public static class Globals
    {
        #region Turntable
        public const int FIRST_QUADRANT = 0;
        public const int SECOND_QUADRANT = 1;
        public const int THIRD_QUADRANT = 2;
        public const int FOURTH_QUADRANT = 3;

        public const int ID_FIRST_MOTION = 0;
        public const int ID_SECOND_MOTION = 1;
        public const int ID_THIRD_MOTION = 2;
        public const int ID_FOURTH_MOTION = 3;
        public const int ID_FIFTH_MOTION = 4;
        
        public const byte STATUS_POSITION_CHANGED = 0X03;
        public const byte STATUS_MACHINE_READY = 0X05;
        public const byte STATUS_MACHINE_BUSY = 0X06;
        public const byte STATUS_MACHINE_ERROR = 0X07;

        public const double PULSES_PER_MILLIMETRE = 800;
        public const double PULSES_PER_DEGREE = 2000;
        public const double PULSES_PER_STEP = 1;
        public const double Dsd = 2000;
        public const double RADIUS_OF_TURNTABLE = 300; //mm

        public const double Yt = 21.0;
        public const double Zt = 21.0;
        public const double Yf = 11.0;
        public const double Zf = 11.0;

        public const double FILM_PLATE_WIDTH = 10.0;
        public const double FILM_PLATE_LENGTH = 10.0;
                
        #endregion
    }
}
