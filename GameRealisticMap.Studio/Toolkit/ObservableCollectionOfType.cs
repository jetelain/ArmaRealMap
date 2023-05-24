using System.Collections.Specialized;
using System.Linq;
using Caliburn.Micro;

namespace GameRealisticMap.Studio.Toolkit
{
    public class ObservableCollectionOfType<T,X> : BindableCollection<T>
    {
        private readonly IObservableCollection<X> source;

        public ObservableCollectionOfType(IObservableCollection<X> source)
            : base(source.OfType<T>())
        {
            this.source = source;
            source.CollectionChanged += Source_CollectionChanged;
        }

        private void Source_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                var oldItems = e.OldItems.OfType<T>().ToList();
                if (oldItems.Count > 0)
                {
                    RemoveRange(oldItems);
                }
            }
            if (e.NewItems != null)
            {
                var newItems = e.NewItems.OfType<T>().ToList();
                if (newItems.Count > 0)
                {
                    if (e.NewStartingIndex - e.NewItems.Count + 1 == source.Count)
                    {
                        AddRange(newItems);
                    }
                    else
                    {
                        // TODO: Insert at correct index
                        AddRange(newItems);
                    }
                }
            }
        }
    }
}
