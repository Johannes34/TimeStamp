using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace TimeStamp
{
    public partial class TimelineControl : UserControl, IThemed
    {
        public TimelineControl()
        {
            InitializeComponent();

            DoubleBuffered = true;

            // default theme:

            ForeColor = SystemColors.GrayText;
            EdgeColor = SystemColors.InactiveCaptionText;

            HighlightForeColor = SystemColors.GradientActiveCaption;
            HighlightEdgeColor = SystemColors.ActiveCaption;

            HighlightInActiveForeColor = SystemColors.GradientInactiveCaption;
            HighlightInActiveEdgeColor = SystemColors.InactiveCaption;
        }


        // Properties:


        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<TimelineSection> Sections { get; set; } = new List<TimelineSection>();

        public bool DrawDisplayTexts { get; set; } = true;

        public int TimelineThickness { get; set; } = 8;

        public int EmptyTimelineThickness { get; set; } = 1;

        public Size MajorTicklineSize { get; set; } = new Size(6, 14);

        public Size MinorTicklineSize { get; set; } = new Size(4, 10);


        public double MinimumValue { get; set; }

        public double MaximumValue { get; set; }

        public bool DrawSectionDisplayTexts { get; set; }

        public bool AddSectionMode { get; set; } = false;

        public Dictionary<string, double> CustomSectionDisplayTexts { get; private set; } = new Dictionary<string, double>();

        private double CalculatePercentLocation(double actualValue)
        {
            if (actualValue <= MinimumValue)
                return 0;
            else if (actualValue >= MaximumValue)
                return 1;
            else
                return (actualValue - MinimumValue) / (MaximumValue - MinimumValue);
        }

        private double CalculateActualValue(double percentLocation)
        {
            return MinimumValue + (percentLocation * (MaximumValue - MinimumValue));
        }

        // Theming:

        public Color EdgeColor { get; set; } = Color.Black;
        public Color HighlightEdgeColor { get; set; } = Color.Black;
        public Color HighlightForeColor { get; set; } = Color.Black;

        public Color HighlightInActiveForeColor { get; set; }
        public Color HighlightInActiveEdgeColor { get; set; }


        // Helper Functions:

        private Color GetEdgeColor(TimelineSection section, TimelineSectionComponents component)
        {
            bool isActivelyHighlighted = section.MouseOver == component;
            bool isInActivelyHighlighted = !isActivelyHighlighted && section.MouseOver != TimelineSectionComponents.None;

            IThemed componentItem = section;
            if (component == TimelineSectionComponents.StartSeparator)
                componentItem = section.Start;
            else if (component == TimelineSectionComponents.EndSeparator)
                componentItem = section.End;

            return GetEdgeColor(componentItem, isActivelyHighlighted, isInActivelyHighlighted);
        }

        private Color GetEdgeColor(IThemed item, bool isActivelyHighlighted, bool isInActivelyHighlighted)
        {
            if (isActivelyHighlighted)
                return item.HighlightEdgeColor.IsEmpty ? HighlightEdgeColor : item.HighlightEdgeColor;
            else if (isInActivelyHighlighted)
                return item.HighlightInActiveEdgeColor.IsEmpty ? HighlightInActiveEdgeColor : item.HighlightInActiveEdgeColor;

            return item.EdgeColor.IsEmpty ? EdgeColor : item.EdgeColor;
        }

        private Color GetForeColor(TimelineSection section, TimelineSectionComponents component)
        {
            bool isActivelyHighlighted = section.MouseOver == component;
            bool isInActivelyHighlighted = !isActivelyHighlighted && section.MouseOver != TimelineSectionComponents.None;

            IThemed componentItem = section;
            if (component == TimelineSectionComponents.StartSeparator)
                componentItem = section.Start;
            else if (component == TimelineSectionComponents.EndSeparator)
                componentItem = section.End;

            return GetForeColor(componentItem, isActivelyHighlighted, isInActivelyHighlighted);
        }

        private Color GetForeColor(IThemed item, bool isActivelyHighlighted, bool isInActivelyHighlighted)
        {
            if (isActivelyHighlighted)
                return item.HighlightForeColor.IsEmpty ? HighlightForeColor : item.HighlightForeColor;
            else if (isInActivelyHighlighted)
                return item.HighlightInActiveForeColor.IsEmpty ? HighlightInActiveForeColor : item.HighlightInActiveForeColor;

            return item.ForeColor.IsEmpty ? ForeColor : item.ForeColor;
        }

        private Pen GetPen(TimelineSection section, TimelineSectionComponents component)
        {
            switch (section.Style)
            {
                case DrawingStyle.Solid:
                    return new Pen(GetForeColor(section, component), TimelineThickness);
                case DrawingStyle.Twisted:
                    var tileImg = new Bitmap(TimelineThickness, TimelineThickness);
                    var grp = Graphics.FromImage(tileImg);
                    var color = GetForeColor(section, component);
                    grp.FillRectangle(Brushes.Transparent, new RectangleF(0, 0, TimelineThickness, TimelineThickness));
                    int x = -TimelineThickness / 2;
                    int distance = TimelineThickness;
                    int top = 0;
                    int bot = TimelineThickness;
                    for (int i = 0; i < 3; i++)
                        grp.DrawLine(new Pen(color, 1), x + i * (TimelineThickness / 2), top, x + i * (TimelineThickness / 2) + distance, bot);
                    var twistedPen = new Pen(new TextureBrush(tileImg, System.Drawing.Drawing2D.WrapMode.Tile), TimelineThickness);
                    return twistedPen;
                default:
                    throw new NotImplementedException();
            }
        }

        private int GetTimelineY()
        {
            return Padding.Top + (DrawSectionDisplayTexts ? TextRenderer.MeasureText("Test", Font).Height : 0) + (Math.Max(MajorTicklineSize.Height, MinorTicklineSize.Height) + 1) / 2;
        }

        public int TimelineY => GetTimelineY();

        public int CalculateX(double actualValue)
        {
            return (int)(Padding.Left + (Width - Padding.Horizontal) * CalculatePercentLocation(actualValue));
        }

        public double CalculateValue(int x)
        {
            var percentPos = (x - Padding.Left) / (double)(Width - Padding.Horizontal);
            return CalculateActualValue(percentPos);
        }

        private Point GetStartPoint(TimelineSection section)
        {
            return GetStartPoint(section.Start);
        }
        private Point GetStartPoint(TimelineLocation separator)
        {
            int startX = CalculateX(separator.Value);
            int y = GetTimelineY();

            return new Point(startX, y);
        }

        private Point GetEndPoint(TimelineSection section)
        {
            int endX = CalculateX(section.End.Value);
            int y = GetTimelineY();

            return new Point(endX, y);
        }

        private Rectangle GetStartBounds(TimelineSection section)
        {
            var startPoint = GetStartPoint(section);

            var size = IsMajorTickline(section.Start) ? MajorTicklineSize : MinorTicklineSize;
            return new Rectangle(startPoint.X - size.Width / 2, startPoint.Y - size.Height / 2, size.Width, size.Height);
        }

        private Rectangle GetEndBounds(TimelineSection section)
        {
            var endPoint = GetEndPoint(section);

            var size = IsMajorTickline(section.End) ? MajorTicklineSize : MinorTicklineSize;
            return new Rectangle(endPoint.X - size.Width / 2, endPoint.Y - size.Height / 2, size.Width, size.Height);
        }

        private Rectangle GetLineBounds(TimelineSection section)
        {
            var startPoint = GetStartPoint(section);
            var endPoint = GetEndPoint(section);

            return new Rectangle(startPoint.X, startPoint.Y - TimelineThickness / 2, endPoint.X - startPoint.X, TimelineThickness);
        }

        private bool IsMajorTickline(TimelineLocation point)
        {
            var collection = (DraggedSectionsPreview ?? Sections);
            var index = collection.IndexOf(point.OfSection);
            if (index == -1)
                return false;
            if (point == point.OfSection.Start)
            {
                return index == 0 || (collection[index - 1].End.DisplayText != point.DisplayText || collection[index - 1].TooltipHeader != point.OfSection.TooltipHeader);
            }
            else
            {
                return index == collection.Count - 1 || (collection[index + 1].Start.DisplayText != point.DisplayText || collection[index + 1].TooltipHeader != point.OfSection.TooltipHeader);
            }
        }

        // Painting:

        protected override void OnPaint(PaintEventArgs e)
        {
            var sectionsToDraw = DraggedSectionsPreview ?? Sections;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            List<RectangleF> drawnTextAreas = new List<RectangleF>();

            // draw pending add section item:
            if (AddSectionMode && !double.IsNaN(CurrentMousePosition))
            {
                var x = CalculateX(CurrentMousePosition);
                int height = (int)(TimelineThickness / 1.2);

                if (!double.IsNaN(AddSectionStartMousePosition))
                {
                    var endX = CalculateX(AddSectionStartMousePosition);
                    e.Graphics.DrawRectangle(new Pen(ForeColor, EmptyTimelineThickness), Math.Min(x, endX), GetTimelineY() - height, Math.Max(x, endX) - Math.Min(x, endX), 2 * height);
                }
                else if (!IsMouseOverAnyTimelineElement)
                {
                    e.Graphics.DrawLine(new Pen(ForeColor, EmptyTimelineThickness), x, GetTimelineY() - height, x, GetTimelineY() + height);
                }
            }

            // draw mouse cursor position display text
            if (DrawDisplayTexts && !double.IsNaN(CurrentMousePosition))
            {
                var x = CalculateX(CurrentMousePosition);
                var text = TimeSpan.FromMinutes(CurrentMousePosition).ToString("hh\\:mm");
                DrawSeparatorText(e, text, new Point(x, GetTimelineY()), true, drawnTextAreas, EdgeColor);
            }

            // draw empty timeline:
            if (EmptyTimelineThickness > 0)
            {
                e.Graphics.DrawLine(new Pen(ForeColor, EmptyTimelineThickness), Padding.Left, GetTimelineY(), Width - Padding.Horizontal, GetTimelineY());
            }

            if (sectionsToDraw.Any())
            {
                // draw section display texts (above timeline):
                if (DrawSectionDisplayTexts)
                {
                    Dictionary<string, RectangleF> drawnSectionDisplayTexts = new Dictionary<string, RectangleF>();
                    var sectionTexts = sectionsToDraw.Select(s => new Tuple<string, int, Color>(s.DisplayText, GetStartPoint(s).X, s.ForeColor));
                    var customTexts = CustomSectionDisplayTexts.Select(s => new Tuple<string, int, Color>(s.Key, CalculateX(s.Value), ForeColor));
                    var allTexts = sectionTexts.Concat(customTexts).OrderBy(t => t.Item2).ToArray();
                    foreach (var text in allTexts)
                    {
                        if (!String.IsNullOrWhiteSpace(text.Item1) && !drawnSectionDisplayTexts.ContainsKey(text.Item1))
                        {
                            var nextFreeX = drawnSectionDisplayTexts.Any() ? (int)drawnSectionDisplayTexts.Max(t => t.Value.Right) : 0;
                            var textPoint = new Point(Math.Max(text.Item2, nextFreeX), Padding.Top);
                            var textBounds = new RectangleF(textPoint, e.Graphics.MeasureString(text.Item1, Font));
                            using (var b = new SolidBrush(text.Item3))
                            {
                                e.Graphics.DrawString(text.Item1, Font, b, textBounds);
                            }
                            drawnSectionDisplayTexts.Add(text.Item1, textBounds);
                        }
                    }
                }

                // draw sections and separators:
                foreach (var section in sectionsToDraw.OrderBy(s => s.Start.Value))
                {
                    var start = GetStartPoint(section);
                    var end = GetEndPoint(section);

                    e.Graphics.DrawLine(GetPen(section, TimelineSectionComponents.Line), start, end);

                    DrawSeparator(e, section, TimelineSectionComponents.StartSeparator);

                    DrawSeparator(e, section, TimelineSectionComponents.EndSeparator);
                }

                // draw separator texts:
                if (DrawDisplayTexts)
                {
                    // Order/priorization of drawn separator texts (order of texts being draw and reserving space first):
                    foreach (var separator in sectionsToDraw.SelectMany(s => new[] { s.Start, s.End }).Where(s => s != null).OrderByDescending(s => s.DisplayTextOrder).ThenBy(s => s.Value))
                    {
                        var start = GetStartPoint(separator);

                        DrawSeparatorText(e, separator.DisplayText, start, IsMajorTickline(separator), drawnTextAreas, EdgeColor);
                    }
                }
            }

            base.OnPaint(e);
        }

        private void DrawSeparator(PaintEventArgs e, TimelineSection section, TimelineSectionComponents component)
        {
            Rectangle bounds;
            bool isMajor;

            switch (component)
            {
                case TimelineSectionComponents.StartSeparator:
                    bounds = GetStartBounds(section);
                    isMajor = IsMajorTickline(section.Start);
                    break;
                case TimelineSectionComponents.EndSeparator:
                    if (!section.End.IsVisible)
                        return;
                    bounds = GetEndBounds(section);
                    isMajor = IsMajorTickline(section.End);
                    break;
                default:
                    throw new NotImplementedException();
            }

            if (isMajor)
            {
                e.Graphics.FillRectangle(new SolidBrush(GetForeColor(section, component)), bounds);
                e.Graphics.DrawRectangle(new Pen(GetEdgeColor(section, component), 1), bounds);
            }
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(GetForeColor(section, component)), bounds);
                e.Graphics.DrawRectangle(new Pen(GetEdgeColor(section, component), 1), bounds);
            }
        }

        private void DrawSeparatorText(PaintEventArgs e, string text, Point point, bool isMajor, List<RectangleF> drawnTextAreas, Color color)
        {
            if (!String.IsNullOrEmpty(text))
            {
                var font = isMajor ? Font : new Font(Font.FontFamily, Font.Size - 1, Font.Style);

                var size = e.Graphics.MeasureString(text, font);
                var proposedLocation = new Point((int)(point.X - size.Width / 2), point.Y + Math.Max(MajorTicklineSize.Height, MinorTicklineSize.Height) + 3);
                var rectangle = new RectangleF(proposedLocation, size);

                // allow slight left/right movement for drawing nodes when space is blocked:
                bool hasSpace = !drawnTextAreas.Any(r => r.IntersectsWith(rectangle));
                if (!hasSpace)
                {
                    int allowedDisplacement = 15;
                    RectangleF leftTemp = new RectangleF(rectangle.Location, rectangle.Size);
                    RectangleF rightTemp = new RectangleF(rectangle.Location, rectangle.Size);
                    for (int i = 0; i < allowedDisplacement; i++)
                    {
                        leftTemp.Offset(-1, 0);
                        hasSpace = !drawnTextAreas.Any(r => r.IntersectsWith(leftTemp));
                        if (hasSpace)
                        {
                            rectangle = leftTemp;
                            break;
                        }

                        rightTemp.Offset(1, 0);
                        hasSpace = !drawnTextAreas.Any(r => r.IntersectsWith(rightTemp));
                        if (hasSpace)
                        {
                            rectangle = rightTemp;
                            break;
                        }
                    }
                }

                // draw text:
                if (hasSpace)
                {
                    e.Graphics.DrawString(text, font, new SolidBrush(color), rectangle);
                    drawnTextAreas.Add(rectangle);
                }
#if DEBUG
                // debug: draw dotted red rectangle of blocked/undrawn item
                else
                {
                    e.Graphics.DrawRectangle(new Pen(Color.LightSalmon) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash }, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
                }
#endif
            }
        }

        protected override void OnResize(EventArgs e)
        {
            Invalidate();
            base.OnResize(e);
        }

        // Mouse Over:

        private double CurrentMousePosition { get; set; } = double.NaN;
        private bool IsMouseOverAnyTimelineElement { get; set; }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!double.IsNaN(AddSectionStartMousePosition) && e.Button == MouseButtons.None)
                AddSectionStartMousePosition = double.NaN;

            // show current mouse position value:
            var val = CalculateValue(e.Location.X);
            if (CurrentMousePosition != val)
            {
                CurrentMousePosition = val;
                Invalidate();
            }

            if (Sections.Any())
            {
                if (Drag(e, false))
                    return;
                else
                {
                    // Change color on mouse over:
                    foreach (var section in Sections)
                    {
                        var start = GetStartBounds(section);
                        var end = GetEndBounds(section);
                        var line = GetLineBounds(section);

                        if (start.Contains(e.Location))
                        {
                            if (section.MouseOver != TimelineSectionComponents.StartSeparator)
                            {
                                section.MouseOver = TimelineSectionComponents.StartSeparator;
                                Invalidate();
                                toolTip1.Show($"{section.Start.DisplayText}{Environment.NewLine}{section.TooltipHeader}{Environment.NewLine}{section.TooltipBody}", this, start.Left, this.Height /*start.Top - 50*/);
                            }
                        }
                        else if (section.End.IsVisible && end.Contains(e.Location))
                        {
                            if (section.MouseOver != TimelineSectionComponents.EndSeparator)
                            {
                                section.MouseOver = TimelineSectionComponents.EndSeparator;
                                Invalidate();
                                toolTip1.Show($"{section.End.DisplayText}{Environment.NewLine}{section.TooltipHeader}{Environment.NewLine}{section.TooltipBody}", this, end.Left, this.Height /*end.Top - 50*/);
                            }
                        }
                        else if (line.Contains(e.Location))
                        {
                            if (section.MouseOver != TimelineSectionComponents.Line)
                            {
                                section.MouseOver = TimelineSectionComponents.Line;
                                Invalidate();
                                string duration = String.IsNullOrEmpty(section.TooltipDurationCustomText) ? $"{section.Start.DisplayText} - {section.End.DisplayText}" : section.TooltipDurationCustomText;
                                toolTip1.Show($"{duration}{Environment.NewLine}{section.TooltipHeader}{Environment.NewLine}{section.TooltipBody}", this, line.Left + line.Width / 2, this.Height /*start.Top - 50*/);
                            }
                        }
                        else
                        {
                            if (section.MouseOver != TimelineSectionComponents.None)
                            {
                                section.MouseOver = TimelineSectionComponents.None;
                                Invalidate();
                            }
                        }
                    }

                    IsMouseOverAnyTimelineElement = Sections.Any(s => s.MouseOver != TimelineSectionComponents.None);
                    if (!IsMouseOverAnyTimelineElement)
                    {
                        toolTip1.Hide(this);
                    }
                }
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            toolTip1.Hide(this);
            CurrentMousePosition = double.NaN;
            Invalidate();

            base.OnMouseLeave(e);
        }


        // Dragging:

        private bool Drag(MouseEventArgs e, bool commitChange)
        {
            if (DraggedItem == null)
                return false;

            var newActualPos = CalculateValue(e.Location.X);

            // Dragging:
            if (DraggedItem.OfType<TimelineLocation>().Any() && OnDragSeparator != null)
            {
                var end = DraggedItem.OfType<TimelineLocation>().FirstOrDefault(l => l.OfSection.End == l);
                var start = DraggedItem.OfType<TimelineLocation>().FirstOrDefault(l => l.OfSection.Start == l);
                DraggedSectionsPreview = OnDragSeparator(this, end, start, newActualPos, commitChange);
            }
            else if (DraggedItem.OfType<TimelineSection>().Any() && OnDragSection != null)
            {
                var actualStarPos = CalculateValue(DragStartLocation.X);

                var sec = DraggedItem.OfType<TimelineSection>().First();
                DraggedSectionsPreview = OnDragSection(this, sec, newActualPos - actualStarPos, commitChange);
            }

            if (commitChange)
                Sections = DraggedSectionsPreview;

            Invalidate();

            return true;
        }

        public delegate List<TimelineSection> DragSeparatorPreview(TimelineControl sender, TimelineLocation draggedEndSeparator, TimelineLocation draggedStartSeparator, double newPosition, bool commitPreview);
        public delegate List<TimelineSection> DragSectionPreview(TimelineControl sender, TimelineSection draggedSection, double offsetValue, bool commitPreview);
        public DragSeparatorPreview OnDragSeparator { get; set; }
        public DragSectionPreview OnDragSection { get; set; }

        private List<IThemed> DraggedItem { get; set; }

        private Point DragStartLocation { get; set; }

        private List<TimelineSection> DraggedSectionsPreview { get; set; }

        private double AddSectionStartMousePosition { get; set; } = double.NaN;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (AddSectionMode && !IsMouseOverAnyTimelineElement)
                {
                    AddSectionStartMousePosition = CalculateValue(e.Location.X);
                }

                if (Sections.Any())
                {
                    var dragged = new List<IThemed>();
                    foreach (var section in Sections)
                    {
                        var start = GetStartBounds(section);
                        var end = GetEndBounds(section);
                        var line = GetLineBounds(section);

                        if (start.Contains(e.Location))
                        {
                            dragged.Add(section.Start);
                            Debug.WriteLine($"Mouse Down on Start Point {section.Start.DisplayText} of '{section.TooltipHeader}'");
                        }
                        else if (section.End.IsVisible && end.Contains(e.Location))
                        {
                            dragged.Add(section.End);
                            Console.WriteLine($"Mouse Down on End Point {section.End.DisplayText} of '{section.TooltipHeader}'");
                        }
                        else if (line.Contains(e.Location))
                        {
                            dragged.Add(section);
                            Console.WriteLine($"Mouse Down on Line of '{section.TooltipHeader}'");
                        }
                    }

                    if (dragged.Any())
                    {
                        DraggedItem = dragged;
                        DragStartLocation = e.Location;
                    }
                }
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (DraggedItem != null)
                {
                    // Commit changes:
                    Drag(e, true);

                    DraggedItem = null;
                    DraggedSectionsPreview = null;
                    DragStartLocation = Point.Empty;
                }

                if (AddSectionMode && !double.IsNaN(AddSectionStartMousePosition))
                {
                    OnAddSection?.Invoke(this, Math.Min(CurrentMousePosition, AddSectionStartMousePosition), Math.Max(CurrentMousePosition, AddSectionStartMousePosition), e);
                }
                AddSectionStartMousePosition = double.NaN;

                Invalidate();
            }

            base.OnMouseUp(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (DraggedItem != null && e.KeyCode == Keys.Escape)
            {
                DraggedItem = null;
                DraggedSectionsPreview = null;
                DragStartLocation = Point.Empty;

                Invalidate();
            }

            base.OnKeyDown(e);
        }

        // Other Mouse Interaction:

        public delegate void SectionClick(TimelineControl sender, TimelineSection clickedSection, MouseEventArgs e, double clickedPosition);
        public SectionClick OnSectionClicked { get; set; }


        public delegate void SeparatorClick(TimelineControl sender, TimelineSection clickedEndSeparator, TimelineSection clickedStartSeparator, MouseEventArgs e, double clickedPosition);
        public SeparatorClick OnSeparatorClicked { get; set; }


        public delegate void AddSectionHandler(TimelineControl sender, double start, double end, MouseEventArgs e);
        public AddSectionHandler OnAddSection { get; set; }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            var newActualPos = CalculateValue(e.Location.X);

            var clickedLine = Sections.FirstOrDefault(s => s.MouseOver == TimelineSectionComponents.Line);
            if (clickedLine != null)
            {
                OnSectionClicked?.Invoke(this, clickedLine, e, newActualPos);
            }

            var clickedSeparator = Sections.Where(s => s.MouseOver == TimelineSectionComponents.StartSeparator || s.MouseOver == TimelineSectionComponents.EndSeparator).ToList();
            if (clickedSeparator.Any())
            {
                var end = clickedSeparator.FirstOrDefault(s => s.MouseOver == TimelineSectionComponents.EndSeparator);
                var start = clickedSeparator.FirstOrDefault(s => s.MouseOver == TimelineSectionComponents.StartSeparator);
                OnSeparatorClicked?.Invoke(this, end, start, e, newActualPos);
            }

            base.OnMouseClick(e);
        }
    }

    public class TimelineSection : IThemed
    {
        public TimelineSection(object keyObject, TimeSpan start, TimeSpan end, bool isEndVisible = true, string formatString = null)
        {
            Start = new TimelineLocation(this)
            {
                Value = start.TotalMinutes,
                DisplayText = start.ToString(formatString ?? "hh\\:mm"),
            };

            End = new TimelineLocation(this)
            {
                Value = end.TotalMinutes,
                DisplayText = end.ToString(formatString ?? "hh\\:mm"),
                IsVisible = isEndVisible
            };

            Tag = keyObject;
        }

        public TimelineLocation Start { get; set; }
        public TimelineLocation End { get; set; }


        public string TooltipDurationCustomText { get; set; }
        public string TooltipHeader { get; set; }
        public string TooltipBody { get; set; }
        public string DisplayText { get; set; }

        public object Tag { get; set; }

        public Color ForeColor { get; set; }
        public Color EdgeColor { get; set; }
        public Color HighlightForeColor { get; set; }
        public Color HighlightEdgeColor { get; set; }
        public Color HighlightInActiveForeColor { get; set; }
        public Color HighlightInActiveEdgeColor { get; set; }

        public DrawingStyle Style { get; set; } = DrawingStyle.Solid;

        internal TimelineSectionComponents MouseOver { get; set; }

        public override string ToString()
        {
            return Start.ToString() + " - " + End.ToString() + " " + TooltipHeader;
        }
    }

    public enum DrawingStyle
    {
        Solid,
        Twisted,
    }

    internal enum TimelineSectionComponents
    {
        None,
        StartSeparator,
        EndSeparator,
        Line
    }

    public class TimelineLocation : IThemed
    {
        internal TimelineLocation(TimelineSection ofSection)
        {
            OfSection = ofSection;
        }

        public double Value { get; set; }

        public string DisplayText { get; set; }
        /// <summary>
        /// The order / priorization for the <see cref="DisplayText"/>. The higher the value, the more important it is. This will affect the order of drawing and therefore reserving space in the text area. If a lower order number tries to be drawn on a space already reserved, it will not be drawn.
        /// </summary>
        public int DisplayTextOrder { get; set; }


        public TimelineSection OfSection { get; set; }

        public Color ForeColor { get; set; }
        public Color EdgeColor { get; set; }
        public Color HighlightForeColor { get; set; }
        public Color HighlightEdgeColor { get; set; }
        public Color HighlightInActiveForeColor { get; set; }
        public Color HighlightInActiveEdgeColor { get; set; }


        public bool IsVisible { get; set; }

        public bool IsMajorTickline { get; set; }


        public override string ToString()
        {
            return DisplayText;
        }
    }


    public interface IThemed
    {
        Color ForeColor { get; set; }
        Color EdgeColor { get; set; }
        Color HighlightForeColor { get; set; }
        Color HighlightEdgeColor { get; set; }
        Color HighlightInActiveForeColor { get; set; }
        Color HighlightInActiveEdgeColor { get; set; }
    }
}
