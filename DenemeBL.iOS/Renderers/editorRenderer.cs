using System;
using DenemeBL.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Editor), typeof(editorRenderer))]
namespace DenemeBL.iOS.Renderers
{
    public class editorRenderer : EditorRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.ScrollEnabled = false;
                Control.TextContainerInset = new UIEdgeInsets(7.5f, 0, 7.5f, 0);
                Control.ContentInset = new UIEdgeInsets(0, -5, 0, -5);
                Control.TintColor = UIColor.White;
            }
        }
    }
}
