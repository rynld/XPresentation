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
    public class ConvertWPF
    {
        public static FrameworkElement Convert(VisualElement element)
        {
            FrameworkElement nuevo = Type(element);
            TransformGroup union = new TransformGroup();

            for (int i = 0; i < 4; i++)//El orden de los grupos es primero skew, despues  escaldo, rotacion y traslacion
                union.Children.Add(new TransformGroup());

            nuevo.RenderTransform = union;
            nuevo.RenderTransformOrigin = new Point(0.5, 0.5);
            if ((element.Height != double.PositiveInfinity && element.Height != double.NegativeInfinity) && (element.Width != double.PositiveInfinity && element.Width != double.NegativeInfinity))
            {
                nuevo.Height = element.Height;
                nuevo.Width = element.Width;
            }
            nuevo.Name = element.Name;

            ((TransformGroup)union.Children[3]).Children.Add(new TranslateTransform(element.Position.X, element.Position.Y));
            foreach (var item in element.Transformations)
            {
                if (item.GetType() == typeof(TranslateTransformation))
                    ((TransformGroup)union.Children[3]).Children.Add(ConvertToTranslation((TranslateTransformation)item));

                else if (item.GetType() == typeof(RotateTransformation))
                    ((TransformGroup)union.Children[2]).Children.Add(ConvertToRotation((RotateTransformation)item));

                else if (item.GetType() == typeof(ScaleTransformation))
                    ((TransformGroup)union.Children[1]).Children.Add(ConvertToScale((ScaleTransformation)item));

                else if (item.GetType() == typeof(SkewTransformation))
                    ((TransformGroup)union.Children[0]).Children.Add(ConvertToSkew((SkewTransformation)item));
            }

            FinalConvertion(element, nuevo);

            return nuevo;
        }

        public static void FinalConvertion(VisualElement element, FrameworkElement frame)
        {
            if (element is Rectangulo)
                ConvertToRectangle((Rectangulo)element, (Rectangle)frame);

            if (element is Elipse)
                ConvertToEllipse((Elipse)element, (Ellipse)frame);

            if (element is Linea)
                ConvertToLine((Linea)element, (Line)frame);

            if (element is TextoBox)
                ConvertToText((TextoBox)element, (System.Windows.Controls.Label)frame);

            if (element is Group)
                ConvertToGroup((Group)element, (Grid)frame);

            if (element is Arrow)
                ConvertToArrow((Arrow)element, (Polygon)frame);

            if (element is Pie)
                ConvertToPie((Pie)element, (Path)frame);

            if (element is Polygono)
                ConvertToPolygono((Polygono)element, (Polygon)frame);

            if (element is Arco)
                ConvertToArc((Arco)element, (Path)frame);

            if (element is Curve)
                ConvertToCurve((Curve)element, (Path)frame);
        }

        static FrameworkElement Type(VisualElement element)
        {
            if (element is Rectangulo)
                return new Rectangle();

            if (element is Elipse)
                return new Ellipse();

            if (element is Linea)
                return new Line();

            if (element is TextoBox)
                return new System.Windows.Controls.Label();

            if (element is Group)
                return new Grid();

            if (element is Pie)
                return new Path();

            if (element is Arrow)
                return new Polygon();

            if (element is Polygono)
                return new Polygon();

            if (element is Arco)
                return new Path();

            if (element is Curve)
                return new Path();

            return null;
        }

        #region//Convert properties
        /// <summary>
        /// Convert a brush to a WPF brush 
        /// </summary>
        /// <param name="brocha">The brush to convert </param>
        /// <returns></returns>
        public static Brush ConvertToBrush(Brocha brocha)
        {
            try
            {
                if (typeof(SolidaBrush) == brocha.GetType())//If the brush is solid
                    return new SolidColorBrush(ConvertToColor(((SolidaBrush)brocha).Color));

                if (brocha.GetType() == typeof(LinearGradienteBrush))//If the brush is a linear gradient
                {
                    LinearGradienteBrush mybrush = (LinearGradienteBrush)brocha;
                    return new LinearGradientBrush(ConvertToColor(mybrush.Color_Ini), ConvertToColor(mybrush.Color_Fin), ConvertToPoint(mybrush.Ini), ConvertToPoint(mybrush.Fin));
                }

                if (brocha.GetType() == typeof(RadialGradienteBrush))//If
                {
                    RadialGradienteBrush mybrush = (RadialGradienteBrush)brocha;
                    RadialGradientBrush newbrush = new RadialGradientBrush(ConvertToColor(mybrush.Color_Ini), ConvertToColor(mybrush.Color_Fin));
                    newbrush.Center = ConvertToPoint(mybrush.Center);
                    newbrush.RadiusX = mybrush.RadiusX;
                    newbrush.RadiusY = mybrush.RadiusY;

                    return newbrush;
                }


                if (brocha.GetType() == typeof(ImagenBrush))//If the bruch is an imagen
                    return new ImageBrush(new BitmapImage(new Uri(((ImagenBrush)brocha).Source)));
            }
            catch (Exception)
            {

                return null;
            }


            return null;
        }

        /// <summary>
        /// Convert a color from a string
        /// </summary>
        /// <param name="my_color">The name of the color to find</param>
        /// <returns>The new color</returns>
        public static Color ConvertToColor(string my_color)
        {

            PropertyInfo properties = typeof(Colors).GetProperty(my_color);
            Color color = new Color();

            return (Color)properties.GetValue(color, null);
        }

        public static Point ConvertToPoint(Punto punto)
        {
            return new Point(punto.X, punto.Y);
        }

        public static Pen ConvertToPen(Pluma pluma)
        {
            Pen nueva = new Pen(ConvertToBrush(pluma.Stroke), pluma.Widht);
            Array ar = (typeof(PenLineCap)).GetEnumValues();

            foreach (var item in ar)
            {
                if (item.ToString() == pluma.Begin)
                    nueva.StartLineCap = (PenLineCap)item;
            }

            foreach (var item in ar)
            {
                if (item.ToString() == pluma.End)
                    nueva.EndLineCap = (PenLineCap)item;
            }

            return nueva;
        }

        public static TimeSpan ConvertToTime(Time tiempo)
        {
            return new TimeSpan(tiempo.Hour, tiempo.Minutes, tiempo.Seg);
        }

        public static TranslateTransform ConvertToTranslation(TranslateTransformation trans)
        {
            return new TranslateTransform(trans.Delta_X, trans.Delta_Y);
        }

        public static RotateTransform ConvertToRotation(RotateTransformation trans)
        {
            return new RotateTransform(trans.Angle);
        }

        public static SkewTransform ConvertToSkew(SkewTransformation trans)
        {
            return new SkewTransform(trans.Factor_X, trans.Factor_Y);
        }

        public static ScaleTransform ConvertToScale(ScaleTransformation trans)
        {
            return new ScaleTransform(trans.X, trans.Y);
        }

        #endregion

        #region//Convertion of classe

        static void ConvertToRectangle(Rectangulo rec, Rectangle newrectangle)
        {
            newrectangle.RadiusX = rec.RadiusX;
            newrectangle.RadiusY = rec.RadiusY;
            newrectangle.Stroke = ConvertToBrush(rec.Pen.Stroke);
            newrectangle.StrokeThickness = rec.Pen.Widht;
            newrectangle.Fill = ConvertToBrush(rec.Background);
            newrectangle.StrokeStartLineCap = ConvertToPen(rec.Pen).StartLineCap;
            newrectangle.StrokeEndLineCap = ConvertToPen(rec.Pen).EndLineCap;

        }

        public static void ConvertToEllipse(Elipse elipse, Ellipse newelipse)
        {
            newelipse.StrokeStartLineCap = ConvertToPen(elipse.Pen).StartLineCap;
            newelipse.StrokeEndLineCap = ConvertToPen(elipse.Pen).EndLineCap;
            newelipse.Fill = ConvertToBrush(elipse.Background);
            newelipse.Stroke = ConvertToBrush(elipse.Pen.Stroke);
            newelipse.StrokeThickness = elipse.Pen.Widht;
        }

        public static void ConvertToPolygono(Polygono pol, Polygon nueva)
        {
            nueva.Points.Clear();
            foreach (var item in pol.Points)
                nueva.Points.Add(ConvertToPoint(item));

            nueva.Height = double.NaN;
            nueva.Width = double.NaN;
            nueva.Fill = ConvertToBrush(pol.Background);
            nueva.StrokeThickness = pol.Pen.Widht;
            nueva.Stroke = ConvertToBrush(pol.Pen.Stroke);
            nueva.StrokeStartLineCap = ConvertToPen(pol.Pen).StartLineCap;
            nueva.StrokeEndLineCap = ConvertToPen(pol.Pen).EndLineCap;
        }

        public static void ConvertToArc(Arco arco, Path path)
        {
            path.Width = double.NaN;
            path.Height = double.NaN;
            path.StrokeThickness = arco.Pen.Widht;
            path.Stroke = ConvertToBrush(arco.Pen.Stroke);
            path.Fill = ConvertToBrush(arco.Background);
            path.StrokeStartLineCap = ConvertToPen(arco.Pen).StartLineCap;
            path.StrokeEndLineCap = ConvertToPen(arco.Pen).EndLineCap;


            ArcSegment arc = new ArcSegment(new Point(arco.Width, 0), new Size(arco.Width, arco.Height), 0, false, SweepDirection.Clockwise, true);

            PathFigure fig = new PathFigure();
            fig.StartPoint = new Point(0, 0);
            fig.Segments.Add(arc);
            List<PathFigure> lista = new List<PathFigure>();
            lista.Add(fig);
            PathGeometry geo = new PathGeometry(lista);
            path.Data = geo;

        }

        public static void ConvertToGroup(Group grupo, Grid nueva)
        {
            Rectangle fondo = new Rectangle();
            fondo.Width = nueva.Width;
            fondo.Height = nueva.Height;

            fondo.Fill = ConvertToBrush(grupo.Background);
            nueva.Children.Add(fondo);
            nueva.ClipToBounds = true;
            nueva.Background = fondo.Fill;
            fondo.StrokeStartLineCap = ConvertToPen(grupo.Pen).StartLineCap;
            fondo.StrokeEndLineCap = ConvertToPen(grupo.Pen).EndLineCap;

            fondo.StrokeThickness = grupo.Pen.Widht;
            fondo.Stroke = ConvertToBrush(grupo.Pen.Stroke);

            foreach (var item in grupo.Childrens)
            {
                FrameworkElement frame = Convert(item);
                frame.VerticalAlignment = VerticalAlignment.Top;
                frame.HorizontalAlignment = HorizontalAlignment.Left;
                frame.RenderTransform = null;
                Thickness thic = new Thickness();
                if (item is Arrow)
                {
                    thic.Left = Math.Abs(grupo.Position.X - ((Arrow)item).Position.X);
                    thic.Top = Math.Abs(grupo.Position.Y - ((Arrow)item).Begin.Y);
                }
                else
                {
                    thic.Left = Math.Abs(grupo.Position.X - item.Position.X);
                    thic.Top = Math.Abs(grupo.Position.Y - item.Position.Y);
                }
                frame.Margin = thic;
                nueva.Children.Add(frame);
            }
        }

        public static void ConvertToArrow(Arrow arrow, Polygon pol)
        {
            pol.Height = double.NaN;
            pol.Width = double.NaN;
            Point point = ConvertToPoint(arrow.Begin);

            pol.Points.Add(new Point(0, 0));
            pol.Points.Add(new Point(0, arrow.Height / 2));
            pol.Points.Add(new Point(arrow.Width / 2, arrow.Height / 2));
            pol.Points.Add(new Point(arrow.Width / 2, arrow.Height));
            pol.Points.Add(new Point(arrow.Width, arrow.Height / 4));

            pol.Points.Add(new Point(arrow.Width / 2, -arrow.Height / 2));
            pol.Points.Add(new Point(arrow.Width / 2, 0));


            pol.Fill = ConvertToBrush(arrow.Background);
            pol.Stroke = ConvertToBrush(arrow.Pen.Stroke);
            pol.StrokeThickness = arrow.Pen.Widht;
            pol.StrokeStartLineCap = ConvertToPen(arrow.Pen).StartLineCap;
            pol.StrokeEndLineCap = ConvertToPen(arrow.Pen).EndLineCap;

        }

        public static Line ConvertToLine(Linea linea, Line nueva)
        {
            nueva.X1 = linea.Inicio.X;
            nueva.Y1 = linea.Inicio.Y;
            nueva.X2 = linea.Destino.X;
            nueva.Y2 = linea.Destino.Y;
            nueva.Stroke = ConvertToBrush(linea.Pen.Stroke);
            nueva.StrokeThickness = linea.Pen.Widht;
            nueva.StrokeStartLineCap = ConvertToPen(linea.Pen).StartLineCap;
            nueva.StrokeEndLineCap = ConvertToPen(linea.Pen).EndLineCap;

            return nueva;
        }

        public static void ConvertToPie(Pie pastel, Path path)
        {
            path.Height = double.NaN;
            path.Width = double.NaN;
            path.Fill = ConvertToBrush(pastel.Background);
            path.Stroke = ConvertToBrush(pastel.Pen.Stroke);
            path.StrokeStartLineCap = ConvertToPen(pastel.Pen).StartLineCap;
            path.StrokeEndLineCap = ConvertToPen(pastel.Pen).EndLineCap;

            path.RenderTransformOrigin = new Point(0.5, 0.5);

            PathFigure fig1 = new PathFigure();
            List<PathFigure> lista = new List<PathFigure>();
            lista.Add(fig1);

            fig1.IsClosed = true;
            fig1.StartPoint = new Point(0, pastel.Height);

            fig1.Segments.Add(new ArcSegment(new Point(pastel.Width, (pastel.Height)), new Size(pastel.Width / 2, pastel.Height), 0, false, SweepDirection.Clockwise, true));
            fig1.Segments.Add(new LineSegment(new Point(pastel.Central.X, pastel.Central.Y), true));

            PathGeometry path_geo = new PathGeometry(lista);

            path.Data = path_geo;

        }

        public static void ConvertToCurve(Curve curve, Path path)
        {
            path.Height = double.NaN;
            path.Width = double.NaN;
            path.StrokeThickness = curve.Pen.Widht;
            path.Stroke = ConvertToBrush(curve.Pen.Stroke);
            path.Fill = ConvertToBrush(curve.Background);
            path.StrokeStartLineCap = ConvertToPen(curve.Pen).StartLineCap;
            path.StrokeEndLineCap = ConvertToPen(curve.Pen).EndLineCap;
            path.RenderTransformOrigin = new Point(0.5, 0.5);

            PathFigure fig = new PathFigure();
            //fig.StartPoint = ConvertToPoint(curve.StartPoint);
            fig.StartPoint = new Point(0, 0);
            BezierSegment bezier = new BezierSegment(new Point(0, 0), ConvertToPoint(curve.EndPoint), ConvertToPoint(curve.CurvePoint), true);
            fig.Segments.Add(bezier);
            List<PathFigure> lista = new List<PathFigure>();
            lista.Add(fig);
            PathGeometry geo = new PathGeometry(lista);
            path.Data = geo;
        }

        public static void ConvertToText(TextoBox textbox, System.Windows.Controls.Label label)
        {
            TextBlock finaltext = new TextBlock();
            finaltext.FontSize = textbox.FontSize;

            finaltext.Text = textbox.Texto;
            finaltext.TextAlignment = (TextAlignment)textbox.HorizontalAligment;
            finaltext.TextWrapping = TextWrapping.WrapWithOverflow;

            if (textbox.Subrayate)
                finaltext.TextDecorations.Add(TextDecorations.Underline);

            if (textbox.Bold)
                finaltext.FontWeight = FontWeights.Bold;

            if (textbox.Cursive)
                finaltext.FontStyle = FontStyles.Italic;

            finaltext.Foreground = ConvertToBrush(new SolidaBrush(textbox.ColorLetter));

            label.Background = ConvertToBrush(textbox.Background);
            label.Content = finaltext;
            label.VerticalContentAlignment = (VerticalAlignment)textbox.VerticalAligment;
            label.BorderThickness = new Thickness(textbox.Pen.Widht);
            label.BorderBrush = ConvertToBrush(textbox.Pen.Stroke);
        }

        #endregion
    }
}
