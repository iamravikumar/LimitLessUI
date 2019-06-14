﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;


/*
End-User Licence Agreement (EULA) for WithoutCaps Software 

This version is current as of May 27, 2017. Please consult withoutcapsdev@gmail.com for any new versions of this EULA.

You can only use the software known as "LimitlessUI" which is currently maintained by the WithoutCaps Team after you agree to this licence. By using this software, you agree to all of the clauses in the WithoutCaps Software EULA.

PLEASE READ CAREFULLY BEFORE USING THIS PRODUCT: This End-User Licence Agreement(EULA) is a legal agreement between you (either an individual or as a single entity) and the entity that is known as the WithoutCaps Team.

(a) Introduction. This is the End-User Licence Agreement (EULA) for the software known as "LimitlessUI" which is currently maintained by the WithoutCaps Team. This EULA outlines the clauses of the licence that the WithoutCaps Team is willing to grant you (either as an individual or as a single entity) to use this software.

(b) Licence. The entity known as the WithoutCaps Team will grant a free of charge, fully-revocable, non-exclusive, non-transferable licence to any person obtaining a copy of the software known as "LimitlessUI" as well as the associated documentation. The aforementioned documentation consists of the End-User Licence Agreement (EULA) for the product known as "LimitlessUI" which is currently maintained by the WithoutCaps Team. This licence permits you to use, modify and re-distribute this software non-commercially so long as you (either an individual or as a single entity) has permission from the WithoutCaps Team to do so. If the user wants to re-distribute software made by the WithoutCaps Team this EULA must be included in the software package.

(c) Ownership. The software known as "LimitlessUI" and produced by the WithoutCaps Team is licenced, not sold, to you (either an individual or as a single entity) and as such the WithoutCaps Software Team reserves any rights not explicitly granted to you (either an individual or as a single entity).

The WithoutCaps Team also reserves the right to revoke any persons (either an individual or as a single entity) licence without previous notification or agreements as long as said the person (either an individual or as a single entity) didn't adhere to the End-User Licence Agreement (EULA) distributed with this software.

Notwithstanding the terms and conditions of this EULA, any part of the software contained within the product known as "LimitlessUI" which is currently maintained by the WithoutCaps Team which constitutes Third Party Software and as such now is licenced to you subject to the terms and conditions of the software licence agreement accompanying such Third Party Software. Whatever the form of the licence, whether it be in the form of a discrete agreement, shrink wrap licence or electronic licence terms are accepted at the time of acceptance of the End-User Licence Agreement for the software known as "LimitlessUI" which is currently maintained by the WithoutCaps Team.

(d) Limitation of Liability. THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

Copyright (c) 2017 WithoutCaps
*/


namespace LimitlessUI
{
    public class Chart_WOC : Control
    {
        private int _lineThikness = 1;
        private int _chartLineThikness = 3;
        private int _minYValue;
        private int _maxYValue = 100;
        private int _chartLength = 100;

        private bool _hovering = false;
        private float _xLineIncrement;
        private float _valueInterval = 50;

        private PointF _chartArea = new PointF(0, 0);
        public List<Serie> Series = new List<Serie>();


        public Chart_WOC()
        {
            ForeColor = Color.FromArgb(42, 42, 42);
            DoubleBuffered = true;
            PaddingChanged += (sender, e) =>
            {
                CalculateConsts();
                Invalidate();
            };
            MouseMove += (sender, e) => Invalidate();
            MouseEnter += (sender, e) => _hovering = true;
            MouseLeave += (sender, e) =>
            {
                _hovering = false;
                Invalidate();
            };
            SizeChanged += (sender, e) => CalculateConsts();
            CalculateConsts();
        }


        private void CalculateConsts()
        {
            _chartArea.X = Width - Padding.Right - Padding.Left;
            _chartArea.Y = Height - Padding.Top - Padding.Bottom;
            _xLineIncrement = (float) Math.Round(_chartArea.X / _chartLength);
        }

        private void DrawChartArea(Graphics g, Pen pen1, float chartAreaLength, float labelsCountY)
        {
            g.TranslateTransform(Padding.Left, Height - Padding.Bottom);
            g.DrawLine(pen1, 0, 0, chartAreaLength, 0);
            g.DrawLine(pen1, 0, 0, 0, -(Height - Padding.Top - Padding.Bottom));

            float yLabelsCord = 0;
            for (int i = 0; i <= labelsCountY; i++)
            {
                var label = (i * _valueInterval).ToString();
                var labelSize = g.MeasureString(label, Font);
                g.DrawString(label, Font, new SolidBrush(ForeColor), -labelSize.Width,
                    -(yLabelsCord + labelSize.Height / 2));
                yLabelsCord += (Height - Padding.Top - Padding.Bottom) / labelsCountY;
            }
        }

