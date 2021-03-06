namespace Tools.WPF.ViewModels
{
    using Interfaces;

    public class PositionViewModel : NotifyPropertyChanged, IPositionViewModel
    {
        public PositionViewModel()
        {
        }

        public PositionViewModel(double left, double top)
        {
            InitializeWith(left, top);
        }

        public IPositionViewModel InitializeWith(double left, double top)
        {
            Left = left;
            Top = top;

           return this;
        }

        double _left;

        public double Left
        {
            get { return _left; }
            set
            {
                _left = value;
                RaisePropertyChanged(() => Left);
            }
        }

        double _top;

        public double Top
        {
            get { return _top; }
            set
            {
                _top = value;
                RaisePropertyChanged(() => Top);
            }
        }

        public override string ToString()
        {
            return string.Format("Left: {0}, Top: {1}", _left, _top);
        }
    }
}