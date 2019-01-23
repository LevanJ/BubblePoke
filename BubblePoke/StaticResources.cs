using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BubblePoke
{
    public static class SR
    {
        #region Colors
        public static Dictionary<int, Brush[]> BColors = new Dictionary<int, Brush[]>()
        {
            {-1, new Brush[]
            {
                Brushes.White,
                new BrushConverter().ConvertFrom("#EEEEEE") as SolidColorBrush,
                new BrushConverter().ConvertFrom("#AAAAAA") as SolidColorBrush,
            } },
            {0, new Brush[]
            {
                Brushes.Red,
                new BrushConverter().ConvertFrom("#FF6666") as SolidColorBrush,
                new BrushConverter().ConvertFrom("#FF9999") as SolidColorBrush,
            } },
            {1, new Brush[]
            {
                Brushes.Blue,
                new BrushConverter().ConvertFrom("#6666FF") as SolidColorBrush,
                new BrushConverter().ConvertFrom("#9999FF") as SolidColorBrush,
            } },
            {2, new Brush[]
            {
                Brushes.Cyan,
                new BrushConverter().ConvertFrom("#FF85FCFC") as SolidColorBrush,
                new BrushConverter().ConvertFrom("#FF9FF9F9") as SolidColorBrush,
            } },
            {3, new Brush[]
            {
                Brushes.Lime,
                new BrushConverter().ConvertFrom("#FF96FD96") as SolidColorBrush,
                new BrushConverter().ConvertFrom("#FFB7FFB7") as SolidColorBrush,
            } },
            {4, new Brush[]
            {
                Brushes.Yellow,
                new BrushConverter().ConvertFrom("#FFF7F765") as SolidColorBrush,
                new BrushConverter().ConvertFrom("#FFF7F78C") as SolidColorBrush,
            } },
            {5, new Brush[]
            {
                Brushes.Gray,
                new BrushConverter().ConvertFrom("#FFA6A3A3") as SolidColorBrush,
                new BrushConverter().ConvertFrom("#FFC3C3C3") as SolidColorBrush,
            } },
            {6, new Brush[]
            {
                Brushes.Orange,
                new BrushConverter().ConvertFrom("#FFFFD382") as SolidColorBrush,
                new BrushConverter().ConvertFrom("#FFF9DBA5") as SolidColorBrush,
            } },
            {7, new Brush[]
            {
                Brushes.Violet,
                new BrushConverter().ConvertFrom("#FFFAA8FA") as SolidColorBrush,
                new BrushConverter().ConvertFrom("#FFFCC6FC") as SolidColorBrush,
            } },
        };
        #endregion
    }
}
