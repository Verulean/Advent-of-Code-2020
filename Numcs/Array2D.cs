namespace Numcs;

public class Array2D<T> where T : IEquatable<T>
{
    // CONSTRUCTORS
    // Given size
    public Array2D(int m, int n)
    {
        _m = m;
        _n = n;
        _grid = new T[_m, _n];
    }

    // Given source array
    public Array2D(T[,] grid)
    {
        _m = grid.GetLength(0);
        _n = grid.GetLength(1);
        _grid = new T[_m, _n];

        for (var i = 0; i < _m; i++)
        {
            for (var j = 0; j < _n; j++)
            {
                _grid[i, j] = grid[i, j];
            } 
        }
    }
    
    // HIDDEN MEMBERS
    protected T[,] _grid;
    protected int _m;
    protected int _n;
    
    // BASIC PROPERTIES
    public (int, int) Shape => (_m, _n);
    public int Count => _m * _n;
    public Array2D<T> Copy() => new Array2D<T>(_grid);
    public T[,] ToArray()
    {
        var arr = new T[_m, _n];
        for (var i = 0; i < _m; i++)
        {
            for (var j = 0; j < _n; j++)
            {
                arr[i, j] = _grid[i, j];
            }
        }

        return arr;
    }

    // INDEXERS
    // Single element
    public T this[int i, int j]
    {
        get => _grid[i, j];
        set => _grid[i, j] = value;
    }

    // Row-wise sub-array
    public Array1D<T> this[int i, Range rj]
    {
        get
        {
            var (j, lj) = rj.GetOffsetAndLength(_n);
            var subArr = new T[lj];
            for (var dj = 0; dj < lj; dj++)
            {
                subArr[dj] = this[i, j + dj];
            }

            return new Array1D<T>(subArr);
        }

        set
        {
            var (j, lj) = rj.GetOffsetAndLength(_n);
            switch (value)
            {
                case T v:
                {
                    for (var dj = 0; dj < lj; dj++)
                    {
                        this[i, j + dj] = v;
                    }

                    break;
                }
                case not null:
                {
                    for (var dj = 0; dj < lj; dj++)
                    {
                        this[i, j + dj] = value[dj];
                    }

                    break;
                }
            }
        }
    }
    
    // Column-wise sub-array
    public Array1D<T> this[Range ri, int j]
    {
        get
        {
            var (i, li) = ri.GetOffsetAndLength(_m);
            var subArr = new T[li];
            for (var di = 0; di < li; di++)
            {
                subArr[di] = this[i + di, j];
            }

            return new Array1D<T>(subArr);
        }

        set
        {
            var (i, li) = ri.GetOffsetAndLength(_m);
            switch (value)
            {
                case T v:
                {
                    for (var di = 0; di < li; di++)
                    {
                        this[i + di, j] = v;
                    }

                    break;
                }
                case not null:
                {
                    for (var di = 0; di < li; di++)
                    {
                        this[i + di, j] = value[di];
                    }

                    break;
                }
            }
        }
    }
    
    // Proper sub-array
    public Array2D<T> this[Range ri, Range rj]
    {
        get
        {
            var (i, li) = ri.GetOffsetAndLength(_m);
            var (j, lj) = rj.GetOffsetAndLength(_n);
            var subArr = new T[li, lj];

            for (var di = 0; di < li; di++)
            {
                for (var dj = 0; dj < lj; dj++)
                {
                    subArr[i + di, j + dj] = this[i + di, j + dj];
                }
            }

            return new Array2D<T>(subArr);
        }

        set
        {
            var (i, li) = ri.GetOffsetAndLength(_m);
            var (j, lj) = rj.GetOffsetAndLength(_n);

            switch (value)
            {
                case T v:
                {
                    for (var di = 0; di < li; di++)
                    {
                        for (var dj = 0; dj < lj; dj++)
                        {
                            this[i + di, j + dj] = v;
                        }
                    }

                    break;
                }
                case not null:
                {
                    for (var di = 0; di < li; di++)
                    {
                        for (var dj = 0; dj < lj; dj++)
                        {
                            this[i + di, j + dj] = value[di, dj];
                        }
                    }

                    break;
                }
            }
        }
    }

    // ARRAY TRANSFORMATIONS
    // Reverse the order of elements along axis 1 (left/right)
    public Array2D<T> FlipLR()
    {
        var arr = new T[_m, _n];
        for (var i = 0; i < _m; i++)
        {
            for (var j = 0; j < _n; j++)
            {
                arr[i, j] = this[i, _n - j - 1];
            }
        }

        return new Array2D<T>(arr);
    }

    // Reverse the order of elements along axis 0 (up/down)
    public Array2D<T> FlipUD()
    {
        var arr = new T[_m, _n];
        for (var i = 0; i < _m; i++)
        {
            for (var j = 0; j < _n; j++)
            {
                arr[i, j] = this[_m - i - 1, j];
            }
        }

        return new Array2D<T>(arr);
    }

    // Reverse the order of elements along a given axis
    public Array2D<T> Flip(int axis)
    {
        return axis switch
        {
            0 => FlipUD(),
            1 => FlipLR(),
            _ => throw new ArgumentException($"Cannot flip Array2D along axis {axis}.")
        };
    }

    // Rotate by a multiple of 90 degrees counterclockwise
    public Array2D<T> Rot90(int k = 1)
    {
        k = (k % 4 + 4) % 4;
        var (m, n) = k % 2 == 1 ? (_n, _m) : (_m, _n);
        var arr = new T[m, n];

        switch (k)
        {
            case 0:
                arr = ToArray();
                break;
            case 1:
            {
                for (var i = 0; i < m; i++)
                {
                    for (var j = 0; j < n; j++)
                    {
                        arr[i, j] = _grid[j, m - i - 1];
                    }
                }
                
                break;
            }
            case 2:
            {
                for (var i = 0; i < m; i++)
                {
                    for (var j = 0; j < n; j++)
                    {
                        arr[i, j] = _grid[m - i - 1, n - j - 1];
                    }
                }
                
                break;
            }
            case 3:
            {
                for (var i = 0; i < m; i++)
                {
                    for (var j = 0; j < n; j++)
                    {
                        arr[i, j] = _grid[n - j - 1, i];
                    }
                }
                
                break;
            }
        }

        return new Array2D<T>(arr);
    }
}
