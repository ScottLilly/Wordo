using System.ComponentModel;
using Wordo.Core;

namespace Wordo.Models;

public class Letter : INotifyPropertyChanged
{
    private readonly string _value;

    public bool WasGuessed { get; private set; }
    public string DisplayValue => WasGuessed ? _value : "?";

    public event PropertyChangedEventHandler? PropertyChanged;

    public Letter(string value)
    {
        _value = value;
    }

    public void CompareWithGuess(string value)
    {
        if (_value.Matches(value))
        {
            WasGuessed = true;
        }
    }
}