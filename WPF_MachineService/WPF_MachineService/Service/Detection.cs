using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_MachineService.Service
{
    public class Detection
    {
        public string name { get; set; }
        public int @class { get; set; }
        public double confidence { get; set; }
        public Box box { get; set; }

    }
    public class Box
    {
        public double x1 { get; set; }
        public double y1 { get; set; }
        public double x2 { get; set; }
        public double y2 { get; set; }
    }
}
