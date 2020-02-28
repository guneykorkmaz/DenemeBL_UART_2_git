using System;
using DenemeBL.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Picker), typeof(pickerRenderer))]
namespace DenemeBL.iOS.Renderers
{
    public class pickerRenderer : PickerRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.BorderStyle = UITextBorderStyle.None;
                Control.BackgroundColor = UIColor.FromRGBA(0, 0, 0, 0);
            }
        }
    }
}
