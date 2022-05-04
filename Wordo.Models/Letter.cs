using System.ComponentModel;

namespace Wordo.Models;

public class Letter : INotifyPropertyChanged
{
    private readonly string _value;

    public bool WasGuessed { get; set; }
    public string DisplayValue => WasGuessed ? _value : "?";

    public event PropertyChangedEventHandler? PropertyChanged;

    public Letter(string value)
    {
        _value = value;
    }
}