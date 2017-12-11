using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AllocationApp.Models;
using Xamarin.Forms;

namespace AllocationApp
{
    class StatekindBoolConverter :  IValueConverter
    {
        //Source To Target
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
           
            if (value is StateKind kind)
            {
                if (kind == StateKind.Checked)
                {
                    return "checked.png";
                }
                if (kind == StateKind.NoChecked)
                {
                    return "nochecked.png";
                }
                if (kind == StateKind.OverChecked)
                {
                    return "overchecked.png";
                }
            }
            return "checked.png";
        }

        //Target To Source
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value.ToString())
            {
                case "checked.png":
                    return StateKind.Checked;
                case "nochecked.png":
                    return StateKind.NoChecked;
                case "overchecked.png":
                    return StateKind.OverChecked;
                default:
                    return StateKind.Checked;
            }
        }
    }
}
