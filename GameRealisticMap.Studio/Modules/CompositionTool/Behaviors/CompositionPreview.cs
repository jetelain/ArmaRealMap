using System;
using Caliburn.Micro;
using GameRealisticMap.Studio.Modules.Arma3Data;
using GameRealisticMap.Studio.Modules.CompositionTool.ViewModels;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows;
using System.Windows.Controls;

namespace GameRealisticMap.Studio.Modules.CompositionTool.Behaviors
{
    public class CompositionPreview : PropertyChangedBase
    {
        private readonly CompositionViewModel vm;
        private readonly IArma3Previews arma3Previews;
        private Uri previewCache;

        internal CompositionPreview(CompositionViewModel vm, IArma3Previews arma3Previews)
        {
            this.vm = vm;
            this.arma3Previews = arma3Previews;
            previewCache = GetPreviewFast();
            vm.PropertyChanged += VmUpdated;
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.RegisterAttached("Source", typeof(CompositionViewModel), typeof(CompositionPreview), new PropertyMetadata(CompositionPreviewChanged));

        internal static void SetSource(Image target, CompositionViewModel value)
        {
            target.SetValue(SourceProperty, value);
        }

        public static void CompositionPreviewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var img = d as Image;
            var vm = e.NewValue as CompositionViewModel;
            if (img != null && vm != null)
            {
                var source = new CompositionPreview(vm, IoC.Get<IArma3Previews>());
                BindingOperations.SetBinding(img, Image.SourceProperty, new Binding("Preview")
                {
                    Source = source,
                    IsAsync = true,
                    FallbackValue = source.PreviewFast
                });
            }
        }

        private Uri GetPreviewFast()
        {
            var model = vm.SingleModel;
            if (model != null)
            {
                return arma3Previews.GetPreviewFast(model.Path);
            }
            return Arma3Previews.NoPreview;
        }

        private Uri GetPreviewSlow()
        {
            var model = vm.SingleModel;
            if (model != null)
            {
                return arma3Previews.GetPreview(model.Path).Result;
            }
            return Arma3Previews.NoPreview;
        }

        private void VmUpdated(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SingleModel")
            {
                previewCache = GetPreviewFast();
                NotifyOfPropertyChange(nameof(Preview));
            }
        }

        public Uri PreviewFast => previewCache;

        public Uri Preview
        {
            get
            {
                if (previewCache.IsFile)
                {
                    return previewCache;
                }
                return previewCache = GetPreviewSlow();
            }
        }
    }
}
