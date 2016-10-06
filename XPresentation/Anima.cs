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
    class Anima//Faltan arreglar los comentarios
    {
        public static int Animate(FrameworkElement element, VisualElement visual, Animation my_animacion, DoubleAnimation animation)
        {
            ResetValues(animation);
            TimeSpan span = ConvertWPF.ConvertToTime(my_animacion.Duration);
            if (my_animacion.Duration.Hour != 0 || my_animacion.Duration.Minutes != 0 || my_animacion.Duration.Seg != 0)
                animation.Duration = span;
            else
                animation.Duration = new Duration(new TimeSpan(0, 0, 5));
            return (int)(typeof(Anima)).GetMethod(my_animacion.Type.ToString()).Invoke(null, new object[] { element, visual, animation });

        }

        /// <summary>
        /// Asociada a la animacion Aparece.El elemento debe tener visibilidad oculta
        /// </summary>
        /// <param name="element"></param>
        public static int Appear(FrameworkElement element, VisualElement visual, DoubleAnimation animation)
        {
            element.Visibility = Visibility.Visible;
            return 1;
        }

        /// <summary>
        /// Asociada a la animacion Desvanecer.Hay que revisar esto,porque
        /// aqui lo que hago es que el elemento aparezca gradualmente,
        /// lo otro seria que  se desapareciera gradualemente.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="visual"></param>
        /// <param name="animation"></param>
        public static int Fade(FrameworkElement element, VisualElement visual, DoubleAnimation animation)
        {
            animation.From = 0;
            animation.To = 1;
            element.BeginAnimation(FrameworkElement.OpacityProperty, animation);
            return 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="visual"></param>
        /// <param name="animation"></param>
        public static int LeftRight(FrameworkElement element, VisualElement visual, DoubleAnimation animation)
        {
            animation.From = -SumatoriaX((TransformGroup)((TransformGroup)element.RenderTransform).Children[3]);
            animation.To = 0;
            TranslateTransform trans = new TranslateTransform();
            trans.Y = visual.Position.Y;
            trans.BeginAnimation(TranslateTransform.XProperty, animation);
            ((TransformGroup)((TransformGroup)element.RenderTransform).Children[3]).Children.Add(trans);
            return 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="visual"></param>
        /// <param name="animation"></param>
        public static int BottomUp(FrameworkElement element, VisualElement visual, DoubleAnimation animation)
        {
            animation.From = -SumatoriaY((TransformGroup)((TransformGroup)element.RenderTransform).Children[3]); ;
            animation.To = 0;
            TranslateTransform trans = new TranslateTransform();
            trans.X = visual.Position.X;
            trans.BeginAnimation(TranslateTransform.YProperty, animation);
            ((TransformGroup)((TransformGroup)element.RenderTransform).Children[3]).Children.Add(trans);
            return 1;
        }

        /// <summary>
        /// Asociada a la animacion Zoom
        /// </summary>
        /// <param name="element"></param>
        /// <param name="visual"></param>
        /// <param name="animation"></param>
        public static int Zoom(FrameworkElement element, VisualElement visual, DoubleAnimation animation)
        {
            animation.From = 0;
            animation.To = 1;

            ScaleTransform scale = new ScaleTransform(1, 1, 0.5, 0.5);

            ((TransformGroup)((TransformGroup)element.RenderTransform).Children[1]).Children.Add(scale);
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, animation);

            return 1;
        }

        /// <summary>
        /// Simula la animacion rotula
        /// </summary>
        /// <param name="element">Element to apply the animation</param>
        /// <param name="visual">The element that has all correct properties</param>
        /// <param name="animation">A animation that has a predefinidos values</param>
        public static int Swivel(FrameworkElement element, VisualElement visual, DoubleAnimation animation)
        {
            animation.From = 1;
            animation.To = -1;
            animation.AutoReverse = true;
            ScaleTransform scale = new ScaleTransform();
            ((TransformGroup)((TransformGroup)element.RenderTransform).Children[1]).Children.Add(scale);
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, animation);

            return 1;
        }

        public static int Spin(FrameworkElement element, VisualElement visual, DoubleAnimation animation)
        {
            RotateTransform rotate = new RotateTransform();
            ((TransformGroup)((TransformGroup)element.RenderTransform).Children[2]).Children.Add(rotate);
            animation.From = 0;
            animation.To = 360;
            element.RenderTransformOrigin = new Point(0.5, 0.5);
            rotate.BeginAnimation(RotateTransform.AngleProperty, animation);
            return 1;
        }

        public static int GrowTurn(FrameworkElement element, VisualElement visual, DoubleAnimation animation)
        {

            Zoom(element, visual, animation);


            Spin(element, visual, animation);

            return 2;
        }

        static double SumatoriaX(TransformGroup traslations)
        {
            double total = 0;
            for (int i = 0; i < traslations.Children.Count; i++)
            {
                TranslateTransform trans = (TranslateTransform)traslations.Children[i];
                total += trans.X;
            }
            return total;
        }

        static double SumatoriaY(TransformGroup traslations)
        {
            double total = 0;
            for (int i = 0; i < traslations.Children.Count; i++)
                total += ((TranslateTransform)traslations.Children[i]).Y;
            return total;
        }

        static void ResetValues(DoubleAnimation animation)
        {
            animation.From = 0;
            animation.To = 0;
            animation.AutoReverse = false;
            animation.BeginTime = new TimeSpan();
            animation.Duration = new Duration();
            animation.SpeedRatio = 1;
        }
    }

}
