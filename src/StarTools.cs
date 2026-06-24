using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace StarTools
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    internal sealed class MainForm : Form
    {
        private static readonly Color Background = Color.FromArgb(9, 15, 26);
        private static readonly Color PanelColor = Color.FromArgb(17, 24, 39);
        private static readonly Color CardColor = Color.FromArgb(23, 32, 51);
        private static readonly Color InputColor = Color.FromArgb(15, 23, 42);
        private static readonly Color Foreground = Color.FromArgb(229, 231, 235);
        private static readonly Color Muted = Color.FromArgb(148, 163, 184);
        private static readonly Color Accent = Color.FromArgb(37, 99, 235);
        private static readonly Color Danger = Color.FromArgb(127, 29, 29);

        private readonly TabControl tabs = new TabControl();

        private TextBox cyclicInput;
        private ComboBox cyclicMode;
        private TextBox cyclicResult;

        private TextBox requestedFrom;
        private TextBox requestedTo;
        private FlowLayoutPanel routeRowsPanel;
        private Label headingMessage;
        private TextBox headingForwardResult;
        private TextBox headingReverseResult;
        private TextBox headingPathResult;
        private readonly List<RouteRow> routeRows = new List<RouteRow>();

        public MainForm()
        {
            Text = "Star Conquest Tools";
            AutoScaleMode = AutoScaleMode.Dpi;
            MinimumSize = new Size(780, 560);
            Size = new Size(1040, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Background;
            ForeColor = Foreground;
            Font = new Font("Segoe UI", 10F);
            AccessibleName = "Star Conquest Tools";
            AccessibleDescription = "Combined cyclic time converter and three dimensional heading calculator.";

            tabs.Dock = DockStyle.Fill;
            tabs.Padding = new Point(18, 8);
            tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabs.DrawItem += DrawTab;
            SetAccessible(tabs, "Star Conquest Tools tabs", "Choose between cyclic time conversion and three dimensional heading calculation.");
            Controls.Add(tabs);

            tabs.TabPages.Add(BuildCyclicTab());
            tabs.TabPages.Add(BuildHeadingTab());
            ApplyTheme(this);
        }

        private void DrawTab(object sender, DrawItemEventArgs e)
        {
            TabPage page = tabs.TabPages[e.Index];
            Rectangle bounds = e.Bounds;
            Color color = e.Index == tabs.SelectedIndex ? Accent : PanelColor;
            using (SolidBrush background = new SolidBrush(color))
            using (SolidBrush foreground = new SolidBrush(Foreground))
            {
                e.Graphics.FillRectangle(background, bounds);
                StringFormat format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                e.Graphics.DrawString(page.Text, Font, foreground, bounds, format);
            }
        }

        private TabPage BuildCyclicTab()
        {
            TabPage page = NewTab("Cyclic Time");
            Panel content = NewContentPanel();
            page.Controls.Add(content);

            Label title = NewTitle("Cyclic Time Converter");
            title.Location = new Point(28, 24);
            content.Controls.Add(title);

            Label note = NewNote("Converts base-25 cyclic time to Earth elapsed time or a calendar date. Digits are 0-9 and A-O.");
            note.Location = new Point(28, 72);
            note.Size = new Size(980, 52);
            content.Controls.Add(note);

            Label inputLabel = NewLabel("Base-25 time (example: E17L.KF0)", true);
            inputLabel.Location = new Point(28, 148);
            content.Controls.Add(inputLabel);

            cyclicInput = NewTextBox();
            cyclicInput.Location = new Point(28, 177);
            cyclicInput.Size = new Size(420, 32);
            cyclicInput.TabIndex = 0;
            cyclicInput.CharacterCasing = CharacterCasing.Upper;
            SetAccessible(cyclicInput, "Base-25 time", "Enter a base-25 cyclic time, for example E17L dot KF0.");
            cyclicInput.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter) CalculateCyclic();
            };
            content.Controls.Add(cyclicInput);

            Label modeLabel = NewLabel("Output", true);
            modeLabel.Location = new Point(28, 231);
            content.Controls.Add(modeLabel);

            cyclicMode = new ComboBox
            {
                Location = new Point(28, 260),
                Size = new Size(420, 32),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = InputColor,
                ForeColor = Foreground,
                FlatStyle = FlatStyle.Flat,
                TabIndex = 1
            };
            SetAccessible(cyclicMode, "Cyclic output type", "Choose whether to show elapsed Earth time or a calendar date and time from now.");
            cyclicMode.Items.Add("Elapsed Earth time");
            cyclicMode.Items.Add("Calendar date/time from now");
            cyclicMode.SelectedIndex = 0;
            content.Controls.Add(cyclicMode);

            Button calculate = NewButton("Convert", Accent);
            calculate.Location = new Point(28, 316);
            calculate.TabIndex = 2;
            SetAccessible(calculate, "Convert cyclic time", "Convert the entered base-25 cyclic time.");
            calculate.Click += delegate { CalculateCyclic(); };
            content.Controls.Add(calculate);

            Button clear = NewButton("Clear", Danger);
            clear.Location = new Point(150, 316);
            clear.TabIndex = 3;
            SetAccessible(clear, "Clear cyclic time", "Clear the cyclic time input and result.");
            clear.Click += delegate
            {
                cyclicInput.Clear();
                cyclicResult.Text = "Enter a cyclic time to see the result.";
                cyclicInput.Focus();
            };
            content.Controls.Add(clear);

            Label resultLabel = NewLabel("Result", true);
            resultLabel.Location = new Point(28, 384);
            content.Controls.Add(resultLabel);

            cyclicResult = NewOutputBox();
            cyclicResult.Location = new Point(28, 414);
            cyclicResult.Size = new Size(980, 120);
            cyclicResult.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            cyclicResult.TabIndex = 4;
            SetAccessible(cyclicResult, "Cyclic conversion result", "Shows the converted cyclic time result.");
            cyclicResult.Text = "Enter a cyclic time to see the result.";
            content.Controls.Add(cyclicResult);

            Label explanation = NewLabel(
                "One cycle = 15,625 seconds; one subcycle = 625 seconds; one minicycle = 25 seconds.",
                false);
            explanation.ForeColor = Muted;
            explanation.Location = new Point(28, 556);
            explanation.Size = new Size(950, 28);
            content.Controls.Add(explanation);

            return page;
        }

        private TabPage BuildHeadingTab()
        {
            TabPage page = NewTab("3D Heading");
            Panel content = NewContentPanel();
            page.Controls.Add(content);

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 9,
                BackColor = PanelColor,
                Padding = new Padding(16, 12, 16, 12),
                Margin = Padding.Empty
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 82F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 86F));
            content.Controls.Add(layout);

            Label title = NewTitle("3D Heading Calculator");
            title.Dock = DockStyle.Fill;
            layout.Controls.Add(title, 0, 0);

            Label note = NewNote(
                "East = 0 degrees, north = 90, west = 180, south = 270. Vertical is 0 level, positive upward, negative downward.");
            note.Dock = DockStyle.Fill;
            note.Margin = new Padding(0, 0, 0, 6);
            layout.Controls.Add(note, 0, 1);

            Panel requestPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardColor,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, 6)
            };
            layout.Controls.Add(requestPanel, 0, 2);

            TableLayoutPanel requestLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(10, 3, 10, 3),
                BackColor = CardColor
            };
            requestLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            requestLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            requestLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 18F));
            requestLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            requestPanel.Controls.Add(requestLayout);

            Label neededTitle = NewLabel("Route you need", true);
            neededTitle.Dock = DockStyle.Fill;
            neededTitle.Margin = new Padding(0);
            requestLayout.Controls.Add(neededTitle, 0, 0);

            Label neededHelp = NewLabel("Fill these in for the route you want calculated.", false);
            neededHelp.Dock = DockStyle.Fill;
            neededHelp.Margin = new Padding(0);
            neededHelp.ForeColor = Muted;
            neededHelp.TextAlign = ContentAlignment.MiddleRight;
            requestLayout.Controls.Add(neededHelp, 1, 0);

            requestedFrom = NewTextBox();
            requestedFrom.TabIndex = 0;
            SetAccessible(requestedFrom, "Starting point you need", "Enter the place where the route you want calculated begins.");
            requestLayout.Controls.Add(NewFieldPanel("Starting point you need", requestedFrom), 0, 1);

            requestedTo = NewTextBox();
            requestedTo.TabIndex = 1;
            SetAccessible(requestedTo, "Destination you need", "Enter the place where the route you want calculated ends.");
            requestLayout.Controls.Add(NewFieldPanel("Destination you need", requestedTo), 1, 1);

            Label knownRoutesLabel = NewLabel("Known routes", true);
            knownRoutesLabel.Dock = DockStyle.Fill;
            knownRoutesLabel.Margin = new Padding(0);
            layout.Controls.Add(knownRoutesLabel, 0, 3);

            TableLayoutPanel header = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 1,
                BackColor = CardColor,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            SetRouteColumnWidths(header);
            string[] headings = { "From", "To", "Horizontal", "Vertical", "Distance", "Actions" };
            for (int i = 0; i < headings.Length; i++)
            {
                Label label = NewLabel(headings[i], true);
                label.Dock = DockStyle.Fill;
                label.TextAlign = ContentAlignment.MiddleLeft;
                header.Controls.Add(label, i, 0);
            }
            layout.Controls.Add(header, 0, 4);

            routeRowsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = PanelColor,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            routeRowsPanel.SizeChanged += delegate { ResizeRouteRows(); };
            SetAccessible(routeRowsPanel, "Known route entries", "Enter at least two known routes. Use Add route to enter up to five.");
            layout.Controls.Add(routeRowsPanel, 0, 5);
            AddRouteRow();
            AddRouteRow();

            FlowLayoutPanel buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = Padding.Empty,
                Padding = new Padding(0, 4, 0, 0),
                BackColor = PanelColor
            };
            layout.Controls.Add(buttons, 0, 6);

            Button example = NewButton("Load example", Color.FromArgb(51, 65, 85));
            example.TabIndex = 0;
            SetAccessible(example, "Load heading example", "Fill the heading calculator with a sample route.");
            example.Click += delegate { LoadHeadingExample(); };
            buttons.Controls.Add(example);

            Button clear = NewButton("Clear", Danger);
            clear.TabIndex = 1;
            SetAccessible(clear, "Clear heading calculator", "Clear the route you need, all known routes, and the heading result.");
            clear.Click += delegate { ResetHeadingRows(); };
            buttons.Controls.Add(clear);

            Button calculate = NewButton("Calculate", Accent);
            calculate.TabIndex = 2;
            SetAccessible(calculate, "Calculate heading", "Calculate the heading for the route you need.");
            calculate.Click += delegate { CalculateHeading(); };
            buttons.Controls.Add(calculate);

            headingMessage = NewLabel("", true);
            headingMessage.ForeColor = Color.FromArgb(252, 165, 165);
            headingMessage.Dock = DockStyle.Fill;
            headingMessage.Margin = new Padding(0);
            SetAccessible(headingMessage, "Heading calculator message", "Shows validation messages for the heading calculator.");
            layout.Controls.Add(headingMessage, 0, 7);

            TableLayoutPanel resultPanel = NewHeadingResultPanel();
            layout.Controls.Add(resultPanel, 0, 8);
            ResetHeadingResultText();

            return page;
        }

        private static void SetRouteColumnWidths(TableLayoutPanel panel)
        {
            panel.ColumnStyles.Clear();
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 158F));
        }

        private void AddRouteRow()
        {
            if (routeRows.Count >= 5) return;

            RouteRow row = new RouteRow();
            row.Panel = new TableLayoutPanel
            {
                Width = RouteRowWidth(),
                Height = 29,
                ColumnCount = 6,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 3),
                Padding = Padding.Empty,
                BackColor = PanelColor
            };
            SetRouteColumnWidths(row.Panel);

            row.From = NewTextBox();
            row.To = NewTextBox();
            row.Horizontal = NewTextBox();
            row.Vertical = NewTextBox();
            row.Distance = NewTextBox();
            row.From.TabIndex = 0;
            row.To.TabIndex = 1;
            row.Horizontal.TabIndex = 2;
            row.Vertical.TabIndex = 3;
            row.Distance.TabIndex = 4;
            DockRouteTextBox(row.From);
            DockRouteTextBox(row.To);
            DockRouteTextBox(row.Horizontal);
            DockRouteTextBox(row.Vertical);
            DockRouteTextBox(row.Distance);
            row.ActionPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = Padding.Empty,
                Padding = new Padding(3, 1, 0, 0),
                BackColor = PanelColor
            };
            row.Remove = NewSmallButton("Remove", Danger);
            row.Remove.TabIndex = 5;
            row.Remove.Click += delegate { RemoveRouteRow(row); };

            row.Panel.Controls.Add(row.From, 0, 0);
            row.Panel.Controls.Add(row.To, 1, 0);
            row.Panel.Controls.Add(row.Horizontal, 2, 0);
            row.Panel.Controls.Add(row.Vertical, 3, 0);
            row.Panel.Controls.Add(row.Distance, 4, 0);
            row.ActionPanel.Controls.Add(row.Remove);
            row.Panel.Controls.Add(row.ActionPanel, 5, 0);

            routeRows.Add(row);
            routeRowsPanel.Controls.Add(row.Panel);
            RefreshRouteActions();
            ResizeRouteRows();
            ApplyTheme(row.Panel);
        }

        private void RemoveRouteRow(RouteRow row)
        {
            if (routeRows.Count <= 2) return;
            routeRowsPanel.Controls.Remove(row.Panel);
            row.Panel.Dispose();
            routeRows.Remove(row);
            RefreshRouteActions();
        }

        private void RefreshRouteActions()
        {
            for (int i = 0; i < routeRows.Count; i++)
            {
                RouteRow row = routeRows[i];
                int number = i + 1;
                row.Remove.Enabled = routeRows.Count > 2;
                SetAccessible(row.From, "Known route " + number + " starting point", "Enter the starting place for known route " + number + ".");
                SetAccessible(row.To, "Known route " + number + " destination", "Enter the destination for known route " + number + ".");
                SetAccessible(row.Horizontal, "Known route " + number + " horizontal heading", "Enter the horizontal heading in degrees for known route " + number + ".");
                SetAccessible(row.Vertical, "Known route " + number + " vertical heading", "Enter the vertical heading in degrees from negative 90 to 90 for known route " + number + ".");
                SetAccessible(row.Distance, "Known route " + number + " distance", "Enter the distance for known route " + number + ".");
                SetAccessible(row.Remove, "Remove known route " + number, "Remove known route " + number + ". At least two known routes are required.");
                if (row.Add != null)
                {
                    row.ActionPanel.Controls.Remove(row.Add);
                    row.Add.Dispose();
                    row.Add = null;
                }
            }

            if (routeRows.Count == 0) return;
            RouteRow last = routeRows[routeRows.Count - 1];
            last.Add = NewSmallButton("+ Add route", Color.FromArgb(51, 65, 85));
            last.Add.Enabled = routeRows.Count < 5;
            last.Add.TabIndex = 6;
            SetAccessible(last.Add, "Add known route", "Add another known route. You can enter up to five known routes.");
            last.Add.Click += delegate { AddRouteRow(); };
            last.ActionPanel.Controls.Add(last.Add);
            ResizeRouteRows();
        }

        private int RouteRowWidth()
        {
            if (routeRowsPanel == null) return 760;
            int width = routeRowsPanel.ClientSize.Width - 4;
            return width < 660 ? 660 : width;
        }

        private void ResizeRouteRows()
        {
            if (routeRowsPanel == null) return;
            int width = RouteRowWidth();
            foreach (RouteRow row in routeRows) row.Panel.Width = width;
        }

        private void ResetHeadingRows()
        {
            requestedFrom.Clear();
            requestedTo.Clear();
            foreach (RouteRow row in routeRows) row.Panel.Dispose();
            routeRows.Clear();
            routeRowsPanel.Controls.Clear();
            AddRouteRow();
            AddRouteRow();
            headingMessage.Text = "";
            ResetHeadingResultText();
        }

        private void LoadHeadingExample()
        {
            ResetHeadingRows();
            requestedFrom.Text = "Farport";
            requestedTo.Text = "Tom Town";
            SetRouteRow(routeRows[0], "Farport", "Andy's", "330", "-22", "312");
            SetRouteRow(routeRows[1], "Andy's", "Tom Town", "330", "87", "77");
            CalculateHeading();
        }

        private static void SetRouteRow(RouteRow row, string from, string to, string horizontal, string vertical, string distance)
        {
            row.From.Text = from;
            row.To.Text = to;
            row.Horizontal.Text = horizontal;
            row.Vertical.Text = vertical;
            row.Distance.Text = distance;
        }

        private void CalculateCyclic()
        {
            try
            {
                long seconds = CyclicConverter.ToSeconds(cyclicInput.Text);
                if (cyclicMode.SelectedIndex == 0)
                {
                    cyclicResult.Text = "Elapsed Earth time\r\n" + CyclicConverter.FormatElapsed(seconds) +
                        "\r\n\r\nTotal seconds: " + seconds.ToString("N0", CultureInfo.CurrentCulture);
                }
                else
                {
                    DateTime result = DateTime.Now.AddSeconds(seconds);
                    cyclicResult.Text = "Calendar date/time from now\r\n" + result.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            catch (Exception ex)
            {
                cyclicResult.Text = "Please check the cyclic time.\r\n" + ex.Message;
            }
        }

        private void CalculateHeading()
        {
            List<string> errors = new List<string>();
            string start = requestedFrom.Text.Trim();
            string destination = requestedTo.Text.Trim();
            if (start.Length == 0) errors.Add("Enter the starting point needed.");
            if (destination.Length == 0) errors.Add("Enter the destination needed.");

            Dictionary<string, List<Edge>> graph = new Dictionary<string, List<Edge>>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < routeRows.Count; i++)
            {
                RouteRow row = routeRows[i];
                string from = row.From.Text.Trim();
                string to = row.To.Text.Trim();
                double horizontal;
                double vertical;
                double distance;
                int number = i + 1;

                if (from.Length == 0 || to.Length == 0)
                    errors.Add("Route " + number + ": From and To are required.");
                if (!TryNumber(row.Horizontal.Text, out horizontal))
                    errors.Add("Route " + number + ": horizontal heading is invalid.");
                if (!TryNumber(row.Vertical.Text, out vertical) || vertical < -90 || vertical > 90)
                    errors.Add("Route " + number + ": vertical heading must be between -90 and 90.");
                if (!TryNumber(row.Distance.Text, out distance) || distance < 0)
                    errors.Add("Route " + number + ": distance must be zero or greater.");

                if (from.Length > 0 && to.Length > 0 &&
                    TryNumber(row.Horizontal.Text, out horizontal) &&
                    TryNumber(row.Vertical.Text, out vertical) &&
                    TryNumber(row.Distance.Text, out distance) &&
                    vertical >= -90 && vertical <= 90 && distance >= 0)
                {
                    Vector vector = HeadingMath.FromHeading(horizontal, vertical, distance);
                    AddEdge(graph, from, new Edge(from, to, vector));
                    AddEdge(graph, to, new Edge(to, from, vector.Negated()));
                }
            }

            if (errors.Count > 0)
            {
                headingMessage.Text = string.Join("  ", errors.ToArray());
                SetHeadingResultText("Calculation could not be completed.", "", "");
                return;
            }

            PathState solution = FindPath(graph, start, destination);
            if (solution == null)
            {
                headingMessage.Text = "No connected route was found. Check the place names and known routes.";
                SetHeadingResultText("Calculation could not be completed.", "", "");
                return;
            }

            headingMessage.Text = "";
            Heading forward = HeadingMath.ToHeading(solution.Vector);
            Heading reverse = HeadingMath.ToHeading(solution.Vector.Negated());
            string path = solution.Edges.Count == 0 ? start : FormatPath(solution.Edges);
            SetHeadingResultText(
                start + " -> " + destination + "\r\n" +
                "Heading: " + FormatCompact(forward.Horizontal) + ", " + FormatCompact(forward.Vertical) +
                "\r\nDistance: " + forward.Distance.ToString("0.00"),
                destination + " -> " + start + "\r\n" +
                "Heading: " + FormatCompact(reverse.Horizontal) + ", " + FormatCompact(reverse.Vertical) +
                "\r\nDistance: " + reverse.Distance.ToString("0.00"),
                "Path: " + path);
        }

        private void ResetHeadingResultText()
        {
            SetHeadingResultText("Enter known routes.", "Enter the route you need.", "");
        }

        private void SetHeadingResultText(string forward, string reverse, string path)
        {
            if (headingForwardResult != null) headingForwardResult.Text = forward;
            if (headingReverseResult != null) headingReverseResult.Text = reverse;
            if (headingPathResult != null) headingPathResult.Text = path;
        }

        private static bool TryNumber(string value, out double result)
        {
            return double.TryParse(value.Trim(), NumberStyles.Float, CultureInfo.CurrentCulture, out result) ||
                   double.TryParse(value.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        }

        private static void AddEdge(Dictionary<string, List<Edge>> graph, string name, Edge edge)
        {
            List<Edge> edges;
            if (!graph.TryGetValue(name, out edges))
            {
                edges = new List<Edge>();
                graph[name] = edges;
            }
            edges.Add(edge);
        }

        private static PathState FindPath(Dictionary<string, List<Edge>> graph, string start, string destination)
        {
            Queue<PathState> queue = new Queue<PathState>();
            HashSet<string> visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            queue.Enqueue(new PathState(start, new Vector(0, 0, 0), new List<Edge>()));
            visited.Add(start);

            while (queue.Count > 0)
            {
                PathState current = queue.Dequeue();
                if (string.Equals(current.Name, destination, StringComparison.OrdinalIgnoreCase)) return current;

                List<Edge> edges;
                if (!graph.TryGetValue(current.Name, out edges)) continue;
                foreach (Edge edge in edges)
                {
                    if (!visited.Add(edge.To)) continue;
                    List<Edge> path = new List<Edge>(current.Edges);
                    path.Add(edge);
                    queue.Enqueue(new PathState(edge.To, current.Vector.Plus(edge.Vector), path));
                }
            }
            return null;
        }

        private static string FormatPath(List<Edge> edges)
        {
            StringBuilder builder = new StringBuilder(edges[0].From);
            foreach (Edge edge in edges) builder.Append(" -> ").Append(edge.To);
            return builder.ToString();
        }

        private static string FormatCompact(double value)
        {
            double rounded = Math.Round(value);
            return Math.Abs(value - rounded) < 0.000001
                ? rounded.ToString("0")
                : value.ToString("0.00");
        }

        private TabPage NewTab(string text)
        {
            TabPage page = new TabPage(text) { BackColor = Background, ForeColor = Foreground };
            SetAccessible(page, text + " tab", "Shows the " + text + " tool.");
            return page;
        }

        private Panel NewContentPanel()
        {
            return new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = PanelColor,
                Padding = new Padding(10)
            };
        }

        private Label NewTitle(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = false,
                Size = new Size(760, 32),
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Foreground
            };
        }

        private Label NewNote(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = false,
                Padding = new Padding(8, 6, 8, 6),
                BackColor = CardColor,
                ForeColor = Foreground
            };
        }

        private Label NewLabel(string text, bool bold)
        {
            return new Label
            {
                Text = text,
                AutoSize = false,
                AutoEllipsis = true,
                Size = new Size(360, 22),
                Font = new Font("Segoe UI", 9.25F, bold ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = Foreground
            };
        }

        private TableLayoutPanel NewHeadingResultPanel()
        {
            TableLayoutPanel panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Margin = new Padding(0),
                Padding = new Padding(0),
                BackColor = PanelColor
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 18F));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));

            Label forwardLabel = NewLabel("Forward route", true);
            forwardLabel.Dock = DockStyle.Fill;
            forwardLabel.Margin = new Padding(0, 0, 5, 0);
            panel.Controls.Add(forwardLabel, 0, 0);

            Label reverseLabel = NewLabel("Reverse route", true);
            reverseLabel.Dock = DockStyle.Fill;
            reverseLabel.Margin = new Padding(5, 0, 0, 0);
            panel.Controls.Add(reverseLabel, 1, 0);

            headingForwardResult = NewCompactOutputBox();
            headingForwardResult.Margin = new Padding(0, 0, 5, 2);
            SetAccessible(headingForwardResult, "Forward heading result", "Shows the calculated route, heading, and distance from the starting point to the destination.");
            panel.Controls.Add(headingForwardResult, 0, 1);

            headingReverseResult = NewCompactOutputBox();
            headingReverseResult.Margin = new Padding(5, 0, 0, 2);
            SetAccessible(headingReverseResult, "Reverse heading result", "Shows the calculated route, heading, and distance from the destination back to the starting point.");
            panel.Controls.Add(headingReverseResult, 1, 1);

            headingPathResult = NewCompactOutputBox();
            headingPathResult.Margin = new Padding(0);
            SetAccessible(headingPathResult, "Connected path result", "Shows the known routes used to connect the starting point and destination.");
            panel.Controls.Add(headingPathResult, 0, 2);
            panel.SetColumnSpan(headingPathResult, 2);

            return panel;
        }

        private TableLayoutPanel NewFieldPanel(string labelText, Control field)
        {
            TableLayoutPanel panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(0),
                Padding = new Padding(0),
                BackColor = CardColor
            };
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 18F));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            Label label = NewLabel(labelText, true);
            label.Dock = DockStyle.Fill;
            label.Margin = new Padding(0);
            panel.Controls.Add(label, 0, 0);

            field.Dock = DockStyle.Fill;
            field.Margin = new Padding(0, 2, 10, 1);
            field.MinimumSize = new Size(0, 28);
            panel.Controls.Add(field, 0, 1);
            return panel;
        }

        private TextBox NewTextBox()
        {
            return new TextBox
            {
                Margin = new Padding(3, 2, 5, 2),
                BackColor = InputColor,
                ForeColor = Foreground,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9.25F)
            };
        }

        private static void DockRouteTextBox(TextBox textBox)
        {
            textBox.Dock = DockStyle.Fill;
        }

        private TextBox NewOutputBox()
        {
            return new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = InputColor,
                ForeColor = Foreground,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 10F)
            };
        }

        private TextBox NewCompactOutputBox()
        {
            return new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.None,
                BackColor = InputColor,
                ForeColor = Foreground,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 8.75F)
            };
        }

        private Button NewButton(string text, Color color)
        {
            return new Button
            {
                Text = text,
                AutoSize = true,
                MinimumSize = new Size(92, 29),
                FlatStyle = FlatStyle.Flat,
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                UseVisualStyleBackColor = false
            };
        }

        private Button NewSmallButton(string text, Color color)
        {
            Button button = NewButton(text, color);
            button.MinimumSize = Size.Empty;
            button.AutoSize = false;
            button.Size = new Size(text.StartsWith("+") ? 82 : 64, 26);
            button.MinimumSize = button.Size;
            button.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            button.Margin = new Padding(0, 0, 4, 0);
            return button;
        }

        private static void SetAccessible(Control control, string name, string description)
        {
            control.AccessibleName = name;
            control.AccessibleDescription = description;
        }

        private static void ApplyTheme(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                if (!(control is TextBox) && !(control is ComboBox) && !(control is Button))
                {
                    if (control.BackColor == SystemColors.Control) control.BackColor = PanelColor;
                    control.ForeColor = Foreground;
                }
                ApplyTheme(control);
            }
        }

        private sealed class RouteRow
        {
            public TableLayoutPanel Panel;
            public TextBox From;
            public TextBox To;
            public TextBox Horizontal;
            public TextBox Vertical;
            public TextBox Distance;
            public FlowLayoutPanel ActionPanel;
            public Button Remove;
            public Button Add;
        }

        private sealed class Edge
        {
            public readonly string From;
            public readonly string To;
            public readonly Vector Vector;

            public Edge(string from, string to, Vector vector)
            {
                From = from;
                To = to;
                Vector = vector;
            }
        }

        private sealed class PathState
        {
            public readonly string Name;
            public readonly Vector Vector;
            public readonly List<Edge> Edges;

            public PathState(string name, Vector vector, List<Edge> edges)
            {
                Name = name;
                Vector = vector;
                Edges = edges;
            }
        }
    }

    internal static class CyclicConverter
    {
        private const long Minicycle = 25;
        private const long Subcycle = 625;
        private const long Cycle = 15625;

        public static long ToSeconds(string input)
        {
            string value = (input ?? "").Trim().ToUpperInvariant();
            if (value.Length == 0) throw new FormatException("Enter a base-25 time.");
            string[] pieces = value.Split('.');
            if (pieces.Length > 2 || pieces[0].Length == 0)
                throw new FormatException("Use the format DAYS.HMS, such as E17L.KF0.");

            long days = ParseBase25(pieces[0]);
            string rest = pieces.Length == 2 ? pieces[1] : "";
            if (rest.Length > 3) throw new FormatException("Use no more than three digits after the decimal point.");
            while (rest.Length < 3) rest += "0";

            long hours = Digit(rest[0]);
            long minutes = Digit(rest[1]);
            long seconds = Digit(rest[2]);
            checked
            {
                return days * Cycle + hours * Subcycle + minutes * Minicycle + seconds;
            }
        }

        private static long ParseBase25(string value)
        {
            long result = 0;
            checked
            {
                foreach (char character in value) result = result * 25 + Digit(character);
            }
            return result;
        }

        private static int Digit(char character)
        {
            if (character >= '0' && character <= '9') return character - '0';
            if (character >= 'A' && character <= 'O') return 10 + character - 'A';
            throw new FormatException("Only digits 0-9 and letters A-O are valid.");
        }

        public static string FormatElapsed(long totalSeconds)
        {
            long days = totalSeconds / 86400;
            long seconds = totalSeconds % 86400;
            long years = days / 365;
            days %= 365;
            long months = days / 30;
            days %= 30;
            long hours = seconds / 3600;
            seconds %= 3600;
            long minutes = seconds / 60;
            seconds %= 60;

            List<string> parts = new List<string>();
            AddPart(parts, years, "year");
            AddPart(parts, months, "month");
            AddPart(parts, days, "day");
            AddPart(parts, hours, "hour");
            AddPart(parts, minutes, "minute");
            AddPart(parts, seconds, "second");
            return parts.Count == 0 ? "0 seconds" : string.Join(", ", parts.ToArray());
        }

        private static void AddPart(List<string> parts, long value, string name)
        {
            if (value != 0) parts.Add(value + " " + name + (value == 1 ? "" : "s"));
        }
    }

    internal struct Vector
    {
        public readonly double East;
        public readonly double North;
        public readonly double Up;

        public Vector(double east, double north, double up)
        {
            East = east;
            North = north;
            Up = up;
        }

        public Vector Plus(Vector other) { return new Vector(East + other.East, North + other.North, Up + other.Up); }
        public Vector Negated() { return new Vector(-East, -North, -Up); }
    }

    internal struct Heading
    {
        public readonly double Horizontal;
        public readonly double Vertical;
        public readonly double Distance;

        public Heading(double horizontal, double vertical, double distance)
        {
            Horizontal = horizontal;
            Vertical = vertical;
            Distance = distance;
        }
    }

    internal static class HeadingMath
    {
        public static Vector FromHeading(double horizontal, double vertical, double distance)
        {
            double h = horizontal * Math.PI / 180;
            double v = vertical * Math.PI / 180;
            double flat = distance * Math.Cos(v);
            return new Vector(flat * Math.Cos(h), flat * Math.Sin(h), distance * Math.Sin(v));
        }

        public static Heading ToHeading(Vector vector)
        {
            double flat = Math.Sqrt(vector.East * vector.East + vector.North * vector.North);
            double distance = Math.Sqrt(flat * flat + vector.Up * vector.Up);
            if (distance < 1e-12) return new Heading(0, 0, 0);
            double horizontal = Math.Atan2(vector.North, vector.East) * 180 / Math.PI;
            horizontal = ((horizontal % 360) + 360) % 360;
            double vertical = Math.Atan2(vector.Up, flat) * 180 / Math.PI;
            return new Heading(horizontal, vertical, distance);
        }
    }
}
