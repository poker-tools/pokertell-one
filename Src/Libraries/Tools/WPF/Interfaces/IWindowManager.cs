namespace Tools.WPF.Interfaces
{
    using System;

    using Tools.Interfaces;

    public interface IWindowManager : IDisposable
    {
        object DataContext { get; set; }

        IWindowManager Show();

        IWindowManager BringIntoView();

        IWindowManager Hide();

        IntPtr Handle { get; }

        double Left { get; set; }

        double Top { get; set; }

        double Width { get; set; }

        double Height { get; set; }

        IWindowManager ShowDialog();

        IWindowManager Activate();
    }
}