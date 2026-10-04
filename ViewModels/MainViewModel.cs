using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Lab3.Models;

namespace Lab3.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private const double XMin = 0.0;
        private const double XMax = 2.0 * Math.PI;
        private const double PlotWidth = 800.0;
        private const double PlotHeight = 420.0;
        private const double YMin = -1.1;
        private const double YMax = 1.1;

        private string h1Text = "0.8";
        private string h2Text = "0.1";

        private bool showReference = true;
        private bool showSupport = true;
        private bool showLinear = true;
        private bool showHermite = true;
        private bool showError = true;

        private string h1Error = "";
        private string h2Error = "";
        private string status = "Введіть параметри та натисніть «Побудувати».";

        public PointCollection ReferencePoints { get; private set; } =
            new PointCollection();

        public PointCollection SupportPoints { get; private set; } =
            new PointCollection();

        public PointCollection LinearPoints { get; private set; } =
            new PointCollection();

        public PointCollection HermitePoints { get; private set; } =
            new PointCollection();

        public PointCollection LinearErrorPoints { get; private set; } =
            new PointCollection();

        public PointCollection HermiteErrorPoints { get; private set; } =
            new PointCollection();

        public ObservableCollection<Point> SupportMarkers { get; } =
            new ObservableCollection<Point>();

        public string H1Text
        {
            get => h1Text;
            set
            {
                if (h1Text == value)
                    return;

                h1Text = value;
                OnPropertyChanged();
            }
        }

        public string H2Text
        {
            get => h2Text;
            set
            {
                if (h2Text == value)
                    return;

                h2Text = value;
                OnPropertyChanged();
            }
        }

        public bool ShowReference
        {
            get => showReference;
            set
            {
                if (showReference == value)
                    return;

                showReference = value;
                OnPropertyChanged();
            }
        }

        public bool ShowSupport
        {
            get => showSupport;
            set
            {
                if (showSupport == value)
                    return;

                showSupport = value;
                OnPropertyChanged();
            }
        }

        public bool ShowLinear
        {
            get => showLinear;
            set
            {
                if (showLinear == value)
                    return;

                showLinear = value;
                OnPropertyChanged();
            }
        }

        public bool ShowHermite
        {
            get => showHermite;
            set
            {
                if (showHermite == value)
                    return;

                showHermite = value;
                OnPropertyChanged();
            }
        }

        public bool ShowError
        {
            get => showError;
            set
            {
                if (showError == value)
                    return;

                showError = value;
                OnPropertyChanged();
            }
        }

        public string H1Error
        {
            get => h1Error;
            private set
            {
                if (h1Error == value)
                    return;

                h1Error = value;
                OnPropertyChanged();
            }
        }

        public string H2Error
        {
            get => h2Error;
            private set
            {
                if (h2Error == value)
                    return;

                h2Error = value;
                OnPropertyChanged();
            }
        }

        public string Status
        {
            get => status;
            private set
            {
                if (status == value)
                    return;

                status = value;
                OnPropertyChanged();
            }
        }

        public ICommand BuildCommand { get; }
        public ICommand CheckCommand { get; }

        public MainViewModel()
        {
            BuildCommand = new RelayCommand(Build);
            CheckCommand = new RelayCommand(RunChecks);

            Build();
        }

        private void Build()
        {
            H1Error = "";
            H2Error = "";

            if (!TryParsePositive(H1Text, out double h1))
            {
                H1Error = "h₁ має бути додатним числом.";
                Status = "Виправте значення h₁.";
                return;
            }

            if (!TryParsePositive(H2Text, out double h2))
            {
                H2Error = "h₂ має бути додатним числом.";
                Status = "Виправте значення h₂.";
                return;
            }

            if (h2 >= h1)
            {
                H2Error = "h₂ повинно бути меншим за h₁.";
                Status = "Потрібно виконати умову h₂ < h₁.";
                return;
            }

            List<PointD> supportPoints = BuildSupportPoints(h1);
            List<PointD> finePoints = BuildFinePoints(h2);

            Interpolation[] interpolations =
            {
                new LinearInterpolation(),
                new HermiteInterpolation()
            };

            List<PointD> linearData = new List<PointD>();
            List<PointD> hermiteData = new List<PointD>();

            foreach (PointD point in finePoints)
            {
                linearData.Add(
                    new PointD(
                        point.X,
                        interpolations[0].Calculate(
                            supportPoints,
                            point.X)));

                hermiteData.Add(
                    new PointD(
                        point.X,
                        interpolations[1].Calculate(
                            supportPoints,
                            point.X)));
            }

            List<PointD> referenceData = BuildReferencePoints();

            ReferencePoints = ToScreenPoints(
                referenceData,
                YMin,
                YMax);

            SupportPoints = ToScreenPoints(
                supportPoints,
                YMin,
                YMax);

            LinearPoints = ToScreenPoints(
                linearData,
                YMin,
                YMax);

            HermitePoints = ToScreenPoints(
                hermiteData,
                YMin,
                YMax);

            BuildErrorPoints(
                finePoints,
                linearData,
                hermiteData);

            BuildSupportMarkers(supportPoints);

            OnPropertyChanged(nameof(ReferencePoints));
            OnPropertyChanged(nameof(SupportPoints));
            OnPropertyChanged(nameof(LinearPoints));
            OnPropertyChanged(nameof(HermitePoints));
            OnPropertyChanged(nameof(LinearErrorPoints));
            OnPropertyChanged(nameof(HermiteErrorPoints));

            Status =
                $"Побудовано. Опорних точок: {supportPoints.Count}, " +
                $"точок A2: {finePoints.Count}.";
        }

        private List<PointD> BuildSupportPoints(double step)
        {
            return BuildPoints(step);
        }

        private List<PointD> BuildFinePoints(double step)
        {
            return BuildPoints(step);
        }

        private List<PointD> BuildPoints(double step)
        {
            var points = new List<PointD>();

            double x = XMin;

            while (x < XMax)
            {
                points.Add(
                    new PointD(
                        x,
                        Math.Sin(x)));

                x += step;
            }

            if (points.Count == 0 ||
                Math.Abs(points[points.Count - 1].X - XMax) > 1e-10)
            {
                points.Add(
                    new PointD(
                        XMax,
                        Math.Sin(XMax)));
            }

            return points;
        }

        private List<PointD> BuildReferencePoints()
        {
            return BuildPoints(0.02);
        }

        private PointCollection ToScreenPoints(
            IReadOnlyList<PointD> points,
            double minY,
            double maxY)
        {
            var result = new PointCollection();

            foreach (PointD point in points)
            {
                double screenX =
                    (point.X - XMin) /
                    (XMax - XMin) *
                    PlotWidth;

                double screenY =
                    PlotHeight -
                    (point.Y - minY) /
                    (maxY - minY) *
                    PlotHeight;

                result.Add(
                    new Point(
                        screenX,
                        screenY));
            }

            return result;
        }

        private void BuildErrorPoints(
            IReadOnlyList<PointD> original,
            IReadOnlyList<PointD> linear,
            IReadOnlyList<PointD> hermite)
        {
            double maxError = 0.0;

            for (int i = 0; i < original.Count; i++)
            {
                double linearError =
                    Math.Abs(
                        original[i].Y -
                        linear[i].Y);

                double hermiteError =
                    Math.Abs(
                        original[i].Y -
                        hermite[i].Y);

                maxError =
                    Math.Max(
                        maxError,
                        Math.Max(
                            linearError,
                            hermiteError));
            }

            if (maxError < 0.000001)
                maxError = 0.000001;

            var linearErrors =
                new List<PointD>();

            var hermiteErrors =
                new List<PointD>();

            for (int i = 0; i < original.Count; i++)
            {
                linearErrors.Add(
                    new PointD(
                        original[i].X,
                        Math.Abs(
                            original[i].Y -
                            linear[i].Y)));

                hermiteErrors.Add(
                    new PointD(
                        original[i].X,
                        Math.Abs(
                            original[i].Y -
                            hermite[i].Y)));
            }

            LinearErrorPoints =
                ToErrorScreenPoints(
                    linearErrors,
                    maxError);

            HermiteErrorPoints =
                ToErrorScreenPoints(
                    hermiteErrors,
                    maxError);
        }

        private PointCollection ToErrorScreenPoints(
            IReadOnlyList<PointD> points,
            double maxError)
        {
            var result = new PointCollection();

            foreach (PointD point in points)
            {
                double screenX =
                    (point.X - XMin) /
                    (XMax - XMin) *
                    PlotWidth;

                double screenY =
                    PlotHeight -
                    point.Y /
                    maxError *
                    PlotHeight;

                result.Add(
                    new Point(
                        screenX,
                        screenY));
            }

            return result;
        }

        private void BuildSupportMarkers(
            IReadOnlyList<PointD> points)
        {
            SupportMarkers.Clear();

            foreach (PointD point in points)
            {
                double screenX =
                    (point.X - XMin) /
                    (XMax - XMin) *
                    PlotWidth;

                double screenY =
                    PlotHeight -
                    (point.Y - YMin) /
                    (YMax - YMin) *
                    PlotHeight;

                SupportMarkers.Add(
                    new Point(
                        screenX,
                        screenY));
            }
        }

        private void RunChecks()
        {
            try
            {
                var points =
                    new List<PointD>
                    {
                        new PointD(0.0, 0.0),
                        new PointD(1.0, Math.Sin(1.0)),
                        new PointD(2.0, Math.Sin(2.0)),
                        new PointD(3.0, Math.Sin(3.0))
                    };

                Interpolation[] methods =
                {
                    new LinearInterpolation(),
                    new HermiteInterpolation()
                };

                foreach (Interpolation method in methods)
                {
                    foreach (PointD point in points)
                    {
                        double result =
                            method.Calculate(
                                points,
                                point.X);

                        if (Math.Abs(result - point.Y) > 1e-10)
                        {
                            Status =
                                "Перевірку вузлів не пройдено.";
                            return;
                        }
                    }
                }

                var irregularPoints =
                    new List<PointD>
                    {
                        new PointD(0.0, 0.0),
                        new PointD(0.4, Math.Sin(0.4)),
                        new PointD(1.7, Math.Sin(1.7)),
                        new PointD(3.0, Math.Sin(3.0))
                    };

                double testValue =
                    methods[1].Calculate(
                        irregularPoints,
                        1.0);

                if (double.IsNaN(testValue) ||
                    double.IsInfinity(testValue))
                {
                    Status =
                        "Перевірку нерівновіддалених точок не пройдено.";
                    return;
                }

                try
                {
                    var invalidPoints =
                        new List<PointD>
                        {
                            new PointD(0.0, 0.0),
                            new PointD(1.0, 1.0),
                            new PointD(1.0, 2.0)
                        };

                    methods[0].Calculate(
                        invalidPoints,
                        0.5);

                    Status =
                        "Помилку однакових X не виявлено.";
                    return;
                }
                catch (ArgumentException)
                {
                }

                Status =
                    "Перевірку пройдено: вузли, нерівновіддалені " +
                    "точки та заборона однакових X працюють правильно.";
            }
            catch (Exception ex)
            {
                Status =
                    $"Помилка перевірки: {ex.Message}";
            }
        }

        private static bool TryParsePositive(
            string text,
            out double value)
        {
            return double.TryParse(
                       text,
                       NumberStyles.Float,
                       CultureInfo.CurrentCulture,
                       out value)
                   && value > 0;
        }

        public event PropertyChangedEventHandler?
            PropertyChanged;

        protected void OnPropertyChanged(
            [CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
    }
}