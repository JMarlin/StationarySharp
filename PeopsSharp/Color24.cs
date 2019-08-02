using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StationarySharp {

    class Color24 {

        public UInt32 raw_value = 0;

        public byte Red {
            get { return (byte)(this.raw_value & 0xFF); }
        }

        public byte Green {
            get { return (byte)((this.raw_value >> 8) & 0xFF); }
        }

        public byte Blue {
            get { return (byte)((this.raw_value >> 16) & 0xFF); }
        }

        public void SetRGB(byte r, byte g, byte b) {

            this.raw_value = ((UInt32)r) | (((UInt32)g) << 8) | (((UInt32)b) << 16);
        }

		public static Color24 FromRGB(byte r, byte g, byte b) {

            Color24 ret_color = new Color24();

            ret_color.SetRGB(r, g, b);

            return ret_color;
        }

        public Color24 Clone() {
        
            Color24 ret_color = new Color24();

            ret_color.raw_value = this.raw_value;

            return ret_color;
        }
    }
}
