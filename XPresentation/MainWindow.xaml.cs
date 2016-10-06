using System;
using System.Collections.Generic;
using System.Collections;
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
using System.Threading;

namespace Presentation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Window main_presentation;//The window where the presentation is showed.

        HierarchyXpresentation.Presentation presentation;
        XPresentation xp;

        List<Slide> slides;


        Queue<Animation> animaciones;//The animation of a slide
        DoubleAnimation animation_animaciones;//The animations that controls my animations
        DoubleAnimation animation_transitions;//The animation that controls my transition

        bool next_animation;//Move to the next animation
        bool exist_erros;
        bool finalized_transition;

        int animaciones_pendientes;//The amount of animations that are running
        int pos_diap;//Position of the slide that is in the screen

        public MainWindow()
        {
            InitializeComponent();
            XmlText.AcceptsReturn = true;

            this.presentation = new HierarchyXpresentation.Presentation();
            this.xp = new XPresentation(presentation.Childrens, Main_Canvas);
            this.animaciones = new Queue<Animation>();

            this.slides = new List<Slide>();
            this.FinalizePresentation += new EndAnimation(MainWindow_FinalizePresentation);

            this.animation_animaciones = new DoubleAnimation();
            this.animation_animaciones.Completed += new EventHandler(animation_Completed);
            this.animation_transitions = new DoubleAnimation();
            this.animation_transitions.Completed += new EventHandler(animation_transitions_Completed);

            this.main_presentation = new Window();
            this.main_presentation.KeyDown += new KeyEventHandler(main_presentation_KeyDown);

            this.main_presentation.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            this.main_presentation.VerticalContentAlignment = VerticalAlignment.Stretch;

            this.main_presentation.WindowState = WindowState.Maximized;
            this.main_presentation.WindowStyle = WindowStyle.None;

            this.main_presentation.MouseLeftButtonDown += new MouseButtonEventHandler(main_presentation_MouseLeftButtonDown);

            this.XmlText.Text = "<Presentation>\n\t<Slide>\n\t</Slide>\n</Presentation>";

        }

        /// <summary>
        /// Draw all slides and his elements int the listbox
        /// </summary>
        void DrawToListBox()
        {
            for (int i = 0; i < slides.Count; i++)
            {
                Canvas canvas = new Canvas();
                canvas.Width = Main_Canvas.ActualWidth;
                canvas.Height = Main_Canvas.ActualHeight;
                canvas.Margin = new Thickness(2);

                if (slides[i].Childrens.Count > 0)
                {
                    SetElements(canvas, slides[i], true);
                    canvas.LayoutTransform = new ScaleTransform(ListBox.ActualWidth / Main_Canvas.ActualWidth, 0.5);

                    ListBox.Items.Add(canvas);
                }
            }
            ListBox.SelectedIndex = 0;
            ListBox_SelectionChanged(null, null);
        }

        /// <summary>
        /// Set the elements of a slide into a canvas.
        /// </summary>
        /// <param name="canvas">A Canvas where goes the elements</param>
        /// <param name="diap">The slide that contains the elements</param>
        /// <param name="for_show">Is true if the presentation is in edition mode</param>
        public void SetElements(Canvas canvas, Slide diap, bool for_show)
        {
            canvas.Children.Clear();
            canvas.ClipToBounds = true;
            canvas.Background = ConvertWPF.ConvertToBrush(diap.Background);
            for (int i = 0; i < diap.Childrens.Count; i++)
            {
                FrameworkElement frame = ConvertWPF.Convert(diap.Childrens[i]);
                if (!for_show && HasAsoiciateAnimation(diap.Childrens[i]))
                    frame.Visibility = Visibility.Collapsed;
                canvas.Children.Add(frame);

            }

        }

        /// <summary>
        /// Initialize a completed presentation 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Begin_Click(object sender, RoutedEventArgs e)
        {
            if (!exist_erros)
            {
                this.Hide();
                this.main_presentation.Show();
                this.animaciones_pendientes = 0;
                CreateQueue();
                if (animaciones.Count > 0)
                    if (animaciones.Peek().Activation != BeginMode.Click) this.next_animation = true;
                DoTransition(slides[pos_diap].Transition.ToString());
            }
        }

        /// <summary>
        /// Begin a group of animations that start at the same time 
        /// </summary>
        /// <param name="canvas">The canvas where are the elements</param>
        void CreateAnimations(Canvas canvas)
        {
            while (animaciones.Count > 0 && next_animation)
            {
                Animation aux = animaciones.Peek();
                if (aux.Activation == BeginMode.Before)
                {
                    FrameworkElement frame = FindElement(aux.Name_Object, canvas);
                    animaciones_pendientes += Anima.Animate(frame, FindVisualElement(aux.Name_Object), aux, animation_animaciones);
                    frame.Visibility = Visibility.Visible;
                    animaciones.Dequeue();
                }
                else
                {
                    next_animation = false;
                    break;
                }
            }
        }

        /// <summary>
        /// Do the transition of an slide
        /// </summary>
        /// <param name="name_transicion">The name of the transition to do</param>
        void DoTransition(string name_transicion)
        {
            this.finalized_transition = false;
            #region//Initialitation of auxiliar components.
            Grid contenedor = new Grid();

            Viewbox viewbox1 = new Viewbox();
            Viewbox viewbox2 = new Viewbox();
            Canvas nuevo = new Canvas();
            Canvas old = new Canvas();

            contenedor.Height = main_presentation.ActualHeight;
            contenedor.Width = main_presentation.ActualWidth;
            nuevo.Height = old.Height = Main_Canvas.ActualHeight;
            nuevo.Width = old.Width = Main_Canvas.ActualWidth;

            viewbox1.Child = nuevo;
            viewbox2.Child = old;
            viewbox1.Height = main_presentation.ActualHeight;
            viewbox1.Width = main_presentation.ActualWidth;

            contenedor.Children.Add(viewbox2);
            contenedor.Children.Add(viewbox1);
            this.main_presentation.Content = contenedor;

            contenedor.Margin = new Thickness(0);
            viewbox1.HorizontalAlignment = HorizontalAlignment.Stretch;
            viewbox1.VerticalAlignment = VerticalAlignment.Stretch;
            viewbox2.HorizontalAlignment = HorizontalAlignment.Stretch;
            viewbox2.VerticalAlignment = VerticalAlignment.Stretch;
            viewbox1.Stretch = Stretch.Fill;
            viewbox2.Stretch = Stretch.Fill;
            #endregion

            switch (name_transicion)
            {
                case "Empuje":
                    if (pos_diap < slides.Count)
                        SetElements(nuevo, slides[pos_diap], false);

                    if (pos_diap == 0)
                        old.Background = Brushes.White;
                    else
                        SetElements(old, slides[pos_diap - 1], false);

                    Transiciones.Empuje(contenedor, animation_transitions);
                    break;

                case "Barrido":
                    if (pos_diap < slides.Count)
                        SetElements(nuevo, slides[pos_diap], false);

                    if (pos_diap == 0) old.Background = Brushes.White;
                    else
                        SetElements(old, slides[pos_diap - 1], false);

                    Transiciones.Barrido(nuevo, animation_transitions);

                    break;

                case "Normal":
                    SetElements(nuevo, slides[pos_diap], false);
                    animation_transitions_Completed(null, null);
                    break;


                default:
                    break;
            }

        }

        /// <summary>
        /// Check the xml code, and display all element that haven't erros
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XmlText_TextChanged(object sender, TextChangedEventArgs e)
        {
            List<XmlError> error = new List<XmlError>();
            ListBox.Items.Clear();

            error = presentation.Update(XmlText.Text);//Poner un hilo aqui

            if (error.Count != 0)
            {
                this.exist_erros = true;
                for (int i = 0; i < error.Count; i++)//Falta que todos los mensajes se escriban, no solo uno
                    Xml_Error.Text = error[i].Message;
            }
            else
            {
                this.exist_erros = false;
                Xml_Error.Text = "No Errors.";
            }
            slides = presentation.Childrens;

            DrawToListBox();
        }

        /// <summary>
        /// Show the slide that is selectioned in the listbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = ListBox.SelectedIndex;
            Canvas aux = new Canvas();

            if (index >= 0 && index < slides.Count)
            {
                Main_Canvas.Children.Clear();
                SetElements(Main_Canvas, slides[index], true);
            }
        }

        /// <summary>
        /// Change the size of the elements en the listbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ListBox.Items.Clear();
            DrawToListBox();
        }

        /// <summary>
        /// Activate a subset of animations, when all the pending animations finished
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void animation_Completed(object sender, EventArgs e)
        {
            if (animaciones_pendientes > 0)
                animaciones_pendientes--;
            if (animaciones_pendientes == 0 && animaciones.Count > 0)
            {
                Canvas aux_canvas = (Canvas)((Viewbox)((Grid)main_presentation.Content).Children[1]).Child;
                Animation aux = animaciones.Peek();
                if (aux.Activation != BeginMode.Click)
                {

                    FrameworkElement frame = FindElement(aux.Name_Object, aux_canvas);
                    animaciones_pendientes += Anima.Animate(frame, FindVisualElement(aux.Name_Object), aux, animation_animaciones);
                    frame.Visibility = Visibility.Visible;
                    animaciones.Dequeue();
                    next_animation = true;
                    CreateAnimations(aux_canvas);
                }
            }
        }

        /// <summary>
        /// Initialize the animations of the current slide
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void animation_transitions_Completed(object sender, EventArgs e)
        {
            this.finalized_transition = true;
            animaciones_pendientes = 0;
            CreateAnimations((Canvas)((Viewbox)((Grid)main_presentation.Content).Children[1]).Child);
        }

        void main_presentation_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (finalized_transition)
            {
                if (animaciones_pendientes == 0 && animaciones.Count > 0)
                {
                    Canvas aux_canvas = (Canvas)((Viewbox)((Grid)main_presentation.Content).Children[1]).Child;
                    this.next_animation = true;
                    Animation aux = animaciones.Peek();

                    FrameworkElement frame = FindElement(aux.Name_Object, aux_canvas);
                    animaciones_pendientes += Anima.Animate(frame, FindVisualElement(aux.Name_Object), aux, animation_animaciones);
                    frame.Visibility = Visibility.Visible;
                    animaciones.Dequeue();

                    CreateAnimations(aux_canvas);
                }
                else if (animaciones_pendientes == 0 && animaciones.Count == 0)
                {
                    if (e.ButtonState == MouseButtonState.Pressed)
                    {
                        pos_diap++;
                        if (pos_diap < slides.Count)
                        {
                            CreateQueue();
                            DoTransition(slides[pos_diap].Transition.ToString());
                        }
                        else
                            FinalizePresentation.Invoke();
                    }


                }
            }
        }

        void main_presentation_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                MainWindow_FinalizePresentation();
        }

        /// <summary>
        /// Close the windows of the presentation, and reset all values
        /// </summary>
        void MainWindow_FinalizePresentation()
        {
            this.Show();
            this.main_presentation.Hide();
            this.animaciones_pendientes = 0;
            this.pos_diap = 0;
            this.animaciones = new Queue<Animation>();
            XmlText_TextChanged(null, null);
        }

        #region//Metodos auxiliares

        /// <summary>
        /// Create the queue of animations
        /// </summary>
        void CreateQueue()
        {
            animaciones = new Queue<Animation>();
            if (pos_diap < slides.Count)
            {
                for (int i = 0; i < slides[pos_diap].Animations.Count; i++)
                    animaciones.Enqueue(slides[pos_diap].Animations[i]);
            }
        }

        /// <summary>
        /// Find a framework element in a canvas for his name
        /// </summary>
        /// <param name="name">Name of the object</param>
        /// <param name"canvas">The canvas where will search the element</param>
        /// <returns>A Canvas'elements, if it's null means that the element with this name doesn't exist</returns>
        FrameworkElement FindElement(string name, Canvas canvas)
        {
            for (int i = 0; i < canvas.Children.Count; i++)
                if (((FrameworkElement)canvas.Children[i]).Name == name)
                    return (FrameworkElement)canvas.Children[i];
            return null;
        }

        /// <summary>
        /// Search a element by his name in the list of slides.
        /// </summary>
        /// <param name="name">The name of the element</param>
        /// <returns>The element, if its null means that this element doesn't exist</returns>
        VisualElement FindVisualElement(string name)
        {
            for (int i = 0; i < slides[pos_diap].Childrens.Count; i++)
                if (slides[pos_diap].Childrens[i].Name == name) return slides[pos_diap].Childrens[i];
            return null;
        }

        /// <summary>
        /// Draw all elements that haven't a animation.
        /// </summary>
        void PutSimpleElements()
        {
            List<FrameworkElement> simple_elements = new List<FrameworkElement>();
            foreach (var item in animaciones)
                for (int i = 0; i < Main_Canvas.Children.Count; i++)
                    if (item.Name_Object != ((FrameworkElement)Main_Canvas.Children[i]).Name && i == Main_Canvas.Children.Count - 1)
                        Main_Canvas.Children[i].Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Check if an element have an animation
        /// </summary>
        /// <param name="element">The element to check if exist in the queue of animations</param>
        /// <returns>Return true if this element is animates, false otherwise</returns>
        bool HasAsoiciateAnimation(VisualElement element)
        {
            foreach (var item in animaciones)
                if (item.Name_Object == element.Name) return true;
            return false;
        }

        #endregion

        delegate void EndAnimation();

        /// <summary>
        /// Occurs when the presentation ended
        /// </summary>
        event EndAnimation FinalizePresentation;
    }
}
