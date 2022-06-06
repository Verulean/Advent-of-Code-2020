using System.Collections;

namespace Numcs;

public class Array1D<T> : IEnumerable<T> where T : IEquatable<T>
{
    // CONSTRUCTORS
    // Given length
    public Array1D(int m)
    {
        _m = m;
        _vec = new T[_m];
    }
    
    // Given source vector
    public Array1D(IReadOnlyCollection<T> vec)
    {
        _m = vec.Count;
        _vec = vec.ToArray();
    }
    
    // HIDDEN MEMBERS
    protected T[] _vec;
    protected int _m;
    
    // BASIC PROPERTIES
    public int Count => _m;
    public int Length => _m;
    public Array1D<T> Copy() => new Array1D<T>(_vec);
    public T[] ToArray() => _vec.ToArray();

    // INDEXERS
    // Single element
    public T this[int i]
    {
        get => _vec[i];
        set => _vec[i] = value;
    }

    // Sub-array
    public Array1D<T> this[Range r]
    {
        get
        {
            var (i, l) = r.GetOffsetAndLength(_m);
            return new Array1D<T>(_vec[i..(i + l)]);
        }

        set
        {
            var (i, l) = r.GetOffsetAndLength(_m);
            switch (value)
            {
                case T v:
                {
                    for (var di = 0; di < l; di++)
                    {
                        this[i + di] = v;
                    }

                    break;
                }
                case not null:
                {
                    for (var di = 0; di < l; di++)
                    {
                        this[i + di] = value[di];
                    }

                    break;
                }
            }
        }
    }
    
    // TRANSFORMATIONS
    public Array1D<T> Reverse() => new Array1D<T>(_vec.Reverse().ToArray());
    
    // COMPARISONS
    public bool ArrayEquals(Array1D<T> other)
    {
        return Length == other.Length && Enumerable.Range(0, Length).All(i => this[i].Equals(other[i]));
    }

    // INTERFACE IMPLEMENTATIONS
    public IEnumerator<T> GetEnumerator()
    {
        return ((IEnumerable<T>)_vec).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _vec.GetEnumerator();
    }
}
