using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Wordo.Core;

public class AsyncObservableCollection<T> : ObservableCollection<T>
{
    private AsyncOperation asyncOp = null;

    public AsyncObservableCollection()
    {
        CreateAsyncOp();
    }

    private void CreateAsyncOp()
    {
        // Create the AsyncOperation to post events on the creator thread
        asyncOp = AsyncOperationManager.CreateOperation(null);
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        // Post the CollectionChanged event on the creator thread
        asyncOp.Post(RaiseCollectionChanged, e);
    }

    private void RaiseCollectionChanged(object param)
    {
        // We are in the creator thread, call the base implementation directly
        base.OnCollectionChanged((NotifyCollectionChangedEventArgs)param);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        // Post the PropertyChanged event on the creator thread
        asyncOp.Post(RaisePropertyChanged, e);
    }

    private void RaisePropertyChanged(object param)
    {
        // We are in the creator thread, call the base implementation directly
        base.OnPropertyChanged((PropertyChangedEventArgs)param);
    }
}