using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HierarchyXpresentation;
using Microsoft.Win32;
using Microsoft.CSharp;
using System.Reflection;
using System.Reflection.Emit;
using Weboo.XMLTools;

namespace Presentation
{
    class Transiciones
    {
        public static void Empuje(Grid contenedor, DoubleAnimation animation)
        {
            double height = ((Canvas)((Viewbox)contenedor.Children[1]).Child).Height;
            ResetValues(animation);
            animation.From = height;
            animation.To = 0;
            TranslateTransform translate = new TranslateTransform();
            ((Viewbox)contenedor.Children[1]).Child.RenderTransform = translate;
            translate.BeginAnimation(TranslateTransform.YProperty, animation);

            animation.From = 0;
            animation.To = -height;

            TranslateTransform trans = new TranslateTransform();
            ((Viewbox)contenedor.Children[0]).Child.RenderTransform = trans;
            trans.BeginAnimation(TranslateTransform.YProperty, animation);

        }

        public static void Barrido(Canvas nueva, DoubleAnimation animation)
        {
            ResetValues(animation);
            LinearGradientBrush brush = new LinearGradientBrush(Colors.Transparent, Colors.Black, 0);

            nueva.OpacityMask = brush;

            animation.From = 0.9;
            animation.To = 0;
            brush.GradientStops[0].BeginAnimation(GradientStop.OffsetProperty, animation);

            animation.From = 1;
            animation.To = 0;
            animation.SpeedRatio = 0.8;
            brush.GradientStops[1].BeginAnimation(GradientStop.OffsetProperty, animation);


        }

        static void ResetValues(DoubleAnimation animation)
        {
            animation.From = 0;
            animation.To = 0;
            animation.AutoReverse = false;
            animation.BeginTime = new TimeSpan();
            animation.Duration = new Duration(new TimeSpan(0, 0, 3));
            animation.SpeedRatio = 1;
        }
    }
}