        private void DrawMouseLine(Graphics g, Pen pen)
        {
            Point cor = PointToClient(Cursor.Position);
            float a = (cor.X - Padding.Left) / _xLineIncrement;
            int element = (int) Math.Round(a);

            if (_hovering && element > 0 && cor.X < _chartArea.X + Padding.Left)
            {
                pen.Color = ForeColor;
                pen.Width = 1F;

                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                g.DrawLine(pen, cor.X - Padding.Left, 0, cor.X - Padding.Left, -_chartArea.Y);

                var lastY = -_chartArea.Y;

                foreach (Serie serie in Series)
                    if (serie.Selected && element < serie.Values.Count)
                    {
                        var s = serie.Values.ElementAt(element).ToString();
                        var strSize = g.MeasureString(s, Font);

                        var mouseXPos = cor.X - Padding.Left;
                        var labelSizeByTwo = strSize.Height / 2;

                        pen.Color = ForeColor;
                        pen.Width = 1;

                        g.DrawLine(pen, mouseXPos, lastY + labelSizeByTwo, mouseXPos - 10, lastY + labelSizeByTwo);
                        g.DrawString(s, Font, new SolidBrush(ForeColor), mouseXPos - 12 - strSize.Width, lastY);

                        pen.Color = serie.LineColor;
                        pen.Width = 2;
                        g.DrawLine(pen, mouseXPos - 10, lastY + 1, mouseXPos - 10, lastY + strSize.Height - 1);
                        lastY += strSize.Height;
                    }
            }
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            Graphics g = pe.Graphics;
            Pen pen1 = new Pen(ForeColor, _lineThikness);

            DrawChartArea(g, pen1, _chartArea.X, (_maxYValue - _minYValue) / _valueInterval);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; //check
            pen1.Width = _chartLineThikness;
            foreach (Serie serie in Series)
            {
                pen1.Color = serie.LineColor;
                float xLineCord = 0;
                if (serie.Values.Count != 0)
                {
                    PointF lastPoint = new PointF(0, -serie.Values.First());
                    foreach (int yVal in serie.Values)
                    {
                        PointF point = new PointF(xLineCord, -yVal);
                        g.DrawLine(pen1, lastPoint, point);
                        lastPoint = point;
                        lastPoint.X = lastPoint.X - 1;
                        xLineCord += _xLineIncrement;
                    }
                }
            }
            DrawMouseLine(g, pen1);

            pen1.Dispose();
        }

        public void AddSerie(Color lineColor, string name, bool selected) =>
            Series.Add(new Serie(name, lineColor, selected));


        public void AddValue(int serie, int value)
        {
            var values = Series.ElementAt(serie).Values;
            values.Add(value);
            while (_chartLength <= values.Count - 2)
                values.Remove(values.First());

            Invalidate();
        }

        #region Getters and Setters

        public int LineThikness
        {
            get { return _lineThikness; }
            set
            {
                _lineThikness = value;
                Invalidate();
            }
        }

        public int ChartLineThikness
        {
            get { return _chartLineThikness; }
            set
            {
                _chartLineThikness = value;
                Invalidate();
            }
        }

        public int MinYValue
        {
            get { return _minYValue; }
            set
            {
                _minYValue = value; 
                Invalidate();
            }
        }

        public int MaxYValue
        {
            get { return _maxYValue; }
            set
            {
                _maxYValue = value;
                Invalidate();
            }
        }

        public float ValueInterval
        {
            get { return _valueInterval; }
            set
            {
                _valueInterval = value;
                Invalidate();
            }
        }

        public int ChartLength
        {
            get { return _chartLength; }
            set
            {
                _chartLength = value;
                CalculateConsts();
                Invalidate();
            }
        }

        #endregion

        //-----------------------------------[Serie Class]-----------------------------------//
        public class Serie
        {
            public string Name;
            public bool Selected;
            public List<int> Values = new List<int>();
            public Color LineColor;

            public Serie(string name, Color lineColor, bool selected = true)
            {
                LineColor = lineColor;
                Name = name;
                Selected = selected;
            }
        }
    }
}