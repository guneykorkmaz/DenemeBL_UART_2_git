using System;
using DenemeBL.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Entry), typeof(entryRenderer))]
namespace DenemeBL.iOS.Renderers
{
    public class entryRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
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
