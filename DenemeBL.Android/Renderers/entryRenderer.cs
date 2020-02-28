using System;
using Android.Content;
using Android.Graphics.Drawables;
using DenemeBL.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Entry), typeof(entryRenderer))]
namespace DenemeBL.Droid.Renderers
{
    public class entryRenderer : EntryRenderer
    {
        public entryRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
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
