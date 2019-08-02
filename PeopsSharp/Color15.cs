using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StationarySharp {

    class Color15 {

		public UInt16 raw_value = 0;

		public byte Red {
			get { return (byte)(this.raw_value & 0x1F); }
		}

		public byte Green{
			get{ return (byte)((this.raw_value >> 5) & 0x1F); }
		}

		public byte Blue{
			get{ return (byte)((this.raw_value >> 10) & 0x1F); }
		}

        public static byte ComponentClip(byte component) {

            return component > (byte)0x1F ? (byte)0x1F : component;
        }

		public void SetRGB(byte r, byte g, byte b) {

			this.raw_value = (UInt16)((UInt16)Color15.ComponentClip(r) | (((UInt16)Color15.ComponentClip(g)) << (UInt16)5) | (((UInt16)Color15.ComponentClip(b)) << (UInt16)10));
		}

        public static Color15 FromRGB(byte r, byte g, byte b) {

            Color15 ret_color = new Color15();

            ret_color.SetRGB(r, g, b);

            return ret_color;
        }

        public Color15 Clone() {
        
            Color15 ret_color = new Color15();

            ret_color.raw_value = this.raw_value;

            return ret_color;
        }
    }
}
