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
    class XPresentation
    {
        List<Slide> diapositivas;

        Canvas Main_Canvas;

        public XPresentation(List<Slide> diapositivas, Canvas main_canvas)
        {
            this.diapositivas = diapositivas;
            this.Main_Canvas = main_canvas;
        }

        /// <summary>
        /// Put all diapositives'information into the listbox
        /// </summary>
        /// <param name="content">The panel where goes the viewbos.Revisar la gramatica de este comentario</param>
        /// <returns></returns>
        public List<Canvas> Draw()
        {
            List<Canvas> canvas = new List<Canvas>();
            for (int i = 0; i < diapositivas.Count; i++)
                canvas.Add(PutSlides(diapositivas[i]));

            return canvas;
        }

        /// <summary>
        /// Put the information of a slides into a Canvas
        /// </summary>
        /// <param name="slide">The slide where is the information</param>
        /// <returns>A new canvas with all his respective information</returns>
        public Canvas PutSlides(Slide slide)
        {
            Canvas nueva = new Canvas();
            nueva.Height = Main_Canvas.Height;
            nueva.Width = Main_Canvas.Width;
            //nueva.Margin = new Thickness(2);
            nueva.Background = ConvertWPF.ConvertToBrush(slide.Background);

            foreach (var item in slide.Childrens)
                nueva.Children.Add(ConvertWPF.Convert(item));

            return nueva;
        }


        /// <summary>
        /// Start the animations of all the slides'elements
        /// </summary>
        /// <param name="posicion_slide">The posicion of the slide to animate</param>
        public List<FrameworkElement> BeginAnimations(int posicion_slide)
        {
            Slide aux = diapositivas[posicion_slide];
            List<FrameworkElement> lista = new List<FrameworkElement>();
            List<Animation> animacion = diapositivas[posicion_slide].Animations;

            for (int i = 0; i < animacion.Count; i++)
                lista.Add(Begin(Search(animacion[i].Name_Object, aux), animacion[i]));
            return lista;

        }

        VisualElement Search(string name, Slide diap)
        {
            foreach (var item in diap.Childrens)
                if (item.Name == name) return item;
            return null;
        }

        FrameworkElement Begin(VisualElement element, Animation animation)
        {
            if (element == null) throw new Exception("The element name " + animation.Name_Object + " doesn't exisit in the current text");
            FrameworkElement el = ConvertWPF.Convert(element);
           // Anima.Animate(el, element, animation);
            return el;
        }
    }
}
