using System;
using Android.Content;
using Android.Graphics.Drawables;
using DenemeBL.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Editor), typeof(editorRenderer))]
namespace DenemeBL.Droid.Renderers
{
    public class editorRenderer : EditorRenderer
    {
        public editorRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.SetPadding(0, 0, 0, 0);
                Control.Background = new ColorDrawable(Android.Graphics.Color.Transparent);
            }
        }
    }
}
